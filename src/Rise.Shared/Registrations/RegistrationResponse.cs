using System.Collections.Generic;

namespace Rise.Shared.Registrations;

public static class RegistrationResponse
{
    public class PendingList
    {
        public List<RegistrationDto.Pending> Registrations { get; init; } = [];
    }

    public class Updated
    {
        public RegistrationDto.Pending Registration { get; init; } = new();
    }
}
