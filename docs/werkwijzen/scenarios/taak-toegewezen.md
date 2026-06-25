# Taak toegewezen

Dit scenario wordt geactiveerd wanneer een taak wordt toegewezen aan een burger of organisatie via de Objecten API.

---

## Triggercondities

| Veld | Vereiste waarde |
|---|---|
| `kanaal` | `objecten` |
| `resource` | `object` |
| `actie` | `create` |
| Object `type` | Overeenkomstig `ZGW_VARIABLE_OBJECTTYPE_TAAK_UUID` |
| Taak `status` | `open` |

---

## Voorbeeldpayload

```json
{
  "kanaal": "objecten",
  "resource": "object",
  "actie": "create",
  "kenmerken": {
    "objectType": "https://objecttypen.mijnstad.nl/api/v2/objecttypes/<taak-uuid>"
  },
  "resourceUrl": "https://objecten.mijnstad.nl/api/v2/objects/..."
}
```

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

## Vereisten

- Het objecttype UUID moet overeenkomen met `ZGW_VARIABLE_OBJECTTYPE_TAAK_UUID`
- Het objecttype moet op de whitelist staan (`ZGW_WHITELIST_TASKASSIGNED_IDS`)
- De taak `status` moet `open` zijn
- De burger (BSN of KVK) moet contactgegevens hebben in OpenKlant
- Het template-ID moet zijn ingesteld

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
| `ZGW_WHITELIST_TASKASSIGNED_IDS` | Kommagescheiden lijst van toegestane objecttype-identificaties, of `*` |
| `NOTIFY_TEMPLATEID_EMAIL_TASKASSIGNED` | NotifyNL template-UUID voor e-mail |
| `NOTIFY_TEMPLATEID_SMS_TASKASSIGNED` | NotifyNL template-UUID voor sms |
| `NOTIFY_TEMPLATEID_LETTER_TASKASSIGNED` | NotifyNL template-UUID voor brief |
