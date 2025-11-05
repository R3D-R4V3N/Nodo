using Rise.Shared.Common;

namespace Rise.Shared.UserConnections;
public interface IUserConnectionService
{
    Task<Result<UserConnectionResponse.GetFriends>> GetFriendIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default);
    Task<Result<UserConnectionResponse.GetSuggestions>> GetSuggestedFriendsAsync(QueryRequest.SkipTake req, CancellationToken ctx = default);
    Task<Result<UserConnectionResponse.SendFriendRequest>> SendFriendRequestAsync(string targetAccountId, CancellationToken ctx = default);
    Task<Result<UserConnectionResponse.AcceptFriendRequest>> AcceptFriendRequestAsync(string targetAccountId, CancellationToken ctx = default);
    Task<Result<UserConnectionResponse.RejectFriendRequest>> RejectFriendRequestAsync(string targetAccountId, CancellationToken ctx = default);
}
