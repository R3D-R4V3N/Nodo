using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Chats;
using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Chats;
using Rise.Shared.Identity;

namespace Rise.Services.Chats;

public class ChatService(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider,
    IChatMessageDispatcher? messageDispatcher = null) : IChatService
{
    private const string UnauthorizedProfileMessage = "De huidige gebruiker heeft geen geldig profiel.";

    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ISessionContextProvider _sessionContextProvider = sessionContextProvider;
    private readonly IChatMessageDispatcher? _messageDispatcher = messageDispatcher;

    public async Task<Result<ChatResponse.Index>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var accountId = _sessionContextProvider.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var currentUser = await _dbContext.ApplicationUsers
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Unauthorized(UnauthorizedProfileMessage);
        }

        var chatsFromDb = await _dbContext.Chats
            .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .ToListAsync(cancellationToken);

        var supervisorLookup = await BuildSupervisorLookupAsync(chatsFromDb, cancellationToken);

        var chatDtos = chatsFromDb
            .Where(chat => IsChatVisibleToUser(chat, currentUser, supervisorLookup))
            .Select(chat => MapChatToDto(chat, supervisorLookup))
            .ToList();

        return Result.Success(new ChatResponse.Index
        {
            Chats = chatDtos
        });
    }

    public async Task<Result<ChatDto.Index>> GetByIdAsync(int chatId, CancellationToken cancellationToken = default)
    {
        var accountId = _sessionContextProvider.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var currentUser = await _dbContext.ApplicationUsers
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Unauthorized(UnauthorizedProfileMessage);
        }

        var chat = await _dbContext.Chats
            .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .SingleOrDefaultAsync(c => c.Id == chatId, cancellationToken);

        if (chat is null)
        {
            return Result.NotFound($"Chat met id '{chatId}' werd niet gevonden.");
        }

        var supervisorLookup = await BuildSupervisorLookupAsync([chat], cancellationToken);

        if (!IsChatVisibleToUser(chat, currentUser, supervisorLookup))
        {
            return Result.Forbidden("Je hebt geen toegang tot dit gesprek.");
        }

        var dto = MapChatToDto(chat, supervisorLookup);

        return Result.Success(dto);
    }

    public async Task<Result<MessageDto>> CreateMessageAsync(ChatRequest.CreateMessage request, CancellationToken cancellationToken = default)
    {
        var accountId = _sessionContextProvider.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var sender = await _dbContext.ApplicationUsers
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        if (sender is null)
        {
            return Result.Unauthorized(UnauthorizedProfileMessage);
        }

        var chat = await _dbContext.Chats
            .Include(c => c.Participants)
            .SingleOrDefaultAsync(c => c.Id == request.ChatId, cancellationToken);

        if (chat is null)
        {
            return Result.NotFound($"Chat met id '{request.ChatId}' werd niet gevonden.");
        }

        var isParticipant = chat.Participants.Any(p => p.UserId == sender.Id);
        if (!isParticipant)
        {
            return Result.Forbidden("Je maakt geen deel uit van dit gesprek.");
        }

        var message = new Message
        {
            ChatId = chat.Id,
            SenderId = sender.Id,
            Inhoud = request.Content!.Trim()
        };

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync(cancellationToken);

        message.Sender = sender;
        var dto = MapMessageToDto(message, sender);

        if (_messageDispatcher is not null)
        {
            try
            {
                await _messageDispatcher.NotifyMessageCreatedAsync(chat.Id, dto, cancellationToken);
            }
            catch
            {
                // Realtime notificaties mogen een mislukte call niet blokkeren.
            }
        }

        return Result.Success(dto);
    }

    public async Task<Result<ChatEmergencyStatusDto>> ActivateEmergencyAsync(ChatRequest.ToggleEmergency request, CancellationToken cancellationToken = default)
    {
        var accountId = _sessionContextProvider.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var currentUser = await _dbContext.ApplicationUsers
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Unauthorized(UnauthorizedProfileMessage);
        }

        var chat = await _dbContext.Chats
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .SingleOrDefaultAsync(c => c.Id == request.ChatId, cancellationToken);

        if (chat is null)
        {
            return Result.NotFound($"Chat met id '{request.ChatId}' werd niet gevonden.");
        }

        if (!chat.Participants.Any(p => p.UserId == currentUser.Id))
        {
            return Result.Forbidden("Je maakt geen deel uit van dit gesprek.");
        }

        if (chat.IsEmergencyActive)
        {
            return Result.Success(BuildEmergencyStatusDto(chat, await BuildSupervisorLookupAsync([chat], cancellationToken)));
        }

        var supervisorLookup = await BuildSupervisorLookupAsync([chat], cancellationToken);

        var participantsWithoutSupervisor = chat.Participants
            .Where(p => p.User.UserType == UserType.ChatUser)
            .Where(p => !supervisorLookup.TryGetValue(p.UserId, out var supervisors) || supervisors.Count == 0)
            .Select(p => $"{p.User.FirstName} {p.User.LastName}")
            .ToList();

        if (participantsWithoutSupervisor.Count > 0)
        {
            return Result.Conflict($"Voor deze noodmelding ontbreken gekoppelde supervisors: {string.Join(", ", participantsWithoutSupervisor)}.");
        }

        chat.ActivateEmergency(currentUser, DateTime.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var status = BuildEmergencyStatusDto(chat, supervisorLookup);

        if (_messageDispatcher is not null)
        {
            try
            {
                await _messageDispatcher.NotifyEmergencyStatusChangedAsync(chat.Id, status, cancellationToken);
            }
            catch
            {
            }
        }

        return Result.Success(status);
    }

    public async Task<Result<ChatEmergencyStatusDto>> DeactivateEmergencyAsync(ChatRequest.ToggleEmergency request, CancellationToken cancellationToken = default)
    {
        var accountId = _sessionContextProvider.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var currentUser = await _dbContext.ApplicationUsers
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Unauthorized(UnauthorizedProfileMessage);
        }

        var chat = await _dbContext.Chats
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .SingleOrDefaultAsync(c => c.Id == request.ChatId, cancellationToken);

        if (chat is null)
        {
            return Result.NotFound($"Chat met id '{request.ChatId}' werd niet gevonden.");
        }

        if (!chat.IsEmergencyActive)
        {
            return Result.Conflict("Er is geen noodmelding actief om in te trekken.");
        }

        if (chat.EmergencyInitiatorId != currentUser.Id)
        {
            return Result.Forbidden("Alleen de melder kan de noodmelding intrekken.");
        }

        chat.DeactivateEmergency(currentUser);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var supervisorLookup = await BuildSupervisorLookupAsync([chat], cancellationToken);
        var status = BuildEmergencyStatusDto(chat, supervisorLookup);

        if (_messageDispatcher is not null)
        {
            try
            {
                await _messageDispatcher.NotifyEmergencyStatusChangedAsync(chat.Id, status, cancellationToken);
            }
            catch
            {
            }
        }

        return Result.Success(status);
    }

    private async Task<Dictionary<int, List<UserSupervisor>>> BuildSupervisorLookupAsync(IEnumerable<Chat> chats, CancellationToken cancellationToken)
    {
        var participantIds = chats
            .SelectMany(c => c.Participants.Select(p => p.UserId))
            .Distinct()
            .ToList();

        if (participantIds.Count == 0)
        {
            return new Dictionary<int, List<UserSupervisor>>();
        }

        var supervisorLinks = await _dbContext.UserSupervisors
            .Include(us => us.Supervisor)
            .Where(us => participantIds.Contains(us.ChatUserId))
            .ToListAsync(cancellationToken);

        return supervisorLinks
            .GroupBy(us => us.ChatUserId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private static bool IsChatVisibleToUser(Chat chat, ApplicationUser currentUser, IReadOnlyDictionary<int, List<UserSupervisor>> supervisorLookup)
    {
        if (chat.Participants.Any(p => p.UserId == currentUser.Id))
        {
            return true;
        }

        if (!chat.IsEmergencyActive)
        {
            return false;
        }

        return chat.Participants
            .Select(p => p.UserId)
            .Any(userId => supervisorLookup.TryGetValue(userId, out var supervisors) && supervisors.Any(link => link.SupervisorId == currentUser.Id));
    }

    private ChatDto.Index MapChatToDto(Chat chat, IReadOnlyDictionary<int, List<UserSupervisor>> supervisorLookup)
    {
        return new ChatDto.Index
        {
            chatId = chat.Id,
            messages = chat.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(MapMessageToDto)
                .ToList(),
            participants = BuildParticipantDtos(chat),
            emergency = BuildEmergencyStatusDto(chat, supervisorLookup)
        };
    }

    private static List<ChatParticipantDto> BuildParticipantDtos(Chat chat)
    {
        return chat.Participants
            .Select(p => new ChatParticipantDto
            {
                userId = p.UserId,
                displayName = $"{p.User.FirstName} {p.User.LastName}",
                accountId = p.User.AccountId,
                userType = p.User.UserType.ToString()
            })
            .ToList();
    }

    private ChatEmergencyStatusDto BuildEmergencyStatusDto(Chat chat, IReadOnlyDictionary<int, List<UserSupervisor>> supervisorLookup)
    {
        var status = new ChatEmergencyStatusDto
        {
            chatId = chat.Id,
            isActive = chat.IsEmergencyActive,
            activatedAtUtc = chat.EmergencyActivatedAtUtc,
            supervisors = BuildSupervisorDtos(chat, supervisorLookup)
        };

        if (chat.EmergencyInitiatorId is int initiatorId)
        {
            var initiator = chat.Participants.FirstOrDefault(p => p.UserId == initiatorId)?.User;
            if (initiator is not null)
            {
                status.initiatorUserId = initiator.Id;
                status.initiatorName = $"{initiator.FirstName} {initiator.LastName}";
                status.initiatorAccountId = initiator.AccountId;
            }
        }

        return status;
    }

    private List<ChatSupervisorAccessDto> BuildSupervisorDtos(Chat chat, IReadOnlyDictionary<int, List<UserSupervisor>> supervisorLookup)
    {
        var supervisors = new List<ChatSupervisorAccessDto>();

        foreach (var participant in chat.Participants)
        {
            if (!supervisorLookup.TryGetValue(participant.UserId, out var links))
            {
                continue;
            }

            foreach (var link in links)
            {
                supervisors.Add(new ChatSupervisorAccessDto
                {
                    chatUserId = participant.UserId,
                    supervisorUserId = link.SupervisorId,
                    supervisorName = $"{link.Supervisor.FirstName} {link.Supervisor.LastName}",
                    supervisorAccountId = link.Supervisor.AccountId
                });
            }
        }

        return supervisors;
    }

    private static MessageDto MapMessageToDto(Message message)
    {
        var sender = message.Sender ?? throw new InvalidOperationException("Message sender must be loaded.");
        return MapMessageToDto(message, sender);
    }

    private static MessageDto MapMessageToDto(Message message, ApplicationUser sender)
    {
        return new MessageDto
        {
            ChatId = message.ChatId,
            Id = message.Id,
            Content = message.Inhoud,
            Timestamp = message.CreatedAt,
            SenderId = message.SenderId,
            SenderName = $"{sender.FirstName} {sender.LastName}",
            SenderAccountId = sender.AccountId
        };
    }
}
