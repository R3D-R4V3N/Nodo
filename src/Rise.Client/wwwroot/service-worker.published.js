// Based on the default Blazor WebAssembly service worker with offline caching enabled.
self.importScripts('./service-worker-assets.js');
self.importScripts('https://assets.magicbell.io/sdks/webpush/service-worker.js');

const cacheName = 'nodo-offline-cache-' + self.assetsManifest.version;
const toAbsoluteUrl = url => new URL(url, self.location.origin).toString();
const offlineResources = new Set(self.assetsManifest.assets
    .filter(asset => !asset.url.match(/^service-worker\./))
    .map(asset => toAbsoluteUrl(asset.url)));

const extraResources = [
    './',
    'index.html',
    'manifest.webmanifest',
    'css/app.css',
    'css/style.css',
    'Rise.Client.styles.css',
    'js/offlineNotifier.js',
    'js/magicBellPush.js',
    'js/voiceRecorder.js',
    'favicon.png',
    'icon-192.png',
    'icon-512.png'
];
extraResources.map(toAbsoluteUrl).forEach(resource => offlineResources.add(resource));
const offlineRoot = toAbsoluteUrl('./');
const apiPattern = /\/api\//i;

self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(cacheName)
            .then(cache => cache.addAll(Array.from(offlineResources).map(resource => new Request(resource, { cache: 'no-cache' }))))
            .then(() => self.skipWaiting())
    );
});

self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(cacheNames => Promise.all(cacheNames
            .filter(otherCache => otherCache !== cacheName)
            .map(otherCache => caches.delete(otherCache))))
            .then(() => self.clients.claim())
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

    // For navigation requests, fallback to the cached root when offline.
    if (event.request.mode === 'navigate') {
        event.respondWith(
            fetch(event.request)
                .then(response => {
                    const responseClone = response.clone();
                    caches.open(cacheName).then(cache => cache.put(event.request, responseClone));
                    return response;
                })
                .catch(() => caches.open(cacheName).then(cache => cache.match(offlineRoot)))
        );
        return;
    }

    if (offlineResources.has(requestUrl.href) || event.request.destination === 'style' || event.request.destination === 'script') {
        event.respondWith(
            caches.open(cacheName).then(cache =>
                cache.match(event.request).then(cachedResponse => {
                    const fetchPromise = fetch(event.request)
                        .then(networkResponse => {
                            if (networkResponse && networkResponse.ok) {
                                cache.put(event.request, networkResponse.clone());
                            }
                            return networkResponse;
                        })
                        .catch(() => cachedResponse);

                    return cachedResponse || fetchPromise;
                })
            )
        );
    }
});

async function handleApiRequest(request) {
    const cache = await caches.open(cacheName);
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
