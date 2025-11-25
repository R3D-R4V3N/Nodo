# PWA & Offline Functionaliteit

Dit document beschrijft de huidige PWA- en offline-architectuur van de Nodo chatclient: wat er gecachet wordt, hoe IndexedDB wordt gebruikt, welke requests uitgesteld kunnen worden en welke beperkingen er nog zijn.

## Overzicht
- **Service worker caching** voor runtime assets, API-antwoorden en Blazor framework-bestanden (inclusief auth-endpoints) zodat de app sneller start en basisnavigatie beschikbaar blijft zonder netwerk.
- **IndexedDB caches** voor chats, berichten, contacten, organisaties en de laatst bekende sessie zodat de shell kan renderen met recente data tijdens offline gebruik.
- **Offline wachtrij** die berichten (met optionele bijlage) opslaat, placeholders toont in de UI en ze automatisch opnieuw verstuurt zodra er verbinding is.
- **Gebufferde blob-opslag** voor bijlagen zodat bestanden lokaal bewaard blijven tot ze succesvol zijn geüpload.

## Service worker caching
De service worker draait zowel in development (`service-worker.js`) als in productie (`service-worker.published.js`) en gebruikt dezelfde strategieën:

- **Precache bij installatie**
  - Basispagina (`/` en `index.html`), manifest, stijlen, favicon en PWA-iconen.
  - Blazor bootstrapping en framework-resources (`_framework/*`), inclusief het dynamisch inladen van de bestanden uit `blazor.boot.json`.
  - Auth-gerelateerde endpoints (`api/identity/accounts/info` en `api/users/current`) zodat de login-status ook offline kan worden bepaald zolang de cache recent is.
- **Runtime caching**
  - Navigatie: slaat de laatst bezochte shell op; toont een eenvoudige offline fallback als er nog niets gecachet is.
  - Statische assets en framework-bestanden: cache-first met back-up naar netwerk; ververst cache wanneer netwerk beschikbaar is.
  - API-responses: cache-first met korte TTL (60s) voor reeds gecachte antwoorden; anders network-first en een JSON 503-fallback als er niets beschikbaar is.
- **Cachebeheer**
  - Cache-namen zijn versioned (`nodo-cache-*`) en oude caches worden opgeschoond tijdens `activate`.

## IndexedDB caches (app-data)
`CacheStoreService` gebruikt de `rise-cache` IndexedDB database en houdt de volgende stores bij:

- `chats`: chat-overzichten
- `messages-{chatId}`: berichten per chat
- `contacts`: chatcontacten
- `contact-profiles`: profiel van ingelogde gebruiker
- `organizations`: organisaties
- `auth-sessions`: laatst bekende sessie (incl. accountinfo + gebruiker)

### Gebruik in de app
- **Warming bij app-start**: `App.razor.cs` leest de cached sessie (max 12u oud) en chats in zodat de shell alvast kan renderen, zelfs zonder netwerk.
- **Auth state**: `CookieAuthenticationStateProvider` gebruikt de cached sessie als fallback wanneer de auth-endpoints offline of tijdelijk onbereikbaar zijn.
- **Chatdata**: `ChatService` schrijft succesvolle API-responses weg in de cache en leest ze terug wanneer netwerkcalls falen, inclusief berichten per chat en contactinfo.

## Offline wachtrij en bijlagen
De offline wachtrij gebruikt de `rise-offline-queue` database met twee stores: `operations` voor uit te voeren requests en `blobs` voor binaire bijlagen.

### Queuegedrag
- **Opslaan**: bij het falen van een POST naar `/api/chats/{chatId}/messages` wordt de payload (incl. headers en optionele bijlage-metadata) gequeued samen met een client-side `ClientMessageId`.
- **Proces**: de queue draait automatisch elke 30s wanneer het tabblad zichtbaar is én de browser online is, of direct wanneer een `online` event binnenkomt.
- **Verzenden**: queued items worden met dezelfde HTTP-methode/content-type verstuurd; bijlagen worden als multipart-formdata opgebouwd uit de opgeslagen blob bytes.
- **Afhandeling**:
  - Succes: queue-item verwijderd, bijlagen opgeruimd en de teruggekeerde server-`MessageDto` wordt via `MessageFlushed` event naar de UI gepusht.
  - Permanente fout (4xx behalve 408/429): item en eventuele blob worden verwijderd en er wordt een waarschuwing getoond.

### Bijlagenopslag
- Blobs worden opgeslagen met een `blobKey` in de `blobs` store en omgezet naar een lokale `blob:` URL om te tonen in de UI.
- Max. bijlagegrootte is 10 MB (geforceerd bij het inladen van de file).

### UI-integratie
- Nieuwe berichten krijgen lokaal een placeholder met `IsPending = true`, inclusief bijlage-preview via de blob-URL.
- Pending berichten kunnen geannuleerd worden (queue-item wordt verwijderd).
- Zodra de server het bericht bevestigt (via realtime hub of queue-antwoord) wordt de placeholder vervangen door de definitieve serverversie.

## Wat werkt offline
- Starten van de app en navigeren naar eerder bezochte pagina's dankzij precache en runtime cache.
- Vaststellen van login-status op basis van cached sessiegegevens (tot 12u oud).
- Tonen van chatlijsten, contactprofielen, organisaties en eerder geladen berichten uit IndexedDB.
- Berichten (tekst/spraak/bijlage) opstellen en lokaal in de queue zetten; bijlagen blijven lokaal beschikbaar tot upload.

## Wat wordt uitgesteld tot online
- Nieuwe API-calls buiten de gecachede GET-antwoorden (bijv. nieuwe chats ophalen na TTL, profielfoto's die niet eerder zijn geladen).
- Verzenden van gequeue'de berichten en het uploaden van bijlagen naar de server.
- Eventuele token/identiteitsrefresh: auth wordt pas geverifieerd wanneer de auth-endpoints opnieuw bereikbaar zijn.

## Bekende beperkingen / aandachtspunten
- Eerste load vereist online toegang zodat de service worker en basisassets kunnen cachen.
- Auth-cache heeft een TTL van 12 uur; daarna is opnieuw netwerk nodig om de sessie te bevestigen.
- API-cache gebruikt alleen GET en een korte TTL; voor consistente data is een nieuwe online fetch nodig.
- Queue retryt lineair (elke 30s) zolang de pagina open en zichtbaar is; er is geen Background Sync wanneer de tab gesloten is.
- Bijlagen groter dan 10 MB worden geweigerd en niet gequeued.
- De offline fallback-pagina is eenvoudig en toont geen app-shell als er nog geen cache bestaat.
