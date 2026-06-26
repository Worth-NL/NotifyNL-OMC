# NotifyNL API Documentatie (REST)

Je kunt deze API direct integreren als je geen gebruik kunt maken van de beschikbare client libraries, of wanneer je besluit dat je niet alles via het OMC kunt doen.

> **Let op:** Niet alle functionaliteit die hieronder wordt beschreven (zoals het versturen van brieven) wordt in Nederland al ondersteund.

Ontwikkelaars kunnen ook gebruik maken van [ons OpenAPI-bestand](https://github.com/alphagov/notifications-tech-docs/blob/main/openapi/publicapi_spec.json) in combinatie met tools zoals [Swagger](https://swagger.io/) en [Redoc](https://github.com/Redocly/redoc). Je kunt de Swagger Editor niet gebruiken om test-API-aanvragen te doen.

---

## Een aanvraag doen

### Basis-URL

```
https://api.notifynl.nl
```

De autorisatieheader is een [API-sleutel](#api-sleutels) die is gecodeerd met [JSON Web Tokens](https://jwt.io/). Je moet altijd een autorisatieheader meesturen.

JSON Web Tokens bestaan uit een standaard header en een payload. De header ziet er als volgt uit:

```json
{
  "typ": "JWT",
  "alg": "HS256"
}
```

De payload ziet er als volgt uit:

```json
{
  "iss": "26785a09-ab16-4eb0-8407-a37497a57506",
  "iat": 1568818578
}
```

JSON Web Tokens worden gecodeerd met een geheime sleutel in het volgende formaat:

```
3d844edf-8d35-48ac-975b-e847b4f122b0
```

Die geheime sleutel maakt deel uit van je API-sleutel, die het volgende formaat heeft: `{sleutelnaam}-{iss-uuid}-{geheime-sleutel-uuid}`.

Bijvoorbeeld, als je API-sleutel `mijn_test_sleutel-26785a09-ab16-4eb0-8407-a37497a57506-3d844edf-8d35-48ac-975b-e847b4f122b0` is:

- je API-sleutelnaam is `mijn_test_sleutel`
- je `iss` (je dienst-ID) is `26785a09-ab16-4eb0-8407-a37497a57506`
- je geheime sleutel is `3d844edf-8d35-48ac-975b-e847b4f122b0`

`iat` (issued at) is de huidige tijd in UTC in epoch-seconden. Het token verloopt binnen 30 seconden na de huidige tijd.

Raadpleeg de [JSON Web Tokens website](https://jwt.io/) voor meer informatie over het coderen van je autorisatieheader.

Zodra je een gecodeerd en ondertekend token hebt, voeg je dit token als volgt toe aan een header:

```
"Authorization": "Bearer encoded_jwt_token"
```

**Content-header**

De content-header is `application/json`:

```
"Content-type": "application/json"
```

---

## Een bericht versturen

### Sms versturen

```
POST /v2/notifications/sms
```

**Aanvraagbody**

```json
{
  "phone_number": "+31612345678",
  "template_id": "f33517ff-2a88-4f6e-b855-c550268ce08a"
}
```

**Parameters**

**phone\_number (verplicht)**

Het telefoonnummer van de ontvanger. Dit kan een Nederlands of internationaal nummer zijn.

**template\_id (verplicht)**

Zo vind je het template-ID:

1. [Meld je aan bij NotifyNL](https://admin.notifynl.nl)
2. Ga naar de pagina **Templates** en selecteer het gewenste template.
3. Selecteer **Kopieer template-ID naar klembord**.

**personalisation (optioneel)**

Als een template placeholdervelden heeft voor gepersonaliseerde informatie zoals naam of referentienummer, moet je de waarden opgeven als key-value pairs. Bijvoorbeeld:

```json
"personalisation": {
  "first_name": "Amala",
  "application_date": "2018-01-01"
}
```

**reference (optioneel)**

Een identificatie die je zelf kunt aanmaken. Deze referentie identificeert een enkele notificatie of een batch notificaties. Mag geen persoonlijke informatie bevatten.

**sms\_sender\_id (optioneel)**

Een unieke identificatie van de afzender van de sms-notificatie. Te vinden via **Instellingen** → **Sms** → **Beheren** naast **Sms-afzender**.

**Respons**

Bij een geslaagde aanvraag is de responsbody `json` met statuscode `201`:

```json
{
  "id": "740e5834-3a29-46b4-9a6f-16142fde533a",
  "reference": "STRING",
  "content": {
    "body": "BERICHTTEKST",
    "from_number": "AFZENDER"
  },
  "uri": "https://api.notifynl.nl/v2/notifications/740e5834-3a29-46b4-9a6f-16142fde533a",
  "template": {
    "id": "f33517ff-2a88-4f6e-b855-c550268ce08a",
    "version": 1,
    "uri": "https://api.notifynl.nl/v2/template/ceb50d92-100d-4b8b-b559-14fa3b091cd"
  }
}
```

**Foutcodes**

| status\_code | Foutmelding | Oplossing |
|---|---|---|
| `400` | `Can't send to this recipient using a team-only API key` | Gebruik het juiste type [API-sleutel](#api-sleutels) |
| `400` | `Can't send to this recipient when service is in trial mode` | Je dienst kan deze notificatie niet versturen in proefmodus |
| `403` | `Error: Your system clock must be accurate to within 30 seconds` | Controleer je systeemklok |
| `403` | `Invalid token: API key not found` | Gebruik de juiste API-sleutel |
| `429` | `Exceeded rate limit for key type TEAM/TEST/LIVE of 3000 requests per 60 seconds` | Zie [snelheidslimieten](#snelheidslimieten) |
| `429` | `Exceeded send limits (LIMIT NUMBER) for today` | Zie [dagelijkse limieten](#dagelijkse-limieten) |
| `500` | `Internal server error` | Notify kon de aanvraag niet verwerken, probeer opnieuw |

---

### E-mail versturen

```
POST /v2/notifications/email
```

**Aanvraagbody**

```json
{
  "email_address": "ontvanger@voorbeeld.nl",
  "template_id": "f33517ff-2a88-4f6e-b855-c550268ce08a"
}
```

**Parameters**

**email\_address (verplicht)**

Het e-mailadres van de ontvanger.

**template\_id (verplicht)**

Zie [sms versturen](#sms-versturen) voor instructies.

**personalisation (optioneel)**

```json
"personalisation": {
  "first_name": "Amala",
  "application_date": "2018-01-01"
}
```

**reference (optioneel)**

Een identificatie die je zelf kunt aanmaken. Mag geen persoonlijke informatie bevatten.

**email\_reply\_to\_id (optioneel)**

Een e-mailadres dat je hebt opgegeven om antwoorden van gebruikers te ontvangen. Je moet minimaal één reply-to e-mailadres toevoegen voordat je dienst live kan gaan.

**Respons**

Bij een geslaagde aanvraag is de responsbody `json` met statuscode `201`:

```json
{
  "id": "740e5834-3a29-46b4-9a6f-16142fde533a",
  "reference": "STRING",
  "content": {
    "subject": "ONDERWERP",
    "body": "BERICHTTEKST",
    "from_email": "AFZENDER E-MAIL"
  },
  "uri": "https://api.notifynl.nl/v2/notifications/740e5834-3a29-46b4-9a6f-16142fde533a",
  "template": {
    "id": "f33517ff-2a88-4f6e-b855-c550268ce08a",
    "version": 1,
    "uri": "https://api.notifynl.nl/v2/template/f33517ff-2a88-4f6e-b855-c550268ce08a"
  }
}
```

**Foutcodes** — zie [sms versturen](#sms-versturen) voor de volledige tabel.

---

### Brief versturen (toekomstig)

> **Let op:** Het versturen van brieven wordt in Nederland nog niet ondersteund.

```
POST /v2/notifications/letter
```

**Aanvraagbody**

```json
{
  "template_id": "f33517ff-2a88-4f6e-b855-c550268ce08a",
  "personalisation": {
    "address_line_1": "De Bewoner",
    "address_line_2": "Voorbeeldstraat 123",
    "address_line_3": "1234 AB Amsterdam"
  }
}
```

---

### Vooraf samengestelde brief versturen (toekomstig)

```
POST /v2/notifications/letter
```

**Aanvraagbody**

```json
{
  "reference": "STRING",
  "content": "base64GecodeerrdPDFBestand"
}
```

**postage (optioneel)**

Kies `first` of `second` voor de verzendklasse. Standaard is `second`.

---

## Berichtstatus opvragen

Je kunt alleen de status opvragen van berichten die binnen de bewaarperiode vallen. De standaard bewaarperiode is 7 dagen.

### Status van één bericht

```
GET /v2/notifications/{notification_id}
```

**notification\_id (verplicht)**

Het ID van de notificatie, terug te vinden in de respons van de oorspronkelijke API-aanroep.

**Respons**

```json
{
  "id": "740e5834-3a29-46b4-9a6f-16142fde533a",
  "reference": "STRING",
  "email_address": "ontvanger@voorbeeld.nl",
  "phone_number": "+31612345678",
  "type": "sms / email / letter",
  "status": "sending / delivered / permanent-failure / temporary-failure / technical-failure",
  "template": {
    "version": 1,
    "id": "f33517ff-2a88-4f6e-b855-c550268ce08a",
    "uri": "/v2/template/{id}/{version}"
  },
  "body": "STRING",
  "subject": "STRING",
  "created_at": "2024-05-17 15:58:38.342838",
  "sent_at": "2024-05-17 15:58:30.143000",
  "completed_at": "2024-05-17 15:59:10.321000"
}
```

**Foutcodes**

| status\_code | Foutmelding | Oplossing |
|---|---|---|
| `400` | `id is not a valid UUID` | Controleer het notificatie-ID |
| `403` | `Error: Your system clock must be accurate to within 30 seconds` | Controleer je systeemklok |
| `403` | `Invalid token: API key not found` | Gebruik de juiste API-sleutel |
| `404` | `No result found` | Controleer het notificatie-ID |

---

### Status van meerdere berichten

```
GET /v2/notifications
```

Geeft één pagina terug met maximaal 250 berichten en hun statussen.

**Optionele queryparameters**

| Parameter | Beschrijving |
|---|---|
| `template_type` | Filter op `email`, `sms` of `letter` |
| `status` | Filter op berichtstatus |
| `reference` | Filter op referentie |
| `older_than` | Geeft berichten terug ouder dan het opgegeven notificatie-ID |
| `include_jobs` | Inclusief notificaties verstuurd als onderdeel van een batch-upload |

---

## Statusbeschrijvingen

### E-mailstatussen

| Status | Beschrijving |
|---|---|
| `created` | Bericht staat in de wachtrij |
| `sending` | Bericht is verstuurd naar de provider |
| `delivered` | Bericht is succesvol afgeleverd |
| `permanent-failure` | Aflevering mislukt — e-mailadres ongeldig |
| `temporary-failure` | Aflevering tijdelijk mislukt — inbox vol of spamfilter |
| `technical-failure` | Technische fout tussen Notify en de provider |

### Sms-statussen

| Status | Beschrijving |
|---|---|
| `created` | Bericht staat in de wachtrij |
| `sending` | Bericht is verstuurd naar de provider |
| `pending` | Wacht op meer afleverinformatie |
| `sent` | Verstuurd naar een internationaal nummer |
| `delivered` | Succesvol afgeleverd |
| `permanent-failure` | Aflevering mislukt — telefoonnummer ongeldig of geblokkeerd |
| `temporary-failure` | Aflevering tijdelijk mislukt — telefoon uit, geen bereik, inbox vol |
| `technical-failure` | Technische fout — je wordt niet in rekening gebracht |

### Briefstatussen

| Status | Beschrijving |
|---|---|
| `accepted` | Brief is verstuurd naar de drukker |
| `received` | Brief is gedrukt en verstuurd |
| `cancelled` | Verzending geannuleerd |
| `technical-failure` | Onverwachte fout bij de drukker |
| `permanent-failure` | De drukker kan de brief niet afdrukken |

---

## Template opvragen

### Template op ID

```
GET /v2/template/{template_id}
```

Geeft de meest recente versie van het template terug.

### Template op ID en versie

```
GET /v2/template/{template_id}/version/{version}
```

### Alle templates

```
GET /v2/templates
```

Optionele queryparameter: `type` (`email`, `sms`, `letter`).

### Voorbeeld van een template genereren

```
POST /v2/template/{template_id}/preview
```

**Aanvraagbody**

```json
{
  "personalisation": {
    "first_name": "Amala",
    "application_date": "2018-01-01"
  }
}
```

---

## Ontvangen sms-berichten opvragen

```
GET /v2/received-text-messages
```

Geeft één pagina met maximaal 250 ontvangen sms-berichten. Activeer ontvangen sms-berichten via **Instellingen** → **Sms-instellingen** → **Ontvang sms-berichten**.

**older\_than (optioneel)** — het ID van een ontvangen sms-bericht; geeft berichten terug die ouder zijn dan dat bericht.

---

## Foutmeldingen

Foutmeldingen bestaan uit:

- een `status_code`, bijv. `400`
- een `error`, bijv. `BadRequestError`
- een `message`, bijv. `Mobile numbers can only include: 0 1 2 3 4 5 6 7 8 9 ( ) + -`

Gebruik de `status_code` of de `error` in je code — niet de inhoud van `message`, want die kan veranderen.

---

## Testen

Alle tests vinden plaats in de productieomgeving. Er is geen aparte testomgeving.

### Smoke testen

Gebruik de volgende telefoonnummers en e-mailadressen voor smoke testing:

**Telefoonnummers:** `07700900000`, `07700900111`, `07700900222`

**E-mailadressen:** `simulate-delivered@notifications.service.gov.uk`, `simulate-delivered-2@notifications.service.gov.uk`, `simulate-delivered-3@notifications.service.gov.uk`

Deze valideren de aanvraag en simuleren een succesvolle respons, maar versturen geen echt bericht en slaan niets op in de database.

### Faalscenario's testen met een testsleutel

| Telefoonnummer / E-mailadres | Respons |
|---|---|
| `07700900003` | `temporary-failure` |
| `07700900002` | `permanent-failure` |
| `temp-fail@simulator.notify` | `temporary-failure` |
| `perm-fail@simulator.notify` | `permanent-failure` |
| Elk ander geldig nummer of adres | `delivered` |

---

## API-sleutels

Er zijn drie soorten API-sleutels:

- **Test** — voor het testen van je integratie; berichten worden niet echt verstuurd
- **Team en gastlijst** — voor het versturen van echte berichten aan teamleden en je gastlijst tijdens de proefperiode
- **Live** — voor het versturen van berichten aan iedereen; alleen beschikbaar als je dienst live is

Een API-sleutel aanmaken:

1. [Meld je aan bij NotifyNL](https://admin.notifynl.nl)
2. Ga naar de pagina **API-integratie**
3. Selecteer **API-sleutels**
4. Selecteer **Maak een API-sleutel aan**

---

## Limieten

### Snelheidslimieten

Je bent beperkt tot 3.000 berichten per minuut per API-sleuteltype. Bij overschrijding ontvang je een `429`-fout (`RateLimitError`).

### Dagelijkse limieten

| Dienststatus | Type API-sleutel | Dagelijkse limiet |
|---|---|---|
| Live | Team of live | 250.000 e-mails / 250.000 sms-berichten / 20.000 brieven |
| Proef | Team | 50 e-mails of sms-berichten |
| Live of proef | Test | Onbeperkt |

Limieten worden gereset om middernacht UTC.

### Telefoonnetwerklimieten

Als je herhaaldelijk sms-berichten stuurt naar hetzelfde nummer, blokkeren telefoonnetwerken deze. Uurlimiet:

- 20 berichten met dezelfde inhoud
- 100 berichten met willekeurige inhoud

---

## Callbacks

Callbacks zijn `POST`-aanvragen die NotifyNL naar jouw dienst stuurt. Je kunt callbacks ontvangen wanneer:

- een e-mail of sms-bericht is afgeleverd of mislukt
- je dienst een sms-bericht ontvangt

### Callbacks instellen

1. [Meld je aan bij NotifyNL](https://admin.notifynl.nl)
2. Ga naar de pagina **API-integratie**
3. Selecteer **Callbacks**

Je moet opgeven:
- Een URL waarnaar Notify de callback stuurt
- Een bearer-token dat Notify in de autorisatieheader van de aanvragen plaatst

### Callbacks opnieuw proberen

Als Notify een `POST`-aanvraag stuurt naar je dienst maar de aanvraag mislukt, probeert Notify het opnieuw. Notify probeert het elke 5 minuten opnieuw, maximaal 5 keer.

### Afleverbewijzen

De callback-payload bevat de volgende velden:

| Sleutel | Beschrijving | Formaat |
|---|---|---|
| `id` | Notify's ID voor de statusmelding | UUID |
| `reference` | De door de dienst opgegeven referentie | `12345678` of null |
| `to` | Het e-mailadres of telefoonnummer van de ontvanger | `hello@voorbeeld.nl` of `+31612345678` |
| `status` | De status van de notificatie | `delivered`, `permanent-failure`, `temporary-failure` of `technical-failure` |
| `created_at` | Het tijdstip waarop de dienst de aanvraag heeft verstuurd | `2017-05-14T12:15:30.000000Z` |
| `completed_at` | Het laatste tijdstip waarop de status is bijgewerkt | `2017-05-14T12:15:30.000000Z` of null |
| `sent_at` | Het tijdstip waarop de notificatie is verstuurd | `2017-05-14T12:15:30.000000Z` of null |
| `notification_type` | Het type notificatie | `email` of `sms` |
| `template_id` | Het ID van het gebruikte template | UUID |
| `template_version` | Het versienummer van het gebruikte template | `1` |
