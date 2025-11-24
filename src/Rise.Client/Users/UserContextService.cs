using Microsoft.AspNetCore.Components.Authorization;
using Rise.Client.Offline;
using Rise.Shared.Users;
using System.Net.Http.Json;

namespace Rise.Client.Users;

public class UserContextService(
    AuthenticationStateProvider authProvider,
    HttpClient httpClient,
    SessionCacheService sessionCacheService,
    OfflineQueueService offlineQueueService
) : IUserContextService
{
    private readonly AuthenticationStateProvider _authProvider = authProvider;
    private readonly HttpClient _http = httpClient;
    private readonly SessionCacheService _sessionCacheService = sessionCacheService;
    private readonly OfflineQueueService _offlineQueueService = offlineQueueService;
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(30);
    private UserDto.CurrentUser? CurrentUser = null;

    public async Task<Result<UserResponse.CurrentUser>> GetCurrentUserAsync(CancellationToken ctx = default)
        => await _http.GetFromJsonAsync<Result<UserResponse.CurrentUser>>($"/api/users/current", cancellationToken: ctx)!;


    public async Task<UserDto.CurrentUser?> InitializeAsync(CancellationToken ctx = default)
    {
        if (CurrentUser is not null)
            return CurrentUser;

        UserDto.CurrentUser? cachedUser = null;
        try
        {
            cachedUser = await _sessionCacheService.GetCachedCurrentUserAsync(ctx);
        }
        catch
        {
            // Ignore cache errors and continue to live request when available.
        }

        var authState = await _authProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user?.Identity?.IsAuthenticated == false)
        {
            return cachedUser;
        }

        var isOnline = await _offlineQueueService.IsOnlineAsync();
        if (!isOnline)
        {
            CurrentUser = cachedUser;
            return CurrentUser;
        }

        var result = await GetCurrentUserAsync(ctx);

        if (result is { IsSuccess: true, Value.User: not null })
        {
            CurrentUser = result.Value.User;
            await _sessionCacheService.CacheCurrentUserAsync(CurrentUser, CacheLifetime, ctx);
            return CurrentUser;
        }

        if (cachedUser is not null)
        {
            CurrentUser = cachedUser;
            return CurrentUser;
        }

        throw new InvalidOperationException("Kon gebruiker niet verkrijgen");
    }
}
