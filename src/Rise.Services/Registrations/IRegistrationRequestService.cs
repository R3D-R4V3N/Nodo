using Ardalis.Result;
using Rise.Shared.Registrations;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rise.Services.Registrations;

public interface IRegistrationRequestService
{
    Task<Result<IReadOnlyCollection<RegistrationRequestDto.ListItem>>> GetPendingRequestsForSupervisorAsync(
        CancellationToken cancellationToken = default);
}
