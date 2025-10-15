# Profielfoto moderatie testen

Deze gids legt uit hoe je de Razor testpagina `/profilepicturetesting` gebruikt om een profielfoto te controleren met de NudeNet moderatieservice.

## Voorwaarden

- .NET 9 SDK geïnstalleerd.
- Internettoegang tijdens de eerste run zodat het NudeNet-model kan worden gedownload door de `NudityDetector`-bibliotheek.
- Deze repository lokaal gekloond.

## Applicatie starten

### Via de IDE

1. Open `Rise.sln` in Visual Studio, Rider of Visual Studio Code.
2. Stel `Rise.Server` in als opstartproject.
3. Start de applicatie (F5 of `Run`). De server luistert standaard op `https://localhost:5001`.

### Via de command-line

```bash
dotnet watch --project src/Rise.Server/Rise.Server.csproj --no-hot-reload
```

- De eerste keer dat je een afbeelding laat modereren downloadt de `NudityDetector` automatisch het NudeNet-model naar de cachemap van de gebruiker. Dit kan enkele minuten duren.

## Inloggen

De testpagina maakt deel uit van de beveiligde client. Meld je daarom aan met één van de aanwezige testaccounts (wachtwoord `A1b2C3!`). Bijvoorbeeld:

- `emma.supervisor@nodo.chat`
- `jonas.supervisor@nodo.chat`

## De testpagina gebruiken

1. Navigeer na het inloggen naar `https://localhost:5001/profilepicturetesting`.
2. Klik op **Bestand kiezen** en selecteer een afbeelding (`.jpg`, `.png`, ...).
3. Wacht tot de upload en validatie afgerond is. Bij succes verschijnt een alert met de boodschap dat de foto is goedgekeurd. Indien de foto wordt afgekeurd of er een fout optreedt, toont de pagina een alert met de reden.

## Problemen oplossen

- **"Er ging iets mis bij het uploaden"** – Controleer of de server nog draait en dat je sessie niet verlopen is.
- **"Technische fout tijdens het uploaden"** – Bekijk de serverlogs. Mogelijk kon het NudeNet-model niet gedownload worden of ontbrak schrijfrecht op de tijdelijke map.
- **Modeldownload blijft hangen** – Zorg voor stabiele internetverbinding en herstart de applicatie. Het modelbestand wordt in `%LOCALAPPDATA%\\NudeNet` (Windows) of `~/.local/share/NudeNet` (Linux/macOS) opgeslagen.

Zo kun je snel evalueren of de moderatie-service correct werkt voor de gekozen profielfoto.
