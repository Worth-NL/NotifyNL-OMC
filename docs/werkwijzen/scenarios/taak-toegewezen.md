# Taak toegewezen

Dit scenario wordt geactiveerd wanneer een taak wordt toegewezen aan een burger of organisatie via de Objecten API.

---

## Triggercondities

Het OMC activeert dit scenario wanneer een event binnenkomt met de volgende kenmerken:

| Veld | Vereiste waarde |
|---|---|
| `kanaal` | `objecten` |
| `resource` | `object` |
| `actie` | `create` |
| `objectType` UUID | Overeenkomstig `ZGW_VARIABLE_OBJECTTYPE_TAAK_UUID` |

Het OMC haalt het objecttype-UUID uit de `objectType`-URL in het event en vergelijkt dit met de geconfigureerde waarde. Als het niet overeenkomt, wordt het event genegeerd.

### Voorbeeld payload

```json
{
  "actie": "create",
  "kanaal": "objecten",
  "resource": "object",
  "kenmerken": {
    "objectType": "https://objecttypen.mijnstad.nl/api/v1/objecttypes/66666666-6666-6666-6666-666666666666"
  },
  "hoofdObject": "https://objecten.mijnstad.nl/api/v2/objects/77777777-7777-7777-7777-777777777777",
  "resourceUrl": "https://objecten.mijnstad.nl/api/v2/objects/77777777-7777-7777-7777-777777777777",
  "aanmaakdatum": "2026-01-15T10:30:00Z"
}
```

Zie de [taakobjectstructuur](#taakobjectstructuur-objecten-api) hieronder voor de inhoud die het OMC vervolgens ophaalt bij `hoofdObject`/`resourceUrl`.

---

## Verwerkingslogica

Nadat het event herkend is als een taakevent, controleert het OMC de volgende condities **in volgorde**. Als één conditie niet klopt, wordt de verwerking afgebroken.

### Stap 1 — Taakstatus check

```
taak.status == "open"
```

Alleen taken met status `open` leiden tot een notificatie. Taken met een andere status (bijv. `gesloten`) worden overgeslagen.

### Stap 2 — Identificatietype check

```
taak.identificatie.type ∈ ["bsn", "kvk"]
```

De taak moet een `identificatie` bevatten met type `bsn` of `kvk`. Als het identificatietype ontbreekt of een andere waarde heeft, wordt de verwerking afgebroken.

### Stap 3 — Informeren check

```
zaaktype.informeren == true
```

Het zaaktype gekoppeld aan de taak moet `informeren` op `true` hebben staan in Open Zaak.

### Stap 4 — Whitelist check

```
zaaktype.identificatie ∈ ZGW_WHITELIST_TASKASSIGNED_IDS
```

Het zaaktype van de gekoppelde zaak moet voorkomen in de geconfigureerde whitelist.

### Stap 5 — Gegevens ophalen

Het OMC haalt aanvullende gegevens op:

1. `GET /objects/{uuid}` — taakobject (uit Objecten API)
2. `GET /zaken/{uuid}/statussen` — statussen van de gekoppelde zaak
3. `GET /zaaktypen/{uuid}` — zaaktype (voor informeren en whitelist check)
4. `GET /zaken/{uuid}` — zaakgegevens
5. Contactgegevens van de burger via OpenKlant (BSN of KVK)

### Stap 6 — Notificatie versturen

Het OMC verstuurt de notificatie via NotifyNL met het geconfigureerde template en schrijft daarna een contactmoment terug naar OpenKlant.

---

## Vereisten samengevat

| Conditie | Waarde |
|---|---|
| `objectType` UUID | Overeenkomstig `ZGW_VARIABLE_OBJECTTYPE_TAAK_UUID` |
| `taak.status` | `open` |
| `taak.identificatie.type` | `bsn` of `kvk` |
| `zaaktype.informeren` | `true` |
| `zaaktype.identificatie` | Moet op whitelist staan |
| Burger heeft contactgegevens | E-mail of telefoonnummer in OpenKlant |

---

## Taakobjectstructuur (Objecten API)

```json
{
  "record": {
    "data": {
      "status": "open",
      "verloopdatumTijd": "2024-12-31T23:59:59Z",
      "identificatie": {
        "type": "bsn",
        "value": "123456789"
      },
      "titel": "Documenten aanleveren",
      "zaak": "https://openzaak.mijnstad.nl/zaken/api/v1/zaken/...",
      "formtaak": {
        "formulier": {
          "type": "url",
          "value": "https://formulieren.mijnstad.nl/taak/..."
        }
      }
    }
  }
}
```

---

## Beschikbare templateplaceholders

| Placeholder | Bron | Beschrijving |
|---|---|---|
| `((taak.titel))` | taakobject | Titel van de taak |
| `((taak.verloopdatumTijd))` | taakobject | Vervaldatum/-tijd van de taak |
| `((taak.formulier.url))` | taakobject | URL naar het formulier dat ingevuld moet worden |
| `((zaak.identificatie))` | zaak | Zaaknummer gekoppeld aan de taak |
| `((zaak.omschrijving))` | zaak | Omschrijving van de zaak |
| `((klant.voornaam))` | klant | Voornaam van de klant |
| `((klant.voorvoegselAchternaam))` | klant | Tussenvoegsel van de klant |
| `((klant.achternaam))` | klant | Achternaam van de klant |

---

## Relevante omgevingsvariabelen

| Variabele | Beschrijving |
|---|---|
| `ZGW_VARIABLE_OBJECTTYPE_TAAK_UUID` | UUID van het taakobjecttype in ObjectTypen |
| `ZGW_VARIABLE_OBJECTEN_TAAKOBJECTTYPE_VERSION` | Versie van het taakobjecttype (standaard: `1`) |
| `ZGW_WHITELIST_TASKASSIGNED_IDS` | Kommagescheiden lijst van toegestane zaaktype-identificaties, of `*` |
| `NOTIFY_TEMPLATEID_EMAIL_TASKASSIGNED` | NotifyNL template-UUID voor e-mail |
| `NOTIFY_TEMPLATEID_SMS_TASKASSIGNED` | NotifyNL template-UUID voor sms |
| `NOTIFY_TEMPLATEID_LETTER_TASKASSIGNED` | NotifyNL template-UUID voor brief |
