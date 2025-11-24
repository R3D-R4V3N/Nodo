const DEV_CACHE = 'nodo-dev-cache-v3';
const toAbsoluteUrl = url => new URL(url, self.location.origin).toString();
const PRECACHE_URLS = [
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
].map(toAbsoluteUrl);
const FRAMEWORK_RESOURCES = [
    '_framework/blazor.boot.json',
    '_framework/blazor.webassembly.js',
    '_framework/dotnet.js',
    '_framework/dotnet.native.wasm',
    '_framework/dotnet.runtime.js'
].map(toAbsoluteUrl);
const AUTH_ENDPOINTS = [
    '/api/identity/accounts/info',
    '/api/users/current'
].map(toAbsoluteUrl);
const offlineRoot = toAbsoluteUrl('./');

self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(DEV_CACHE)
            .then(async cache => {
                await cache.addAll(PRECACHE_URLS);
                await precacheFramework(cache);
                await precacheAuthEndpoints(cache);
            })
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

const apiPattern = /\/api\//i;

self.addEventListener('fetch', event => {
    if (event.request.method !== 'GET') {
        return;
    }

    const requestUrl = new URL(event.request.url);
    const isAuthRequest = AUTH_ENDPOINTS.includes(toAbsoluteUrl(requestUrl.pathname));

    if (!isAuthRequest && apiPattern.test(event.request.url)) {
        return;
    }

    if (requestUrl.origin !== self.location.origin) {
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

async function precacheFramework(cache) {
    await Promise.allSettled(
        FRAMEWORK_RESOURCES.map(async resource => {
            try {
                const response = await fetch(new Request(resource, { cache: 'no-cache' }));
                if (response.ok) {
                    await cache.put(resource, response.clone());
                }
            } catch {
                // Ignore failures so install can continue offline.
            }
        })
    );
}

async function precacheAuthEndpoints(cache) {
    await Promise.allSettled(
        AUTH_ENDPOINTS.map(async endpoint => {
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
    const cache = await caches.open(DEV_CACHE);
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
    const cache = await caches.open(DEV_CACHE);
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
