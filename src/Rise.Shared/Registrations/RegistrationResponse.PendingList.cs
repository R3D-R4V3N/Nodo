namespace Rise.Shared.Registrations;

public static partial class RegistrationResponse
{
    public class PendingList
    {
        public IEnumerable<RegistrationDto.PendingItem> Registrations { get; set; } = Array.Empty<RegistrationDto.PendingItem>();
    }
}
