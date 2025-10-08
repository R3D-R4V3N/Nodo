let mediaRecorder = null;
let mediaStream = null;
let recordedChunks = [];

export async function startRecording() {
    if (mediaRecorder?.state === "recording") {
        return;
    }

    if (!navigator.mediaDevices?.getUserMedia) {
        throw new Error("Audio opnemen wordt niet ondersteund door deze browser.");
    }

    mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true });
    const mimeType = getSupportedMimeType();

    recordedChunks = [];
    mediaRecorder = new MediaRecorder(mediaStream, mimeType ? { mimeType } : undefined);

    mediaRecorder.ondataavailable = (event) => {
        if (event.data && event.data.size > 0) {
            recordedChunks.push(event.data);
        }
    };

    mediaRecorder.start();
}

export async function stopRecording() {
    if (!mediaRecorder) {
        return null;
    }

    if (mediaRecorder.state === "inactive") {
        cleanup();
        return null;
    }

    const stopPromise = new Promise((resolve) => {
        mediaRecorder.onstop = () => resolve();
    });

    mediaRecorder.stop();
    await stopPromise;

    const blob = new Blob(recordedChunks, { type: mediaRecorder.mimeType });
    cleanup();

    if (!blob.size) {
        return null;
    }

    const arrayBuffer = await blob.arrayBuffer();
    const base64 = await bufferToBase64(arrayBuffer);

    let durationSeconds = 0;
    try {
        const audioContext = new AudioContext();
        const audioBuffer = await audioContext.decodeAudioData(arrayBuffer.slice(0));
        durationSeconds = audioBuffer.duration;
        await audioContext.close();
    } catch (error) {
        console.warn("Kon audio niet decoderen", error);
    }

    return {
        dataUrl: `data:${blob.type};base64,${base64}`,
        durationSeconds
    };
}

export function disposeRecorder() {
    if (mediaRecorder?.state === "recording") {
        mediaRecorder.stop();
    }

    cleanup();
}

function cleanup() {
    if (mediaRecorder) {
        mediaRecorder.ondataavailable = null;
        mediaRecorder.onstop = null;
        mediaRecorder = null;
    }

    if (mediaStream) {
        mediaStream.getTracks().forEach((track) => track.stop());
        mediaStream = null;
    }

    recordedChunks = [];
}

function getSupportedMimeType() {
    const possibleTypes = [
        "audio/webm;codecs=opus",
        "audio/ogg;codecs=opus",
        "audio/webm"
    ];

    for (const type of possibleTypes) {
        if (MediaRecorder.isTypeSupported(type)) {
            return type;
        }
    }

    return null;
}

async function bufferToBase64(buffer) {
    let binary = "";
    const bytes = new Uint8Array(buffer);
    const chunkSize = 0x8000;

    for (let i = 0; i < bytes.length; i += chunkSize) {
        const chunk = bytes.subarray(i, Math.min(i + chunkSize, bytes.length));
        binary += String.fromCharCode(...chunk);
    }

    return btoa(binary);
}
