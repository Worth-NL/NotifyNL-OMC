# Bericht ontvangen

Dit scenario wordt geactiveerd wanneer een nieuw bericht beschikbaar is voor de burger in de berichtenbox.

---

## Triggercondities

Het OMC activeert dit scenario wanneer een event binnenkomt met de volgende kenmerken:

| Veld | Vereiste waarde |
|---|---|
| `kanaal` | `objecten` |
| `resource` | `object` |
| `actie` | `create` |
| `objectType` UUID | Overeenkomstig `ZGW_VARIABLE_OBJECTTYPE_MESSAGE_UUID` |

Het OMC haalt het objecttype-UUID uit de `objectType`-URL in het event en vergelijkt dit met de geconfigureerde waarde. Als het niet overeenkomt, wordt het event genegeerd.

---

## Verwerkingslogica

### Stap 1 — Berichten toegestaan check

```
ZGW_WHITELIST_MESSAGE_ALLOWED == true
```

Dit scenario is standaard uitgeschakeld. Het OMC controleert als eerste of berichten zijn ingeschakeld via de configuratie. Als dit `false` is, wordt het event direct overgeslagen — ongeacht de inhoud.

### Stap 2 — Berichtobject ophalen

Het OMC haalt het berichtobject op uit de Objecten API via de `resourceUrl` in het event.

### Stap 3 — Identificatietype check

Het bericht moet een `identificatie` bevatten met een BSN-waarde. Het OMC gebruikt dit BSN om de contactgegevens van de burger op te halen via OpenKlant.

> Anders dan bij andere scenario's is er bij dit scenario **geen zaak** gekoppeld. Het OMC bevraagt OpenKlant direct op BSN.

### Stap 4 — Gegevens ophalen

1. `GET /objects/{uuid}` — berichtobject (uit Objecten API)
2. Contactgegevens van de burger via OpenKlant op basis van BSN

### Stap 5 — Notificatie versturen

Het OMC verstuurt de notificatie via NotifyNL met het geconfigureerde template. Er wordt geen contactmoment teruggeschreven omdat er geen zaak gekoppeld is.

---

## Vereisten samengevat

| Conditie | Waarde |
|---|---|
| `ZGW_WHITELIST_MESSAGE_ALLOWED` | `true` |
| `objectType` UUID | Overeenkomstig `ZGW_VARIABLE_OBJECTTYPE_MESSAGE_UUID` |
| `bericht.identificatie.type` | `bsn` |
| Burger heeft contactgegevens | E-mail of telefoonnummer in OpenKlant |

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
      "berichtinhoud": "Er is een nieuw bericht voor u beschikbaar."
    }
  }
}
```

---

## Beschikbare templateplaceholders

| Placeholder | Bron | Beschrijving |
|---|---|---|
| `((bericht.onderwerp))` | berichtobject | Onderwerp van het bericht |
| `((bericht.berichtinhoud))` | berichtobject | Inhoud van het bericht |
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
