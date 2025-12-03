using Rise.Shared.Users;

namespace Rise.Shared.Chats;

public static class ChatDto
{
    public class GetChats
    {
        public int ChatId { get; set; }
        public List<UserDto.Chat> Users { get; set; }
        public MessageDto.Chat? LastMessage { get; set; }
    }
    public class GetChat
    {
        public int ChatId { get; set; }
        public List<UserDto.Chat> Users { get; set; }
        public List<MessageDto.Chat> Messages { get; set; } = [];
    }
    public class GetSupervisorChat
    {
        public int ChatId { get; set; }
        public UserDto.Chat User { get; set; }
        public UserDto.Chat Supervisor { get; set; }
        public List<MessageDto.Chat> Messages { get; set; } = [];
    }
}