const DB_NAME = 'nodo-session-cache';
const STORE_NAME = 'payloads';
const DB_VERSION = 1;
const DEFAULT_TTL_MS = 30 * 60 * 1000; // 30 minutes

function openDb() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onupgradeneeded = () => {
            const db = request.result;
            if (!db.objectStoreNames.contains(STORE_NAME)) {
                db.createObjectStore(STORE_NAME, { keyPath: 'key' });
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

async function removeIfExpired(entry, store) {
    if (entry?.expiresAt && entry.expiresAt <= Date.now()) {
        store.delete(entry.key);
        return true;
    }

    return false;
}

export async function clearExpired() {
    const db = await openDb();

    return new Promise((resolve, reject) => {
        const transaction = db.transaction(STORE_NAME, 'readwrite');
        const store = transaction.objectStore(STORE_NAME);
        const request = store.getAll();

        request.onsuccess = () => {
            const entries = request.result ?? [];
            entries.forEach(entry => removeIfExpired(entry, store));
            resolve(true);
        };

        request.onerror = () => reject(request.error);
    });
}

export async function setPayload(key, data, ttlSeconds) {
    const ttl = typeof ttlSeconds === 'number' ? ttlSeconds * 1000 : DEFAULT_TTL_MS;
    const expiresAt = Date.now() + ttl;

    return withStore('readwrite', (store) => store.put({
        key,
        data,
        expiresAt,
        updatedAt: Date.now()
    }));
}

export async function getPayload(key) {
    const db = await openDb();

    return new Promise((resolve, reject) => {
        const transaction = db.transaction(STORE_NAME, 'readwrite');
        const store = transaction.objectStore(STORE_NAME);
        const request = store.get(key);

        request.onsuccess = () => {
            const entry = request.result;
            if (!entry) {
                resolve(null);
                return;
            }

            const expired = removeIfExpired(entry, store);
            resolve(expired ? null : entry.data ?? null);
        };

        request.onerror = () => reject(request.error);
    });
}
