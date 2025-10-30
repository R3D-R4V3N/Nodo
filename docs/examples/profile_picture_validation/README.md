# Profielfoto validatie demo

Deze map bevat een minimale FastAPI-demo waarmee je het Nudity-detectiemodel van [NudeNet](https://github.com/notAI-tech/NudeNet) kunt testen voor profielfoto-validatie.

## Stap 1: naar de juiste map gaan

Open een terminal in de root van de repository en navigeer vervolgens naar de map met de demo:

```bash
cd docs/examples/profile_picture_validation
```

Alle onderstaande commando's gaan ervan uit dat je je in deze map bevindt.

## Stap 2: vereisten installeren

Installeer de afhankelijkheden (bij voorkeur in een virtuele omgeving) met:

```bash
pip install fastapi uvicorn pillow nudenet
```

Het NudeNet-modelbestand wordt automatisch gedownload bij de eerste start.

## Stap 3: applicatie starten

Start daarna de ontwikkelserver:

```bash
uvicorn app:app --reload
```

Daarna is de demo bereikbaar op [http://localhost:8000/profilepicturevalidation](http://localhost:8000/profilepicturevalidation).

Upload een afbeelding om de validatie te testen. Je krijgt een melding of de afbeelding is toegestaan, of geweigerd met de reden(en).
