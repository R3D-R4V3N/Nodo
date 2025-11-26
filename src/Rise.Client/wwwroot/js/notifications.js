(function () {
    const isSupported = () => 'Notification' in window && 'serviceWorker' in navigator && 'PushManager' in window;

    const toAbsolute = (path) => {
        try {
            return new URL(path, window.location.origin).toString();
        } catch {
            return path;
        }
    };

    async function ensureReady() {
        if (!isSupported()) {
            throw new Error('Push notificaties worden niet ondersteund in deze browser.');
        }

        const registration = await navigator.serviceWorker.ready;
        if (!registration) {
            throw new Error('Service worker is niet beschikbaar voor notificaties.');
        }

        return registration;
    }

    async function requestPermission() {
        const permission = await Notification.requestPermission();
        if (permission !== 'granted') {
            return permission;
        }

        await ensureReady();
        return permission;
    }

    async function showDemoNotification(payload) {
        const registration = await ensureReady();
        const permission = await Notification.requestPermission();

        if (permission !== 'granted') {
            return permission;
        }

        const notificationPayload = {
            title: payload?.title ?? 'Nodo push demo',
            body: payload?.body ?? 'Zo ziet een web push notificatie eruit.',
            icon: toAbsolute(payload?.icon ?? '/icon-192.png'),
            badge: toAbsolute(payload?.badge ?? '/icon-192.png'),
            data: payload?.data ?? { url: '/' }
        };

        await registration.showNotification(notificationPayload.title, {
            body: notificationPayload.body,
            icon: notificationPayload.icon,
            badge: notificationPayload.badge,
            data: notificationPayload.data
        });

        return 'shown';
    }

    window.notifications = {
        requestPermission,
        showDemoNotification,
        async requestAndNotify(payload) {
            const result = await showDemoNotification(payload);
            return result;
        }
    };
})();
