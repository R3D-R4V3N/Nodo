// Utility to convert a base64url VAPID key to a UInt8 array required by PushManager.subscribe.
const urlBase64ToUint8Array = (base64String) => {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');

    const rawData = atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
};

async function registerPushSubscription(vapidPublicKey) {
    if (!('serviceWorker' in navigator) || !('PushManager' in window)) {
        console.warn('Push notifications are not supported in this browser.');
        return;
    }

    if (!vapidPublicKey) {
        console.warn('VAPID public key is not configured.');
        return;
    }

    try {
        // Ensure the service worker is ready before interacting with push APIs.
        const registration = await navigator.serviceWorker.ready;

        const permission = await Notification.requestPermission();
        if (permission !== 'granted') {
            console.info('Notifications permission was not granted.');
            return;
        }

        const existingSubscription = await registration.pushManager.getSubscription();
        if (existingSubscription) {
            await sendSubscriptionToServer(existingSubscription);
            return;
        }

        const subscription = await registration.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: urlBase64ToUint8Array(vapidPublicKey)
        });

        await sendSubscriptionToServer(subscription);
    } catch (err) {
        console.error('Failed to register push notifications', err);
    }
}

async function sendSubscriptionToServer(subscription) {
    const response = await fetch('/api/notifications/subscribe', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        credentials: 'include',
        body: JSON.stringify({
            endpoint: subscription.endpoint,
            keys: {
                p256dh: subscription.toJSON().keys.p256dh,
                auth: subscription.toJSON().keys.auth
            }
        })
    });

    if (!response.ok) {
        console.error('Failed to store push subscription', await response.text());
    }
}

// Initialize as soon as the page is loaded and the service worker is registered.
window.setupPushNotifications = async function () {
    try {
        const configResponse = await fetch('/config.json');
        const config = await configResponse.json();
        await registerPushSubscription(config.vapidPublicKey);
    } catch (error) {
        console.error('Unable to start push notification registration', error);
    }
};
