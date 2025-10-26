namespace Rise.Shared.Users;
public interface IUserContextService
{
    Task<Result<UserResponse.CurrentUser>> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    Task<Result<UserResponse.CurrentUser>> UpdateProfileAsync(UserRequest.UpdateProfile request, CancellationToken cancellationToken = default);
}
