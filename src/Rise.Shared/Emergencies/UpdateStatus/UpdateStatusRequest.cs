namespace Rise.Shared.Emergencies;

public static partial class EmergencyRequest
{
    public class UpdateStatus
    {
        public required int EmergencyId { get; set; }
        public required EmergencyStatusDto Status { get; set; }
    }
}
