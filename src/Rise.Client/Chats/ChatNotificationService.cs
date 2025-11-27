using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Rise.Client.State;
using Rise.Shared.Chats;

namespace Rise.Client.Chats;

public class ChatNotificationService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly UserState _userState;
    private int? _activeChatId;

    public ChatNotificationService(IJSRuntime jsRuntime, UserState userState)
    {
        _jsRuntime = jsRuntime;
        _userState = userState;
    }

    public bool IsChatActive(int chatId)
    {
        return _activeChatId == chatId;
    }

    public void SetActiveChat(int? chatId)
    {
        _activeChatId = chatId;
    }

    public async Task RequestPermissionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync(
                "notifications.requestPermission",
                cancellationToken);
        }
        catch (JSException)
        {
            // If the browser blocks permission prompts, continue without interrupting the login/register flow.
        }
    }

    public async Task NotifyMessageAsync(
        MessageDto.Chat message,
        CancellationToken cancellationToken = default)
    {
        if (ShouldSkipNotification(message))
        {
            return;
        }

        var notificationPayload = new
        {
            chatId = message.ChatId,
            senderName = message.User.Name,
            contentPreview = BuildContentPreview(message),
            data = new { url = $"/chat/{message.ChatId}" }
        };

        try
        {
            await _jsRuntime.InvokeAsync<string>(
                "notifications.showMessageNotification",
                cancellationToken,
                notificationPayload);
        }
        catch (JSException)
        {
            // Best effort: if notifications are blocked or JS is unavailable we silently ignore.
        }
    }

    private bool ShouldSkipNotification(MessageDto.Chat message)
    {
        var currentUserId = _userState.User?.Id;
        if (currentUserId.HasValue && message.User.Id == currentUserId)
        {
            return true;
        }

        if (_activeChatId.HasValue && message.ChatId == _activeChatId)
        {
            return true;
        }
        return false;
    }

    private static string BuildContentPreview(MessageDto.Chat message)
    {
        if (!string.IsNullOrWhiteSpace(message.Content))
        {
            return message.Content;
        }

        if (!string.IsNullOrWhiteSpace(message.AudioDataBlob))
        {
            return "Stuurde een spraakbericht.";
        }

        return "Je hebt een nieuw bericht.";
    }
}
