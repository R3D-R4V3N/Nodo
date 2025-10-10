namespace Rise.Shared.Chats;

public class ChatEmergencyStatusDto
{
    public int chatId { get; set; }
    public bool isActive { get; set; }
    public DateTime? activatedAtUtc { get; set; }
    public int? initiatorUserId { get; set; }
    public string? initiatorName { get; set; }
    public string? initiatorAccountId { get; set; }
    public List<ChatSupervisorAccessDto> supervisors { get; set; } = [];
}
