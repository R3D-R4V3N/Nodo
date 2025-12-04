namespace Rise.Shared.Emergencies;

public static partial class EmergencyResponse
{
    public class GetEmergency
    {
        public EmergencyDto.Get Emergency { get; set; }
    }
}