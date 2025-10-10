namespace Rise.Shared.Chats;

public class AlertStatusDto
{
    public int ChatId { get; set; }
    public bool IsActive { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
