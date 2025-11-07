using Ardalis.Result;

namespace Rise.Shared.RegistrationRequests;

public interface IRegistrationRequestService
{
    Task<Result<RegistrationRequestResponse.PendingList>> GetPendingAsync(CancellationToken ct = default);
    Task<Result<RegistrationRequestResponse.Approve>> ApproveAsync(int requestId, RegistrationRequestRequest.Approve request, CancellationToken ct = default);
}
