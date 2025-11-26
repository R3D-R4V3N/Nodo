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

    function buildNotificationPayload(payload) {
        return {
            title: payload?.title ?? 'Nodo',
            body: payload?.body ?? 'Je hebt een nieuwe melding.',
            icon: toAbsolute(payload?.icon ?? '/icon-192.png'),
            badge: toAbsolute(payload?.badge ?? '/icon-192.png'),
            data: payload?.data ?? { url: '/' }
        };
    }

    async function requestPermission() {
        const permission = await Notification.requestPermission();
        if (permission !== 'granted') {
            return permission;
        }

        await ensureReady();
        return permission;
    }

    async function showNotification(payload) {
        const registration = await ensureReady();
        const permission = await Notification.requestPermission();

        if (permission !== 'granted') {
            return permission;
        }

        const notificationPayload = buildNotificationPayload(payload);

        await registration.showNotification(notificationPayload.title, {
            body: notificationPayload.body,
            icon: notificationPayload.icon,
            badge: notificationPayload.badge,
            data: notificationPayload.data
        });

        return 'shown';
    }

    async function showMessageNotification(messagePayload) {
        const { chatId, senderName, contentPreview } = messagePayload ?? {};
        const notificationData = {
            title: senderName ? `Nieuw bericht van ${senderName}` : 'Nieuw bericht',
            body: contentPreview || 'Je hebt een nieuw bericht.',
            data: {
                url: messagePayload?.data?.url ?? `/chat/${chatId ?? ''}`
            }
        };

        return await showNotification(notificationData);
    }

    window.notifications = {
        requestPermission,
        showDemoNotification: showNotification,
        async requestAndNotify(payload) {
            const result = await showNotification(payload);
            return result;
        },
        async showMessageNotification(payload) {
            const result = await showMessageNotification(payload);
            return result;
        }
    };
})();
