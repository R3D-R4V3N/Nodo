import io
import tempfile
from typing import Dict, List, Tuple

import numpy as np
from fastapi import FastAPI, File, HTTPException, UploadFile
from fastapi.responses import HTMLResponse
from nudenet import NudeDetector
from PIL import Image, UnidentifiedImageError

try:
    from nudenet import NudeClassifier
except ImportError:  # pragma: no cover - optional dependency in demo context
    NudeClassifier = None  # type: ignore[assignment]

app = FastAPI(title="Profile Picture Validation Demo")

detector = NudeDetector()
classifier = NudeClassifier() if NudeClassifier is not None else None

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

THRESHOLD = 0.6
CLASSIFIER_THRESHOLD = 0.6
CLASSIFIER_BLOCK_PATTERNS: Tuple[str, ...] = (
    "explicit",
    "nudity",
    "porn",
    "sexual",
    "breast",
    "buttock",
    "genital",
    "crotch",
    "lingerie",
    "underwear",
    "hentai",
)


def _format_reasons(hits: List[dict]) -> List[str]:
    reasons = []
    for hit in hits:
        label = hit.get("label", "Onbekend label")
        score = hit.get("score", 0.0)
        human_readable = LABEL_DESCRIPTIONS.get(label, label)
        source = hit.get("source")
        if source and source != "origineel":
            reasons.append(
                f"{human_readable} (score: {score:.2f}) — gevonden via {source}"
            )
        else:
            reasons.append(f"{human_readable} (score: {score:.2f})")
    return reasons


def _detect_blocking_content(image: Image.Image) -> List[Dict[str, object]]:
    width, _ = image.size
    variants: List[Tuple[str, Image.Image]] = [("origineel", image)]

    if width >= 128:
        variants.append(("gespiegeld", image.transpose(Image.FLIP_LEFT_RIGHT)))

    hits: List[Dict[str, object]] = []

    for variant_name, variant_image in variants:
        predictions = detector.detect(np.array(variant_image))
        for prediction in predictions:
            label = prediction.get("label")
            score = prediction.get("score", 0.0)
            if label in BLOCK_LABELS and score >= THRESHOLD:
                hit = dict(prediction)
                hit["source"] = variant_name
                if variant_name == "gespiegeld" and hit.get("box"):
                    left, top, right, bottom = hit["box"]
                    hit["box"] = [width - right, top, width - left, bottom]
                hits.append(hit)

    return hits


def _classify_blocking_content(image: Image.Image) -> List[Tuple[str, float]]:
    if classifier is None:
        return []

    with tempfile.NamedTemporaryFile(suffix=".jpg") as tmp:
        image.save(tmp, format="JPEG")
        tmp.flush()
        results = classifier.classify(tmp.name)

    if not results:
        return []

    class_scores = next(iter(results.values()))
    flagged: List[Tuple[str, float]] = []
    for label, score in class_scores.items():
        normalized = label.lower()
        if score >= CLASSIFIER_THRESHOLD and any(
            pattern in normalized for pattern in CLASSIFIER_BLOCK_PATTERNS
        ):
            flagged.append((label, float(score)))

    return flagged


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

    hits = _detect_blocking_content(image)
    classifier_hits = _classify_blocking_content(image)

    reasons = _format_reasons(hits)
    for label, score in classifier_hits:
        reasons.append(f"Classifier: {label} (score: {score:.2f})")

    return {
        "reject": bool(hits or classifier_hits),
        "hits": hits,
        "classifier_hits": classifier_hits,
        "reasons": reasons,
    }
