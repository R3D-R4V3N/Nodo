namespace Rise.Shared.Emergencies;

public static partial class EmergencyRequest
{
    public class CreateEmergency
    {
        public required int ChatId { get; set; }
        public required int MessageId { get; set; }
        public required EmergencyTypeDto Type { get; set; }
    }
}
