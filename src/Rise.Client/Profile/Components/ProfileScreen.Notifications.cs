using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Rise.Client.MagicBell;

namespace Rise.Client.Profile.Components;

public partial class ProfileScreen
{
    private bool _isSubscribingForPush;
    private string? _pushSubscriptionMessage;
    private string? _pushSubscriptionError;

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private MagicBellClientOptions MagicBellOptions { get; set; } = default!;

    private bool CanSubscribeForPush => !string.IsNullOrWhiteSpace(MagicBellOptions.PublicKey);

    private async Task EnablePushNotificationsAsync()
    {
        if (_isSubscribingForPush)
        {
            return;
        }

        _pushSubscriptionError = null;
        _pushSubscriptionMessage = null;

        if (!CanSubscribeForPush)
        {
            _pushSubscriptionError = "Pushnotificaties zijn niet geconfigureerd.";
            return;
        }

        var email = UserState.User?.Email;
        var accountId = UserState.User?.AccountId;

        _isSubscribingForPush = true;

        try
        {
            await JsRuntime.InvokeVoidAsync(
                "magicBellPush.subscribe",
                new
                {
                    serviceWorkerPath = MagicBellOptions.ServiceWorkerPath,
                    vapidPublicKey = MagicBellOptions.PublicKey,
                    userEmail = email,
                    userExternalId = accountId
                });

            _pushSubscriptionMessage = "Notificaties zijn ingeschakeld.";
        }
        catch (JSException jsEx)
        {
            _pushSubscriptionError = jsEx.Message;
        }
        catch (Exception ex)
        {
            _pushSubscriptionError = "Kon pushnotificaties niet inschakelen. Probeer later opnieuw.";
            Console.Error.WriteLine(ex);
        }
        finally
        {
            _isSubscribingForPush = false;
        }
    }
}
