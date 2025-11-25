const DB_NAME = 'rise-offline-queue';
const STORE_NAME = 'operations';
const BLOB_STORE_NAME = 'blobs';
const DB_VERSION = 2;

function openDb() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onupgradeneeded = () => {
            const db = request.result;
            if (!db.objectStoreNames.contains(STORE_NAME)) {
                db.createObjectStore(STORE_NAME, { keyPath: 'id', autoIncrement: true });
            }

            if (!db.objectStoreNames.contains(BLOB_STORE_NAME)) {
                db.createObjectStore(BLOB_STORE_NAME, { keyPath: 'key' });
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

export async function putBlob(key, bytes) {
    const db = await openDb();
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(BLOB_STORE_NAME, 'readwrite');
        const store = transaction.objectStore(BLOB_STORE_NAME);
        const request = store.put({ key, blob: bytes });

        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

export async function getBlob(key) {
    const db = await openDb();
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(BLOB_STORE_NAME, 'readonly');
        const store = transaction.objectStore(BLOB_STORE_NAME);
        const request = store.get(key);

        request.onsuccess = () => resolve(request.result?.blob ?? null);
        request.onerror = () => reject(request.error);
    });
}

export async function deleteBlob(key) {
    const db = await openDb();
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(BLOB_STORE_NAME, 'readwrite');
        const store = transaction.objectStore(BLOB_STORE_NAME);
        const request = store.delete(key);

        request.onsuccess = () => resolve(true);
        request.onerror = () => reject(request.error);
    });
}

export async function createBlobUrl(key, contentType) {
    const blobBytes = await getBlob(key);
    if (!blobBytes) return null;

    const blob = new Blob([blobBytes], { type: contentType ?? 'application/octet-stream' });
    return URL.createObjectURL(blob);
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
