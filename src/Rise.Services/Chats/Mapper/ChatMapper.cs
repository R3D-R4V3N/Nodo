using Rise.Domain.Chats;
using Rise.Services.Users.Mapper;
using Rise.Shared.Chats;

namespace Rise.Services.Chats.Mapper;
internal static class ChatMapper
{
    public static ChatDto.GetChats ToIndexDto(this Chat chat) =>
        new ChatDto.GetChats
        {
            ChatId = chat.Id,
            Users = chat.Users.Select(UserMapper.ToChatDto).ToList(),
            Messages = chat.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(MessageMapper.ToChatDto)
                .ToList()
        };
}
