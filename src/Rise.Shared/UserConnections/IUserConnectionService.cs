using Rise.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Shared.UserConnections;
public interface IUserConnectionService
{
    Task<Result<UserConnectionResponse.Index>> GetFriendIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default);
}
