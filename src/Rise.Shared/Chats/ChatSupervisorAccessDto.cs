namespace Rise.Shared.Chats;

public class ChatSupervisorAccessDto
{
    public int chatUserId { get; set; }
    public int supervisorUserId { get; set; }
    public string supervisorName { get; set; } = string.Empty;
    public string supervisorAccountId { get; set; } = string.Empty;
}
