import io
from typing import List

import numpy as np
from fastapi import FastAPI, File, HTTPException, UploadFile
from fastapi.responses import HTMLResponse
from nudenet import NudeDetector
from PIL import Image, UnidentifiedImageError

app = FastAPI(title="Profile Picture Validation Demo")

detector = NudeDetector()

BLOCK_LABELS = {
    "EXPOSED_ANUS",
    "EXPOSED_BREAST_F",
    "EXPOSED_BREAST_M",
    "EXPOSED_GENITALIA_F",
    "EXPOSED_GENITALIA_M",
    "EXPOSED_BUTTOCKS",
    "COVERED_GENITALIA_F",
    "COVERED_GENITALIA_M",
    "COVERED_BUTTOCKS",
    "COVERED_BREAST_F",
}

LABEL_DESCRIPTIONS = {
    "EXPOSED_ANUS": "Naakt - zichtbare anus",
    "EXPOSED_BREAST_F": "Naakt - vrouwelijke borst",
    "EXPOSED_BREAST_M": "Naakt - mannelijke borst",
    "EXPOSED_GENITALIA_F": "Naakt - vrouwelijke geslachtsdelen",
    "EXPOSED_GENITALIA_M": "Naakt - mannelijke geslachtsdelen",
    "EXPOSED_BUTTOCKS": "Naakt - zichtbare billen",
    "COVERED_GENITALIA_F": "Bikini/ondergoed - vrouwelijke geslachtsdelen bedekt",
    "COVERED_GENITALIA_M": "Bikini/ondergoed - mannelijke geslachtsdelen bedekt",
    "COVERED_BUTTOCKS": "Bikini/ondergoed - bedekte billen",
    "COVERED_BREAST_F": "Bikini/ondergoed - vrouwelijke borst bedekt",
}

THRESHOLD = 0.7


def _format_reasons(hits: List[dict]) -> List[str]:
    reasons = []
    for hit in hits:
        label = hit.get("label", "Onbekend label")
        score = hit.get("score", 0.0)
        human_readable = LABEL_DESCRIPTIONS.get(label, label)
        reasons.append(f"{human_readable} (score: {score:.2f})")
    return reasons


@app.get("/profilepicturevalidation", response_class=HTMLResponse)
async def profile_picture_validation_page() -> str:
    return """
    <!DOCTYPE html>
    <html lang=\"nl\">
    <head>
        <meta charset=\"UTF-8\" />
        <title>Profielfoto validatie demo</title>
        <style>
            body { font-family: Arial, sans-serif; margin: 2rem; }
            form { display: flex; flex-direction: column; gap: 1rem; max-width: 320px; }
            .result { margin-top: 1.5rem; font-weight: bold; }
        </style>
    </head>
    <body>
        <h1>Profielfoto validatie demo</h1>
        <p>Selecteer een afbeelding en klik op \"Controleer\" om het model te testen.</p>
        <form id=\"upload-form\">
            <input type=\"file\" id=\"file-input\" name=\"file\" accept=\"image/*\" required />
            <button type=\"submit\">Controleer</button>
        </form>
        <div id=\"result\" class=\"result\"></div>
        <script>
        const form = document.getElementById('upload-form');
        const fileInput = document.getElementById('file-input');
        const result = document.getElementById('result');

        form.addEventListener('submit', async (event) => {
            event.preventDefault();
            result.textContent = 'Bezig met controleren...';
            const file = fileInput.files[0];
            if (!file) {
                alert('Selecteer eerst een afbeelding.');
                result.textContent = '';
                return;
            }

            const formData = new FormData();
            formData.append('file', file);

            try {
                const response = await fetch('/profilepicturevalidation/moderate', {
                    method: 'POST',
                    body: formData
                });

                if (!response.ok) {
                    const error = await response.json();
                    throw new Error(error.detail || 'Onbekende fout');
                }

                const data = await response.json();

                if (data.reject) {
                    alert(`Geweigerd: ${data.reasons.join(', ')}`);
                    result.textContent = `Geweigerd: ${data.reasons.join(', ')}`;
                    result.style.color = '#c0392b';
                } else {
                    alert('Toegestaan: Geen problemen gevonden.');
                    result.textContent = 'Toegestaan: Geen problemen gevonden.';
                    result.style.color = '#27ae60';
                }
            } catch (err) {
                alert(`Fout: ${err.message}`);
                result.textContent = `Fout: ${err.message}`;
                result.style.color = '#c0392b';
            }
        });
        </script>
    </body>
    </html>
    """


@app.post("/profilepicturevalidation/moderate")
async def moderate(file: UploadFile = File(...)):
    contents = await file.read()
    try:
        image = Image.open(io.BytesIO(contents)).convert("RGB")
    except UnidentifiedImageError as exc:
        raise HTTPException(status_code=400, detail="Kon de afbeelding niet openen") from exc

    predictions = detector.detect(np.array(image))
    hits = [p for p in predictions if p.get("label") in BLOCK_LABELS and p.get("score", 0.0) >= THRESHOLD]

    reasons = _format_reasons(hits)

    return {"reject": len(hits) > 0, "hits": hits, "reasons": reasons}
