# Caching, offline queueing en PWA-overzicht

Dit document beschrijft hoe client-side caching, offline verwerking en Progressive Web App (PWA)-strategieën in de Nodo-frontend zijn ingericht. Het doel is om per bestand te verduidelijken welke data wordt gecachet, hoe dit gebeurt (LocalStorage, IndexedDB en de Cache API) en welke offline gedrag wordt ondersteund.

## Kernconcepten

- **LocalStorage** wordt gebruikt voor het cachen van API-responses zodat schermen bij netwerkfouten alsnog data kunnen tonen.
- **IndexedDB** bewaart een queue van uitgaande API-verzoeken die offline worden opgebouwd en later opnieuw worden verzonden.
- **Service workers** gebruiken de Cache API om statische assets en API-responses op te slaan en navigatie/offline fallback te verzorgen.

## LocalStorage-cachelaag

### `Offline/CacheService.cs`
- Biedt een generieke cachelaag bovenop `localStorage` via `CacheAsync<T>` en `TryGetCachedAsync<T>`. Caching faalt silent zodat de UX niet breekt wanneer `localStorage` onbeschikbaar is.【F:src/Rise.Client/Offline/CacheService.cs†L6-L45】
- Serialiseert payloads naar camelCase JSON en schrijft/leest met de gegeven sleutel. Bij ontbrekende of invalide data wordt `default` teruggegeven.【F:src/Rise.Client/Offline/CacheService.cs†L9-L45】

### `Offline/CacheKeys.cs`
- Definieert vaste cache-sleutels voor chat-overzichten, supervisor-chat en per-chatdetails via `GetChatCacheKey(int chatId)`. Deze sleutels worden gebruikt door de diensten die LocalStorage-cache schrijven/lezen.【F:src/Rise.Client/Offline/CacheKeys.cs†L3-L9】

## Caching van chatdata

### `Chats/ChatService.cs`
- **GetAllAsync**: na een succesvolle API-call worden alle chats gecachet onder `offline-cache:chats`. Bij een `HttpRequestException` wordt dezelfde key gebruikt om een eerder resultaat te tonen.【F:src/Rise.Client/Chats/ChatService.cs†L12-L40】
- **GetByIdAsync**: slaat het detail van een chat op onder een id-specifieke sleutel. Bij offline vallen we terug op (1) het detailcache, (2) de lijstcache (met lege messages) of tonen een offlinefout.【F:src/Rise.Client/Chats/ChatService.cs†L43-L89】
- **GetSupervisorChatAsync**: cachet en leest het supervisor-gesprek vanuit een vaste sleutel; offline wordt de cached versie geretourneerd.【F:src/Rise.Client/Chats/ChatService.cs†L92-L122】
- **CreateMessageAsync/QueueMessageAsync**: schrijven zelf niets naar cache, maar gebruiken de offline-queue om berichten op te slaan wanneer er geen verbinding is (zie IndexedDB-sectie).【F:src/Rise.Client/Chats/ChatService.cs†L124-L169】

## Caching van connecties

### `UserConnections/UserConnectionService.cs`
- **GetFriendsAsync**, **GetSuggestedFriendsAsync** en **GetFriendRequestsAsync** cacheen succesvolle responses per categorie (`offline-cache:connections:*`). Bij netwerkfouten wordt de corresponderende cache gelezen om toch data te tonen; ontbrekende cache leidt tot een offlinefoutboodschap.【F:src/Rise.Client/UserConnections/UserConnectionService.cs†L23-L111】
- Cachelogica gebruikt `localStorage.setItem/getItem` met camelCase JSON. Falende writes/reads worden genegeerd zodat de UI niet crasht.【F:src/Rise.Client/UserConnections/UserConnectionService.cs†L283-L342】
- Muterende acties (versturen/accepteren/afwijzen/annuleren/verwijderen) worden niet gecachet maar bij offline scenario’s in de queue geplaatst (zie onderstaande queue-sectie).【F:src/Rise.Client/UserConnections/UserConnectionService.cs†L114-L270】

## Offline queue in IndexedDB

### `Offline/OfflineQueueService.cs`
- Managed in .NET; laadt een JS-module en registreert een online-callback. Biedt API’s om operaties te enqueuen, op te halen, te verwijderen en de queue te verwerken zodra de client online is.【F:src/Rise.Client/Offline/OfflineQueueService.cs†L8-L198】
- `QueueOperationAsync` serialiseert het verzoek (URL, methode, headers en body) en delegeert opslag aan de JS-module. `ProcessQueueAsync` probeert queued items te versturen met de beveiligde API-client en verwijdert ze bij succes; fouten laten items staan voor een volgende poging.【F:src/Rise.Client/Offline/OfflineQueueService.cs†L44-L127】
- `WentOnline`-event kan door componenten gebruikt worden om status te verversen zodra de client opnieuw verbinding heeft.【F:src/Rise.Client/Offline/OfflineQueueService.cs†L20-L95】

### `wwwroot/js/offlineQueue.js`
- JS-implementatie van de queue bovenop IndexedDB (`rise-offline-queue`, store `operations`, versie 1). Bij schema-upgrade wordt de object store aangemaakt met auto-increment id.【F:src/Rise.Client/wwwroot/js/offlineQueue.js†L1-L18】
- `enqueueOperation` schrijft operaties met timestamp; `getOperations` leest alle items gesorteerd op id; `removeOperation` verwijdert op id. Deze functies worden door de C#-service aangeroepen via JS interop.【F:src/Rise.Client/wwwroot/js/offlineQueue.js†L21-L62】
- `registerOnlineCallback` haakt in op de browser `online`-event en roept de .NET-callback aan zodat de queue opnieuw geprobeerd wordt.【F:src/Rise.Client/wwwroot/js/offlineQueue.js†L64-L74】

## PWA- en Cache API-strategie

### `wwwroot/service-worker.js` (development)
- Gebruikt een vaste cache `nodo-dev-cache-v4` en precachet kerntoepassingsbestanden (HTML, manifest, styles, JS-helpers en iconen). Activatie verwijdert oudere dev-caches.【F:src/Rise.Client/wwwroot/service-worker.js†L1-L37】
- **Fetch-handling**:
  - Niet-GET of cross-origin verzoeken worden genegeerd.【F:src/Rise.Client/wwwroot/service-worker.js†L39-L47】
  - API-requests (`/api/`) gebruiken `handleApiRequest`: probeert netwerk, cachet succesvolle responses en valt bij fouten terug op een gecachte response of een 503 JSON-fout.【F:src/Rise.Client/wwwroot/service-worker.js†L49-L107】
  - Overige requests gebruiken een cache-first strategie met netwerk-update: cached antwoord indien beschikbaar, anders netwerk; bij offline navigaties wordt `offlineRoot` (startpagina) geserveerd.【F:src/Rise.Client/wwwroot/service-worker.js†L54-L75】

### `wwwroot/service-worker.published.js` (productie)
- Gebaseerd op Blazor’s offline service worker. Cache-naam bevat de asset-manifestversie zodat releases automatische cache-busting doen.【F:src/Rise.Client/wwwroot/service-worker.published.js†L1-L39】
- Precachet alle assets uit `service-worker-assets.js` plus extra statische resources (root, manifest, styles, helper-JS, iconen). Activatie ruimt oudere versies op.【F:src/Rise.Client/wwwroot/service-worker.published.js†L4-L42】
- **Fetch-strategieën**:
  - API-requests: netwerk-first met caching en fallback naar cache/503 JSON bij offline.【F:src/Rise.Client/wwwroot/service-worker.published.js†L55-L121】
  - Navigatieverzoeken: netwerk-first met cache-write; offline terugval naar rootdocument.【F:src/Rise.Client/wwwroot/service-worker.published.js†L60-L72】
  - Statische assets/scripts/styles: cache-first met netwerk-refresh zodat de cache wordt bijgewerkt wanneer online.【F:src/Rise.Client/wwwroot/service-worker.published.js†L74-L92】

## Wat wordt waar gecachet?

| Medium | Bestanden | Inhoud |
| --- | --- | --- |
| LocalStorage | `CacheService`, `ChatService`, `UserConnectionService` | Chatlijsten en -details, supervisor-chat, vrienden/suggesties/verzoeken lijsten. |
| IndexedDB (`rise-offline-queue`) | `OfflineQueueService`, `offlineQueue.js` | Uitgaande API-operaties (method, url, body, headers) die offline zijn aangemaakt. |
| Cache API (service worker) | `service-worker.js`, `service-worker.published.js` | Statische assets (HTML, CSS, JS, manifest, iconen), API responses en navigatiefallbacks per omgeving. |

## Gebruiksscenario’s

- **Gebruiker opent chats zonder netwerk**: de service worker levert gecachte assets; `ChatService` leest chats uit LocalStorage. Indien aanwezig wordt het chatdetail of een minimale representatie (zonder berichten) getoond.【F:src/Rise.Client/Chats/ChatService.cs†L43-L89】
- **Gebruiker verstuurt bericht of vriendactie offline**: de mutatie wordt via `OfflineQueueService` naar IndexedDB geschreven; bij terug online wordt de queue verwerkt en de server alsnog aangeroepen.【F:src/Rise.Client/Offline/OfflineQueueService.cs†L44-L127】【F:src/Rise.Client/wwwroot/js/offlineQueue.js†L38-L62】
- **Pagina-refresh offline**: service worker serveert gecachte statische assets en navigatiefallback (`offlineRoot`). API-calls vallen terug op LocalStorage-cache of geven een offlinefout.【F:src/Rise.Client/wwwroot/service-worker.js†L54-L107】【F:src/Rise.Client/Offline/CacheService.cs†L15-L45】

## Integratiepunten

- `Program.cs` registreert `OfflineQueueService` en start hem bij applicatieboot zodat de JS-module geladen wordt en queued requests meteen verwerkt kunnen worden.【F:src/Rise.Client/Program.cs†L111-L118】
- Componenten (bijv. chatpagina’s, vriendenoverzicht) kunnen op het `WentOnline`-event abonneren om data te verversen zodra connectiviteit terugkeert.【F:src/Rise.Client/Offline/OfflineQueueService.cs†L20-L95】

Met bovenstaande overzicht kun je snel herleiden welke data waar wordt opgeslagen, welke fallback er geldt per scenario en welke bestanden je moet aanpassen bij wijzigingen aan het offline gedrag.
