namespace Rise.Shared.Chats;

public class MessageDto
{
    public int ChatId { get; set; }
    public int Id { get; set; }            // Unieke id voor de message
    public string Content { get; set; } = string.Empty;   // De tekst van de message
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderAccountId { get; set; } = string.Empty;
    public string? AudioDataUrl { get; set; }
    public double? AudioDurationSeconds { get; set; }
}
