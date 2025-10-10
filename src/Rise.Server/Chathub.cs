using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Rise.Shared.Chats;

namespace Rise.Server.Hubs;

[Authorize]
public class Chathub : Hub
{
    private static readonly ConcurrentDictionary<int, AlertStatusDto> AlertStatuses = new();

    public static string GetGroupName(int chatId) => $"chat-{chatId}";

    public Task JoinChat(int chatId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(chatId));
    }

    public Task LeaveChat(int chatId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(chatId));
    }

    public Task<AlertStatusDto> GetAlertStatus(int chatId)
    {
        if (AlertStatuses.TryGetValue(chatId, out var status))
        {
            return Task.FromResult(status);
        }

        return Task.FromResult(new AlertStatusDto
        {
            ChatId = chatId,
            IsActive = false,
            UpdatedAt = DateTimeOffset.UtcNow
        });
    }

    public async Task SetAlertState(int chatId, bool isActive)
    {
        var updatedBy = Context.User?.Identity?.Name ?? "Onbekende gebruiker";

        var status = new AlertStatusDto
        {
            ChatId = chatId,
            IsActive = isActive,
            UpdatedBy = updatedBy,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        if (isActive)
        {
            AlertStatuses[chatId] = status;
        }
        else
        {
            AlertStatuses.TryRemove(chatId, out _);
        }

        await Clients.Group(GetGroupName(chatId)).SendAsync("AlertStateChanged", status);
    }

    public Task SendMessage(string user, string message)
    {
        return Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public Task SendVoiceMessage(string user, string dataUrl, double durationSeconds)
    {
        return Clients.All.SendAsync("ReceiveVoiceMessage", user, dataUrl, durationSeconds);
    }
}
