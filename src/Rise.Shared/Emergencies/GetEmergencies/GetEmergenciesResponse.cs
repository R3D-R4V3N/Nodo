namespace Rise.Shared.Emergencies;

public static partial class EmergencyResponse
{
    public class GetEmergencies
    {
        public IEnumerable<EmergencyDto.GetEmergencies> Emergencies { get; set; } = [];
        public int TotalCount { get; set; }
    }
}