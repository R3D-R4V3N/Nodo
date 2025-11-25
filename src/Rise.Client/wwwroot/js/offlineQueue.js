const DB_NAME = 'rise-offline-queue';
const STORE_NAME = 'operations';
const DB_VERSION = 1;

function openDb() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onupgradeneeded = () => {
            const db = request.result;
            if (!db.objectStoreNames.contains(STORE_NAME)) {
                db.createObjectStore(STORE_NAME, { keyPath: 'id', autoIncrement: true });
            }
        };

        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

async function withStore(mode, callback) {
    const db = await openDb();

    return new Promise((resolve, reject) => {
        const transaction = db.transaction(STORE_NAME, mode);
        const store = transaction.objectStore(STORE_NAME);
        const request = callback(store);

        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

export function isOnline() {
    return navigator.onLine;
}

export async function enqueueOperation(operation) {
    return withStore('readwrite', (store) => store.add({ ...operation, createdAt: operation.createdAt ?? new Date().toISOString() }));
}

export async function getOperations() {
    const db = await openDb();

    return new Promise((resolve, reject) => {
        const transaction = db.transaction(STORE_NAME, 'readonly');
        const store = transaction.objectStore(STORE_NAME);
        const request = store.getAll();

        request.onsuccess = () => {
            const operations = request.result ?? [];
            operations.sort((a, b) => (a.id ?? 0) - (b.id ?? 0));
            resolve(operations);
        };

        request.onerror = () => reject(request.error);
    });
}

export async function removeOperation(id) {
    return withStore('readwrite', (store) => store.delete(id));
}

export async function getQueueLength() {
    return withStore('readonly', (store) => store.count());
}

export function registerOnlineCallback(dotNetRef) {
    if (!dotNetRef) return null;

    const handler = () => {
        dotNetRef.invokeMethodAsync('OnBrowserOnline');
    };

    window.addEventListener('online', handler);

    return {
        dispose: () => window.removeEventListener('online', handler)
    };
}

export function registerProcessingInterval(dotNetRef, intervalMs) {
    if (!dotNetRef || !intervalMs) return null;

    const invokeWhenOnlineAndVisible = () => {
        if (document.visibilityState !== 'visible') return;
        if (!navigator.onLine) return;

        dotNetRef.invokeMethodAsync('OnBrowserOnline');
    };

    const intervalId = setInterval(invokeWhenOnlineAndVisible, intervalMs);
    const visibilityHandler = () => {
        if (document.visibilityState === 'visible') {
            invokeWhenOnlineAndVisible();
        }
    };

    document.addEventListener('visibilitychange', visibilityHandler);

    return {
        dispose: () => {
            clearInterval(intervalId);
            document.removeEventListener('visibilitychange', visibilityHandler);
        }
    };
}
