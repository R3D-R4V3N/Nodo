namespace Rise.Shared.Friends;

public interface IFriendService
{
    Task<Result> AddFriendAsync(FriendRequest.Add request, CancellationToken ctx = default);
    Task<Result> RemoveFriendAsync(FriendRequest.Remove request, CancellationToken ctx = default);
}
