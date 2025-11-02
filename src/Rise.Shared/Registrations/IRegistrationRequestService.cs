using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;

namespace Rise.Shared.Registrations;

public interface IRegistrationRequestService
{
    Task<Result<List<RegistrationRequestResponse.PendingItem>>> GetPendingAsync(CancellationToken ct = default);
    Task<Result> ApproveAsync(int requestId, RegistrationRequestRequest.Approve request, CancellationToken ct = default);
}
