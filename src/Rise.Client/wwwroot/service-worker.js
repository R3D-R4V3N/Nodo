self.importScripts('https://unpkg.com/@magicbell/webpush@latest/dist/magicbell-sw.js');

const DEV_CACHE = 'nodo-dev-cache-v5';
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
    'js/magicBellPush.js',
    'favicon.png',
    'icon-192.png',
    'icon-512.png'
].map(toAbsoluteUrl);
const offlineRoot = toAbsoluteUrl('./');
const apiPattern = /\/api\//i;

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
