(function () {
    const permissionStates = ['default', 'granted', 'denied'];

    async function ensureServiceWorkerReady() {
        if (!('serviceWorker' in navigator) || !window.isSecureContext) {
            return false;
        }

        try {
            await navigator.serviceWorker.ready;
            return true;
        } catch (error) {
            console.warn('Service worker not ready for notifications', error);
            return false;
        }
    }

    async function requestPermission() {
        if (typeof Notification === 'undefined' || typeof Notification.requestPermission !== 'function') {
            return 'unsupported';
        }

        if (permissionStates.includes(Notification.permission)) {
            if (Notification.permission !== 'default') {
                return Notification.permission;
            }
        }

        await ensureServiceWorkerReady();

        try {
            const result = await Notification.requestPermission();
            return result;
        } catch (error) {
            console.warn('Notification permission request failed', error);
            return Notification.permission;
        }
    }

    window.nodoNotifications = {
        requestPermission
    };
})();
