using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Rise.Client.RealTime;
using Rise.Client.State;
using Rise.Shared.Chats;
using Rise.Shared.Users;

namespace Rise.Client.Helpdesk.Pages;

public partial class SupervisorChat : ComponentBase, IAsyncDisposable
{
    // --- INJECTIES ---
    [Inject] public IChatService ChatService { get; set; } = null!;
    [Inject] public UserState UserState { get; set; } = null!;
    [Inject] public IHubClientFactory HubClientFactory { get; set; } = null!;
    [Inject] public IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    [Inject] public IToastService ToastService { get; set; } = null!;

    // --- STATE ---
    private ChatDto.GetSupervisorChat? _chat;
    private IHubClient? _hubConnection;
    
    // UI State
    private string? _draft = string.Empty;
    private bool _isLoading = true;
    private bool _isSending;
    private bool _shouldScrollToBottom;

    // Element References
    private ElementReference _messagesHost;
    private ElementReference _footerHost;
    private double _footerHeight = 80;
    private bool _footerMeasurementPending = true;
    private const double _footerPaddingBuffer = 24;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        ScheduleFooterMeasurement();

        // 1. Haal de chat op
        var result = await ChatService.GetSupervisorChatAsync();

        if (result.IsSuccess && result.Value is not null)
        {
            _chat = result.Value.Chat;
            // 2. Start realtime verbinding (zodat je antwoorden ontvangt)
            await StartHubConnectionAsync();
        }
        else
        {
            ToastService.ShowError("Kon het gesprek niet laden.");
        }

        _isLoading = false;
        _shouldScrollToBottom = true;
    }

    // --- VERZENDEN ---

    private Task ApplySuggestion(string text)
    {
        _draft = text;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task HandleUserMessage(string text)
    {
        if (_chat is null || string.IsNullOrWhiteSpace(text) || _isSending) return;

        _isSending = true;

        var request = new ChatRequest.CreateMessage
        {
            ChatId = _chat.ChatId,
            Content = text
        };

        try
        {
            // Simpele API call
            var result = await ChatService.CreateMessageAsync(request);

            if (result.IsSuccess)
            {
                _draft = string.Empty; // Veld leegmaken bij succes
            }
            else
            {
                ToastService.ShowError(result.Errors.FirstOrDefault() ?? "Kon bericht niet versturen.");
            }
        }
        catch
        {
            ToastService.ShowError("Er is een fout opgetreden.");
        }
        finally
        {
            _isSending = false;
        }
    }

    // --- REALTIME (ONTVANGEN) ---

    private async Task StartHubConnectionAsync()
    {
        if (_chat is null) return;

        try
        {
            _hubConnection = HubClientFactory.Create();

            // Luister naar nieuwe berichten
            _hubConnection.On<MessageDto.Chat>("MessageCreated", dto => 
                InvokeAsync(() => ProcessIncomingMessage(dto)));

            await _hubConnection.StartAsync();
            await _hubConnection.SendAsync("JoinChat", _chat.ChatId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR error: {ex.Message}");
        }
    }

    private void ProcessIncomingMessage(MessageDto.Chat dto)
    {
        if (_chat is null || dto.ChatId != _chat.ChatId) return;

        _chat.Messages.Add(dto);
        
        ScheduleFooterMeasurement();
        _shouldScrollToBottom = true;
        StateHasChanged();
    }

    // --- UI HELPERS ---

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Hoogte meten van de input bar
        if (_footerMeasurementPending)
        {
            _footerMeasurementPending = false;
            try
            {
                var h = await JSRuntime.InvokeAsync<double>("measureElementHeight", _footerHost);
                if (h > 0 && Math.Abs(h - _footerHeight) > 1)
                {
                    _footerHeight = h;
                    _shouldScrollToBottom = true;
                    StateHasChanged();
                }
            }
            catch { /* Ignore */ }
        }

        // Auto-scroll naar beneden
        if (_shouldScrollToBottom)
        {
            _shouldScrollToBottom = false;
            try
            {
                await JSRuntime.InvokeVoidAsync("scrollToBottom", _messagesHost, true);
            }
            catch { /* Ignore */ }
        }
    }

    private void ScheduleFooterMeasurement() => _footerMeasurementPending = true;

    private string GetMessageHostPaddingStyle() => $"padding-bottom: {Math.Max(0, Math.Ceiling(_footerHeight + _footerPaddingBuffer))}px;";
    
    private string GetChatTitle() => "Begeleider";
    private string GetChatImage() => "https://www.shutterstock.com/image-vector/avatar-gender-neutral-silhouette-vector-600nw-2470054311.jpg";
    private void NavigateBack() => NavigationManager.NavigateTo("/homepage");

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}