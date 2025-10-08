namespace Rise.Domain.Message;

public class VoiceMessage : ChatMessage
{
    // no clue how it actually gets stored
    public required object Blob { get; set; }
    public required string Encoding { get; set; }
    public int Length { get; set; }
}
