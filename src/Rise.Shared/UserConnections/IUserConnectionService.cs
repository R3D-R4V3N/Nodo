using Rise.Shared.Common;

namespace Rise.Shared.UserConnections;
public interface IUserConnectionService
{
    Task<Result<UserConnectionResponse.GetFriends>> GetFriendIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default);
    Task<Result<string>> AddFriendAsync(string targetAccountId, CancellationToken ctx = default);
    Task<Result<string>> AcceptFriendAsync(string requesterAccountId, CancellationToken ctx = default);
    Task<Result<UserConnectionResponse.GetSuggestions>> GetSuggestedFriendsAsync(QueryRequest.SkipTake req, CancellationToken ct = default);
}
