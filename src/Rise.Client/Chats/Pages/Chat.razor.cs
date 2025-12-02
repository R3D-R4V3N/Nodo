using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Rise.Client.Chats.Components;
using Rise.Client.Offline;
using Rise.Client.RealTime;
using Rise.Client.State;
using Rise.Shared.Assets;
using Rise.Shared.Chats;
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
    private string? _selectedAlertReason;
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
        _alertReasons.Add(new AlertPrompt.AlertReason("Ongepast gedrag", MapPinIcon));
        _alertReasons.Add(new AlertPrompt.AlertReason("Spam of fraude", SnowflakeIcon));
        _alertReasons.Add(new AlertPrompt.AlertReason("Ander probleem", BandageIcon));
    }

    private static RenderFragment MapPinIcon => builder =>
    {
        builder.OpenElement(0, "svg");
        builder.AddMultipleAttributes(1, new Dictionary<string, object?>
        {
            ["xmlns"] = "http://www.w3.org/2000/svg",
            ["viewBox"] = "0 0 24 24",
            ["fill"] = "currentColor",
            ["aria-hidden"] = "true",
            ["class"] = "h-6 w-6"
        });
        builder.OpenElement(2, "path");
        builder.AddAttribute(3, "d", "M12 21.75a.75.75 0 0 1-.624-.334C9.273 18.45 6.75 14.047 6.75 10.5A5.25 5.25 0 0 1 12 5.25a5.25 5.25 0 0 1 5.25 5.25c0 3.547-2.523 7.95-4.626 10.916a.75.75 0 0 1-.624.334ZM12 12.75a2.25 2.25 0 1 0 0-4.5 2.25 2.25 0 0 0 0 4.5Z");
        builder.CloseElement();
        builder.CloseElement();
    };

    private static RenderFragment SnowflakeIcon => builder =>
    {
        builder.OpenElement(0, "svg");
        builder.AddMultipleAttributes(1, new Dictionary<string, object?>
        {
            ["xmlns"] = "http://www.w3.org/2000/svg",
            ["viewBox"] = "0 0 24 24",
            ["fill"] = "currentColor",
            ["aria-hidden"] = "true",
            ["class"] = "h-6 w-6"
        });
        builder.OpenElement(2, "path");
        builder.AddAttribute(3, "d", "M11.25 2.25a.75.75 0 0 1 1.5 0V5l1.436-1.436a.75.75 0 1 1 1.061 1.061L12.75 6.122v1.43l2.059-1.187 1.15-3.083a.75.75 0 1 1 1.414.527l-.64 1.72 1.72-.64a.75.75 0 1 1 .527 1.414l-3.083 1.15-2.147 1.238 2.147 1.237 3.083 1.15a.75.75 0 1 1-.527 1.414l-1.72-.64.64 1.72a.75.75 0 0 1-1.414.527l-1.15-3.083-2.059-1.187v1.43l2.497 1.497a.75.75 0 1 1-.776 1.286L12.75 14.5v2.378l1.997 2.29a.75.75 0 0 1-1.14.976L12 17.94l-1.607 1.935a.75.75 0 0 1-1.14-.976l1.997-2.29V14.5l-1.721 1.035a.75.75 0 1 1-.776-1.286l2.497-1.497v-1.43l-2.059 1.187-1.15 3.083a.75.75 0 1 1-1.414-.527l.64-1.72-1.72.64a.75.75 0 0 1-.527-1.414l3.083-1.15 2.147-1.237-2.147-1.238-3.083-1.15a.75.75 0 1 1 .527-1.414l1.72.64-.64-1.72a.75.75 0 0 1 1.414-.527l1.15 3.083 2.059 1.187v-1.43l-2.247-1.32a.75.75 0 1 1 .776-1.286l1.471.864Z");
        builder.CloseElement();
        builder.CloseElement();
    };

    private static RenderFragment BandageIcon => builder =>
    {
        builder.OpenElement(0, "svg");
        builder.AddMultipleAttributes(1, new Dictionary<string, object?>
        {
            ["xmlns"] = "http://www.w3.org/2000/svg",
            ["viewBox"] = "0 0 24 24",
            ["fill"] = "currentColor",
            ["aria-hidden"] = "true",
            ["class"] = "h-6 w-6"
        });
        builder.OpenElement(2, "path");
        builder.AddAttribute(3, "d", "M8.28 3.22a3.75 3.75 0 0 1 5.303 0l3.197 3.197a3.75 3.75 0 0 1 0 5.303l-4.8 4.8a3.75 3.75 0 0 1-5.303 0l-3.197-3.197a3.75 3.75 0 0 1 0-5.303l4.8-4.8Zm1.133 1.133-4.8 4.8a2.25 2.25 0 0 0 0 3.182l3.197 3.197a2.25 2.25 0 0 0 3.182 0l4.8-4.8a2.25 2.25 0 0 0 0-3.182l-3.197-3.197a2.25 2.25 0 0 0-3.182 0Zm1.607 1.31a.75.75 0 0 1 1.06 0l3.9 3.9a.75.75 0 0 1-1.06 1.06l-3.9-3.9a.75.75 0 0 1 0-1.06Zm-1.43 4.37a.75.75 0 1 1 1.06-1.06.75.75 0 0 1-1.06 1.06Zm2.5-2.5a.75.75 0 1 1 1.06-1.06.75.75 0 0 1-1.06 1.06Zm-2.5 2.5a.75.75 0 1 1 1.06-1.06.75.75 0 0 1-1.06 1.06Zm-2.5 2.5a.75.75 0 0 1 1.06-1.06.75.75 0 0 1-1.06 1.06Z");
        builder.CloseElement();
        builder.CloseElement();
    };

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

    private Task HandleAlertReason(string reason)
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