using Microsoft.AspNetCore.Components.Authorization;
using Rise.Client.Offline;
using Rise.Shared.Users;
using System.Net.Http.Json;

namespace Rise.Client.Users;

public class UserContextService(
    AuthenticationStateProvider authProvider,
    HttpClient httpClient,
    CacheStoreService cacheStoreService,
    OfflineQueueService offlineQueueService
) : IUserContextService
{
    private readonly AuthenticationStateProvider _authProvider = authProvider;
    private readonly HttpClient _http = httpClient;
    private readonly CacheStoreService _cache = cacheStoreService;
    private readonly OfflineQueueService _offlineQueue = offlineQueueService;
    private UserDto.CurrentUser? CurrentUser = null;
    private static readonly TimeSpan SessionCacheTtl = TimeSpan.FromHours(12);

    public async Task<Result<UserResponse.CurrentUser>> GetCurrentUserAsync(CancellationToken ctx = default)
        => await _http.GetFromJsonAsync<Result<UserResponse.CurrentUser>]("/api/users/current", cancellationToken: ctx)!;


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

        var cachedSession = await _cache.GetAuthSessionAsync(SessionCacheTtl, ctx);
        if (cachedSession?.User is not null)
        {
            CurrentUser = cachedSession.User;
        }

        var isOnline = await _offlineQueue.IsOnlineAsync();
        if (!isOnline && CurrentUser is not null)
        {
            return CurrentUser;
        }

        var result = await GetCurrentUserAsync(ctx);

        if (result is { IsSuccess: true, Value.User: not null })
        {
            CurrentUser = result.Value.User;
            await _cache.UpsertCurrentUserAsync(CurrentUser.AccountId, CurrentUser, ctx);
            await _cache.UpsertAuthSessionAsync(new CachedSession
            {
                AccountId = CurrentUser.AccountId,
                User = CurrentUser,
                AccountInfo = cachedSession?.AccountInfo
            }, ctx);
            return CurrentUser;
        }

        if (CurrentUser is not null)
        {
            return CurrentUser;
        }

        throw new InvalidOperationException("Kon gebruiker niet verkrijgen");
    }
}
