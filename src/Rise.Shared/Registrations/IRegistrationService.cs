namespace Rise.Shared.Registrations;

public interface IRegistrationService
{
    Task<Result<RegistrationResponse.PendingList>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<Result<RegistrationResponse.Detail>> GetDetailAsync(int registrationId, CancellationToken cancellationToken = default);
    Task<Result> ApproveAsync(RegistrationRequest.Approve request, CancellationToken cancellationToken = default);
    Task<Result> RejectAsync(RegistrationRequest.Reject request, CancellationToken cancellationToken = default);
}
