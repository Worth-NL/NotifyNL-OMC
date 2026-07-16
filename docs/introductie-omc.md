# Introductie op Output Management (OMC)

Je kunt al prima notificaties versturen voordat je de NotifyNL API integreert — bijvoorbeeld door een bulkbestand te uploaden naar NotifyNL met de gegevens die je wilt notificeren.

De echte meerwaarde van NotifyNL krijg je echter pas als je het integreert in je processen. Voor gemeenten die beschikken over ZGW API's is dit eenvoudiger gemaakt door middel van het **Output Management Component (OMC)**. Dit component kun je deployen in je eigen infrastructuur en het verzorgt in één keer de integratie met de ZGW-diensten die hieronder vermeld staan.

Wanneer het OMC draait, reageert het op events die plaatsvinden waarbij een notificatie nodig is — bijvoorbeeld een aangemaakte zaak of taak.

Het OMC is **stateless**: je kunt dus zoveel instanties draaien als je nodig hebt. Bovendien zorgt het OMC ervoor dat Notify geen toegang nodig heeft tot je klant- of zaakregistraties.

Het OMC ontvangt de afleverstatus van notificaties terug vanuit Notify en schrijft die naar de contactmomenten API. Hierdoor kan aflevergeschiedenis zichtbaar worden gemaakt in de MijnOmgeving van je gemeente en is er bovendien een afleverbewijs van afgeleverde notificaties.

---

## Ondersteunde ZGW / Open Services

Het OMC werkt op dit moment samen met de volgende ZGW / Open Services:

- ZGW \| Open Notificaties (Web API service)
- ZGW \| Open Zaak (Web API service)
- ZGW \| Open Klant (Web API service)
- ZGW \| Besluiten (Web API service)
- ZGW \| Objecten (Web API service)
- ZGW \| ObjectTypen (Web API service)
- ZGW \| Klantinteracties (Web API service)

---

## Ondersteunde scenario's

Het OMC ondersteunt zes scenario's voor twee typen klanten, die je kunt koppelen aan NotifyNL-templates in de configuratie:

**Scenario's:**
1. Nieuwe zaak (Zaak aangemaakt) — een nieuwe zaak is aangemaakt door of voor een burger
2. Zaak gewijzigd — een bestaande zaak heeft een statusupdate ontvangen
3. Zaak afgesloten — een zaak is om welke reden dan ook afgesloten
4. Nieuw besluit (Besluit aangemaakt) — een besluit is genomen dat een burger raakt
5. Nieuwe taak (Taak aangemaakt) — een taak is toegewezen aan een burger
6. Nieuw bericht (Bericht aangemaakt) — een nieuw bericht is aangemaakt voor de burger om te lezen in de berichtenbox

**Klanttypen:**
1. Een echte burger — iemand met een BSN
2. Een vertegenwoordiger — iemand die geregistreerd staat als contactpersoon voor een bedrijf (B.V., ZZP of anders)
3. Betrokken partijen (in ontwikkeling) — iemand die is belast met de zorg voor iemand anders

---

## Meer informatie

- [OMC uitrollen](aan-de-slag/uitrollen.md)
- [Configuratie](configuratie/omgevingsvariabelen.md)
- [Scenario's](werkwijzen/scenarios/overzicht.md)
