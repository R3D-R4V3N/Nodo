using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Rise.Client.State;
using Rise.Shared.Users;
using System.Net.Http.Json;

namespace Rise.Client.Users;

public class UserContextService(
    AuthenticationStateProvider authProvider, 
    HttpClient httpClient,
    UserState userState
) : IUserContextService
{
    private readonly AuthenticationStateProvider _authProvider = authProvider;
    private readonly HttpClient _http = httpClient;
    private readonly UserState _userState = userState;

    public async Task<Result<UserResponse.CurrentUser>> GetCurrentUserAsync(CancellationToken ctx = default)
        => await _http.GetFromJsonAsync<Result<UserResponse.CurrentUser>>($"/api/users/current", cancellationToken: ctx)!;

    public async Task SetUserStateAsync(CancellationToken ctx = default)
    {
        if (_userState.User is not null)
            return;

        var authState = await _authProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user?.Identity?.IsAuthenticated == false)
        {
            return;
        }

        var result = await GetCurrentUserAsync(ctx);

        if (result is { IsSuccess: true, Value.User: not null })
        {
            _userState.User = result.Value.User;
        }
        else 
        {
            _userState.User = null;
        }
    }

    public async Task UpdateUserStateAsync()
    {
        var authState = await _authProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user?.Identity?.IsAuthenticated == false)
        {
            _userState.User = null;
        }
        else 
        { 
            var result = await GetCurrentUserAsync();

            if (result is { IsSuccess: true, Value.User: not null })
            {
                _userState.User = result.Value.User;
            }
        }
    }
}
