(async () => {
    let magicBellModulePromise;

    const loadMagicBell = () => {
        if (!magicBellModulePromise) {
            magicBellModulePromise = import('https://cdn.jsdelivr.net/npm/@magicbell/webpush@1.0.0/dist/magicbell-webpush.esm.js');
        }

        return magicBellModulePromise;
    };

    const ensureSupport = () => {
        if (!('Notification' in window)) {
            throw new Error('Deze browser ondersteunt geen notificaties.');
        }

        if (!('serviceWorker' in navigator)) {
            throw new Error('Service workers zijn niet beschikbaar in deze browser.');
        }
    };

    window.magicBellPush = {
        /**
         * Subscribe the current user for MagicBell Web Push notifications.
         * @param {{ serviceWorkerPath?: string, vapidPublicKey?: string, userEmail?: string, userExternalId?: string }} options
         */
        async subscribe(options = {}) {
            ensureSupport();
            const module = await loadMagicBell();

            if (!module?.subscribe) {
                throw new Error('MagicBell Web Push kon niet geladen worden.');
            }

            const subscriptionOptions = {
                serviceWorkerPath: options.serviceWorkerPath ?? '/service-worker.js',
            };

            if (options.vapidPublicKey) {
                subscriptionOptions.vapidPublicKey = options.vapidPublicKey;
            }

            if (options.userEmail) {
                subscriptionOptions.userEmail = options.userEmail;
            }

            if (options.userExternalId) {
                subscriptionOptions.userExternalId = options.userExternalId;
            }

            return module.subscribe(subscriptionOptions);
        }
    };
})();
