using Rise.Shared.Common;

namespace Rise.Shared.UserConnections;
public interface IUserConnectionService
{
    Task<Result<UserConnectionResponse.GetFriends>> GetFriendsAsync(QueryRequest.SkipTake request, CancellationToken ctx = default);
    Task<Result<UserConnectionResponse.GetFriendRequests>> GetFriendRequestsAsync(QueryRequest.SkipTake request, CancellationToken ctx = default);
    Task<Result<UserConnectionResponse.GetSuggestions>> GetSuggestedFriendsAsync(QueryRequest.SkipTake request, CancellationToken ctx = default);
    Task<Result<UserConnectionResponse.SendFriendRequest>> SendFriendRequestAsync(string targetAccountId, CancellationToken ctx = default);
    Task<Result<UserConnectionResponse.AcceptFriendRequest>> AcceptFriendRequestAsync(string targetAccountId, CancellationToken ctx = default);
    Task<Result<UserConnectionResponse.RejectFriendRequest>> RejectFriendRequestAsync(string targetAccountId, CancellationToken ctx = default);
    Task<Result<UserConnectionResponse.CancelFriendRequest>> CancelFriendRequest(string targetAccountId, CancellationToken ctx = default);
    Task<Result<UserConnectionResponse.RemoveFriendRequest>> RemoveFriendAsync(string targetAccountId, CancellationToken ctx = default);
}
