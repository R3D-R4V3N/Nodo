namespace Rise.Domain.Message;

public class VoiceMessage : Message
{
    public required object Blob { get; set; }
    public required string Encoding { get; set; }
    public int Length { get; set; }
}
