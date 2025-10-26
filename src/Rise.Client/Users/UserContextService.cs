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
    private UserDto.CurrentUser? CurrentUser = null;

    public async Task<Result<UserResponse.CurrentUser>> GetCurrentUserAsync(CancellationToken ctx = default)
        => await _http.GetFromJsonAsync<Result<UserResponse.CurrentUser>>($"/api/users/current", cancellationToken: ctx)!;

    public async Task<UserDto.CurrentUser?> InitializeAsync(CancellationToken ctx = default)
    {
        if (CurrentUser is not null) 
            return CurrentUser;

        var authState = await _authProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user?.Identity?.IsAuthenticated == false)
        {
            return null;
        }

        var result = await GetCurrentUserAsync(ctx);

        if (result is { IsSuccess: true, Value.User: not null })
        {
            CurrentUser = result.Value.User;
            return CurrentUser;
        }

        throw new InvalidOperationException("Kon gebruiker niet verkrijgen");
    }

    public async Task<Result<UserResponse.CurrentUser>> UpdateCurrentUserAsync(
        UserRequest.UpdateCurrentUser request,
        CancellationToken ctx = default)
    {
        var response = await _http.PutAsJsonAsync("/api/users/current", request, cancellationToken: ctx);
        Result<UserResponse.CurrentUser>? result = null;

        try
        {
            result = await response.Content.ReadFromJsonAsync<Result<UserResponse.CurrentUser>>(cancellationToken: ctx);
        }
        catch
        {
            // Ignored: we will return a fallback error below.
        }

        if (result is { IsSuccess: true, Value.User: not null })
        {
            CurrentUser = result.Value.User;
            return result;
        }

        return result ?? Result.Error("Er ging iets mis bij het opslaan van je profiel.");
    }
}
