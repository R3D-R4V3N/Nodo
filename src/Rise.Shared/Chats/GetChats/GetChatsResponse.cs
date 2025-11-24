namespace Rise.Shared.Chats;

public static partial class ChatResponse
{
    /// <summary>
    /// Response for listing all chats.
    /// </summary>

    public class GetChats
    {
        public IEnumerable<ChatDto.GetChats> Chats { get; set; } = Array.Empty<ChatDto.GetChats>();
    }
}