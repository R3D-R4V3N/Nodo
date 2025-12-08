using Rise.Domain.Chats;
using Rise.Domain.Users;
using Rise.Services.Users.Mapper;
using Rise.Shared.Chats;

namespace Rise.Services.Chats.Mapper;
internal static class ChatMapper
{
    public static ChatDto.Emergency ToEmergencyDto(this Chat chat) =>
        new ChatDto.Emergency
        {
            ChatId = chat.Id,
            Users = chat.Users.Select(UserMapper.ToChatDto).ToList(),
            Messages = chat.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(MessageMapper.ToEmergencyDto)
                .ToList()!
        };

    public static ChatDto.GetChat ToGetChatDto(this Chat chat) =>
        new ChatDto.GetChat
        {
            ChatId = chat.Id,
            Users = chat.Users.Select(UserMapper.ToChatDto).ToList(),
        };

    public static ChatDto.GetSupervisorChat ToGetSupervisorChatDto(this Chat chat)
    {
        var user = chat.Users.OfType<User>().FirstOrDefault();
        var supervisor = chat.Users.OfType<Supervisor>().FirstOrDefault();

        if (user is null || supervisor is null)
            throw new ArgumentNullException();

        return new ChatDto.GetSupervisorChat
        {
            ChatId = chat.Id,
            User = user.ToChatDto(),
            Supervisor = supervisor.ToChatDto(),
            Messages = chat.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(MessageMapper.ToChatDto)
                .ToList()!
        };
    }

    public static ChatDto.GetChats? ToGetChatsDto(this Chat chat)
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
        };
    }
}
