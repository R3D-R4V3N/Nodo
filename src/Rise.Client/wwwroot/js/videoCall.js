const activeStreams = new WeakMap();

function ensureVideoElement(videoElement) {
    if (!videoElement) {
        throw new Error("Geen video-element gevonden om de stream te tonen.");
    }
    if (videoElement.tagName?.toLowerCase() !== "video") {
        throw new Error("Het opgegeven element is geen <video> element.");
    }

    videoElement.setAttribute("playsinline", "true");
    videoElement.muted = true;
}

export function hasDeviceSupport() {
    return typeof navigator !== "undefined"
        && typeof navigator.mediaDevices !== "undefined"
        && typeof navigator.mediaDevices.getUserMedia === "function";
}

export async function startLocalPreview(videoElement) {
    ensureVideoElement(videoElement);

    await stopLocalPreview(videoElement);

    if (!hasDeviceSupport()) {
        throw new Error("Deze browser ondersteunt geen camera- of microfoonopnames.");
    }

    const constraints = {
        audio: true,
        video: {
            width: { ideal: 1280 },
            height: { ideal: 720 },
            facingMode: "user"
        }
    };

    const stream = await navigator.mediaDevices.getUserMedia(constraints);
    videoElement.srcObject = stream;
    await videoElement.play();
    activeStreams.set(videoElement, stream);
}

export async function stopLocalPreview(videoElement) {
    const stream = activeStreams.get(videoElement);
    if (stream) {
        stream.getTracks().forEach(track => track.stop());
        activeStreams.delete(videoElement);
    }

    if (videoElement) {
        if (videoElement.srcObject) {
            videoElement.pause?.();
        }
        videoElement.srcObject = null;
    }
}
