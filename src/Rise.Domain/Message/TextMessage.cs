namespace Rise.Domain.Message;

public class TextMessage : ChatMessage
{
    public string Text { get; set; } = string.Empty;
}
