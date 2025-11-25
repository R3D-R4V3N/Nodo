const CACHE_VERSION = 'dev-v3';
const CACHE_PREFIX = 'nodo-cache';
const PRECACHE = `${CACHE_PREFIX}-precache-${CACHE_VERSION}`;
const RUNTIME_CACHE = `${CACHE_PREFIX}-runtime-${CACHE_VERSION}`;
const API_CACHE = `${CACHE_PREFIX}-api-${CACHE_VERSION}`;
const VALID_CACHES = [PRECACHE, RUNTIME_CACHE, API_CACHE];

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
    'favicon.png',
    'icon-192.png',
    'icon-512.png',
    '_framework/blazor.webassembly.js',
    '_framework/blazor.boot.json'
].map(toAbsoluteUrl);
const AUTH_URLS = ['api/identity/accounts/info', 'api/users/current'].map(toAbsoluteUrl);
const offlineRoot = toAbsoluteUrl('index.html');

const offlineFallback = new Response(
    '<!doctype html><html><head><meta charset="utf-8"/><title>Offline</title></head><body>' +
    '<h1>You\'re offline</h1><p>The application isn\'t cached yet. Please reconnect and try again.</p>' +
    '</body></html>',
    { headers: { 'Content-Type': 'text/html' } }
);

const apiPattern = /\/api\//i;
const assetPattern = /\.(?:dll|wasm|js|css|json|woff2|png|jpe?g|gif|svg|webp)$/i;
const shortLivedSeconds = 60;

self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(PRECACHE)
            .then(cache => cache.addAll([...PRECACHE_URLS, ...AUTH_URLS]))
            .then(async cache => {
                try {
                    const bootResponse = await fetch('/_framework/blazor.boot.json', { cache: 'no-cache' });
                    if (bootResponse.ok) {
                        const manifest = await bootResponse.clone().json();
                        const frameworkResources = Object.values(manifest.resources || {})
                            .flatMap(resource => Object.values(resource))
                            .filter(Boolean)
                            .map(url => toAbsoluteUrl(`/_framework/${url}`));
                        await cache.addAll(frameworkResources);
                    }
                } catch { /* ignore boot precache failures */ }

                return cache;
            })
            .then(() => self.skipWaiting())
    );
});

self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(cacheNames =>
            Promise.all(
                cacheNames
                    .filter(cacheName => cacheName.startsWith(CACHE_PREFIX) && !VALID_CACHES.includes(cacheName))
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

    if (event.request.mode === 'navigate') {
        event.respondWith(handleNavigationRequest(event.request));
        return;
    }

    if (apiPattern.test(event.request.url)) {
        event.respondWith(handleApiRequest(event.request));
        return;
    }

    if (assetPattern.test(requestUrl.pathname) || requestUrl.pathname.startsWith('/_framework/')) {
        event.respondWith(handleRuntimeAsset(event.request));
    }
});

function handleNavigationRequest(request) {
    return fetch(request)
        .then(response => {
            if (response && response.ok) {
                caches.open(RUNTIME_CACHE).then(cache => cache.put(offlineRoot, response.clone()));
            }
            return response;
        })
        .catch(() => caches.match(offlineRoot).then(match => match || offlineFallback));
}

function handleRuntimeAsset(request) {
    return caches.open(RUNTIME_CACHE).then(cache =>
        cache.match(request).then(cachedResponse => {
            const networkFetch = fetch(request)
                .then(response => {
                    if (response && response.ok) {
                        cache.put(request, response.clone());
                    }
                    return response;
                })
                .catch(() => cachedResponse);

            return cachedResponse || networkFetch;
        })
    );
}

function handleApiRequest(request) {
    return caches.open(API_CACHE).then(cache =>
        cache.match(request).then(cachedResponse => {
            const fetchPromise = fetch(request)
                .then(response => {
                    if (response && response.ok) {
                        cache.put(request, response.clone());
                    }
                    return response;
                })
                .catch(() => undefined);

            if (cachedResponse) {
                const isStale = isResponseStale(cachedResponse, shortLivedSeconds);
                if (isStale) {
                    fetchPromise;
                }
                return cachedResponse;
            }

            return fetchPromise || offlineApiFallback();
        })
    );
}

function offlineApiFallback() {
    return new Response(JSON.stringify({ error: 'offline', message: 'Cached data unavailable.' }), {
        status: 503,
        headers: { 'Content-Type': 'application/json' }
    });
}

function isResponseStale(response, maxAgeSeconds) {
    const dateHeader = response.headers.get('date');
    if (!dateHeader) {
        return true;
    }

    const ageSeconds = (Date.now() - new Date(dateHeader).getTime()) / 1000;
    return ageSeconds > maxAgeSeconds;
}
