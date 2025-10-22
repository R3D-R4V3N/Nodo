namespace Rise.Shared.Users;
public interface IUserService
{
    Task<Result<UserResponse.CurrentUser>> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    Task<Result<UserResponse.CurrentUser>> UpdateCurrentUserAsync(
        UserRequest.UpdateCurrentUser request,
        CancellationToken cancellationToken = default);
}
