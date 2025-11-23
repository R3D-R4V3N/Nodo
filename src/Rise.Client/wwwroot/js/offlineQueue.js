const DB_NAME = 'rise-offline-queue';
const OPERATION_STORE_NAME = 'operations';
const RESPONSE_STORE_NAME = 'responses';
const DB_VERSION = 2;

function openDb() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onupgradeneeded = () => {
            const db = request.result;
            if (!db.objectStoreNames.contains(OPERATION_STORE_NAME)) {
                db.createObjectStore(OPERATION_STORE_NAME, { keyPath: 'id', autoIncrement: true });
            }

            if (!db.objectStoreNames.contains(RESPONSE_STORE_NAME)) {
                db.createObjectStore(RESPONSE_STORE_NAME, { keyPath: 'key' });
            }
        };

        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

async function withStore(storeName, mode, callback) {
    const db = await openDb();

    return new Promise((resolve, reject) => {
        const transaction = db.transaction(storeName, mode);
        const store = transaction.objectStore(storeName);
        const request = callback(store);

        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

export function isOnline() {
    return navigator.onLine;
}

export async function enqueueOperation(operation) {
    return withStore(OPERATION_STORE_NAME, 'readwrite', (store) => store.add({ ...operation, createdAt: operation.createdAt ?? new Date().toISOString() }));
}

export async function getOperations() {
    const db = await openDb();

    return new Promise((resolve, reject) => {
        const transaction = db.transaction(OPERATION_STORE_NAME, 'readonly');
        const store = transaction.objectStore(OPERATION_STORE_NAME);
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
    return withStore(OPERATION_STORE_NAME, 'readwrite', (store) => store.delete(id));
}

export function registerOnlineCallback(dotNetRef) {
    if (!dotNetRef) return;

    const handler = () => {
        dotNetRef.invokeMethodAsync('OnBrowserOnline');
    };

    window.addEventListener('online', handler);

    return () => window.removeEventListener('online', handler);
}

export async function cacheResponse(entry) {
    return withStore(RESPONSE_STORE_NAME, 'readwrite', (store) => store.put({
        ...entry,
        createdAt: entry.createdAt ?? new Date().toISOString()
    }));
}

export async function getCachedResponse(key) {
    return withStore(RESPONSE_STORE_NAME, 'readonly', (store) => store.get(key));
}
