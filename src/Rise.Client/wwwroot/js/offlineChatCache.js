const DB_NAME = 'rise-offline-cache';
const CHAT_LIST_STORE = 'chat-lists';
const CHAT_DETAIL_STORE = 'chat-details';
const DB_VERSION = 1;

function openDb() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onupgradeneeded = () => {
            const db = request.result;
            if (!db.objectStoreNames.contains(CHAT_LIST_STORE)) {
                db.createObjectStore(CHAT_LIST_STORE, { keyPath: 'key' });
            }
            if (!db.objectStoreNames.contains(CHAT_DETAIL_STORE)) {
                db.createObjectStore(CHAT_DETAIL_STORE, { keyPath: 'key' });
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

export async function saveChatList(chats) {
    return withStore(CHAT_LIST_STORE, 'readwrite', store => store.put({ key: 'all', data: chats ?? [] }));
}

export async function getChatList() {
    return withStore(CHAT_LIST_STORE, 'readonly', store => store.get('all'))
        .then(result => result?.data ?? null)
        .catch(() => null);
}

export async function saveChatDetail(chat) {
    if (!chat) {
        return null;
    }

    const chatId = typeof chat.chatId !== 'undefined' ? chat.chatId : chat.ChatId;
    if (typeof chatId === 'undefined') {
        return null;
    }

    return withStore(CHAT_DETAIL_STORE, 'readwrite', store => store.put({ key: chatId, data: chat }));
}

export async function getChatDetail(chatId) {
    return withStore(CHAT_DETAIL_STORE, 'readonly', store => store.get(chatId))
        .then(result => result?.data ?? null)
        .catch(() => null);
}
