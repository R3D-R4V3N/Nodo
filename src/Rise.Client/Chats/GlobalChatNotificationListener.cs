using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Rise.Client.RealTime;
using Rise.Client.State;
using Rise.Shared.Chats;

namespace Rise.Client.Chats;

public class GlobalChatNotificationListener : IAsyncDisposable
{
    private readonly IHubClientFactory _hubClientFactory;
    private readonly IChatService _chatService;
    private readonly ChatNotificationService _notificationService;
    private readonly UserState _userState;

    private readonly SemaphoreSlim _syncLock = new(1, 1);
    private IHubClient? _hubClient;
    private IReadOnlyCollection<int> _joinedChatIds = Array.Empty<int>();
    private bool _started;

    public GlobalChatNotificationListener(
        IHubClientFactory hubClientFactory,
        IChatService chatService,
        ChatNotificationService notificationService,
        UserState userState)
    {
        _hubClientFactory = hubClientFactory;
        _chatService = chatService;
        _notificationService = notificationService;
        _userState = userState;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_userState.User is null)
        {
            return;
        }

        await _syncLock.WaitAsync(cancellationToken);
        try
        {
            if (_started)
            {
                return;
            }

            _started = true;
            await EnsureHubConnectionAsync(cancellationToken);
        }
        finally
        {
            _syncLock.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _syncLock.WaitAsync(cancellationToken);
        try
        {
            _started = false;
            if (_hubClient is null)
            {
                return;
            }

            await _hubClient.DisposeAsync();
            _hubClient = null;
            _joinedChatIds = Array.Empty<int>();
        }
        finally
        {
            _syncLock.Release();
        }
    }

    private async Task EnsureHubConnectionAsync(CancellationToken cancellationToken)
    {
        _hubClient ??= CreateAndConfigureClient();

        if (_hubClient.State == HubConnectionState.Disconnected)
        {
            await _hubClient.StartAsync();
        }

        if (_hubClient.State == HubConnectionState.Connected)
        {
            await JoinKnownChatsAsync(forceRefresh: true, cancellationToken);
        }
    }

    private IHubClient CreateAndConfigureClient()
    {
        var client = _hubClientFactory.Create();

        client.On<MessageDto.Chat>("MessageCreated", HandleIncomingMessage);
        client.Reconnected += _ => JoinKnownChatsAsync(forceRefresh: false);
        client.Closed += _ =>
        {
            _joinedChatIds = Array.Empty<int>();
            return Task.CompletedTask;
        };

        return client;
    }

    private void HandleIncomingMessage(MessageDto.Chat message)
    {
        // Fire and forget to avoid blocking SignalR pipeline.
        _ = _notificationService.NotifyMessageAsync(message);
    }

    private async Task JoinKnownChatsAsync(bool forceRefresh, CancellationToken cancellationToken = default)
    {
        if (_hubClient?.State != HubConnectionState.Connected)
        {
            return;
        }

        IReadOnlyCollection<int> chatIds = _joinedChatIds;
        if (forceRefresh || chatIds.Count == 0)
        {
            var chatsResult = await _chatService.GetAllAsync(cancellationToken);
            if (!chatsResult.IsSuccess || chatsResult.Value?.Chats is null)
            {
                return;
            }

            chatIds = chatsResult.Value.Chats.Select(chat => chat.ChatId).ToList();
            _joinedChatIds = chatIds;
        }

        foreach (var chatId in chatIds)
        {
            await _hubClient!.SendAsync("JoinChat", chatId);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _syncLock.Dispose();
    }
}
