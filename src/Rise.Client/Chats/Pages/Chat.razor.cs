using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Rise.Client.Chats.Components;
using Rise.Shared.Assets;
using Rise.Shared.Chats;
using Rise.Shared.Users;
using System.Globalization;

namespace Rise.Client.Chats.Pages;
public partial class Chat
{
    [Parameter] public int ChatId { get; set; }
    [CascadingParameter] public UserDto.CurrentUser? CurrentUser { get; set; }

    private ChatDto.GetChats? _chat;
    private readonly List<MessageDto.Chat> _messages = new();
    private readonly HashSet<int> _messageIds = new();
    private readonly SemaphoreSlim _hubConnectionLock = new(1, 1);
    private HubConnection? _hubConnection;
    private int? _joinedChatId;
    private string? _draft = string.Empty;
    private string? _errorMessage;
    private string? _connectionError;
    private string? _loadError;
    private bool _isLoading = true;
    private bool _isSending;
    private bool _shouldScrollToBottom;
    private ElementReference _messagesHost;
    private ElementReference _footerHost;
    private double _footerHeight = 200;
    private bool _footerMeasurementPending = true;
    private const double FooterPaddingBuffer = 24;

    protected override async Task OnParametersSetAsync()
    {
        _loadError = null;
        _errorMessage = null;
        ScheduleFooterMeasurement();
        _isLoading = true;

        var result = await ChatService.GetByIdAsync(ChatId);

        if (result.IsSuccess && result.Value is not null)
        {
            _chat = result.Value;
            _messages.Clear();
            _messages.AddRange(_chat.Messages
                .OrderBy(m => m.Timestamp));

            _messageIds.Clear();
            foreach (var message in _chat.Messages)
            {
                _messageIds.Add(message.Id);
            }
        }
        else
        {
            _chat = null;
            _messages.Clear();
            _messageIds.Clear();
            _loadError = result.Errors.FirstOrDefault() ?? "Het gesprek kon niet geladen worden.";
        }

        _isLoading = false;
        _shouldScrollToBottom = true;
        ScheduleFooterMeasurement();

        await EnsureHubConnectionAsync();
    }

    private async Task SendMessageAsync(string text)
    {
        if (_chat is null || _isSending || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        _isSending = true;
        _errorMessage = null;
        ScheduleFooterMeasurement();

        try
        {
            text = Filter.Censor(text);
            var request = new ChatRequest.CreateMessage
            {
                ChatId = _chat.ChatId,
                Content = text
            };

            var result = await ChatService.CreateMessageAsync(request);

            if (!result.IsSuccess || result.Value is null)
            {
                _errorMessage = result.Errors.FirstOrDefault() ?? "Het bericht kon niet verzonden worden.";
                ScheduleFooterMeasurement();
                return;
            }

            TryAddMessage(result.Value);
        }
        finally
        {
            _isSending = false;
            StateHasChanged();
        }
    }

    private Task ApplySuggestion(string text)
    {
        _draft = text;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task HandleVoiceMessageAsync(RecordedAudio audio)
    {
        if (_chat is null || _isSending)
        {
            return;
        }

        _isSending = true;
        _errorMessage = null;
        ScheduleFooterMeasurement();

        try
        {
            var request = new ChatRequest.CreateMessage
            {
                ChatId = _chat.ChatId,
                AudioDataUrl = audio.DataUrl,
                AudioDurationSeconds = audio.DurationSeconds
            };

            var result = await ChatService.CreateMessageAsync(request);

            if (!result.IsSuccess || result.Value is null)
            {
                var validationMessage = result.ValidationErrors.FirstOrDefault()?.ErrorMessage;
                _errorMessage = validationMessage
                    ?? result.Errors.FirstOrDefault()
                    ?? "Het spraakbericht kon niet verzonden worden.";
                ScheduleFooterMeasurement();
                return;
            }

            TryAddMessage(result.Value);
        }
        finally
        {
            _isSending = false;
            StateHasChanged();
        }
    }

    private async Task EnsureHubConnectionAsync()
    {
        await _hubConnectionLock.WaitAsync();
        try
        {
            if (_hubConnection is null)
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(NavigationManager.ToAbsoluteUri("/chathub"))
                    .WithAutomaticReconnect()
                    .Build();

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
        if (!string.IsNullOrWhiteSpace(dto.Content))
        {
            dto.Content = Filter.Censor(dto.Content);
        }
        if (TryAddMessage(dto))
        {
            StateHasChanged();
        }
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
            if (_joinedChatId is int previousId)
            {
                await _hubConnection.SendAsync("LeaveChat", previousId);
                _joinedChatId = null;
            }

            return;
        }

        if (_joinedChatId == _chat.ChatId)
        {
            return;
        }

        if (_joinedChatId is int oldChatId)
        {
            await _hubConnection.SendAsync("LeaveChat", oldChatId);
        }

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

        if (!_messageIds.Add(dto.Id))
        {
            return false;
        }

        _chat.Messages.Add(dto);
        _chat.Messages = _chat.Messages
            .OrderBy(m => m.Timestamp)
            .ToList();

        _messages.Add(dto);
        _messages.Sort((a, b) => Nullable.Compare(a.Timestamp, b.Timestamp));
        _shouldScrollToBottom = true;

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
            .Where(x => x.Id != CurrentUser!.Id)
            .Select(x => x.Name)
            .ToList() ?? ["Unknown"];

        return string.Join(", ", participantNames);
    }

    private string GetAvatarUrl()
    {
        return _chat?
            .Users
            .FirstOrDefault(x => x.Id != CurrentUser!.Id)
            ?.AvatarUrl ?? DefaultImages.Profile;
    }

    private string GetStatusText()
    {
        var lastMessage = _messages.LastOrDefault();
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
        // TODO: Hook up with actual alert functionality.
        return Task.CompletedTask;
    }

    private string GetMessageHostPaddingStyle()
    {
        var padding = Math.Max(0, Math.Ceiling(_footerHeight + FooterPaddingBuffer));
        return $"padding-bottom: {padding}px;";
    }

    private void ScheduleFooterMeasurement()
    {
        _footerMeasurementPending = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }

        _hubConnectionLock.Dispose();
    }
}