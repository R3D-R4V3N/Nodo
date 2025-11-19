// Based on the default Blazor WebAssembly service worker with offline caching enabled.
self.importScripts('./service-worker-assets.js');

const cacheName = 'nodo-offline-cache-' + self.assetsManifest.version;
const offlineResources = new Set(self.assetsManifest.assets
    .filter(asset => !asset.url.match(/^service-worker\./))
    .map(asset => asset.url));
offlineResources.add('./');

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

    if (requestUrl.origin === self.location.origin && offlineResources.has(requestUrl.pathname)) {
        event.respondWith(caches.open(cacheName).then(cache => cache.match(event.request).then(response => {
            return response || fetch(event.request).then(fetchResponse => {
                cache.put(event.request, fetchResponse.clone());
                return fetchResponse;
            });
        })));
        return;
    }

    // For navigation requests, fallback to the cached root when offline.
    if (event.request.mode === 'navigate') {
        event.respondWith(
            fetch(event.request).catch(() => caches.open(cacheName).then(cache => cache.match('./')))
        );
    }
});
