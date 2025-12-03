namespace Rise.Shared.Emergencies;

public static partial class EmergencyResponse
{
    public class GetEmergencies
    {
        public IEnumerable<EmergencyDto.Get> Emergencies { get; set; } = [];
        public int TotalCount { get; set; }
    }
}