if ('serviceWorker' in navigator && window.isSecureContext) {
    window.addEventListener('load', () => {
        const baseHref = document.querySelector('base')?.getAttribute('href') ?? '/';
        const normalizedBase = baseHref.endsWith('/') ? baseHref : `${baseHref}/`;
        const serviceWorkerUrl = `${normalizedBase}service-worker.js`;

        navigator.serviceWorker.register(serviceWorkerUrl)
            .catch(err => console.error('Service worker registration failed', err));
    });
}