using Microsoft.EntityFrameworkCore;
using Rise.Domain.Chats;
using Rise.Domain.Messages;
using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Services.Chats.Mapper;
using Rise.Services.Identity;
using Rise.Shared.Common;
using Rise.Shared.Chats;
using Rise.Shared.Identity;
using Rise.Domain.Common.ValueObjects;

namespace Rise.Services.Chats;

public class ChatService(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider,
    IChatMessageDispatcher? messageDispatcher = null) : IChatService
{

    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ISessionContextProvider _sessionContextProvider = sessionContextProvider;
    private readonly IChatMessageDispatcher? _messageDispatcher = messageDispatcher;

    public async Task<Result<ChatResponse.GetChats>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var principal = _sessionContextProvider.User;
        if (principal is null)
        {
            return Result.Unauthorized();
        }

        var accountId = principal.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var sender = await FindProfileByAccountIdAsync(accountId, cancellationToken);

        if (sender is null)
        {
            if (principal.IsInRole(AppRoles.Administrator))
            {
                return Result.Success(new ChatResponse.GetChats
                {
                    Chats = []
                });
            }

            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        var chatsFromDb = await _dbContext.Chats
            .Where(c => c.ChatType == ChatType.Private || c.ChatType == ChatType.Group)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt)
                    .ThenByDescending(m => m.Id)
                    .Take(1)
                ).ThenInclude(m => m.Sender)
            .Include(c => c.Users)
            .Where(c => c.Users.Contains(sender))
            .ToListAsync(cancellationToken);

        var chatIds = chatsFromDb
            .Select(chat => chat.Id)
            .ToList();

        var lastReadLookup = await _dbContext.ChatMessageHistories
            .Where(history => history.UserId == sender.Id && chatIds.Contains(history.ChatId))
            .ToDictionaryAsync(history => history.ChatId, cancellationToken);

        var unreadLookup = await _dbContext.Messages
            .Where(message => chatIds.Contains(EF.Property<int>(message, "ChatId")))
            .Select(message => new
            {
                ChatId = EF.Property<int>(message, "ChatId"),
                message.Id,
                message.CreatedAt
            })
            .GroupBy(message => message.ChatId)
            .ToDictionaryAsync(
                group => group.Key,
                group =>
                {
                    lastReadLookup.TryGetValue(group.Key, out var lastRead);

                    return group.Count(message => IsUnread(message.CreatedAt, message.Id, lastRead));
                },
                cancellationToken);

        var chatDtos = chatsFromDb
            .Select(chat =>
            {
                var dto = ChatMapper.ToGetChatsDto(chat)!;
                dto.UnreadCount = unreadLookup.TryGetValue(chat.Id, out var count) ? count : 0;
                return dto;
            })
            .ToList();

        return Result.Success(new ChatResponse.GetChats
        {
            Chats = chatDtos!
        });
    }

    public async Task<Result<ChatResponse.GetChat>> GetByIdAsync(int chatId, CancellationToken cancellationToken = default)
    {
        var principal = _sessionContextProvider.User;
        if (principal is null)
        {
            return Result.Unauthorized();
        }

        var accountId = principal.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var sender = await FindProfileByAccountIdAsync(accountId, cancellationToken);

        if (sender is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        // no clue how 'c.Id == chatId && c.Users.Contains(sender)' translates to sql
        var chat = await _dbContext.Chats
            .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
            .Include(c => c.Users)
            .SingleOrDefaultAsync(c => c.Id == chatId && c.Users.Contains(sender), cancellationToken);

        if (chat is null)
        {
            return Result.NotFound($"Chat met id '{chatId}' werd niet gevonden.");
        }

        await MarkChatAsReadAsync(chat, sender, cancellationToken);

        var dto = chat.ToGetChatDto();

        return Result.Success(new ChatResponse.GetChat()
        {
            Chat = dto
        });
    }

    private async Task MarkChatAsReadAsync(Chat chat, BaseUser reader, CancellationToken cancellationToken)
    {
        var lastMessage = chat.Messages
            .OrderByDescending(message => message.CreatedAt)
            .ThenByDescending(message => message.Id)
            .FirstOrDefault();

        if (lastMessage is null)
        {
            return;
        }

        var history = await _dbContext.ChatMessageHistories
            .SingleOrDefaultAsync(history => history.ChatId == chat.Id && history.UserId == reader.Id, cancellationToken);

        var hasNewerTimestamp = history?.LastReadAt is null || lastMessage.CreatedAt > history.LastReadAt;
        var hasNewerMessageId = history?.LastReadMessageId is null || lastMessage.Id > history.LastReadMessageId;

        if (!hasNewerTimestamp && !hasNewerMessageId)
        {
            return;
        }

        if (history is null)
        {
            history = new ChatMessageHistory
            {
                ChatId = chat.Id,
                UserId = reader.Id
            };

            _dbContext.ChatMessageHistories.Add(history);
        }

        history.LastReadAt = lastMessage.CreatedAt;
        history.LastReadMessageId = lastMessage.Id;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Result<MessageDto.Chat>> CreateMessageAsync(ChatRequest.CreateMessage request, CancellationToken cancellationToken = default)
    {
        var principal = _sessionContextProvider.User;
        if (principal is null)
        {
            return Result.Unauthorized();
        }

        var accountId = principal.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var sender = await FindProfileByAccountIdAsync(accountId, cancellationToken);

        if (sender is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        // no clue how 'c.Id == chatId && c.Users.Contains(sender)' translates to sql
        var chat = await _dbContext.Chats
            .Include(c => c.Users)
            .SingleOrDefaultAsync(c => c.Id == request.ChatId && c.Users.Contains(sender), cancellationToken);

        if (chat is null)
        {
            return Result.NotFound($"Chat met id '{request.ChatId}' werd niet gevonden.");
        }

        byte[]? audioBytes = null;
        string? audioContentType = null;
        double? audioDurationSeconds = null;

        var audioDataUrl = string.IsNullOrWhiteSpace(request.AudioDataBlob)
            ? null
            : request.AudioDataBlob.Trim();

        if (!string.IsNullOrWhiteSpace(audioDataUrl))
        {
            if (!AudioHelperMethods.TryParseAudioDataUrl(audioDataUrl, out audioContentType, out audioBytes, out var parseError))
            {
                return Result.Invalid(new ValidationError(nameof(request.AudioDataBlob), parseError ?? "Ongeldige audio data-URL."));
            }

            if (request.AudioDurationSeconds.HasValue)
            {
                var duration = request.AudioDurationSeconds.Value;
                if (duration > 0 && double.IsFinite(duration))
                {
                    audioDurationSeconds = duration;
                }
            }
        }

        var textMessage = request.Content;
        if (textMessage is null && audioBytes is null)
        {
            return Result.Invalid(new ValidationError(nameof(request.Content), "Een bericht moet tekst of audio bevatten."));
        }



        var message = new Message
        {
            Chat = chat,
            Sender = sender,
            Text = TextMessage.Create(textMessage!),
            AudioContentType = audioContentType,
            AudioData = audioBytes,
            AudioDurationSeconds = audioDurationSeconds
        };

        chat.AddMessage(message);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = message.ToChatDto()!;

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

    public Task<Result<int>> QueueMessageAsync(ChatRequest.CreateMessage request, CancellationToken cancellationToken = default)
    {
        // Server-side queuing is not supported; offline queuing is a client-only concern.
        return Task.FromResult(Result<int>.Error("Offline wachtrij is enkel beschikbaar in de client."));
    }

    private async Task<BaseUser?> FindProfileByAccountIdAsync(string accountId, CancellationToken cancellationToken)
    {
        var profile = await _dbContext
            .Set<BaseUser>()
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        if (profile is not null)
        {
            return profile;
        }

        profile = await _dbContext.Supervisors
            .SingleOrDefaultAsync(s => s.AccountId == accountId, cancellationToken);

        if (profile is not null)
        {
            return profile;
        }

        return await _dbContext.Users
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);
    }

    private static bool IsUnread(DateTime createdAt, int messageId, ChatMessageHistory? history)
    {
        if (history is null)
        {
            return true;
        }

        var newerThanTimestamp = history.LastReadAt.HasValue && createdAt > history.LastReadAt.Value;
        var newerThanMessageId = history.LastReadMessageId.HasValue && messageId > history.LastReadMessageId.Value;

        return newerThanTimestamp || newerThanMessageId;
    }
}
