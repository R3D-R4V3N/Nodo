using System;
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
                .Select(MapToDto)
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
                .Select(MapToDto)
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
            if (!TryParseAudioDataUrl(audioDataUrl, out audioContentType, out audioBytes, out var parseError))
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
        var dto = MapToDto(message, sender);

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

    private static MessageDto MapToDto(Message message)
    {
        var sender = message.Sender ?? throw new InvalidOperationException("Message sender must be loaded.");
        return MapToDto(message, sender);
    }

    private static MessageDto MapToDto(Message message, ApplicationUser sender)
    {
        return new MessageDto
        {
            ChatId = message.ChatId,
            Id = message.Id,
            Content = message.Inhoud ?? string.Empty,
            Timestamp = message.CreatedAt,
            SenderId = message.SenderId,
            SenderName = $"{sender.FirstName} {sender.LastName}",
            SenderAccountId = sender.AccountId,
            AudioDataUrl = BuildAudioDataUrl(message),
            AudioDurationSeconds = message.AudioDurationSeconds
        };
    }

    private static string? BuildAudioDataUrl(Message message)
    {
        if (message.AudioData is not { Length: > 0 } || string.IsNullOrWhiteSpace(message.AudioContentType))
        {
            return null;
        }

        var base64 = Convert.ToBase64String(message.AudioData);
        return $"data:{message.AudioContentType};base64,{base64}";
    }

    private static bool TryParseAudioDataUrl(
        string audioDataUrl,
        out string? contentType,
        out byte[]? data,
        out string? errorMessage)
    {
        contentType = null;
        data = null;
        errorMessage = null;

        if (!audioDataUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            errorMessage = "Audio data-URL moet starten met 'data:'.";
            return false;
        }

        var commaIndex = audioDataUrl.IndexOf(',');
        if (commaIndex <= 0 || commaIndex >= audioDataUrl.Length - 1)
        {
            errorMessage = "Audio data-URL mist inhoud.";
            return false;
        }

        var metadata = audioDataUrl.Substring("data:".Length, commaIndex - "data:".Length);
        if (string.IsNullOrWhiteSpace(metadata))
        {
            errorMessage = "Audio contenttype ontbreekt.";
            return false;
        }

        var base64MarkerIndex = metadata.IndexOf(";base64", StringComparison.OrdinalIgnoreCase);
        if (base64MarkerIndex < 0)
        {
            errorMessage = "Audio data-URL moet base64-gecodeerd zijn.";
            return false;
        }

        var contentTypeSegment = metadata[..base64MarkerIndex];
        var contentTypeParts = contentTypeSegment
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        contentType = contentTypeParts.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(contentType))
        {
            errorMessage = "Audio contenttype ontbreekt.";
            return false;
        }

        var base64 = audioDataUrl[(commaIndex + 1)..].Trim();
        if (string.IsNullOrWhiteSpace(base64))
        {
            errorMessage = "Audio bevat geen data.";
            return false;
        }

        try
        {
            data = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            errorMessage = "Audio kon niet gedecodeerd worden.";
            return false;
        }

        if (data.Length == 0)
        {
            errorMessage = "Audio bevat geen data.";
            return false;
        }

        return true;
    }
}
