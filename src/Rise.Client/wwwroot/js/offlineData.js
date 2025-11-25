const DB_NAME = 'rise-offline-cache';
const DB_VERSION = 1;
const CHAT_LIST_STORE = 'chatLists';
const CHAT_STORE = 'chats';

function openDb() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onupgradeneeded = () => {
            const db = request.result;
            if (!db.objectStoreNames.contains(CHAT_LIST_STORE)) {
                db.createObjectStore(CHAT_LIST_STORE, { keyPath: 'id' });
            }
            if (!db.objectStoreNames.contains(CHAT_STORE)) {
                db.createObjectStore(CHAT_STORE, { keyPath: 'chatId' });
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

        let request;
        try {
            request = callback(store);
        } catch (err) {
            reject(err);
            return;
        }

        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

export async function cacheChatList(chatList) {
    try {
        await withStore(CHAT_LIST_STORE, 'readwrite', (store) => store.put({
            id: 'all',
            payload: chatList,
            updatedAt: new Date().toISOString()
        }));
    } catch (err) {
        console.error('Failed to cache chat list', err);
    }
}

export async function getCachedChatList() {
    try {
        const record = await withStore(CHAT_LIST_STORE, 'readonly', (store) => store.get('all'));
        return record?.payload ?? null;
    } catch (err) {
        console.warn('Failed to read cached chat list', err);
        return null;
    }
}

export async function cacheChat(chatId, chat) {
    try {
        await withStore(CHAT_STORE, 'readwrite', (store) => store.put({
            chatId: Number(chatId),
            payload: chat,
            updatedAt: new Date().toISOString()
        }));
    } catch (err) {
        console.error('Failed to cache chat', err);
    }
}

export async function getCachedChat(chatId) {
    try {
        const record = await withStore(CHAT_STORE, 'readonly', (store) => store.get(Number(chatId)));
        return record?.payload ?? null;
    } catch (err) {
        console.warn('Failed to read cached chat', err);
        return null;
    }
}
