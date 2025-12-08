using System;
using System.Collections.Generic;
using Rise.Shared.Chats;

namespace Rise.Client.State;

public class ChatState
{
    private readonly Dictionary<int, ChatStateItem> _states = new();
    //public IReadOnlyDictionary<int, ChatStateItem> States => _states;

    public int? ActiveChatId { get; private set; }

    public event Action? OnChange;

    public void InitializeCounts(IEnumerable<ChatDto.GetChats> chats)
    {
        var changed = false;

        foreach (var chat in chats)
        {
            if (_states.TryGetValue(chat.ChatId, out var existing) && existing.UnReadCount == chat.UnreadCount)
            {
                existing.HasNextPage = true;
                continue;
            }

            _states[chat.ChatId] = new ChatStateItem()
            {
                UnReadCount = Math.Max(0, chat.UnreadCount)
            };

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
    public bool HasNextPage(int chatId)
    {
        return _states.TryGetValue(chatId, out var item)
            ? item.HasNextPage : false;
    }
    public void FetchedAllMessages(int chatId)
    {
        _states.TryAdd(chatId, new ChatStateItem());
        _states[chatId].HasNextPage = false;
    }

    public void IncrementUnread(int chatId)
    {
        if (ActiveChatId == chatId)
        {
            ResetUnread(chatId);
            return;
        }

        _states.TryAdd(chatId, new ChatStateItem());
        _states[chatId].UnReadCount++;
        NotifyStateChanged();
    }

    public void ResetUnread(int chatId)
    {
        if (_states.TryGetValue(chatId, out var current) && current.UnReadCount == 0)
        {
            NotifyStateChanged();
            return;
        }

        _states.TryAdd(chatId, new ChatStateItem());
        _states[chatId].UnReadCount = 0;
        NotifyStateChanged();
    }

    public int GetUnreadCount(int chatId)
    {
        return _states.TryGetValue(chatId, out var count)
            ? count.UnReadCount
            : 0;
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    private class ChatStateItem
    {
        public int UnReadCount { get; set; }
        public bool HasNextPage { get; set; } = true;
    }
}
