using System.Threading;
using System.Threading.Tasks;

namespace Rise.Client.Offline;

public interface IConnectionService
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
