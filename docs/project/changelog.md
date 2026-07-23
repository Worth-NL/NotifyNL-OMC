# Changelog

---

## v2.1.0

- [MijnOverheid](../integraties/mijnoverheid.md)-integratie toegevoegd: het eindpunt `POST /Events/MijnZaken` normaliseert inkomende ZGW CloudEvents/NotificationEvents en stuurt relevante zaakgebeurtenissen door naar MijnOverheid, met whitelist- en natuurlijk-persoonfiltering
- Ondersteuning toegevoegd voor de gebeurtenissen "zaak geopend" en "zaak verwijderd" (voorheen werd alleen "zaak gemuteerd" herkend vanuit NotificationEvent-payloads)
- Diverse correcties aan de MijnZaken-payloadverwerking, veldmapping en tijdstipnauwkeurigheid, zodat het eindpunt end-to-end correct werkt

---

## v2.0.0 ⚠️ Breaking changes

- Bijgewerkt naar .NET 10
- PostGuard-integratie geïmplementeerd voor het versturen van versleutelde PDF's te openen met Yivi-wallets
- Alle `Zhv` eindpuntreferenties hernoemd naar `Zgw` in omgevingsvariabelen
- Werkwijze versie 2 is nu de standaard (`OMC_FEATURE_WORKFLOW_VERSION=2`)
- `ZGW_AUTH_KEY_OPENKLANT` is nu vereist voor werkwijze v2
- KVK-ondersteuning toegevoegd naast BSN
- Brieftemplates (`NOTIFY_TEMPLATEID_LETTER_*`) toegevoegd

---

## v1.17.19

- Voegt aangepaste GovUkNotify-client toe voor het accepteren van extra velden bij het versturen van brieven; geeft `202` terug bij bevestiging zonder referentie

## v1.17.18

- Voegt Keycloak-logica en BRP / Haal Centraal-integratie toe met uitgebreide logging voor testen

## v1.17.17

- Voegt test-eindpunt toe voor het versturen van brieven via NotifyNL

## v1.17.16

- Verwijst Object opnieuw naar Expoints-specifieke payload

## v1.17.15

- Nieuwe KTO-implementatie waarbij KTO als notificatie wordt behandeld

## v1.17.14

- Eerste opschoning: logt uitgaande API-aanroepen naar ZGW en hun responses in Sentry; haalt zaakstatus en type niet meer tweemaal op; controleert notificatieverwachting eerder in zaakscenario's

## v1.17.13

- Bugfix: vangt fouten van OpenKlant op en toont ze

## v1.17.12

- Bugfix: stelt relevante services in op scoped scope om race conditions te voorkomen

## v1.17.11

- Voorkomt mogelijke race conditions door QueryBase niet te gebruiken

## v1.17.10

- Voegt `CaseResultType` toe en neemt dit op in `NotifyData` voor het scenario Zaak afgesloten

## v1.17.9

- Bugfix: als de initiatorrol geen BSN heeft, wordt niet geprobeerd partijen op te vragen (OpenKlant geeft in dat geval een lijst terug)

## v1.17.8

- Bugfix: verwijdert KTO-uitvoering uit `ITelemetryService`

## v1.17.7

- Bugfix: vergelijking vond plaats op beschrijving in plaats van referentie; `ActorId` toegevoegd

## v1.17.6

- Wijzigt OpenKlant-variabelen in `appsettings.json` naar: `"CodeObjectType": "zaak"`, `"CodeRegister": "open-zaak"`, `"CodeObjectTypeId": "uuid"` conform ZGW-standaarden

## v1.17.5

- Bugfix: voegt JSON-escape-logica toe bij het opbouwen van `ContactMomentenJsonBody`

## v1.17.4

- Voegt notificatieonderwerp en -inhoud toe aan het contactmoment

## v1.17.3

- Herstelt het scenario Zaak aangemaakt zodat het controleert of het `volgnummer` van de triggerende status gelijk is aan `1`

## v1.17.2

- Maakt `voorkeursAdres` (voorkeursadres) optioneel — als dit niet is ingesteld, is een digitale verwijzing naar de zaak vereist voor het versturen van notificaties

## v1.17.1

- Wijzigt `Bsn` naar `bsn` in queryparameters (OpenKlant accepteert geen hoofdletters)

## v1.17.0 ⚠️ Breaking change

- `appsettings.json` gewijzigd omdat OpenKlant `PartijIdentificator` van een string naar een enum heeft veranderd. Vereist OpenKlant **v2.12.0 of hoger**.

## v1.16.0 ⚠️ Breaking change

- `ZGW_ENDPOINT_*`-variabelen moeten nu het HTTP-protocol als prefix bevatten (bijv. `https://openzaak.mijnstad.nl/...`)

## v1.15.8

- Voegt omgevingsvariabele `OMC_CONTEXT_PATH` toe voor ondersteuning van reverse proxy-padprefixen. Standaard: lege string.

## v1.15.7

- Voegt contactmoment-callback toe aan documentatie

## v1.15.6

- Voegt documentatie toe voor het scenario Zaak aangemaakt

## v1.15.5

- Bugfix: verwerkt meerdere rollen waarvan sommige geen `inpBsn` hebben

## v1.15.4

- Bugfix: corrigeert het onjuist instellen van het distributiekanaal dat soms fouten veroorzaakte in NotifyNL

## v1.15.3

- Werkt `DetermineDistributionChannel` bij om te controleren op zowel `"Telefoon"` als `"telefoonnummer"` als typen digitaal adres (OpenKlant v2.4.0 wijzigde de waarde)

## v1.15.2

- Documentatie-updates

## v1.15.1 ⚠️ Breaking change (launchSettings)

- Voegt meer persoonsgegevens toe aan KTO-aanroep naar Expoints. Breaking changes in `launchSettings.json` — zie de `KTO_*`-sectie in [Omgevingsvariabelen](../configuratie/omgevingsvariabelen.md).

## v1.15.0 ⚠️ Breaking change (launchSettings)

- Introduceert Klanttevredenheidsonderzoek (KTO)-integratie via Expoints. Breaking changes in `launchSettings.json` — zie de `KTO_*`-sectie.

## v1.14.6

- Maakt vergelijking van digitaaladrestype hoofdletterongevoelig (accepteert zowel `"Email"` als `"email"`)

## v1.14.5

- Ontbrekende `.image.tag`-update in chart

## v1.14.4

- Corrigeert Base64-decodering voor post-merge uitrolling

## v1.14.3

- Updates voor test- en bouwautomatisering

## v1.14.2

- Patcht CVE-2024-21907 en consolideert afhankelijkheden

## v1.14.1

- Versienummeringspatch

## v1.14.0

- Voegt optie toe om het voorkeursdigitale adres van een burger te overschrijven op basis van zaaknummer

## v1.13.2

- Updates voor test- en bouwautomatisering

## v1.13.1

- Documentatie-updates (oude paden)
- Code-opschoning: generieke methodenaamgeving, gestroomlijnde parameters
