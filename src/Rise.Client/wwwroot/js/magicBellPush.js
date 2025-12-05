(async () => {
    let magicBellModulePromise;

    const loadMagicBell = () => {
        if (!magicBellModulePromise) {
            magicBellModulePromise = import('https://cdn.jsdelivr.net/npm/@magicbell/webpush@1.0.0/dist/magicbell-webpush.esm.js');
        }

        return magicBellModulePromise;
    };

    const ensureServiceWorkerIsReachable = async (serviceWorkerPath) => {
        const response = await fetch(serviceWorkerPath, { method: 'GET' });

        if (!response.ok) {
            throw new Error(`Service worker niet gevonden (${response.status}). Controleer het pad of of het bestand wordt gepubliceerd.`);
        }

        const contentType = response.headers.get('content-type') ?? '';
        if (!contentType.toLowerCase().includes('javascript')) {
            throw new Error('Service worker-bestand lijkt geen JavaScript terug te geven. Mogelijk wordt een HTML-pagina geserveerd.');
        }
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

            await ensureServiceWorkerIsReachable(subscriptionOptions.serviceWorkerPath);

            try {
                return await module.subscribe(subscriptionOptions);
            } catch (error) {
                if (error instanceof SyntaxError && typeof error.message === 'string' && error.message.includes('Unexpected token <')) {
                    throw new Error('Kon de service worker niet registreren: er wordt HTML teruggestuurd in plaats van JavaScript. Controleer het pad en of het bestand publiek beschikbaar is.');
                }

                throw error;
            }
        }
    };
})();
