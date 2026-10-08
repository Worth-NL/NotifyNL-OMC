# Changelog

---

## v2.3.1

- Herontwerpt het dashboard `/status/flow` naar de "Stroom"-weergave van OMC3: vaste kolommen (Invoer → OMC → Uitvoer → Afleverbevestiging) met de registers in één rij eronder, in plaats van een automatisch geplaatste graaf waarvan de lijnen met elk scenario meer kruisten
- Voegt Output Patronen en de controles samen tot één OMC-blok: kies een flow en de controles van die flow lichten op, in de volgorde waarin het scenario ze uitvoert; registers en uitvoer die de flow niet gebruikt dimmen
- Een klik op een controle toont wat er gecontroleerd wordt, waar het is ingesteld, wat er gebeurt als het niet klopt (206, 412 of overgeslagen) en waar het in de code staat, met het log eronder
- Voegt de flows Printstraat en Berichtenbox (MOBB) toe aan het dashboard, met Printstraat en Logius Berichtenbox als uitvoer en de BerichtenAPI (OpenVTB) en DocumentenAPI als registers
- Traceert de Berichtenbox-flow (MOBB): elke controle en elke terugval (Berichtenbox → e-mail → brief via BRP) is nu te volgen in de live trace. Voorheen liet een bericht helemaal geen trace achter
- Rustigere live trace: één bolletje per bericht dat elke lijn één keer doorloopt. Stappen waarbij het bolletje niet beweegt duren geen volle seconde meer, en het bolletje begint niet meer bij elke stap opnieuw, waardoor één bericht eruitzag als meerdere
- Koppelingen die nog niet bestaan staan achter "Toon geplande koppelingen"; de afhankelijkheid van de Dagre-layout is verwijderd
- De flowpagina is alleen nog het diagram met de live trace-log ernaast: de trace loopt zodra de pagina open is, en de introductietekst, de metriekbalk (omgevingsschakelaar, belasting, afhandeltijd, sparkline) en de trace- en herstelknop zijn weg
- **Het statusdashboard is nu opt-in.** De pagina's en API's (`/status`, `/status/flow`, `/status/scenarios`, `/status/stream`, `/status/trace/stream`) hebben geen authenticatie, dus ze worden alleen geserveerd met `DASHBOARD_ENABLED=true` en geven anders `404`. De Docker-image bevat het dashboard alleen als hij gebouwd is met `--build-arg BUILD_DASHBOARD=true`. Omgevingen die het dashboard gebruiken hebben beide nodig
- Houdt persoonsgegevens uit de live trace-stream: de foutmelding van een mislukte registercall bevat de request-URI (bij een partij-zoekactie in OpenKlant met het BSN of KVK-nummer erin) en de response body. Elk trace-detail wordt nu opgeschoond voordat het de stream in gaat: geen URI of response body, geen querystrings, lange cijferreeksen gemaskeerd, lengte begrensd
- Werkt het dashboard bij naar Next.js 16.4.0 en zet KaTeX (via Mermaid) vast op 0.18.2 of hoger, waarmee de critical- en low-meldingen van npm audit in de runtime-dependencies zijn opgelost

## v2.3.0

- Voegt het scenario [Product aangemaakt](../werkwijzen/scenarios/producten.md) toe: een `producten`/`product`/`create`-event uit Open Product wordt opgehaald, getoetst aan een producttype-whitelist en per e-mail verstuurd aan elke eigenaar van het product
- Organisaties worden nu bij naam aangesproken: heeft een partij geen achternaam, dan vult de organisatienaam `((klant.achternaam))`, zodat "Beste ((klant.voornaam)) ((klant.voorvoegselAchternaam)) ((klant.achternaam))" in elk scenario "Beste <bedrijfsnaam>" wordt in plaats van "Beste "
- Een eigenaar met een `vestigingsnummer` wordt in OpenKlant gekoppeld aan díe vestiging (`vestigingsnummer` onder het `kvk_nummer` via `subIdentificatorVan`) in plaats van aan de organisatie. Vereist OpenKlant 2.16.0 of hoger
- Voegt een Open Producten-client toe (`ZGW_ENDPOINT_OPENPRODUCTEN`, `ZGW_AUTH_KEY_OPENPRODUCTEN`), inclusief sleutel-, endpoint- en connectiviteitsregels op de statuspagina
- Open Product is optioneel: alle nieuwe variabelen mogen leeg blijven, en een lege `ZGW_ENDPOINT_OPENPRODUCTEN` schakelt het scenario uit (de statuspagina toont het als uitgeschakeld, product-events worden afgebroken met `206`). Bestaande installaties hoeven bij het upgraden niets aan hun configuratie te veranderen
- Voegt `ZGW_WHITELIST_PRODUCTCREATE_IDS` toe, getoetst op de `code` van het producttype — Open Product kent geen `identificatie`-veld om op te whitelisten
- Zoekt per eigenaar de partij op in OpenKlant via BSN of KVK-nummer; de eigen UUID van een eigenaar is de primaire sleutel van Open Product en heeft geen relatie met OpenKlant
- Geeft voorrang aan het digitale adres dat een partij als `portaalvoorkeur` heeft gemarkeerd, en beperkt de adreszoektocht tot e-mail — een partij met een telefoonnummer als voorkeursadres wordt daardoor niet langer gelezen als "geen e-mailadres bekend"
- Vermeldt de reden in het mislukte contactmoment (`Reden: …`): geen e-mailadres bekend, een weigering of fout van NotifyNL, of de bezorgstatus van een mislukte afleverstatus. Een mislukt contactmoment heeft een partij nodig, dus een eigenaar zonder partij beëindigt de hele notificatie nog steeds met een `206`
- Registreert een mislukt contactmoment per eigenaar die niet bereikt kon worden; geslaagde contactmomenten worden zoals bij de andere kanalen vanuit de NotifyNL-afleverstatuscallback geschreven
- Een partij die volgens OpenKlant niet bestaat — voor een BSN, KVK-nummer of partij-id — breekt de notificatie nu in elk scenario af met `206`, met een reden die het soort identificator noemt (nooit de waarde). Voorheen antwoordden de zaakscenario's met `412`, waardoor Open Notificaties een event bleef aanbieden dat nooit kon slagen. Is OpenKlant onbereikbaar of foutief, dan blijft het `412` en wordt het opnieuw aangeboden
- Verstuurt pas nadat het product, het producttype, de publicatiestatus én alle eigenaren zijn gevalideerd — mislukt een van die controles, dan wordt niemand genotificeerd

## v2.2.2

- Brieven worden niet meer verstuurd met het sms-template: `LetterComponent`
- Een briefnotificatie waarvan de templatevariabele niet is ingesteld geeft nu een `412` en wordt door Open Notificaties opnieuw aangeboden; Het OMC start ook zonder de brieftemplates gewoon op, en e-mail, sms, Berichtenbox en printstraat zijn niet geraakt — zie [Omgevingsvariabelen](../configuratie/omgevingsvariabelen.md)
- Omgevingsvariabelen in de documentatie gecorrigeerd: de integratiepagina's voor [BRP / Haal Centraal](../integraties/brp-haalcentraal.md), [KTO / Expoints](../integraties/kto-expoints.md) en [PostGuard](../integraties/postguard.md) beschreven variabelen die de code nooit uitleest, waardoor inrichten volgens die pagina's een stilzwijgend halve deployment opleverde. Ook de scenario- en architectuurpagina's waren afgedreven op objecttype-, template- en HTTP-poolingnamen
- Het KTO-scenario wordt niet aangestuurd door "Zaak afgesloten" en kent geen koppeling van zaaktypen aan enquêtes; de werkelijke werking (een object uit de Objecten API) is nu beschreven

---

## v2.2.1

- De uitgaande CloudEvent van het [MijnZaken](../integraties/mijnoverheid.md) "zaak gemuteerd"-scenario gebruikt nu `datumStatusGezet` van de status als `time`, in plaats van de eigen verwerkingstijd van het OMC (of `laatstGemuteerd` van de zaak)

---

## v2.2.0

- [Printstraat-scenario](../werkwijzen/scenarios/print-printstraat.md) toegevoegd: een vooraf gegenereerde PDF-brief wordt verstuurd op basis van een object uit de Objecten API, het contactmoment wordt geregistreerd vanuit de afleverstatus-callback en het object wordt daarna verwijderd
- Ondersteuning voor het versturen van voorgecompileerde brieven toegevoegd aan de Notify-client; brieven worden op het klantcontact vastgelegd met kanaal "brief"
- Een `validation-failed`-status van Notify voor een brief geldt nu als mislukte aflevering in plaats van als succes
- Een ontbrekende OpenKlant-partij wordt nu ter plekke aangemaakt in plaats van af te breken met een harde 412
- Semaphore-lek in `HttpNetworkService` verholpen: een mislukte HTTP-aanroep gaf zijn plek in de gedeelde begrenzer nooit vrij. Genoeg mislukkingen achter elkaar putten die begrenzer uit, waarna elke volgende uitgaande aanroep blijft hangen tot een herstart
- De natuurlijk-persoonfilter bij [MijnOverheid](../integraties/mijnoverheid.md) "zaak geopend" geldt nu ook bij de eerste opening van een zaak, in plaats van pas zodra `laatstGeopend` gevuld is
- Een `hoofdObject` waarvan het laatste padsegment geen UUID is, wordt nu afgewezen met een toelichting; voorheen leverde dat een `subject` van `/` op die verderop stilzwijgend werd genegeerd
- De permanente afbrekingen van het [Berichtenbox-scenario](../werkwijzen/scenarios/berichtenbox-mobb.md) (lege berichttekst, ontvanger zonder BSN, berichttype niet op de whitelist) worden gerapporteerd als een no-op succes in plaats van als fout, zodat Open VTB een bericht dat nooit kan slagen niet blijft herleveren
- `MessageBoxScenario` is geregistreerd in de DI-container; een notificatie op het kanaal `berichten` liep voorheen op een fout in plaats van te worden afgehandeld

---

## v2.1.0

- [MijnOverheid](../integraties/mijnoverheid.md)-integratie toegevoegd: het eindpunt `POST /Events/MijnZaken` normaliseert inkomende ZGW CloudEvents/NotificationEvents en stuurt relevante zaakgebeurtenissen door naar MijnOverheid, met whitelist- en natuurlijk-persoonfiltering
- Ondersteuning toegevoegd voor de gebeurtenissen "zaak geopend" en "zaak verwijderd" (voorheen werd alleen "zaak gemuteerd" herkend vanuit NotificationEvent-payloads)
- Diverse correcties aan de MijnZaken-payloadverwerking, veldmapping en tijdstipnauwkeurigheid, zodat het eindpunt end-to-end correct werkt
- Configuratiestatus-dashboard toegevoegd (statisch geserveerde React/Next.js SPA) met een live scenario-tracevisualisatie

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
