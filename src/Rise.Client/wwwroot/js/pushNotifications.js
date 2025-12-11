(function () {
    console.log("pushNotifications.js loaded");

    function urlBase64ToUint8Array(base64String) {
        const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
        const base64 = (base64String + padding)
            .replace(/-/g, '+')
            .replace(/_/g, '/');

        const rawData = atob(base64);
        const outputArray = new Uint8Array(rawData.length);

        for (let i = 0; i < rawData.length; ++i) {
            outputArray[i] = rawData.charCodeAt(i);
        }
        return outputArray;
    }

    async function requestPushSubscription(vapidPublicKeyBase64) {
        console.log("[nodoPush] requestPushSubscription gestart");

        if (!('Notification' in window)) {
            throw new Error('Browser ondersteunt Notification API niet.');
        }
        if (!('serviceWorker' in navigator)) {
            throw new Error('Service workers worden niet ondersteund.');
        }
        if (!('PushManager' in window)) {
            throw new Error('PushManager (Push API) wordt niet ondersteund.');
        }

        console.log("[nodoPush] Notification.requestPermission...");
        const permission = await Notification.requestPermission();
        console.log("[nodoPush] Permission resultaat:", permission);

        if (permission !== 'granted') {
            throw new Error('Notificaties zijn niet toegestaan door de gebruiker.');
        }

        console.log("[nodoPush] Service worker registratie ophalen...");
        const registration = await navigator.serviceWorker.getRegistration();

        if (!registration) {
            console.error("[nodoPush] Geen service worker registratie gevonden!");
            throw new Error('Service worker is niet geregistreerd. Kan geen push subscription maken.');
        }

        console.log("[nodoPush] Service worker ok, pushManager.subscribe uitvoeren...");

        const applicationServerKey = urlBase64ToUint8Array(vapidPublicKeyBase64);

        const subscription = await registration.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey
        });

        const rawP256dh = subscription.getKey('p256dh');
        const rawAuth = subscription.getKey('auth');

        const p256dh = btoa(String.fromCharCode.apply(null, new Uint8Array(rawP256dh)));
        const auth = btoa(String.fromCharCode.apply(null, new Uint8Array(rawAuth)));

        console.log("[nodoPush] Subscription succesvol aangemaakt");

        return {
            endpoint: subscription.endpoint,
            p256dh,
            auth
        };
    }

    async function hasActiveSubscription() {
        if (!('Notification' in window) || !('serviceWorker' in navigator) || !('PushManager' in window)) {
            return false;
        }

        const registration = await navigator.serviceWorker.getRegistration();

        if (!registration) {
            return false;
        }

        const subscription = await registration.pushManager.getSubscription();
        return !!subscription;
    }
    window.nodoPush = {
        requestPushSubscription,
        hasActiveSubscription
    };
})();
