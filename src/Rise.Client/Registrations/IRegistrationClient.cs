using Rise.Shared.Organizations;
using Rise.Shared.Registrations;

namespace Rise.Client.Registrations;

public interface IRegistrationClient
{
    Task<Result<OrganizationResponse.List>> GetOrganizationsAsync(CancellationToken cancellationToken = default);
    Task<Result<RegistrationResponse.PendingList>> GetPendingRegistrationsAsync(CancellationToken cancellationToken = default);
    Task<Result<RegistrationResponse.Updated>> AssignSupervisorAsync(int registrationId, CancellationToken cancellationToken = default);
    Task<Result> ApproveRegistrationAsync(int registrationId, CancellationToken cancellationToken = default);
}
