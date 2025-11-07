using Rise.Domain.Users.Registrations;
using Rise.Shared.Registrations;

namespace Rise.Services.Registrations;

internal static class RegistrationStatusExtensions
{
    public static RegistrationStatusDto ToDto(this RegistrationStatus status) => status switch
    {
        RegistrationStatus.Pending => RegistrationStatusDto.Pending,
        RegistrationStatus.Approved => RegistrationStatusDto.Approved,
        RegistrationStatus.Rejected => RegistrationStatusDto.Rejected,
        _ => RegistrationStatusDto.Pending,
    };
}
