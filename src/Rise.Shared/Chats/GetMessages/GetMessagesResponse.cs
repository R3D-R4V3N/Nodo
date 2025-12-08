namespace Rise.Shared.Chats;

public static partial class ChatResponse
{
    public class GetMessages
    {
        public IEnumerable<MessageDto.Chat> Messages { get; set; }
        public int BatchCount { get; set; }
    }
}