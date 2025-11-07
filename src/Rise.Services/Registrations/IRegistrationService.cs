using Rise.Shared.Organizations;
using Rise.Shared.Registrations;

namespace Rise.Services.Registrations;

public interface IRegistrationService
{
    Task<Result<OrganizationResponse.List>> GetOrganizationsAsync(CancellationToken cancellationToken = default);
    Task<Result<RegistrationResponse.PendingList>> GetPendingRegistrationsAsync(CancellationToken cancellationToken = default);
    Task<Result<RegistrationResponse.Updated>> AssignSupervisorAsync(RegistrationRequest.AssignSupervisor request, CancellationToken cancellationToken = default);
    Task<Result> ApproveRegistrationAsync(RegistrationRequest.Approve request, CancellationToken cancellationToken = default);
}
