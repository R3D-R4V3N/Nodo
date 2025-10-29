namespace Rise.Shared.Users;

public interface IUserService
{
    Task<Result<UserResponse.CurrentUser>> GetUserAsync(string accountId, CancellationToken cancellationToken = default);
}
