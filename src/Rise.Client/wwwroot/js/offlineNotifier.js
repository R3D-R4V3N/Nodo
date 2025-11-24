const createSubscription = dotNetRef => {
    const notify = () => {
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('UpdateOnlineStatus', navigator.onLine);
        }
    };

    window.addEventListener('online', notify);
    window.addEventListener('offline', notify);
    notify();

    return {
        dispose: () => {
            window.removeEventListener('online', notify);
            window.removeEventListener('offline', notify);
        }
    };
};

export function registerStatusChanged(dotNetRef) {
    if (!dotNetRef) {
        return createSubscription(null);
    }

    return createSubscription(dotNetRef);
}