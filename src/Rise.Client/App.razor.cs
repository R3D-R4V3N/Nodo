using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Rise.Client.Chats;
using Rise.Client.State;
using Rise.Client.Users;

namespace Rise.Client;
public partial class App : IDisposable
{
    [Inject] public AuthenticationStateProvider AuthStateProvider { get; set; }
    [Inject] public UserContextService UserContext { get; set; }
    [Inject] public UserState UserState { get; set; } = default!;
    [Inject] public GlobalChatNotificationListener NotificationListener { get; set; } = default!;
    [Inject] public ChatNotificationService ChatNotificationService { get; set; } = default!;
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        AuthStateProvider.AuthenticationStateChanged += OnAuthStateChanged;
        _isLoading = true;
        try
        {
            await UserContext.SetUserStateAsync();
            await ChatNotificationService.RequestPermissionAsync();
            await SyncNotificationListenerAsync();
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }

    private async void OnAuthStateChanged(Task<AuthenticationState> task)
    {
        _isLoading = true;
        try
        {
            await UserContext.UpdateUserStateAsync();
            await ChatNotificationService.RequestPermissionAsync();
            await SyncNotificationListenerAsync();
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }

    private async Task SyncNotificationListenerAsync()
    {
        if (UserState.User is null)
        {
            await NotificationListener.StopAsync();
            return;
        }

        await NotificationListener.StartAsync();
    }

    public void Dispose()
    {
        AuthStateProvider.AuthenticationStateChanged -= OnAuthStateChanged;
        NotificationListener.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}