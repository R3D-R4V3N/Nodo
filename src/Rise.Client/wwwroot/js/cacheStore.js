const DB_NAME = 'rise-cache';
const DEFAULT_VERSION = 1;
const BASE_STORES = ['chats', 'contacts'];

let dbPromise = null;

function openDatabase(version, stores = []) {
    return new Promise((resolve, reject) => {
        const uniqueStores = [...new Set([...BASE_STORES, ...stores])];
        const request = indexedDB.open(DB_NAME, version);

        request.onupgradeneeded = () => {
            const db = request.result;
            uniqueStores.forEach((store) => {
                if (!db.objectStoreNames.contains(store)) {
                    db.createObjectStore(store, { keyPath: 'id' });
                }
            });
        };

        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

async function getDatabase(requiredStores = []) {
    if (!dbPromise) {
        dbPromise = openDatabase(DEFAULT_VERSION, requiredStores);
    }

    let db = await dbPromise;
    const missingStores = requiredStores.filter((store) => !db.objectStoreNames.contains(store));

    if (missingStores.length === 0) {
        return db;
    }

    dbPromise = upgradeDatabase(db, missingStores);
    db = await dbPromise;
    return db;
}

function upgradeDatabase(currentDb, newStores) {
    const nextVersion = currentDb.version + 1;
    currentDb.close();
    return openDatabase(nextVersion, newStores);
}

function runTransaction(db, storeName, mode, action) {
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(storeName, mode);
        const store = transaction.objectStore(storeName);

        action(store);

        transaction.oncomplete = () => resolve(true);
        transaction.onerror = () => reject(transaction.error);
    });
}

export async function upsertMany(storeName, items) {
    const db = await getDatabase([storeName]);
    const normalizedItems = (items ?? []).map((item) => ({
        ...item,
        updatedAt: item.updatedAt ?? new Date().toISOString(),
    }));

    return runTransaction(db, storeName, 'readwrite', (store) => {
        normalizedItems.forEach((item) => store.put(item));
    });
}

export async function getAll(storeName) {
    const db = await getDatabase([storeName]);

    return new Promise((resolve, reject) => {
        const transaction = db.transaction(storeName, 'readonly');
        const store = transaction.objectStore(storeName);
        const request = store.getAll();

        request.onsuccess = () => resolve(request.result ?? []);
        request.onerror = () => reject(request.error);
    });
}

export async function clearStore(storeName) {
    const db = await getDatabase([storeName]);
    return runTransaction(db, storeName, 'readwrite', (store) => store.clear());
}
