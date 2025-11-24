// Based on the default Blazor WebAssembly service worker with offline caching enabled.
self.importScripts('./service-worker-assets.js');

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
    'js/offlineQueue.js',
    'js/sessionCache.js',
    'js/voiceRecorder.js',
    'favicon.png',
    'icon-192.png',
    'icon-512.png'
];
extraResources.map(toAbsoluteUrl).forEach(resource => offlineResources.add(resource));
const authEndpoints = new Set([
    '/api/identity/accounts/info',
    '/api/users/current'
].map(toAbsoluteUrl));
const offlineRoot = toAbsoluteUrl('./');

self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(cacheName)
            .then(async cache => {
                await cache.addAll(Array.from(offlineResources).map(resource => new Request(resource, { cache: 'no-cache' })));
                await precacheAuthEndpoints(cache);
            })
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

const apiPattern = /\/api\//i;
self.addEventListener('fetch', event => {
    if (event.request.method !== 'GET') {
        return;
    }

    const requestUrl = new URL(event.request.url);
    const isAuthRequest = authEndpoints.has(toAbsoluteUrl(requestUrl.pathname));

    if (!isAuthRequest && apiPattern.test(event.request.url)) {
        return;
    }

    if (requestUrl.origin !== self.location.origin) {
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

    if (isAuthRequest) {
        event.respondWith(handleAuthRequest(event.request));
        return;
    }

    if (requestUrl.pathname.startsWith('/_framework/')) {
        event.respondWith(cacheFirst(event.request));
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

async function precacheAuthEndpoints(cache) {
    await Promise.allSettled(
        Array.from(authEndpoints).map(async endpoint => {
            try {
                const request = new Request(endpoint, { credentials: 'include' });
                const response = await fetch(request);
                if (response.ok) {
                    await cache.put(request, response.clone());
                }
            } catch {
                // Ignore failures; auth responses can be refreshed when online.
            }
        })
    );
}

async function handleAuthRequest(request) {
    const cache = await caches.open(cacheName);
    const cachedResponse = await cache.match(request);

    try {
        const networkResponse = await fetch(request);
        if (networkResponse && networkResponse.ok) {
            await cache.put(request, networkResponse.clone());
        }
        return networkResponse;
    } catch {
        if (cachedResponse) {
            return cachedResponse;
        }

        return Response.error();
    }
}

async function cacheFirst(request) {
    const cache = await caches.open(cacheName);
    const cachedResponse = await cache.match(request);
    if (cachedResponse) {
        return cachedResponse;
    }

    const response = await fetch(request);
    if (response && response.ok) {
        cache.put(request, response.clone());
    }
    return response;
}
