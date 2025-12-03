namespace Rise.Shared.Users;

public interface IUserService
{
    Task<Result<UserResponse.CurrentUser>> GetUserAsync(string accountId, CancellationToken cancellationToken = default);

    Task<Result<UserResponse.CurrentUser>> UpdateUserAsync(
        string userToChangeAccountId,
        UserRequest.UpdateCurrentUser request,
        CancellationToken cancellationToken = default);

    Task<Result<UserResponse.CurrentUser>> UpdatePersonalInfoAsync(
        string userToChangeAccountId,
        UserRequest.UpdatePersonalInfo request,
        CancellationToken cancellationToken = default);

    Task<Result<UserResponse.CurrentUser>> UpdateInterestsAsync(
        string userToChangeAccountId,
        UserRequest.UpdateInterests request,
        CancellationToken cancellationToken = default);

    Task<Result<UserResponse.CurrentUser>> UpdateDefaultChatLinesAsync(
        string userToChangeAccountId,
        UserRequest.UpdateDefaultChatLines request,
        CancellationToken cancellationToken = default);
}
