namespace Rise.Shared.Users;

public interface IUserService
{
    Task<Result<UserResponse.CurrentUser>> GetUserAsync(string accountId, CancellationToken cancellationToken = default);
    Task<Result<UserResponse.CurrentUser>> UpdateUserAsync(string userToChangeAccountId, UserRequest.UpdateCurrentUser request,
        CancellationToken cancellationToken = default);
    Task<Result<UserResponse.CurrentUser>> UpdateProfileInfoAsync(string userToChangeAccountId, UserRequest.UpdateProfileInfo request,
        CancellationToken cancellationToken = default);
    Task<Result<UserResponse.CurrentUser>> UpdatePreferencesAsync(string userToChangeAccountId, UserRequest.UpdatePreferences request,
        CancellationToken cancellationToken = default);
    Task<Result<UserResponse.CurrentUser>> UpdateChatLinesAsync(string userToChangeAccountId, UserRequest.UpdateChatLines request,
        CancellationToken cancellationToken = default);
}
