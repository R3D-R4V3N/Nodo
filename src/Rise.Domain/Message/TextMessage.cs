namespace Rise.Domain.Message;

public class TextMessage : Message
{
    public required string Text { get; set; }
}
