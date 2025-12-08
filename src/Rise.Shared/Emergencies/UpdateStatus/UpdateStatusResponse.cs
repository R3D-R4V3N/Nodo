namespace Rise.Shared.Emergencies;

public static partial class EmergencyResponse
{
    public class Resolve
    {
        public required EmergencyDto.GetEmergencies Emergency { get; set; }
    }
}
