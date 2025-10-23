namespace Rise.Shared.Users;
public interface IUserService
{
    Task<Result<UserResponse.CurrentUser>> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
