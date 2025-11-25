const DB_NAME = 'rise-api-cache';
const STORE_NAME = 'responses';
const DB_VERSION = 2;
const MAX_ENTRIES = 200;

const buildKey = (method, url) => `${(method ?? 'GET').toUpperCase()}:${url}`;

function openDb() {
    if (!('indexedDB' in self)) {
        return Promise.reject(new Error('IndexedDB not supported'));
    }

    return new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onupgradeneeded = () => {
            const db = request.result;
            if (db.objectStoreNames.contains(STORE_NAME)) {
                db.deleteObjectStore(STORE_NAME);
            }

            const store = db.createObjectStore(STORE_NAME, { keyPath: 'key' });
            store.createIndex('updatedAt', 'updatedAt', { unique: false });
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

export function setCachedResponse(response) {
    const now = new Date().toISOString();
    const record = {
        ...response,
        key: buildKey(response.method, response.url),
        updatedAt: now
    };

    return withStore('readwrite', (store) => {
        const putRequest = store.put(record);
        putRequest.onsuccess = () => trimStore(store);
        return putRequest;
    }).catch(() => undefined);
}

export function getCachedResponse(url, method = 'GET') {
    const key = buildKey(method, url);
    return withStore('readonly', (store) => store.get(key)).catch(() => null);
}

function trimStore(store) {
    const countRequest = store.count();
    countRequest.onsuccess = () => {
        const total = countRequest.result ?? 0;
        if (total <= MAX_ENTRIES) return;

        const toDelete = total - MAX_ENTRIES;
        const index = store.index('updatedAt');
        let removed = 0;
        index.openCursor().onsuccess = (event) => {
            const cursor = event.target.result;
            if (!cursor || removed >= toDelete) return;

            store.delete(cursor.primaryKey);
            removed += 1;
            cursor.continue();
        };
    };
}
