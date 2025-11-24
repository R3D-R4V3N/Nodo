using Rise.Shared.Users;

namespace Rise.Shared.Chats;

public static class MessageDto
{
    public record Chat
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime? Timestamp { get; set; } = DateTime.UtcNow;
        public required UserDto.Message User { get; set; }
        public string? AudioDataUrl { get; set; }
        public TimeSpan? AudioDuration { get; set; }
        public bool IsPending { get; set; }
        public int? QueuedOperationId { get; set; }
        public Guid? ClientMessageId { get; set; }
    }
}
