using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Rise.Client.Chats;
using Rise.Client.Chats.Components;
using Rise.Client.Offline;
using Rise.Client.RealTime;
using Rise.Client.State;
using Rise.Shared.Assets;
using Rise.Shared.BlobStorage;
using Rise.Shared.Chats;
using Rise.Shared.Emergencies;
using System.Collections.Generic;
using System.Globalization;

namespace Rise.Client.Emergencies.Pages;
public partial class Emergency
{
    [Parameter] public int EmergencyId { get; set; }
    [Inject] public UserState UserState { get; set; }

    private EmergencyDto.GetEmergency? _emergency;
    private List<MessageDto.Chat> _messages = [];
    private bool _loadFailed;
    private bool _isLoading = true;
    private bool _isSending;
    private readonly List<AlertPrompt.AlertReason> _alertReasons = new();
    private bool _isAlertOpen;
    private bool _shouldScrollToBottom;
    private ElementReference _messagesHost;
    private ElementReference _footerHost;
    private double _footerHeight = 200;
    private bool _footerMeasurementPending = true;
    private const double _footerPaddingBuffer = 24;
    [Inject] private ChatMessageDispatchService MessageDispatchService { get; set; } = null!;
    [Inject] public IEmergencyService EmergencyService { get; set; } = null!;
    [Inject] public IToastService ToastService { get; set; } = null!;
    [Inject] public IJSRuntime JSRuntime { get; set; } = null!;

    protected override void OnInitialized()
    {
        InitializeAlertReasons();
    }

    protected override async Task OnParametersSetAsync()
    {
        _loadFailed = false;
        _isLoading = true;
        var emergencyResult = await EmergencyService.GetEmergencyAsync(EmergencyId);
        if (emergencyResult.IsSuccess && emergencyResult.Value is not null)
        {
            _emergency = emergencyResult.Value.Emergency;
            _messages = _emergency.Chat.Messages;
        }
        else
        {
            _emergency = null;
            var loadError = emergencyResult.Errors.FirstOrDefault()
                ?? "Noodmelding kon niet geladen worden.";
            ToastService.ShowError(loadError);
            _loadFailed = true;
        }

        _isLoading = false;
        ScheduleFooterMeasurement();
        _shouldScrollToBottom = true;
        StateHasChanged();
    }
    private bool _shouldIgnoreFirstIntersection = true;

    private void InitializeAlertReasons()
    {
        _alertReasons.Clear();
        _alertReasons.AddRange(AlertCatalog.Reasons);
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
        var participantNames = _emergency?
            .Chat
            .Users
            .Where(x => x.Id != UserState.User!.Id)
            .Select(x => x.Name)
            .ToList() ?? ["Unknown"];

        return string.Join(", ", participantNames);
    }

    private string GetChatImage()
    {
        return _emergency?
            .Chat
            .Users
            .FirstOrDefault(x => x.AccountId == _emergency.ReporterAccountId)
            ?.AvatarUrl
            ?? DefaultImages.Profile;
    }

    private string GetChatStatusText()
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
        if (_emergency is null)
        {
            return string.Empty;
        }

        var first = _messages
            .OrderBy(m => m.Timestamp)
            .FirstOrDefault();

        return first?.Timestamp?.ToString("d MMM yyyy", CultureInfo.GetCultureInfo("nl-BE")) ?? string.Empty;
    }

    private async Task NavigateBack()
    {
        await JSRuntime.InvokeVoidAsync("history.back");
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
}