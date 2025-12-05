using System;
using System.Collections.Generic;
using Rise.Shared.Chats;

namespace Rise.Client.State;

public class ChatState
{
    private readonly Dictionary<int, int> _unreadCounts = new();

    public IReadOnlyDictionary<int, int> UnreadCounts => _unreadCounts;

    public int? ActiveChatId { get; private set; }

    public event Action? OnChange;

    public void InitializeCounts(IEnumerable<ChatDto.GetChats> chats)
    {
        var changed = false;

        foreach (var chat in chats)
        {
            if (_unreadCounts.TryGetValue(chat.ChatId, out var existing) && existing == chat.UnreadCount)
            {
                continue;
            }

            _unreadCounts[chat.ChatId] = Math.Max(0, chat.UnreadCount);
            changed = true;
        }

        if (changed)
        {
            NotifyStateChanged();
        }
    }

    public void SetActiveChat(int? chatId)
    {
        ActiveChatId = chatId;

        if (chatId.HasValue)
        {
            ResetUnread(chatId.Value);
            return;
        }

        NotifyStateChanged();
    }

    public void IncrementUnread(int chatId)
    {
        if (ActiveChatId == chatId)
        {
            ResetUnread(chatId);
            return;
        }

        if (!_unreadCounts.ContainsKey(chatId))
        {
            _unreadCounts[chatId] = 0;
        }

        _unreadCounts[chatId]++;
        NotifyStateChanged();
    }

    public void ResetUnread(int chatId)
    {
        if (_unreadCounts.TryGetValue(chatId, out var current) && current == 0)
        {
            NotifyStateChanged();
            return;
        }

        _unreadCounts[chatId] = 0;
        NotifyStateChanged();
    }

    public int GetUnreadCount(int chatId)
    {
        return _unreadCounts.TryGetValue(chatId, out var count)
            ? count
            : 0;
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
