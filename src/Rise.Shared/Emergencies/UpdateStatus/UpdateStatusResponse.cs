namespace Rise.Shared.Emergencies;

public static partial class EmergencyResponse
{
    public class UpdateStatus
    {
        public required EmergencyDto.Get Emergency { get; set; }
    }
}
