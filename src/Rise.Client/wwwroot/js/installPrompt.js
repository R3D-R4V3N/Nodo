(function () {
    let deferredPrompt = null;
    let dotNetHelper = null;

    function notifyAvailable() {
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync('OnInstallAvailable');
        }
    }

    function handleBeforeInstallPrompt(event) {
        event.preventDefault();
        deferredPrompt = event;
        notifyAvailable();
    }

    window.pwaInstall = {
        register(dotNetReference) {
            dotNetHelper = dotNetReference;
            window.removeEventListener('beforeinstallprompt', handleBeforeInstallPrompt);
            window.addEventListener('beforeinstallprompt', handleBeforeInstallPrompt);

            if (deferredPrompt) {
                notifyAvailable();
            }
        },
        canPrompt() {
            return !!deferredPrompt;
        },
        async promptInstall() {
            if (!deferredPrompt) {
                return false;
            }

            const promptEvent = deferredPrompt;
            promptEvent.prompt();

            const result = await promptEvent.userChoice;
            if (result.outcome === 'accepted') {
                deferredPrompt = null;
            }

            return result.outcome === 'accepted';
        }
    };
})();
