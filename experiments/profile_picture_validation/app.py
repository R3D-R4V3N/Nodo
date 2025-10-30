from fastapi import FastAPI, File, UploadFile, Request
from fastapi.responses import HTMLResponse
from fastapi.templating import Jinja2Templates
from nudenet import NudeDetector
from PIL import Image
import io
from typing import List, Dict

app = FastAPI(title="Profile Picture Validation Demo")

templates = Jinja2Templates(directory="experiments/profile_picture_validation/templates")

# Instantiate the detector once. The model is downloaded automatically the first time.
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
THRESHOLD = 0.7


def evaluate_image_bytes(data: bytes) -> Dict[str, object]:
    """Run NudeNet on the supplied image bytes and return the decision."""
    image = Image.open(io.BytesIO(data)).convert("RGB")
    predictions: List[Dict[str, object]] = detector.detect(image)
    hits = [p for p in predictions if p["label"] in BLOCK_LABELS and p["score"] >= THRESHOLD]
    return {
        "reject": len(hits) > 0,
        "hits": hits,
    }


@app.get("/profilepicturevalidation", response_class=HTMLResponse)
async def get_profile_picture_validation(request: Request):
    """Render the upload form."""
    return templates.TemplateResponse(
        "profile_picture_validation.html",
        {
            "request": request,
            "result": None,
        },
    )


@app.post("/profilepicturevalidation", response_class=HTMLResponse)
async def post_profile_picture_validation(request: Request, file: UploadFile = File(...)):
    data = await file.read()
    decision = evaluate_image_bytes(data)

    if decision["reject"]:
        verdict = "Afgewezen"
        message = "Deze foto is niet toegestaan: " + ", ".join(
            f"{hit['label']} ({hit['score']:.2f})" for hit in decision["hits"]
        )
        status = "error"
    else:
        verdict = "Toegestaan"
        message = "Deze foto voldoet aan de richtlijnen."
        status = "success"

    return templates.TemplateResponse(
        "profile_picture_validation.html",
        {
            "request": request,
            "result": {
                "verdict": verdict,
                "message": message,
                "status": status,
            },
        },
    )


@app.post("/moderate")
async def moderate(file: UploadFile = File(...)):
    data = await file.read()
    decision = evaluate_image_bytes(data)
    return decision


if __name__ == "__main__":
    import uvicorn

    uvicorn.run("experiments.profile_picture_validation.app:app", host="0.0.0.0", port=8000, reload=True)
