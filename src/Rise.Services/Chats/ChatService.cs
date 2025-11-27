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
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt)
                    .ThenByDescending(m => m.Id)
                    .Take(1)
                ).ThenInclude(m => m.Sender)
            .Include(c => c.Users)
            .Where(c => c.Users.Contains(sender))
            .ToListAsync(cancellationToken);

        var chatIds = chatsFromDb
            .Select(c => c.Id)
            .ToList();

        var lastReadLookup = await _dbContext.ChatMessageHistory
            .Where(h => h.User.AccountId == accountId && chatIds.Contains(h.Chat.Id))
            .Select(h => new
            {
                ChatId = h.Chat.Id,
                LastReadAt = (DateTime?)h.LastReadMessage.CreatedAt,
                LastReadMessageId = (int?)h.LastReadMessage.Id
            })
            .ToDictionaryAsync(x => x.ChatId, x => x, cancellationToken);

        var messageGroups = await _dbContext.Messages
            .Where(m => chatIds.Contains(m.Chat.Id))
            .Select(m => new { ChatId = m.Chat.Id, m.Id, m.CreatedAt })
            .ToListAsync(cancellationToken);

        var unreadCounts = messageGroups
            .GroupBy(m => m.ChatId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    if (!lastReadLookup.TryGetValue(g.Key, out var lastRead) || lastRead is null)
                    {
                        return g.Count();
                    }

                    return g.Count(m =>
                        m.CreatedAt > lastRead.LastReadAt!.Value ||
                        (m.CreatedAt == lastRead.LastReadAt!.Value && m.Id > lastRead.LastReadMessageId));
                });

        var chatDtos = chatsFromDb
            .Select(chat =>
            {
                unreadCounts.TryGetValue(chat.Id, out var unread);
                return ChatMapper.ToGetChatsDto(chat, unread);
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

        var lastMessage = chat.Messages
            .OrderBy(m => m.CreatedAt)
            .LastOrDefault();

        if (lastMessage is not null)
        {
            var historyUpdated = await UpdateReadHistoryAsync(chat, sender, lastMessage, cancellationToken);
            if (historyUpdated)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        var dto = chat.ToGetChatDto();

        return Result.Success(new ChatResponse.GetChat() 
        { 
            Chat = dto
        });
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

        await UpdateReadHistoryAsync(chat, sender, message, cancellationToken);

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

    private async Task<bool> UpdateReadHistoryAsync(Chat chat, BaseUser user, Message lastMessage, CancellationToken cancellationToken)
    {
        var existingHistory = await _dbContext.ChatMessageHistory
            .Include(h => h.User)
            .Include(h => h.Chat)
            .Include(h => h.LastReadMessage)
            .SingleOrDefaultAsync(h => h.Chat.Id == chat.Id && h.User.Id == user.Id, cancellationToken);

        if (existingHistory is null)
        {
            var item = new MessageHistoryItem
            {
                Chat = chat,
                User = user,
                LastReadMessage = lastMessage,
            };

            _dbContext.ChatMessageHistory.Add(item);
            return true;
        }

        if (existingHistory.LastReadMessage.Id == lastMessage.Id)
        {
            return false;
        }

        existingHistory.LastReadMessage = lastMessage;
        return true;
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
}
