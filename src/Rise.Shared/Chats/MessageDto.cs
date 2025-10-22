using Rise.Shared.Users;

namespace Rise.Shared.Chats;

public static class MessageDto
{
<<<<<<< HEAD
    public int ChatId { get; set; }
    public int Id { get; set; }            // Unieke id voor de message
    public string Content { get; set; } = string.Empty;   // De tekst van de message
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderAccountId { get; set; } = string.Empty;
    public string? AudioDataUrl { get; set; }
    public double? AudioDurationSeconds { get; set; }
=======
    public record Chat
    {
        public int Id { get; set; } 
        public int ChatId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime? Timestamp { get; set; } = DateTime.UtcNow;
        public required UserDto.Message User { get; set; }
        public string? AudioDataUrl { get; set; }
        public TimeSpan? AudioDuration { get; set; }
    }
>>>>>>> codex/add-alert-message-for-supervisor-monitoring
}
