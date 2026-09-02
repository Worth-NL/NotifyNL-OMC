# Berichtenbox (MOBB)

Dit scenario stuurt een bericht uit OpenVTB (Open Verzend- en Toegangsbeheer Berichten) door naar de MijnOverheid Berichtenbox (MOBB) van de burger. Lukt dat niet — de burger heeft geen MOBB-abonnement, het bericht is er niet geschikt voor, of de verzending mislukt — dan valt het OMC automatisch terug op digitale post (e-mail) en vervolgens op een brief.

> **Status:** dit scenario draagt in de code meerdere `First-version draft`/`TODO (unconfirmed)`/`TODO (nog speculatief)`-markeringen (o.a. de `codeObjecttype`/`codeRegister` van de Bericht-koppeling in OpenKlant, en of NotifyNL brief-verzending al productierijp is). Behandel dit hoofdstuk als een werkbeschrijving van de huidige implementatie, niet als een bevestigd, volledig productierijp contract.

---

## Triggercondities

Anders dan de overige scenario's wordt dit scenario **niet** geactiveerd via de Notificaties API (`kanaal`/`resource`/`actie`/`kenmerken`). OpenVTB's Berichten-component post rechtstreeks een [CloudEvent](https://github.com/cloudevents/spec) naar hetzelfde `POST /Events/Listen`-eindpunt. Het OMC herkent een CloudEvent aan de aanwezigheid van `specversion`, `type`, `source` en `id`, en routeert het — vóórdat er ook maar geprobeerd wordt het als een gewone `NotificationEvent` te deserialiseren — op basis van het `type`-veld:

| Laag | Voorwaarde |
|---|---|
| Domeinroutering (`NotifyProcessor`) | `type` begint met `nl.overheid.berichten.` (Open VTB's Berichten-namespace) |
| Scenario zelf (`MessageBoxScenarioImplementation`) | `type` is exact `nl.overheid.berichten.bericht-gepubliceerd` |

Open VTB's Berichten-component kent op dit moment twee event-types: `bericht-geregistreerd` (bij het aanmaken, vóórdat het bericht per se al zichtbaar hoeft te zijn — bijvoorbeeld bij een toekomstige `publicatiedatum`) en `bericht-gepubliceerd` (zodra het bericht daadwerkelijk actief is). Alleen het laatste triggert verwerking; `bericht-geregistreerd` en elk toekomstig, nog niet bestaand type worden als veilige no-op behandeld (HTTP 202, niets verwerkt) in plaats van als fout — zodat een nieuw event-type dat Open VTB ooit toevoegt (Taken kent er bijvoorbeeld al 6) niet per ongeluk verkeerd wordt opgepikt.

Het CloudEvent zelf bevat geen berichtinhoud — alleen de `subject` (het Bericht-UUID) wordt gebruikt om het volledige bericht bij OpenVTB op te halen.

### Voorbeeld payload

```json
{
  "specversion": "1.0",
  "type": "nl.overheid.berichten.bericht-gepubliceerd",
  "source": "urn:nld:oin:123456789000:openvtb",
  "subject": "55555555-5555-5555-5555-555555555555",
  "id": "66666666-6666-6666-6666-666666666666",
  "time": "2026-01-15T10:30:00Z"
}
```

---

## Verwerkingslogica

### Stap 1 — Bericht-UUID en volledig bericht ophalen

Het OMC leest het Bericht-UUID uit `subject` en haalt het volledige bericht op bij OpenVTB (`GET {ZGW_ENDPOINT_OPENVTB}/berichten/{uuid}`). Ontbreekt `subject`, of is het geen geldige UUID, dan wordt verwerking afgebroken.

### Stap 2 — Berichttekst-check

Is `berichtTekst` leeg of ontbrekend, dan wordt het bericht zonder verdere verwerking (en zonder terugval) verworpen.

### Stap 3 — Ontvanger-BSN ontleden

`ontvanger` moet een BSN-draagbare URN zijn (bevat een `:bsn:XXXXXXXXX`-segment). Alleen BSN wordt op dit moment ondersteund; een andere of onherkenbare URN breekt de verwerking af zonder terugval.

### Stap 4 — Klant/partij opzoeken

Het OMC zoekt de partij op via OpenKlant aan de hand van de BSN — net als bij de printstraat is een digitaal adres hierbij **niet vereist** (`requireDigitalAddress: false`): een burger zonder e-mailadres of telefoonnummer kan alsnog via de brief-terugval bereikt worden.

Ook dit scenario garandeert niet dat er al een partij voor deze burger bestaat in OpenKlant — een Berichtenbox-bericht kan het eerste contact zijn dat het OMC ooit met deze burger heeft. Bestaat de partij nog niet, dan maakt het OMC deze zelf aan (kale partij, `soortPartij: persoon`, uitsluitend voorzien van de BSN-`partijIdentificator`) en zoekt vervolgens opnieuw op (`createIfMissing: true`) — dit voorkomt dezelfde eerdere harde 412-mislukking die ook bij de printstraat gold, en geldt zowel bij de eerste verwerking als bij de heropzoeking vanuit een terugval-callback (zie [Terugvalketen](#terugvalketen) hieronder).

### Stap 5 — Whitelist-check

```
ZGW_WHITELIST_VTBMESSAGE_TYPES bevat berichtType, of staat op *
```

Deze check geldt **ongeacht kanaal** — ook een terugval naar e-mail of brief wordt tegengehouden als het berichttype niet is toegestaan. Net als bij de overige zaaktype-whitelists in het OMC betekent een **lege** waarde dat *niets* wordt toegestaan (niet "alles"): zonder deze variabele expliciet te zetten (naar `*` of een kommagescheiden lijst van berichttypes) verwerkt dit scenario dus geen enkel bericht.

### Stap 6 — MOBB-geschiktheid

Is het bericht niet gemarkeerd als geschikt voor de Berichtenbox (`mijnOverheidBerichtenbox: false` — bijvoorbeeld omdat de burger geen MOBB-abonnement heeft), dan slaat het OMC de MOBB-verzending helemaal over en gaat direct naar de [digitale-post/brief-terugval](#terugvalketen).

### Stap 7 — Verzending via MOBB

Is het bericht wel MOBB-geschikt:

1. De berichttekst wordt aangevuld met een vaste postfix (`" [GENERATED BY OMC]"`).
2. Maximaal 2 bruikbare bijlagen worden opgehaald bij de Documenten API (zie [Bijlagen](#bijlagen)).
3. Het OMC bouwt een gecomprimeerde, Base64-gecodeerde `reference` (het originele CloudEvent + Bericht-UUID + partij-UUID) en verstuurt het bericht via NotifyNL's MOBB-kanaal.

Wordt de verzending *synchroon* door NotifyNL geweigerd (bijv. ongeldige invoer), dan valt het OMC direct terug op de [digitale-post/brief-keten](#terugvalketen) — zonder dat hiervoor een eigen "mislukte poging"-contactmoment wordt aangemaakt (dat gebeurt alleen bij een latere, asynchrone afleverstatus-mislukking, zie hieronder).

Wordt de verzending geaccepteerd, dan wacht het OMC op de asynchrone afleverstatus-callback van NotifyNL voordat een contactmoment wordt geregistreerd.

---

## Terugvalketen

Berichtenbox kent, anders dan de andere scenario's, een keten van tot drie kanalen in plaats van één bepaald kanaal:

```
MOBB  →  digitale post (e-mail)  →  brief
```

Deze keten wordt op twee verschillende momenten doorlopen:

1. **Synchroon**, direct binnen de eerste verwerking — als MOBB niet geschikt is (stap 6 hierboven), of als de MOBB-verzending zelf direct wordt geweigerd.
2. **Asynchroon**, vanuit NotifyNL's afleverstatus-callback (`POST /Notify/Confirm`) — als een *eerder geaccepteerde* verzending (MOBB of e-mail) uiteindelijk toch niet is afgeleverd. Omdat de `reference` bewust alleen het Bericht-UUID bevat (niet de BSN — zelfde overweging als bij de reguliere `NotifyReference`), herleidt het OMC bij deze callback de ontvanger opnieuw via OpenVTB/OpenKlant. Ook déze heropzoeking gebruikt `createIfMissing: true` — dit kan immers evengoed de eerste keer zijn dat het OMC deze burger herleidt.

### Digitale post (e-mail)

Heeft de partij geen e-mailadres in OpenKlant, dan gaat het OMC direct naar de brief-terugval. Is er wel een e-mailadres, dan verstuurt het OMC een e-mail met template `NOTIFY_TEMPLATEID_EMAIL_MESSAGEBOX` en personalisatie `klant.voornaam`/`klant.voorvoegselAchternaam`/`klant.achternaam`. Wordt deze e-mail (synchroon of via de afleverstatus-callback) alsnog geweigerd, dan valt het OMC verder terug op de brief — met de vlag "was een mislukte notificatie" aan (zie hieronder).

### Brief

Anders dan bij de overige brief-verzendingen in het OMC (die het postadres uit OpenKlant halen) haalt deze terugval het postadres op bij **BRP** (Basisregistratie Personen, via Haal Centraal) — BRP is voor déze use case de bedoelde bron voor postadressen. Levert BRP geen bruikbaar adres (geen `adresregel1`), dan kan er geen brief verstuurd worden en stopt de keten hier zonder verdere terugval.

De brief wordt, zoals bij de klassieke ZGW-scenario's, **template-gebaseerd** verstuurd (`NOTIFY_TEMPLATEID_LETTER_MESSAGEBOX`/`MessageBoxLetter` — NotifyNL rendert de brief zelf uit de personalisatie) — dit is dus **niet** hetzelfde verzendmechanisme als de printstraat's *precompiled*-PDF-verzending (zie [Print (printstraat)](print-printstraat.md)). Eventuele bijlagen van het oorspronkelijke bericht worden wel meegestuurd (als ruwe Base64-inhoud, niet in de `{file, filename}`-vorm die MOBB gebruikt).

Personalisatie-velden (bevestigd tegen een live 400-respons van de template zelf):

| Veld | Bron |
|---|---|
| `address_line_1` | BRP `aanschrijfwijze.naam` (of `aanhef`/`gebruikInLopendeTekst` als terugval) |
| `address_line_2` | BRP `adresregel1` |
| `address_line_3` | BRP `adresregel2` |
| `address_line_4` | BRP `adresregel3`, alleen aanwezig als BRP deze regel levert |
| `address_line_4` óf `address_line_5` | BRP `land.omschrijving`, alleen aanwezig als BRP deze levert — hergebruikt index 4 als `adresregel3` ontbrak, anders index 5 |
| `bericht` | De oorspronkelijke berichttekst (`berichtTekst`) — alleen de tekst, geen apart onderwerp-veld |
| `wasMOBB` | Altijd `true` — elke brief die dit scenario verstuurt is per definitie een MOBB-terugval |
| `wasNotificatie` | `true` als een digitale-post (e-mail) poging hieraan voorafging en mislukte, anders `false` |

Mislukt de brief-verzending (synchroon of via callback), dan bestaat er geen verdere terugval — dit is het laatste kanaal in de keten.

---

## Contactmoment-registratie

Net als bij de overige scenario's wordt een contactmoment alleen geregistreerd bij een definitieve afleverstatus (`Success` of `Failure`), gedreven door NotifyNL's afleverstatus-callback — nooit bij tussentijdse/informatieve statussen.

- `kanaal` volgt het daadwerkelijk gebruikte NotifyNL-kanaal: `Mobb`, `Email` of `Brief`.
- `onderwerp` krijgt een label-voorvoegsel dat aangeeft via welke poging dit contactmoment ontstond — omdat één Bericht meerdere contactmomenten kan opleveren naarmate de terugvalketen wordt doorlopen: `[MOBB]`, `[MOBB-fallback: Email]`, `[MOBB-fallback: Email (na mislukte notificatie)]`, `[MOBB-fallback: Letter]`.
- `onderwerpobject` verwijst naar het Bericht zelf via `VARIABLES_OPENKLANT_CODEOBJECTTYPE_BERICHT`/`CODEREGISTER_BERICHT` — deze twee zijn, anders dan de `zaak`/`open-zaak`-constanten elders, nog **niet bevestigd** tegen een echte OpenKlant-omgeving.
- `inhoud` wordt afgekapt op 1000 tekens.

---

## Bijlagen

Bijlagen (`bijlagen` op het Bericht) worden op volgorde doorlopen totdat er 2 bruikbare zijn gevonden (er is geen prioriteits-/volgordeveld, dus de volgorde van de bron wordt vertrouwd). Een bijlage wordt overgeslagen (zonder een slot te kosten) als:

- deze een standaard, vooraf geüploade berichttype-bijlage is (`isBerichtTypeBijlage: true` — niet bedoeld voor de Berichtenbox),
- het informatieobject-URN ontbreekt of geen geldig UUID bevat, of
- het ophalen van de documentmetadata of -inhoud bij de Documenten API mislukt (gelogd, niet fataal voor de rest van het bericht).

De daadwerkelijke bestandsinhoud wordt in een tweede aparte aanroep opgehaald — de metadata-respons van de Documenten API levert voor `inhoud` een downloadlink, niet de bytes zelf.

---

## Vereisten samengevat

| Conditie | Waarde |
|---|---|
| `ZGW_WHITELIST_VTBMESSAGE_TYPES` | Bevat `berichtType`, of staat op `*` — **leeg laten blokkeert alle berichten** |
| CloudEvent `type` | `nl.overheid.berichten.bericht-gepubliceerd` |
| `bericht.ontvanger` | BSN-URN (andere identificatietypes nog niet ondersteund) |
| `bericht.berichtTekst` | Niet leeg |
| Burger heeft contactgegevens | Niet vereist voor MOBB/e-mail — brief-terugval vereist wel een bruikbaar BRP-adres |

---

## Berichtstructuur (OpenVTB API)

```json
{
  "url": "https://openvtb.mijnstad.nl/api/v1/berichten/55555555-5555-5555-5555-555555555555",
  "urn": "urn:nld:oin:123456789000:bericht:55555555-5555-5555-5555-555555555555",
  "uuid": "55555555-5555-5555-5555-555555555555",
  "onderwerp": "Uw aanvraag is in behandeling",
  "berichtTekst": "Er is een nieuw bericht voor u beschikbaar.",
  "publicatiedatum": "2026-01-15T10:30:00Z",
  "referentie": "OMC-TEST-0001",
  "ontvanger": "urn:nld:bsn:nummer:123456789",
  "berichtType": "statuswijziging",
  "mijnOverheidBerichtenbox": true,
  "bijlagen": [
    {
      "informatieObject": "urn:nld:oin:123456789000:uuid:77777777-7777-7777-7777-777777777777",
      "omschrijving": "bijlage.pdf",
      "isBerichtTypeBijlage": false
    }
  ]
}
```

---

## Templateplaceholders

MOBB zelf verstuurt de ruwe berichttekst (geen `((placeholder))`-template). Alleen de twee terugvalkanalen zijn template-gebaseerd:

| Kanaal | Template-variabele | Placeholders |
|---|---|---|
| Digitale post (e-mail) | `NOTIFY_TEMPLATEID_EMAIL_MESSAGEBOX` | `((klant.voornaam))`, `((klant.voorvoegselAchternaam))`, `((klant.achternaam))` |
| Brief | `NOTIFY_TEMPLATEID_LETTER_MESSAGEBOX` | `((address_line_1))`…`((address_line_5))`, `((bericht))`, `((wasMOBB))`, `((wasNotificatie))` — zie [Brief](#brief) hierboven |

---

## Relevante omgevingsvariabelen

| Variabele | Beschrijving |
|---|---|
| `ZGW_ENDPOINT_OPENVTB` | Basis-URL van OpenVTB — ook vereist wanneer MOBB niet gebruikt wordt |
| `ZGW_AUTH_KEY_OPENVTB` | API-sleutel voor OpenVTB — ook vereist wanneer MOBB niet gebruikt wordt |
| `ZGW_WHITELIST_VTBMESSAGE_TYPES` | Toegestane berichttypes — leeg = alles geblokkeerd |
| `NOTIFY_TEMPLATEID_EMAIL_MESSAGEBOX` | NotifyNL template-UUID voor de digitale-post (e-mail) terugval |
| `NOTIFY_TEMPLATEID_LETTER_MESSAGEBOX` | NotifyNL template-UUID voor de brief-terugval |
| `VARIABLES_OPENKLANT_CODEOBJECTTYPE_BERICHT` | `codeObjecttype` voor de onderwerpobject-koppeling naar het Bericht (nog niet bevestigd in OpenKlant) |
| `VARIABLES_OPENKLANT_CODEREGISTER_BERICHT` | `codeRegister` voor diezelfde koppeling |
| `VARIABLES_PARTIJIDENTIFICATOR` | `codeSoortObjectId` van de BSN-partijIdentificator, zowel bij het opzoeken als (indien nodig) het aanmaken van de partij (standaard: `bsn`) |
| `VARIABLES_OPENKLANT_CODEOBJECTTYPE_PARTIJ` | `codeObjecttype` gebruikt bij het aanmaken van een ontbrekende partij (standaard: `natuurlijk_persoon`) |
| `VARIABLES_OPENKLANT_CODEREGISTER_PARTIJ` | `codeRegister` gebruikt bij het aanmaken van een ontbrekende partij (standaard: `brp`) |

De brief-terugval vereist daarnaast een werkende BRP/Haal Centraal-configuratie (`BRP_*`/`KEYCLOAK_*`, sectie "BRP / Haal Centraal") — zie [Omgevingsvariabelen](../../configuratie/omgevingsvariabelen.md) voor de volledige lijst.
