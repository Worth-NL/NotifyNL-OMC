# Print (printstraat)

Dit scenario wordt geactiveerd wanneer een al samengestelde PDF-brief via de Objecten API wordt aangeboden om te printen en te versturen. In tegenstelling tot de andere scenario's stelt het OMC hier zelf geen inhoud samen — de aanleverende partij (bijvoorbeeld GZAC/Ritense) levert een kant-en-klare PDF, en het OMC haalt deze op, stuurt hem ongewijzigd door naar NotifyNL, en registreert het resultaat.

---

## Triggercondities

Het OMC activeert dit scenario wanneer een event binnenkomt met de volgende kenmerken:

| Veld | Vereiste waarde |
|---|---|
| `kanaal` | `objecten` |
| `resource` | `object` |
| `actie` | `create` |
| `objectType` UUID | Overeenkomstig `ZGW_VARIABLE_OBJECTTYPE_PRINTOBJECTTYPE_UUID` |

### Voorbeeld payload

```json
{
  "actie": "create",
  "kanaal": "objecten",
  "resource": "object",
  "kenmerken": {
    "objectType": "https://objecttypen.mijnstad.nl/api/v1/objecttypes/88888888-8888-8888-8888-888888888888"
  },
  "hoofdObject": "https://objecten.mijnstad.nl/api/v2/objects/99999999-9999-9999-9999-999999999999",
  "resourceUrl": "https://objecten.mijnstad.nl/api/v2/objects/99999999-9999-9999-9999-999999999999",
  "aanmaakdatum": "2026-01-15T10:30:00Z"
}
```

Zie de [printobjectstructuur](#printobjectstructuur-objecten-api) hieronder voor de inhoud die het OMC vervolgens ophaalt bij `hoofdObject`/`resourceUrl`.

---

## Verwerkingslogica

### Stap 1 — Printschakelaar

```
ZGW_WHITELIST_PRINT_ALLOWED == true
```

Dit scenario heeft een eigen, aparte aan/uit-schakelaar (niet de gebruikelijke zaaktype-whitelist), omdat print niet aan een zaaktype gekoppeld is.

### Stap 2 — Printobject ophalen

Het OMC haalt het printobject op bij `hoofdObject`/`resourceUrl` uit de Objecten API.

### Stap 3 — PDF-URL valideren (SSRF-bescherming)

De `pdfurl` in het printobject wordt geleverd door de aanleverende partij en is dus niet vertrouwd totdat is aangetoond dat deze naar de eigen, geconfigureerde Documenten API (`ZGW_ENDPOINT_DOCUMENTEN`) wijst — schema, host én poort moeten exact overeenkomen. Zonder deze controle zou het OMC misbruikt kunnen worden als open proxy om willekeurige URL's op te halen met de eigen inloggegevens van het OMC. Een `pdfurl` die niet overeenkomt, breekt de verwerking af.

### Stap 4 — Betrokkene-URN ontleden

`contact_betrokkene_urn` moet een BSN-draagbare URN zijn (bijv. `urn:nld:bsn:nummer:123456789`). Alleen BSN wordt op dit moment ondersteund; een KVK-URN of een onherkenbare URN breekt de verwerking af.

### Stap 5 — Klant opzoeken

Het OMC zoekt de partij op via OpenKlant aan de hand van de BSN. Anders dan bij de overige scenario's is een digitaal adres (e-mail/telefoon) **niet vereist** — een burger zonder e-mailadres of telefoonnummer is voor een fysieke brief een normale ontvanger.

### Stap 6 — PDF downloaden

Het OMC haalt de daadwerkelijke bestandsinhoud op bij de gevalideerde `pdfurl`.

### Stap 7 — Versturen als precompiled letter

Het OMC stuurt de PDF **ongewijzigd** ("zonder oplegger") naar NotifyNL als *precompiled letter* — er wordt geen template of personalisatie meegegeven; NotifyNL leest het adres uit het adresvenster van de PDF zelf. Dit is een fundamenteel ander verzendmechanisme dan de template-gebaseerde brieven van de overige scenario's (zie [Scenario's — Overzicht](overzicht.md)).

Een geslaagde verzending (HTTP 201 van NotifyNL) betekent alleen dat NotifyNL de PDF **geaccepteerd** heeft — de inhoud wordt asynchroon gevalideerd en kan alsnog worden afgekeurd. Er wordt op dit moment nog **geen** contactmoment geregistreerd en het brontobject wordt nog **niet** verwijderd.

### Stap 8 — Afleverstatus verwerken (asynchroon, via de callback)

Wanneer NotifyNL de afleverstatus terugmeldt op de callback:

- **Succes of mislukking** → het OMC registreert altijd een contactmoment (kanaal `Brief`), ook bij een mislukte aflevering.
- **Alleen bij bevestigd succes** wordt het brontobject in de Objecten API verwijderd (zodat het niet opnieuw wordt opgepikt). Bij een mislukking blijft het object staan zodat de brief later opnieuw geprobeerd kan worden.
- Als de registratie van het contactmoment zelf mislukt, blijft het object ook staan.

> **Let op:** NotifyNL meldt voor post normaliter de status `received` ("de leverancier heeft de brief geprint en verstuurd") als eindstatus — niet `delivered`. Controleer bij het inrichten van een omgeving of de afleverstatusverwerking deze status ook daadwerkelijk als succes herkent.

---

## Vereisten samengevat

| Conditie | Waarde |
|---|---|
| `ZGW_WHITELIST_PRINT_ALLOWED` | `true` |
| `objectType` UUID | Overeenkomstig `ZGW_VARIABLE_OBJECTTYPE_PRINTOBJECTTYPE_UUID` |
| `pdfurl` | Moet exact op de origin van `ZGW_ENDPOINT_DOCUMENTEN` wijzen |
| `contact_betrokkene_urn` | BSN-URN (KVK nog niet ondersteund) |
| Burger heeft contactgegevens | Niet vereist — brief gaat ook zonder e-mail/telefoon |

---

## Printobjectstructuur (Objecten API)

```json
{
  "record": {
    "data": {
      "pdfurl": "https://documenten.mijnstad.nl/api/v1/enkelvoudiginformatieobjecten/...",
      "contact_onderwerp": "Beschikking aanvraag 2026-001234",
      "contact_onderwerpobjectidentificator": {
        "objectId": "...",
        "codeObjecttype": "zaak",
        "codeRegister": "openzaak",
        "codeSoortObjectId": "uuid"
      },
      "contact_betrokkene_urn": "urn:nld:bsn:nummer:123456789"
    }
  }
}
```

`contact_onderwerp` en `contact_onderwerpobjectidentificator` worden gebruikt bij de registratie van het contactmoment (`onderwerp`/`onderwerpobject`) — als `contact_onderwerpobjectidentificator` ontbreekt, valt het OMC terug op de geconfigureerde standaardwaarden.

---

## Templateplaceholders

Niet van toepassing. Dit scenario stelt zelf geen inhoud samen — de PDF *is* de brief. Er is dan ook geen `NOTIFY_TEMPLATEID_LETTER_*`-variabele voor dit scenario.

---

## Relevante omgevingsvariabelen

| Variabele | Beschrijving |
|---|---|
| `ZGW_WHITELIST_PRINT_ALLOWED` | `true`/`false` — schakelt dit scenario in of uit |
| `ZGW_VARIABLE_OBJECTTYPE_PRINTOBJECTTYPE_UUID` | UUID van het printobjecttype in ObjectTypen |
| `ZGW_ENDPOINT_DOCUMENTEN` | Basis-URL van de Documenten API — `pdfurl` moet hier exact op de origin van overeenkomen |

Zie [Omgevingsvariabelen](../../configuratie/omgevingsvariabelen.md) voor de volledige lijst.
