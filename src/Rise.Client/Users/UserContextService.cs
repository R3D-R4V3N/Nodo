using Microsoft.AspNetCore.Components.Authorization;
using Rise.Shared.Users;
using System.Linq;
using System.Net.Http.Json;

namespace Rise.Client.Users;

public class UserContextService(
    AuthenticationStateProvider authProvider,
    HttpClient httpClient
)
{
    private readonly AuthenticationStateProvider _authProvider = authProvider;
    private readonly HttpClient _http = httpClient;
    private UserDto.CurrentUser? CurrentUser = null;

    public UserDto.CurrentUser? Current => CurrentUser;

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

        var result = await _http
            .GetFromJsonAsync<Result<UserResponse.CurrentUser>>($"/api/users/current", cancellationToken: ctx);

        if (result is { IsSuccess: true, Value.User: not null })
        {
            CurrentUser = result.Value.User;
            return CurrentUser;
        }

        throw new InvalidOperationException("Kon gebruiker niet verkrijgen");
    }

    public async Task<UserDto.CurrentUser> UpdateProfileAsync(
        UserRequest.UpdateCurrentUser request,
        CancellationToken ctx = default)
    {
        var response = await _http.PutAsJsonAsync("/api/users/current", request, cancellationToken: ctx);

        var result = await response.Content
            .ReadFromJsonAsync<Result<UserResponse.CurrentUser>>(cancellationToken: ctx);

        if (response.IsSuccessStatusCode && result is { IsSuccess: true, Value.User: not null })
        {
            CurrentUser = result.Value.User;
            return CurrentUser;
        }

        var message = result?.Errors?.FirstOrDefault()
            ?? $"Kon wijzigingen niet opslaan (status {(int)response.StatusCode}).";

        throw new InvalidOperationException(message);
    }
}
