using Microsoft.AspNetCore.Components.Authorization;
using Rise.Shared.Users;
using System.Net.Http.Json;

namespace Rise.Client.Users;

public class UserContextService(
    AuthenticationStateProvider authProvider,
    HttpClient httpClient
) : IUserContextService
{
    private readonly AuthenticationStateProvider _authProvider = authProvider;
    private readonly HttpClient _http = httpClient;
    private UserDto.CurrentUser? _currentUser;

    public async Task<Result<UserResponse.CurrentUser>> GetCurrentUserAsync(CancellationToken ctx = default)
        => await _http.GetFromJsonAsync<Result<UserResponse.CurrentUser>>($"/api/users/current", cancellationToken: ctx)!;

    public async Task<Result<UserResponse.CurrentUser>> UpdateProfileAsync(UserRequest.UpdateProfile request, CancellationToken ctx = default)
    {
        var response = await _http.PutAsJsonAsync("/api/users/current", request, cancellationToken: ctx);
        var result = await response.Content.ReadFromJsonAsync<Result<UserResponse.CurrentUser>>(cancellationToken: ctx);

        if (result is { IsSuccess: true, Value.User: not null })
        {
            _currentUser = result.Value.User;
        }

        return result ?? Result.Error("Kon het profiel niet bijwerken.");
    }

    public async Task<UserDto.CurrentUser?> InitializeAsync(CancellationToken ctx = default)
    {
        if (_currentUser is not null)
            return _currentUser;

        var authState = await _authProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user?.Identity?.IsAuthenticated == false)
        {
            return null;
        }

        var result = await GetCurrentUserAsync(ctx);

        if (result is { IsSuccess: true, Value.User: not null })
        {
            _currentUser = result.Value.User;
            return _currentUser;
        }

        throw new InvalidOperationException("Kon gebruiker niet verkrijgen");
    }
}
