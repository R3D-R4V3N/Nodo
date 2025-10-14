# Seed data overzicht

Dit document beschrijft alle standaardgegevens die `DbSeeder` invult voor de chatapp.
Op die manier kan je snel zien welke accounts bestaan, wie met wie bevriend is en
welke gesprekken al voorbeeldberichten bevatten.

## Snelstart

- Standaard wachtwoord voor alle accounts: **Nodo.1**.
- Rollen: `Administrator`, `Supervisor` en `User`.
- Accounts zonder gekoppeld profiel (handig voor beheer):
  - `admin@example.com` (Administrator)
  - `admin@nodo.chat` (Administrator)

## Accounts met profiel

### Supervisors

| Voornaam | Achternaam    | E-mail                         | Beschrijving |
| ---      | ---           | ---                            | --- |
| Super    | Visor         | supervisor@example.com         | Here to help you. |
| Emma     | Claes         | emma.supervisor@nodo.chat      | Coach voor dagelijkse structuur en zelfvertrouwen. |
| Jonas    | Van Lint      | jonas.supervisor@nodo.chat     | Helpt bij plannen en houdt wekelijks groepsmomenten. |
| Ella     | Vervoort      | ella.supervisor@nodo.chat      | Creatieve begeleider voor beeldende therapie. |

### Chatters

| Voornaam | Achternaam   | E-mail                | Korte bio |
| ---      | ---          | ---                   | --- |
| John     | Doe          | user1@example.com     | Houdt van katten en rustige gesprekken. |
| Stacey   | Willington   | user2@example.com     | Deelt graag verhalen over haar hulphond. |
| Noor     | Vermeulen    | noor@nodo.chat        | Praat graag over muziek en wil nieuwe vrienden maken. |
| Milan    | Peeters      | milan@nodo.chat       | Zoekt iemand om samen over games te praten. |
| Lina     | Jacobs       | lina@nodo.chat        | Vindt het fijn om vragen te kunnen stellen in een veilige omgeving. |
| Kyandro  | Voet         | kyandro@nodo.chat     | Helpt vaak bij technische vragen en deelt programmeertips. |
| Jasper   | Vermeersch   | jasper@nodo.chat      | Vindt het leuk om te discussiëren over technologie en innovatie. |
| Bjorn    | Van Damme    | bjorn@nodo.chat       | Praat graag over sport en houdt van teamwork. |
| Thibo    | De Smet      | thibo@nodo.chat       | Is nieuwsgierig en stelt vaak interessante vragen. |
| Saar     | Vandenberg   | saar@nodo.chat        | Deelt graag foto's van haar tekeningen. |
| Yassin   | El Amrani    | yassin@nodo.chat      | Leert zelfstandig koken en zoekt tips van vrienden. |
| Lotte    | De Wilde     | lotte@nodo.chat       | Wordt blij van dansen en deelt positieve boodschappen. |
| Amina    | Karim        | amina@nodo.chat       | Houdt van creatieve projecten en begeleidt graag groepsspelletjes. |

## Vriendschappen en connecties

### Bevestigde vrienden

| Persoon A | Persoon B | Context |
| --- | --- | --- |
| Noor | Milan | Ontmoetten elkaar in de game-avond en spreken vaak af. |
| Kyandro | Jasper | Werken samen aan technische projecten. |
| Bjorn | Thibo | Sportmaatjes die elkaar motiveren. |
| Saar | Yassin | Delen tips rond koken en dagelijkse routines. |
| Lotte | Amina | Organiseren samen creatieve activiteiten. |

### Openstaande verzoeken

| Aanvrager | Ontvanger | Reden |
| --- | --- | --- |
| Noor | Lina | Noor vraagt ondersteuning bij vragen in de community. |
| Milan | Saar | Milan nodigt Saar uit voor de gamegroep. |
| John | Bjorn | John zoekt een sportbuddy. |
| Stacey | Noor | Stacey wil Noor beter leren kennen. |
| Amina | Kyandro | Amina zoekt programmeertips voor een nieuw project. |

## Gesprekken en voorbeeldberichten

De seeder maakt vier chats aan. In een lege database krijgen ze de onderstaande ID's in oplopende volgorde.

| Chat-ID | Naam (conceptueel)         | Deelnemers (voornaam)                         | Waar gaat het over? |
| ---     | ---                        | ---                                           | --- |
| 1       | Individuele check-in       | Noor, Emma                                    | Voorbereiden op een spannende dag met rustgevende tips. |
| 2       | Vrijdagavond groep         | Milan, Saar, Yassin, Jonas                    | Afstemmen van een gezellige online game-avond met begeleiding. |
| 3       | Creatieve hoek             | Lotte, Amina, Ella                            | Ideeën uitwisselen voor creatieve projecten en collages. |
| 4       | Technische hulplijn        | Jasper, Kyandro, Emma                         | Vragen over het gebruik van de app en apparatuur. |

Gebruik deze tabel om snel te controleren welke berichten bij welk scenario horen. Elk gesprek bevat meerdere berichten die
verschillende stemmen laten horen (deelnemer ↔ supervisor) zodat je flows kan testen.

## Supervisors in een oogopslag

- Emma begeleidt de individuele check-in (chat 1) en de technische hulplijn (chat 4).
- Jonas ondersteunt de vrijdagavondgroep (chat 2).
- Ella begeleidt de creatieve hoek (chat 3).
- Super Visor (`supervisor@example.com`) kan overal bijspringen maar heeft nog geen vaste chat in de seed.

