using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Rise.Client.Chats.Components;
using Rise.Client.Offline;
using Rise.Client.RealTime;
using Rise.Client.State;
using Rise.Shared.Assets;
using Rise.Shared.Chats;
using Rise.Shared.Emergencies;
using Rise.Shared.Users;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace Rise.Client.Chats.Pages;
public partial class Chat : IAsyncDisposable
{
    [Parameter] public int ChatId { get; set; }
    [Inject] public UserState UserState { get; set; }
    [Inject] public OfflineQueueService OfflineQueueService { get; set; } = null!;
    // Onnodig complex, zie Talk over Factory

    private ChatDto.GetChat? _chat;
    private readonly SemaphoreSlim _hubConnectionLock = new(1, 1);
    [Inject] private IHubClientFactory HubClientFactory { get; set; } = null!;
    private IHubClient? _hubConnection;
    private int? _joinedChatId;
    private string? _draft = string.Empty;
    private string? _errorMessage;
    private string? _connectionError;
    private string? _loadError;
    private bool _isLoading = true;
    private bool _isSending;
    private readonly List<AlertPrompt.AlertReason> _alertReasons = new();
    private bool _isAlertOpen;
    private EmergencyTypeDto? _selectedAlertReason;
    private bool _shouldScrollToBottom;
    private ElementReference _messagesHost;
    private ElementReference _footerHost;
    private double _footerHeight = 200;
    private bool _footerMeasurementPending = true;
    private const double _footerPaddingBuffer = 24;
    [Inject] private ChatMessageDispatchService MessageDispatchService { get; set; } = null!;

    protected override void OnInitialized()
    {
        OfflineQueueService.WentOnline += HandleWentOnlineAsync;
        InitializeAlertReasons();
    }

    protected override async Task OnParametersSetAsync()
    {
        _loadError = null;
        _errorMessage = null;
        _isLoading = true;
        ScheduleFooterMeasurement();

        var result = await ChatService.GetByIdAsync(ChatId);

        if (result.IsSuccess && result.Value is not null)
        {
            _chat = result.Value.Chat;
        }
        else
        {
            _chat = null;
            _loadError = result.Errors.FirstOrDefault() 
                ?? "Het gesprek kon niet geladen worden.";
        }

        _isLoading = false;
        _shouldScrollToBottom = true;

        var isOnline = await OfflineQueueService.IsOnlineAsync();
        if (isOnline)
        {
            await EnsureHubConnectionAsync();
        }
    }

    private Task ApplySuggestion(string text)
    {
        _draft = text;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task HandleTextMessageAsync(string text)
    {
        if (_chat is null || string.IsNullOrWhiteSpace(text) || _isSending)
        {
            return;
        }

        var request = new ChatRequest.CreateMessage
        {
            ChatId = _chat.ChatId,
            Content = text
        };

        await DispatchMessageAsync(request, "Het bericht kon niet verzonden worden.");
    }

    private async Task HandleVoiceMessageAsync(RecordedAudio audio)
    {
        if (_chat is null || _isSending)
        {
            return;
        }

        var request = new ChatRequest.CreateMessage
        {
            ChatId = _chat.ChatId,
            AudioDataBlob = audio.DataUrl,
            AudioDurationSeconds = audio.DurationSeconds
        };

        await DispatchMessageAsync(request, "Het spraakbericht kon niet verzonden worden.");
    }

    private Task HandleWentOnlineAsync()
    {
        return InvokeAsync(async () =>
        {
            _connectionError = null;
            await EnsureHubConnectionAsync();
        });
    }

    private void InitializeAlertReasons()
    {
        _alertReasons.Clear();
        _alertReasons.AddRange(AlertCatalog.Reasons);
    }

    private async Task DispatchMessageAsync(ChatRequest.CreateMessage createRequest, string errorMessage)
    {
        try
        {
            _isSending = true;
            _errorMessage = null;

            var result = await MessageDispatchService.DispatchAsync(_chat!, createRequest);

            if (result.PendingMessage is not null)
            {
                AddPendingMessage(result.PendingMessage);
                return;
            }

            if (result.IsSuccess)
            {
                return;
            }

            var validationMessage = result.ServerResult?
                .ValidationErrors
                .FirstOrDefault()
                ?.ErrorMessage;

            _errorMessage = result.Error
                ?? validationMessage
                ?? result.ServerResult?.Errors.FirstOrDefault()
                ?? errorMessage;
        }
        catch
        {
            _errorMessage ??= errorMessage;
        }
        finally
        {
            _isSending = false;
        }
    }

    private void AddPendingMessage(MessageDto.Chat pendingMessage)
    {
        if (_chat is null)
        {
            return;
        }

        _chat.Messages.Add(pendingMessage);
        ScheduleFooterMeasurement();
        _shouldScrollToBottom = true;
        StateHasChanged();
    }

    private async Task CancelPendingMessageAsync(MessageDto.Chat message)
    {
        if (_chat is null || !message.IsPending)
        {
            return;
        }

        if (message.QueuedOperationId is int queuedId)
        {
            try
            {
                await OfflineQueueService.RemoveOperationAsync(queuedId);
            }
            catch
            {
                // Best-effort removal; if it fails the message will retry on reconnect.
            }
        }

        _chat.Messages.Remove(message);
        ScheduleFooterMeasurement();
        StateHasChanged();
    }

    private async Task EnsureHubConnectionAsync()
    {
        await _hubConnectionLock.WaitAsync();
        try
        {
            if (_hubConnection is null)
            {
                _hubConnection = HubClientFactory.Create();

                _hubConnection.On<MessageDto.Chat>("MessageCreated", dto =>
                    InvokeAsync(() => ProcessIncomingMessage(dto)));

                _hubConnection.Reconnecting += error => InvokeAsync(() =>
                {
                    _connectionError = "Realtime verbinding wordt hersteld…";
                    StateHasChanged();
                });

                _hubConnection.Reconnected += _ => InvokeAsync(async () =>
                {
                    _connectionError = null;
                    await JoinCurrentChatAfterReconnectAsync();
                    StateHasChanged();
                });

                _hubConnection.Closed += error => InvokeAsync(() =>
                {
                    _joinedChatId = null;
                    _connectionError = "Realtime verbinding werd verbroken. Vernieuw de pagina om opnieuw te verbinden.";
                    StateHasChanged();
                });
            }

            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
            }

            if (_hubConnection.State != HubConnectionState.Connected)
            {
                return;
            }

            await JoinCurrentChatCoreAsync();
        }
        catch (Exception ex)
        {
            _connectionError = $"Realtime verbinding mislukt: {ex.Message}";
        }
        finally
        {
            _hubConnectionLock.Release();
        }
    }

    private void ProcessIncomingMessage(MessageDto.Chat dto)
    {
        if (_chat is null || dto.ChatId != _chat.ChatId)
        {
            return;
        }

        RemovePendingPlaceholder(dto);
        if (TryAddMessage(dto))
        {
            ScheduleFooterMeasurement();
            _shouldScrollToBottom = true;
            StateHasChanged();
        }
    }

    private void RemovePendingPlaceholder(MessageDto.Chat dto)
    {
        if (_chat is null)
        {
            return;
        }

        var pendingMessage = _chat.Messages.FirstOrDefault(message =>
            message.IsPending
            && message.ChatId == dto.ChatId
            && message.User.Id == dto.User.Id
            && (MatchesQueuedOperation(dto, message) || PendingContentMatches(dto, message)));

        if (pendingMessage is not null)
        {
            _chat.Messages.Remove(pendingMessage);
        }
    }

    private static bool MatchesQueuedOperation(MessageDto.Chat incoming, MessageDto.Chat pending)
    {
        return pending.QueuedOperationId.HasValue && incoming.QueuedOperationId == pending.QueuedOperationId;
    }

    private static bool PendingContentMatches(MessageDto.Chat incoming, MessageDto.Chat pending)
    {
        if (!string.IsNullOrWhiteSpace(incoming.AudioDataBlob) || !string.IsNullOrWhiteSpace(pending.AudioDataBlob))
        {
            return string.Equals(incoming.AudioDataBlob, pending.AudioDataBlob, StringComparison.Ordinal);
        }

        return string.Equals(incoming.Content, pending.Content, StringComparison.Ordinal);
    }

    private async Task JoinCurrentChatAfterReconnectAsync()
    {
        await _hubConnectionLock.WaitAsync();
        try
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await JoinCurrentChatCoreAsync();
            }
        }
        catch (Exception ex)
        {
            _connectionError = $"Realtime verbinding mislukt: {ex.Message}";
        }
        finally
        {
            _hubConnectionLock.Release();
        }
    }

    private async Task JoinCurrentChatCoreAsync()
    {
        if (_hubConnection is null)
        {
            return;
        }

        if (_chat is null)
        {
            await LeaveCurrentChatAsync();
            return;
        }

        if (_joinedChatId == _chat.ChatId)
        {
            return;
        }

        await LeaveCurrentChatAsync();

        await _hubConnection.SendAsync("JoinChat", _chat.ChatId);
        _joinedChatId = _chat.ChatId;
        _connectionError = null;
    }

    private bool TryAddMessage(MessageDto.Chat dto)
    {
        if (_chat is null)
        {
            return false;
        }

        _chat.Messages.Add(dto);

        // is sort needed?
        // if yes: better to use a sorted datastructure than to sort on every new message

        //_chat.Messages = _chat.Messages
        //    .OrderBy(m => m.Timestamp)
        //    .ToList();

        //_messages.Sort((a, b) => Nullable.Compare(a.Timestamp, b.Timestamp));

        return true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (_footerMeasurementPending)
        {
            _footerMeasurementPending = false;

            try
            {
                var measuredHeight = await JSRuntime.InvokeAsync<double>("measureElementHeight", _footerHost);
                if (!double.IsNaN(measuredHeight)
                    && !double.IsInfinity(measuredHeight)
                    && measuredHeight > 0
                    && Math.Abs(measuredHeight - _footerHeight) > 1)
                {
                    _footerHeight = measuredHeight;
                    _shouldScrollToBottom = true;
                    StateHasChanged();
                    return;
                }
            }
            catch (JSDisconnectedException)
            {
                // Ignore when JS runtime is no longer available.
            }
        }

        if (_shouldScrollToBottom)
        {
            _shouldScrollToBottom = false;

            try
            {
                await JSRuntime.InvokeVoidAsync("scrollToBottom", _messagesHost, true);
            }
            catch (JSDisconnectedException)
            {
                // Ignore: JS runtime no longer available (e.g., during prerender or disposal).
            }
        }
    }

    private string GetChatTitle()
    {
        var participantNames = _chat?
            .Users
            .Where(x => x.Id != UserState.User!.Id)
            .Select(x => x.Name)
            .ToList() ?? ["Unknown"];

        return string.Join(", ", participantNames);
    }

    private string GetChatImage()
    {
        return _chat?
            .Users
            .FirstOrDefault(x => x.Id != UserState.User!.Id)
            ?.AvatarUrl ?? DefaultImages.Profile;
    }

    private string GetChatStatusText()
    {
        var lastMessage = _chat?.Messages.LastOrDefault();
        if (lastMessage?.Timestamp is DateTime timestamp)
        {
            return $"Laatste bericht {timestamp.ToLocalTime():HH:mm}";
        }

        return "Online";
    }

    private string GetConversationDateLabel()
    {
        if (_chat is null)
        {
            return string.Empty;
        }

        var first = _chat.Messages
            .OrderBy(m => m.Timestamp)
            .FirstOrDefault();

        return first?.Timestamp?.ToString("d MMM yyyy", CultureInfo.GetCultureInfo("nl-BE")) ?? string.Empty;
    }

    private Task NavigateBack()
    {
        NavigationManager.NavigateTo("/homepage");
        return Task.CompletedTask;
    }

    private Task TriggerAlert()
    {
        _isAlertOpen = true;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task CloseAlert()
    {
        _isAlertOpen = false;
        return Task.CompletedTask;
    }

    private Task HandleAlertReason(EmergencyTypeDto reason)
    {
        _selectedAlertReason = reason;
        _isAlertOpen = false;
        return Task.CompletedTask;
    }

    private string GetMessageHostPaddingStyle()
    {
        var padding = Math.Max(0, Math.Ceiling(_footerHeight + _footerPaddingBuffer));
        return $"padding-bottom: {padding}px;";
    }

    private void ScheduleFooterMeasurement()
    {
        _footerMeasurementPending = true;
    }

    public async ValueTask DisposeAsync()
    {
        OfflineQueueService.WentOnline -= HandleWentOnlineAsync;
        await LeaveCurrentChatAsync();
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }

        _hubConnectionLock.Dispose();
    }

    private async Task LeaveCurrentChatAsync()
    {
        if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
            return;

        if (_joinedChatId is int oldChatId)
        {
            try
            {
                await _hubConnection.SendAsync("LeaveChat", oldChatId);
            }
            finally
            {
                _joinedChatId = null;
            }
        }
    }
    private void HandleOtherUserProfileNavigation()
    {
        if (_chat is null || _chat.Users is null || UserState.User is null)
            return;

        var otherUser = _chat.Users
            .FirstOrDefault(u => !string.Equals(u.AccountId, UserState.User.AccountId, StringComparison.OrdinalIgnoreCase));

        if (otherUser is null)
            return;

        NavigationManager.NavigateTo($"/FriendProfilePage/{otherUser.AccountId}");
    }
}