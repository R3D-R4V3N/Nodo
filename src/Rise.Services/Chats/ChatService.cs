using Microsoft.EntityFrameworkCore;
using Rise.Domain.Chats;
using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Chats;
using Rise.Services.Chats.Mapper;
using Rise.Shared.Identity;

namespace Rise.Services.Chats;

public class ChatService(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider,
    IChatMessageDispatcher? messageDispatcher = null) : IChatService
{

    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ISessionContextProvider _sessionContextProvider = sessionContextProvider;
    private readonly IChatMessageDispatcher? _messageDispatcher = messageDispatcher;

    public async Task<Result<ChatResponse.Index>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var chatsFromDb = await _dbContext.Chats
            .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
            .ToListAsync(cancellationToken);

        var chatDtos = chatsFromDb.Select(c => new ChatDto.Index
        {
            chatId = c.Id,
            messages = c.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(MessageMapper.MapToDto)
                .ToList()
        }).ToList();

        return Result.Success(new ChatResponse.Index
        {
            Chats = chatDtos
        });
    }

    public async Task<Result<ChatDto.Index>> GetByIdAsync(int chatId, CancellationToken cancellationToken = default)
    {
        var chat = await _dbContext.Chats
            .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
            .SingleOrDefaultAsync(c => c.Id == chatId, cancellationToken);

        if (chat is null)
        {
            return Result.NotFound($"Chat met id '{chatId}' werd niet gevonden.");
        }

        var dto = new ChatDto.Index
        {
            chatId = chat.Id,
            messages = chat.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(MessageMapper.MapToDto)
                .ToList()
        };

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
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        var chat = await _dbContext.Chats
            .SingleOrDefaultAsync(c => c.Id == request.ChatId, cancellationToken);

        if (chat is null)
        {
            return Result.NotFound($"Chat met id '{request.ChatId}' werd niet gevonden.");
        }

        var trimmedContent = string.IsNullOrWhiteSpace(request.Content)
            ? null
            : request.Content.Trim();

        byte[]? audioBytes = null;
        string? audioContentType = null;
        double? audioDurationSeconds = null;

        var audioDataUrl = string.IsNullOrWhiteSpace(request.AudioDataUrl)
            ? null
            : request.AudioDataUrl.Trim();

        if (!string.IsNullOrWhiteSpace(audioDataUrl))
        {
            if (!AudioHelperMethods.TryParseAudioDataUrl(audioDataUrl, out audioContentType, out audioBytes, out var parseError))
            {
                return Result.Invalid(new ValidationError(nameof(request.AudioDataUrl), parseError ?? "Ongeldige audio data-URL."));
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

        if (trimmedContent is null && audioBytes is null)
        {
            return Result.Invalid(new ValidationError(nameof(request.Content), "Een bericht moet tekst of audio bevatten."));
        }

        var message = new Message
        {
            ChatId = chat.Id,
            SenderId = sender.Id,
            Inhoud = trimmedContent,
            AudioContentType = audioContentType,
            AudioData = audioBytes,
            AudioDurationSeconds = audioDurationSeconds
        };

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync(cancellationToken);

        message.Sender = sender;
        var dto = message.MapToDto();

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
}
