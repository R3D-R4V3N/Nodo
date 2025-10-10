namespace Rise.Shared.Chats;

public static class ChatDto
{
    public class Index
    {
        public int chatId { get; set; }
        public List<MessageDto> messages { get; set; } = [];
        public bool isSupervisorAlertActive { get; set; }

    }
}