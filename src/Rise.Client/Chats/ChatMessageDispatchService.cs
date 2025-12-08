using System;
using System.Linq;
using Ardalis.Result;
using Rise.Client.Offline;
using Rise.Client.State;
using Rise.Shared.Chats;
using Rise.Shared.Users;

namespace Rise.Client.Chats;

public class ChatMessageDispatchService
{
    private readonly IChatService _chatService;
    private readonly OfflineQueueService _offlineQueueService;
    private readonly UserState _userState;

    public ChatMessageDispatchService(IChatService chatService, OfflineQueueService offlineQueueService, UserState userState)
    {
        _chatService = chatService;
        _offlineQueueService = offlineQueueService;
        _userState = userState;
    }

    public async Task<ChatMessageDispatchResult> DispatchAsync(ChatDto.GetChat chat, ChatRequest.CreateMessage request, CancellationToken cancellationToken = default)
    {
        var isOnline = await _offlineQueueService.IsOnlineAsync();
        if (!isOnline)
        {
            return await QueuePendingMessageAsync(chat, request, cancellationToken);
        }

        var result = await _chatService.CreateMessageAsync(request, cancellationToken);
        if (result.IsSuccess)
        {
            return new ChatMessageDispatchResult { ServerResult = result };
        }

        if (IndicatesQueued(result))
        {
            return new ChatMessageDispatchResult { ServerResult = result };
        }

        var error = result.ValidationErrors.FirstOrDefault()?.ErrorMessage
            ?? result.Errors.FirstOrDefault();

        return new ChatMessageDispatchResult
        {
            ServerResult = result,
            Error = error
        };
    }

    private async Task<ChatMessageDispatchResult> QueuePendingMessageAsync(ChatDto.GetChat chat, ChatRequest.CreateMessage request, CancellationToken cancellationToken)
    {
        var queuedResult = await _chatService.QueueMessageAsync(request, cancellationToken);
        if (queuedResult.IsSuccess)
        {
            return new ChatMessageDispatchResult
            {
                PendingMessage = BuildPendingMessage(chat, request, queuedResult.Value)
            };
        }

        var error = queuedResult.Errors.FirstOrDefault()
            ?? queuedResult.ValidationErrors.FirstOrDefault()?.ErrorMessage
            ?? "Het bericht kon niet offline opgeslagen worden.";

        return new ChatMessageDispatchResult
        {
            Error = error,
            ServerResult = Result<MessageDto.Chat>.Error(error)
        };
    }

    private MessageDto.Chat BuildPendingMessage(ChatDto.GetChat chat, ChatRequest.CreateMessage request, int queuedOperationId)
    {
        if (_userState.User is null)
        {
            throw new InvalidOperationException("Een gebruiker is vereist om een tijdelijk bericht op te bouwen.");
        }

        //var pendingId = -1;

        return new MessageDto.Chat
        {
            //Id = pendingId,
            ChatId = chat.ChatId,
            Content = request.Content ?? string.Empty,
            Timestamp = DateTime.UtcNow,
            User = new UserDto.Message
            {
                Id = _userState.User.Id,
                Name = $"{_userState.User.FirstName} {_userState.User.LastName}",
                AccountId = _userState.User.AccountId,
                AvatarUrl = _userState.User.AvatarUrl
            },
            AudioDataBlob = request.AudioDataBlob,
            AudioDuration = request.AudioDurationSeconds.HasValue
                ? TimeSpan.FromSeconds(request.AudioDurationSeconds.Value)
                : null,
            IsPending = true,
            QueuedOperationId = queuedOperationId
        };
    }

    private static bool IndicatesQueued(Result<MessageDto.Chat> result)
    {
        return result.Errors.Any(error =>
            error.Contains("opgeslagen", StringComparison.OrdinalIgnoreCase)
            && error.Contains("verbinding", StringComparison.OrdinalIgnoreCase));
    }
}
