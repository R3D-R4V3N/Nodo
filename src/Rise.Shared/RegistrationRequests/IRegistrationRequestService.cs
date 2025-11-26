using Ardalis.Result;

namespace Rise.Shared.RegistrationRequests;

public interface IRegistrationRequestService
{
    Task<Result> CreateAsync(Identity.Accounts.AccountRequest.Register request, CancellationToken ct = default);
    Task<Result<RegistrationRequestResponse.PendingList>> GetPendingAsync(CancellationToken ct = default);
    Task<Result<RegistrationRequestResponse.Approve>> ApproveAsync(int requestId, RegistrationRequestRequest.Approve request, CancellationToken ct = default);
}
