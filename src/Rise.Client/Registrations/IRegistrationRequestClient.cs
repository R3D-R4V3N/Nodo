using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Rise.Shared.Registrations;

namespace Rise.Client.Registrations;

public interface IRegistrationRequestClient
{
    Task<Result<List<RegistrationRequestResponse.PendingItem>>> GetPendingAsync(CancellationToken ct = default);
    Task<Result> ApproveAsync(int requestId, RegistrationRequestRequest.Approve request, CancellationToken ct = default);
}
