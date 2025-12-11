using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Rise.Client.State;
using Rise.Shared.Chats;

namespace Rise.Client.Chats.Components;
public partial class MessageList
{
    
    [Parameter] public IReadOnlyList<MessageDto.Chat> Messages { get; set; } = [];
    [Parameter] public string? TimestampText { get; set; } = "Nov 30, 2023, 9:41 AM";
    [Parameter] public EventCallback<MessageDto.Chat> OnCancelPending { get; set; }
    
    [Parameter] public bool IsEmbedded { get; set; }
    [Inject] public UserState UserState { get; set; }

    private ElementReference _scrollHost;

    private static readonly TimeSpan _messageGroupingWindow = TimeSpan.FromMinutes(2);

    private static bool ShouldGroupWithPrevious(MessageDto.Chat current, MessageDto.Chat? previous)
    {
        if (previous is null)
        {
            return false;
        }

        if (previous.User.Id != current.User.Id)
        {
            return false;
        }

        if (previous.Timestamp is null || current.Timestamp is null)
        {
            return false;
        }

        var difference = current.Timestamp.Value - previous.Timestamp.Value;
        if (difference < TimeSpan.Zero)
        {
            difference = difference.Negate();
        }

        return difference <= _messageGroupingWindow;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // scrol elke render automatisch naar onder
        await JS.InvokeVoidAsync("scrollToBottom", _scrollHost);
    }

    private Task HandleCancel(MessageDto.Chat message)
    {
        return OnCancelPending.HasDelegate
            ? OnCancelPending.InvokeAsync(message)
            : Task.CompletedTask;
    }
}