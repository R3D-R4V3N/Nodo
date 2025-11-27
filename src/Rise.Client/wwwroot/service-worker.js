const DEV_CACHE = 'nodo-dev-cache-v4';
const toAbsoluteUrl = url => new URL(url, self.location.origin).toString();
const PRECACHE_URLS = [
    './',
    'index.html',
    'manifest.webmanifest',
    'css/app.css',
    'css/style.css',
    'Rise.Client.styles.css',
    'js/offlineNotifier.js',
    'js/voiceRecorder.js',
    'js/notifications.js',
    'favicon.png',
    'icon-192.png',
    'icon-512.png'
].map(toAbsoluteUrl);
const offlineRoot = toAbsoluteUrl('./');
const apiPattern = /\/api\//i;
const defaultNotificationIcon = toAbsoluteUrl('icon-192.png');

self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(DEV_CACHE)
            .then(cache => cache.addAll(PRECACHE_URLS))
            .then(() => self.skipWaiting())
    );
});

self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(cacheNames =>
            Promise.all(
                cacheNames
                    .filter(cacheName => cacheName !== DEV_CACHE)
                    .map(cacheName => caches.delete(cacheName))
            )
        ).then(() => self.clients.claim())
    );
});

self.addEventListener('fetch', event => {
    if (event.request.method !== 'GET') {
        return;
    }

    const requestUrl = new URL(event.request.url);
    if (requestUrl.origin !== self.location.origin) {
        return;
    }

    if (apiPattern.test(requestUrl.pathname)) {
        event.respondWith(handleApiRequest(event.request));
        return;
    }

    event.respondWith(
        caches.open(DEV_CACHE).then(cache =>
            cache.match(event.request).then(cachedResponse => {
                const networkFetch = fetch(event.request)
                    .then(response => {
                        if (response && response.ok) {
                            cache.put(event.request, response.clone());
                        }
                        return response;
                    })
                    .catch(() => {
                        if (cachedResponse) {
                            return cachedResponse;
                        }
                        if (event.request.mode === 'navigate') {
                            return cache.match(offlineRoot);
                        }
                        return Response.error();
                    });

                return cachedResponse || networkFetch;
            })
        )
    );
});

self.addEventListener('push', event => {
    const data = event.data?.json?.() ?? {};
    const title = data.title || 'Nodo';
    const targetUrl = data?.data?.url || (data?.chatId ? `/chat/${data.chatId}` : '/');
    const options = {
        body: data.body || 'Je hebt een nieuwe melding.',
        icon: toAbsoluteUrl(data.icon || defaultNotificationIcon),
        badge: toAbsoluteUrl(data.badge || defaultNotificationIcon),
        data: data.data || { url: targetUrl }
    };

    event.waitUntil(
        self.registration.showNotification(title, options)
    );
});

self.addEventListener('notificationclick', event => {
    event.notification.close();
    const targetUrl = event.notification.data?.url || '/';
    const absoluteUrl = toAbsoluteUrl(targetUrl);

    event.waitUntil(
        clients.matchAll({ type: 'window', includeUncontrolled: true }).then(clientList => {
            const matchingClient = clientList.find(client => client.url === absoluteUrl);
            if (matchingClient) {
                return matchingClient.focus();
            }

            return clients.openWindow(absoluteUrl);
        })
    );
});

async function handleApiRequest(request) {
    const cache = await caches.open(DEV_CACHE);
    const normalizedRequest = new Request(request.url);

    try {
        const networkResponse = await fetch(request);
        if (networkResponse && networkResponse.ok) {
            cache.put(request, networkResponse.clone());
            cache.put(normalizedRequest, networkResponse.clone());
        }

        return networkResponse;
    }
    catch (error) {
        const cachedResponse = await cache.match(request, { ignoreVary: true })
            || await cache.match(normalizedRequest);
        if (cachedResponse) {
            return cachedResponse;
        }

        return new Response(
            JSON.stringify({ message: 'Offline: data is niet beschikbaar. Probeer opnieuw zodra je verbonden bent.' }),
            {
                status: 503,
                headers: { 'Content-Type': 'application/json' }
            });
    }
}
