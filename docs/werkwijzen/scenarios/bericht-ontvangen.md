# Bericht ontvangen

Dit scenario wordt geactiveerd wanneer een nieuw bericht beschikbaar is voor de burger in de berichtenbox.

---

## Triggercondities

| Veld | Vereiste waarde |
|---|---|
| `kanaal` | `objecten` |
| `resource` | `object` |
| `actie` | `create` |
| Object `type` | Overeenkomstig `ZGW_VARIABLE_OBJECTTYPE_MESSAGE_UUID` |

---

## Voorbeeldpayload

```json
{
  "kanaal": "objecten",
  "resource": "object",
  "actie": "create",
  "kenmerken": {
    "objectType": "https://objecttypen.mijnstad.nl/api/v2/objecttypes/<bericht-uuid>"
  },
  "resourceUrl": "https://objecten.mijnstad.nl/api/v2/objects/..."
}
```

---

## Berichtobjectstructuur (Objecten API)

```json
{
  "record": {
    "data": {
      "identificatie": {
        "type": "bsn",
        "value": "123456789"
      },
      "onderwerp": "Uw aanvraag is in behandeling",
      "berichtinhoud": "Er is een nieuw bericht voor u beschikbaar.",
      "zaak": "https://openzaak.mijnstad.nl/zaken/api/v1/zaken/..."
    }
  }
}
```

---

## Vereisten

- Het scenario Bericht ontvangen moet zijn ingeschakeld (`ZGW_WHITELIST_MESSAGE_ALLOWED=true`)
- Het objecttype UUID moet overeenkomen met `ZGW_VARIABLE_OBJECTTYPE_MESSAGE_UUID`
- De burger (BSN of KVK) moet contactgegevens hebben in OpenKlant
- Het template-ID moet zijn ingesteld

---

## Beschikbare templateplaceholders

| Placeholder | Bron | Beschrijving |
|---|---|---|
| `((bericht.onderwerp))` | berichtobject | Onderwerp van het bericht |
| `((bericht.berichtinhoud))` | berichtobject | Inhoud van het bericht |
| `((zaak.identificatie))` | zaak | Zaaknummer gekoppeld aan het bericht |
| `((zaak.omschrijving))` | zaak | Omschrijving van de zaak |
| `((klant.voornaam))` | klant | Voornaam van de klant |
| `((klant.voorvoegselAchternaam))` | klant | Tussenvoegsel van de klant |
| `((klant.achternaam))` | klant | Achternaam van de klant |

---

## Relevante omgevingsvariabelen

| Variabele | Beschrijving |
|---|---|
| `ZGW_WHITELIST_MESSAGE_ALLOWED` | `true` of `false` — schakel het scenario in of uit |
| `ZGW_VARIABLE_OBJECTTYPE_MESSAGE_UUID` | UUID van het berichtobjecttype in ObjectTypen |
| `ZGW_VARIABLE_OBJECTEN_MESSAGEOBJECTTYPE_VERSION` | Versie van het berichtobjecttype (standaard: `1`) |
| `NOTIFY_TEMPLATEID_EMAIL_MESSAGERECEIVED` | NotifyNL template-UUID voor e-mail |
| `NOTIFY_TEMPLATEID_SMS_MESSAGERECEIVED` | NotifyNL template-UUID voor sms |
| `NOTIFY_TEMPLATEID_LETTER_MESSAGERECEIVED` | NotifyNL template-UUID voor brief |
