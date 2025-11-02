using System.Collections.Generic;
using Rise.Shared.Registrations;

namespace Rise.Client.Registrations;

public interface IRegistrationService
{
    Task<Result<List<RegistrationResponse.ListItem>>> GetRequestsAsync(CancellationToken ct = default);

    Task<Result> ApproveAsync(int requestId, int supervisorId, CancellationToken ct = default);

    Task<Result> RejectAsync(int requestId, int supervisorId, CancellationToken ct = default);
}
