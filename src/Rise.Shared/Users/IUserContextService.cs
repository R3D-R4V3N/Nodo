namespace Rise.Shared.Users;
public interface IUserContextService
{
    Task<Result<UserResponse.CurrentUser>> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
