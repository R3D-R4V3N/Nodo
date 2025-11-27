using Rise.Domain.Chats;
using Rise.Services.Users.Mapper;
using Rise.Shared.Chats;

namespace Rise.Services.Chats.Mapper;
internal static class ChatMapper
{
    public static ChatDto.GetChat ToGetChatDto(this Chat chat) =>
        new ChatDto.GetChat
        {
            ChatId = chat.Id,
            Users = chat.Users.Select(UserMapper.ToChatDto).ToList(),
            Messages = chat.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(MessageMapper.ToChatDto)
                .ToList()!
        };

    public static ChatDto.GetChats? ToGetChatsDto(this Chat chat, int unreadCount = 0)
    {
        if (chat is null)
            return null;

        return new ChatDto.GetChats
        {
            ChatId = chat.Id,
            Users = chat
                .Users
                .Select(UserMapper.ToChatDto)
                .ToList(),
            LastMessage = MessageMapper.ToChatDto(chat.Messages.FirstOrDefault()),
            UnreadCount = unreadCount
        };
    }
}
