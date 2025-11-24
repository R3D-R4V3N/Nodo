const DB_NAME = 'rise-api-cache';
const STORE_NAME = 'responses';
const DB_VERSION = 1;

function openDb() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onupgradeneeded = () => {
            const db = request.result;
            if (!db.objectStoreNames.contains(STORE_NAME)) {
                db.createObjectStore(STORE_NAME, { keyPath: 'url' });
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

export function setCachedResponse(response) {
    const record = {
        ...response,
        updatedAt: new Date().toISOString()
    };

    return withStore('readwrite', (store) => store.put(record));
}

export function getCachedResponse(url) {
    return withStore('readonly', (store) => store.get(url));
}
