namespace Rise.Shared.Emergencies;

public static partial class EmergencyRequest
{
    public class Resolve
    {
        public required int EmergencyId { get; set; }
    }
}
