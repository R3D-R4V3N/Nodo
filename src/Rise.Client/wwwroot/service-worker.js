self.addEventListener('install', () => self.skipWaiting());
self.addEventListener('activate', event => event.waitUntil(self.clients.claim()));

// In development we bypass offline caching so that changes appear immediately.
self.addEventListener('fetch', () => { });