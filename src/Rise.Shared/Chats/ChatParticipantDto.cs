namespace Rise.Shared.Chats;

public class ChatParticipantDto
{
    public int userId { get; set; }
    public string displayName { get; set; } = string.Empty;
    public string accountId { get; set; } = string.Empty;
    public string userType { get; set; } = string.Empty;
}
