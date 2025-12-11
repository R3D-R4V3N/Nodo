using Microsoft.EntityFrameworkCore;
using Rise.Domain.Chats;
using Rise.Domain.Common.ValueObjects;
using Rise.Domain.Messages;
using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Services.BlobStorage;
using Rise.Services.Chats.Mapper;
using Rise.Services.Identity;
using Rise.Services.Notifications;
using Rise.Shared.Common;
using Rise.Shared.Chats;
using Rise.Shared.Common;
using Rise.Shared.Identity;
using System.Xml.Linq;

namespace Rise.Services.Chats;

public class ChatService(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider,
    IBlobStorageService blobStorage,
    IChatMessageDispatcher? messageDispatcher = null,
    IPushNotificationService? pushNotificationService = null) : IChatService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ISessionContextProvider _sessionContextProvider = sessionContextProvider;
    private readonly IBlobStorageService _blobStorage = blobStorage;
    private readonly IChatMessageDispatcher? _messageDispatcher = messageDispatcher;
    private readonly IPushNotificationService? _pushNotificationService = pushNotificationService;

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

        var loggedInUser = await FindProfileByAccountIdAsync(accountId, cancellationToken);

        if (loggedInUser is null or Admin _)
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

        var chatsQuery = _dbContext.Chats
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt)
                    .ThenByDescending(m => m.Id)
                    .Take(1)
                ).ThenInclude(m => m.Sender)
            .Include(c => c.Users)
            .Where(c => c.Users.Contains(loggedInUser)) ;

        if (loggedInUser is User _)
        {
            chatsQuery = chatsQuery
                .Where(c => c.ChatType == ChatType.Private || c.ChatType == ChatType.Group);
        }
        else if(loggedInUser is Supervisor _)
        {
            chatsQuery = chatsQuery
                .Where(c => c.ChatType == ChatType.Supervisor);
        }
        
        var chatsFromDb = await chatsQuery
            .ToListAsync(cancellationToken);
        
        var chatIds = chatsFromDb
            .Select(chat => chat.Id)
            .ToList();

        var lastReadLookup = await _dbContext.ChatMessageHistories
            .Where(history => history.UserId == loggedInUser.Id && chatIds.Contains(history.ChatId))
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
            .Include(c => c.Users)
            .Include(c => c.Messages
                .OrderByDescending(message => message.CreatedAt)
                .ThenByDescending(message => message.Id)
                .Take(1))
            .ThenInclude(message => message.Sender)
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

    public async Task<Result<MessageDto.Chat>> CreateMessageAsync(ChatRequest.CreateMessage request, CancellationToken ctx = default)
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

        var sender = await FindProfileByAccountIdAsync(accountId, ctx);

        if (sender is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        // no clue how 'c.Id == chatId && c.Users.Contains(sender)' translates to sql
        var chat = await _dbContext.Chats
            .Include(c => c.Users)
            .SingleOrDefaultAsync(c => c.Id == request.ChatId && c.Users.Contains(sender), ctx);

        if (chat is null)
        {
            return Result.NotFound($"Chat met id '{request.ChatId}' werd niet gevonden.");
        }

        if (request.Content is null && request.AudioDataBlob is null)
        {
            return Result.Invalid(new ValidationError(nameof(request), "Een bericht moet tekst of audio bevatten."));
        }

        string? audioUrl = null;

        if (request.AudioDataBlob is not null)
        {
             audioUrl = await _blobStorage.CreateBlobAsync(
                request.AudioDataBlob.Name,
                request.AudioDataBlob.Base64Data,
                Containers.VoiceMessages,
                ctx
            );
        }

        var message = new Message
        {
            Chat = chat,
            Sender = sender,
            Text = TextMessage.Create(request.Content!),
            AudioUrl = BlobUrl.Create(audioUrl!),
            AudioDurationSeconds = request.AudioDurationSeconds,
        };

        chat.AddMessage(message);

        await _dbContext.SaveChangesAsync(ctx);

        await MarkSenderMessageHistoryAsync(chat.Id, sender.Id, message.CreatedAt, message.Id, ctx);

        var dto = message.ToChatDto()!;

        // 1) Realtime via SignalR
        if (_messageDispatcher is not null)
        {
            try
            {
                await _messageDispatcher.NotifyMessageCreatedAsync(chat.Id, dto, ctx);
            }
            catch
            {
                // Realtime notificaties mogen een mislukte call niet blokkeren.
            }
        }

        // 2) Push notificaties naar alle andere gebruikers in deze chat
        if (_pushNotificationService is not null)
        {
            try
            {
                var recipients = chat.Users
                    .Where(u => u.Id != sender.Id)
                    .Select(u => u.AccountId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .ToList();

                if (recipients.Count > 0)
                {
                    var preview = dto.Content ?? string.Empty;
                    if (preview.Length > 80)
                    {
                        preview = preview[..80] + "...";
                    }

                    var senderDisplayName = sender.ToString(); // BaseUser.ToString() = "FirstName LastName"

                    foreach (var recipientAccountId in recipients)
                    {
                        await _pushNotificationService.SendMessageNotificationAsync(
                            recipientAccountId!,
                            senderDisplayName,
                            preview,
                            url: $"/chat/{chat.Id}",
                            ctx);
                    }
                }
            }
            catch
            {
                // Push mag de chatflow nooit breken; errors hier negeren/loggen.
            }
        }

        return Result.Success(dto);
    }

    private async Task MarkSenderMessageHistoryAsync(int chatId, int senderId, DateTime messageCreatedAt, int messageId, CancellationToken cancellationToken)
    {
        var history = await _dbContext.ChatMessageHistories
            .SingleOrDefaultAsync(
                history => history.ChatId == chatId && history.UserId == senderId
                , cancellationToken
            );

        if (history is null)
        {
            history = new ChatMessageHistory
            {
                ChatId = chatId,
                UserId = senderId
            };

            _dbContext.ChatMessageHistories.Add(history);
        }

        history.LastReadAt = messageCreatedAt;
        history.LastReadMessageId = messageId;

        await _dbContext.SaveChangesAsync(cancellationToken);
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

    public async Task<Result<ChatResponse.GetSupervisorChat>> GetSupervisorChatAsync(CancellationToken cancellationToken = default)
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

        var loggedInUser = await FindProfileByAccountIdAsync(accountId, cancellationToken);

        if (loggedInUser is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        // no clue how 'c.Id == chatId && c.Users.Contains(sender)' translates to sql
        var chat = await _dbContext
            .Chats
            .Where(c => c.ChatType == ChatType.Supervisor)
            .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
            .Include(c => c.Users)
            .SingleOrDefaultAsync(c => c.Users.Contains(loggedInUser), cancellationToken);

        if (chat is null)
        {
            return Result.NotFound($"Chat van '{loggedInUser}' met supervisor werd niet gevonden.");
        }

        return Result.Success(new ChatResponse.GetSupervisorChat()
        {
            Chat = chat.ToGetSupervisorChatDto(),
        });
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

    public async Task<Result<ChatResponse.GetMessages>> GetMessagesAsync(int chatId, QueryRequest.SkipTake request, CancellationToken cancellationToken = default)
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

        var loggedInUser = await FindProfileByAccountIdAsync(accountId, cancellationToken);

        if (loggedInUser is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        // no clue how 'c.Id == chatId && c.Users.Contains(sender)' translates to sql
        var chat = await _dbContext
            .Chats
            .Include(c => c.Messages
                .OrderByDescending(m => m.CreatedAt)
                .ThenByDescending(m => m.Id)
                .Skip(request.Skip)
                .Take(request.Take))
                .ThenInclude(m => m.Sender)
            .Include(c => c.Users)
            .SingleOrDefaultAsync(c => c.Id == chatId, cancellationToken);

        if (chat is null || !chat.Users.Contains(loggedInUser))
        {
            return Result.NotFound($"Chat niet gevonden.");
        }

        return Result.Success(new ChatResponse.GetMessages()
        {
            Messages = chat.Messages.Select(MessageMapper.ToChatDto),
            BatchCount = chat.Messages.Count
        });
    }
}
