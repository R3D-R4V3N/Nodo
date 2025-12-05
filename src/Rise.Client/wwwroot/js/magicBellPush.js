const MAGICBELL_PUBLIC_KEY = 'BMPQibipMaI8tVC74D7hAXeC7Xh-e7XnQ-j6ml16A8Jywy5ADxmZjB-msL68l8CDF3xZzDIZn-MnhKGKZ_V7dGU';
const MAGICBELL_WEBPUSH_MODULE = 'https://unpkg.com/@magicbell/webpush@latest/dist/webpush.esm.js';

let magicBellModulePromise;

async function getMagicBellModule() {
    if (!magicBellModulePromise) {
        magicBellModulePromise = import(MAGICBELL_WEBPUSH_MODULE);
    }

    return magicBellModulePromise;
}

async function ensureServiceWorkerReady() {
    if (!('serviceWorker' in navigator)) {
        throw new Error('Service workers zijn niet beschikbaar in deze browser.');
    }

    return navigator.serviceWorker.ready;
}

export async function subscribeToMagicBell(externalUserId, email) {
    try {
        const registration = await ensureServiceWorkerReady();
        const magicBell = await getMagicBellModule();
        const subscribe = magicBell?.subscribe;

        if (typeof subscribe !== 'function') {
            throw new Error('MagicBell webpush module kon niet geladen worden.');
        }

        await subscribe({
            serviceWorkerRegistration: registration,
            serviceWorkerPath: '/service-worker.js',
            userExternalId: externalUserId,
            userEmail: email,
            vapidKey: MAGICBELL_PUBLIC_KEY
        });

        return true;
    } catch (error) {
        console.error('MagicBell push subscription failed', error);
        return false;
    }
}
