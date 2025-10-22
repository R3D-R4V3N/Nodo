using Rise.Shared.Common;

namespace Rise.Shared.UserConnections;
public interface IUserConnectionService
{
   Task<Result<UserConnectionResponse.GetFriends>> GetFriendIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default);
    Task<Result<string>> AddFriendAsync(string targetAccountId, CancellationToken ctx = default);
    
}
