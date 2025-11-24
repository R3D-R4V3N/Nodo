using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Rise.Client.Offline;
using Rise.Client.State;
using Rise.Client.Users;

namespace Rise.Client;
public partial class App : IDisposable
{
    [Inject] public AuthenticationStateProvider AuthStateProvider { get; set; }
    [Inject] public UserContextService UserContext { get; set; }
    [Inject] public UserState UserState { get; set; } = default!;
    [Inject] public SessionCacheService SessionCacheService { get; set; } = default!;
    [Inject] public OfflineQueueService OfflineQueueService { get; set; } = default!;
    private int? _cachedChatCount;
    private string? _cachedUserName;
    private bool _isOffline;
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        AuthStateProvider.AuthenticationStateChanged += OnAuthStateChanged;
        await UpdateCurrentUserAsync();
    }

    private async void OnAuthStateChanged(Task<AuthenticationState> task)
    {
        await UpdateCurrentUserAsync();
        StateHasChanged();
    }

    private async Task UpdateCurrentUserAsync()
    {
        _isLoading = true;
        try
        {
            await SessionCacheService.ClearExpiredAsync();

            var cachedUser = await SessionCacheService.GetCachedCurrentUserAsync();
            var cachedChats = await SessionCacheService.GetCachedChatsAsync();

            if (cachedUser is not null && UserState.User is null)
            {
                UserState.User = cachedUser;
            }

            _cachedUserName = UserState.User?.Name ?? cachedUser?.Name;
            _cachedChatCount = cachedChats.Count > 0 ? cachedChats.Count : null;

            _isOffline = !await OfflineQueueService.IsOnlineAsync();
            if (_isOffline)
            {
                return;
            }

            var currentUser = await UserContext.InitializeAsync();
            UserState.User = currentUser;

            if (currentUser is not null)
            {
                _cachedUserName = currentUser.Name;
                await SessionCacheService.CacheCurrentUserAsync(currentUser);
            }
        }
        catch
        {
            if (UserState.User is null)
            {
                UserState.User = await SessionCacheService.GetCachedCurrentUserAsync();
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    public void Dispose()
    {
        AuthStateProvider.AuthenticationStateChanged -= OnAuthStateChanged;
    }
}