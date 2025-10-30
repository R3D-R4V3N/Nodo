# Profielfoto validatie demo

Deze map bevat een minimale FastAPI-demo waarmee je het Nudity-detectiemodel van [NudeNet](https://github.com/notAI-tech/NudeNet) kunt testen voor profielfoto-validatie.

## Vereisten

```bash
pip install fastapi uvicorn pillow nudenet
```

Het NudeNet-modelbestand wordt automatisch gedownload bij de eerste start.

## Applicatie starten

```bash
uvicorn app:app --reload
```

Daarna is de demo bereikbaar op [http://localhost:8000/profilepicturevalidation](http://localhost:8000/profilepicturevalidation).

Upload een afbeelding om de validatie te testen. Je krijgt een melding of de afbeelding is toegestaan, of geweigerd met de reden(en).
