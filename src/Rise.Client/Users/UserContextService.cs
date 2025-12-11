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
    public Task<Result<UserResponse.CurrentUser>> GetCurrentUserAsync(CancellationToken ctx = default)
        => httpClient.GetFromJsonAsync<Result<UserResponse.CurrentUser>>($"/api/users/current", cancellationToken: ctx)!;

    public async Task SetUserStateAsync(CancellationToken ctx = default)
    {
        if (userState.User is not null)
            return;

        var authState = await authProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user?.Identity?.IsAuthenticated == false)
        {
            return;
        }

        var result = await GetCurrentUserAsync(ctx);

        if (result is { IsSuccess: true, Value.User: not null })
        {
            userState.User = result.Value.User;
        }
        else 
        {
            userState.User = null;
        }
    }

    public async Task UpdateUserStateAsync()
    {
        var authState = await authProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user?.Identity?.IsAuthenticated == false)
        {
            userState.User = null;
        }
        else 
        { 
            var result = await GetCurrentUserAsync();

            if (result is { IsSuccess: true, Value.User: not null })
            {
                userState.User = result.Value.User;
            }
        }
    }
}
