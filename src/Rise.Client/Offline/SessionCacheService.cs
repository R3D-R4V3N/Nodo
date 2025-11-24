using Microsoft.JSInterop;
using Rise.Shared.Chats;
using Rise.Shared.Identity.Accounts;
using Rise.Shared.Users;

namespace Rise.Client.Offline;

public sealed class SessionCacheService : IAsyncDisposable
{
    private const string ModulePath = "./js/sessionCache.js";
    private static readonly TimeSpan DefaultLifetime = TimeSpan.FromMinutes(30);
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    public SessionCacheService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task ClearExpiredAsync(CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        await _module!.InvokeVoidAsync("clearExpired", cancellationToken);
    }

    public async Task CacheAuthInfoAsync(AccountResponse.Info info, TimeSpan? lifetime = null, CancellationToken cancellationToken = default)
    {
        await CacheAsync("auth-info", info, lifetime, cancellationToken);
    }

    public async Task<AccountResponse.Info?> GetCachedAuthInfoAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<AccountResponse.Info>("auth-info", cancellationToken);
    }

    public async Task CacheCurrentUserAsync(UserDto.CurrentUser user, TimeSpan? lifetime = null, CancellationToken cancellationToken = default)
    {
        await CacheAsync("current-user", user, lifetime, cancellationToken);
    }

    public async Task<UserDto.CurrentUser?> GetCachedCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<UserDto.CurrentUser>("current-user", cancellationToken);
    }

    public async Task CacheChatsAsync(IEnumerable<ChatDto.GetChats> chats, TimeSpan? lifetime = null, CancellationToken cancellationToken = default)
    {
        await CacheAsync("chat-list", chats, lifetime, cancellationToken);
    }

    public async Task<IReadOnlyList<ChatDto.GetChats>> GetCachedChatsAsync(CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<List<ChatDto.GetChats>>("chat-list", cancellationToken);
        return cached ?? Array.Empty<ChatDto.GetChats>();
    }

    private async Task CacheAsync<T>(string key, T value, TimeSpan? lifetime, CancellationToken cancellationToken)
    {
        await EnsureModuleAsync();
        var ttlSeconds = (lifetime ?? DefaultLifetime).TotalSeconds;
        await _module!.InvokeVoidAsync("setPayload", cancellationToken, key, value, ttlSeconds);
    }

    private async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        await EnsureModuleAsync();
        return await _module!.InvokeAsync<T?>("getPayload", cancellationToken, key);
    }

    private async Task EnsureModuleAsync()
    {
        if (_module is not null)
        {
            return;
        }

        _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}
