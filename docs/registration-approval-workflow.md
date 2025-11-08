# Registratie- en goedkeuringsworkflow

Deze handleiding beschrijft de volledige stroom voor een nieuwe gebruiker die zich registreert en voor begeleiders die aanvragen beheren.

## Overzicht

1. **Gebruiker** vult het registratieformulier in, kiest een organisatie en dient de aanvraag in.
2. **Begeleiders** van de gekozen organisatie bekijken de openstaande aanvragen.
3. Een begeleider wijst zichzelf of een collega toe aan de aanvraag en keurt deze goed.
4. De gebruiker kan daarna met zijn oorspronkelijke e-mailadres en wachtwoord inloggen in Nodo.

## Workflow voor gebruikers

1. Navigeer naar [`/register`](../src/Rise.Client/Identity/Register.razor) via de knop op de login- of homepagina.
2. Vul alle verplichte velden in, waaronder voornaam, achternaam, geboortedatum, geslacht, een profielfoto, het e-mailadres, wachtwoord en selecteer een organisatie uit de dropdownlijst. Het formulier laat pas inzenden toe wanneer een organisatie gekozen is.
3. Klik op **Registreren**. Het systeem slaat de aanvraag op als "in behandeling" en toont een bevestigingsscherm dat aangeeft dat een begeleider de aanvraag moet goedkeuren voordat je kunt inloggen.
4. Wacht tot je begeleider de aanvraag heeft verwerkt. Je ontvangt geen automatische e-mail, maar zodra de aanvraag goedgekeurd is kan je inloggen via [`/login`](../src/Rise.Client/Identity/Register.razor).

## Workflow voor begeleiders

1. Meld je aan als begeleider of administrator.
2. Open de pagina [`/registrations/pending`](../src/Rise.Client/Pages/Registrations/PendingRegistrations.razor) vanuit de navigatie. Deze pagina is enkel toegankelijk voor rollen `Supervisor` en `Administrator`.
3. Op de pagina zie je een lijst met openstaande aanvragen voor jouw organisatie, inclusief naam, profielfoto, e-mailadres, geboortedatum, geslacht, geselecteerde organisatie en datum van indienen.
4. Kies in de dropdown **Begeleider** jezelf of een collega aan wie de nieuwe gebruiker wordt gekoppeld.
5. Klik op **Keur goed** om de aanvraag te verwerken. Bij succes verdwijnt de aanvraag uit de lijst en wordt een bevestigingsmelding getoond.
6. De gebruiker ontvangt automatisch een account, wordt gekoppeld aan de gekozen begeleider en kan vanaf dat moment inloggen. De opgegeven persoonlijke gegevens verschijnen meteen op de profielpagina.

## Foutafhandeling

- Wanneer gegevens ontbreken of ongeldig zijn, toont het formulier foutmeldingen en kan de aanvraag niet worden verstuurd.
- Indien een begeleider probeert te keuren zonder een collega te selecteren, verschijnt de melding "Selecteer eerst een begeleider".
- Alle systeemfouten worden gelogd en getoond als algemene foutmeldingen zodat je weet dat je het later opnieuw kunt proberen.

## Veelgestelde vragen

**Kan ik meerdere aanvragen voor hetzelfde e-mailadres indienen?**
Nee. Het systeem weigert dubbele aanvragen en geeft een melding dat er al een lopende aanvraag bestaat.

**Wat als de registratie al is aangemaakt in Identity?**
Tijdens het goedkeuringsproces controleert het systeem of er al een Identity-account met hetzelfde e-mailadres bestaat. Zo ja, dan toont het een fout en wordt het account niet opnieuw aangemaakt.

**Wat gebeurt er na goedkeuring?**
Het systeem maakt automatisch een gebruikersprofiel aan, koppelt de gebruiker aan de geselecteerde organisatie, voegt standaard welkomstberichten toe aan het chatlogboek en bewaart de aangeleverde persoonsgegevens (naam, geboortedatum, geslacht en profielfoto).
