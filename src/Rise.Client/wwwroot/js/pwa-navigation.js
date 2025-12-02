// Detect whether the app is running as an installed PWA (standalone display mode).
export function isPWA() {
    return window.matchMedia('(display-mode: standalone)').matches ||
        window.navigator.standalone === true;
}

// Centralize the redirect logic so it can be reused from both JavaScript and .NET interop.
export function enforcePwaRouting() {
    const path = (window.location.pathname || '/').split('?')[0].split('#')[0];
    const normalizedPath = path.startsWith('/') ? path : `/${path}`;

    const inPwa = isPWA();
    const onPwaPage = normalizedPath.toLowerCase() === '/pwa';

    // When not running in a PWA, force the dedicated /pwa page and block other pages.
    if (!inPwa && !onPwaPage) {
        window.location.replace('/pwa');
        return;
    }

    // When running as a PWA, avoid the /pwa landing page and forward to the homepage.
    if (inPwa && onPwaPage) {
        window.location.replace('/homepage');
    }
}

// Expose helpers globally so Blazor can call them via JS interop as well.
window.pwaNavigation = {
    isPWA,
    enforcePwaRouting
};

// Run once on load so direct hits to pages immediately respect the PWA routing rules.
enforcePwaRouting();
