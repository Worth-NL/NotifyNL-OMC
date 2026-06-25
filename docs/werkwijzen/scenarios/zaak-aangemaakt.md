# Zaak aangemaakt

Dit scenario wordt geactiveerd wanneer een nieuwe zaak voor een burger of organisatie wordt geopend.

---

## Triggercondities

| Veld | Vereiste waarde |
|---|---|
| `kanaal` | `zaken` |
| `resource` | `status` |
| `actie` | `create` |
| `kenmerken.bronorganisatie` | Overeenkomstig geconfigureerde organisatie |
| Status `volgnummer` | `1` (eerste status op de zaak) |

> Het OMC luistert naar `status`-events (niet `zaak`-events) vanwege een race condition in de ZGW-stack waarbij de zaakgegevens mogelijk nog niet beschikbaar zijn ten tijde van het `zaak`-event.

---

## Voorbeeldpayload

```json
{
  "kanaal": "zaken",
  "resource": "status",
  "actie": "create",
  "kenmerken": {
    "bronorganisatie": "123456789",
    "zaaktype": "https://openzaak.mijnstad.nl/catalogi/api/v1/zaaktypen/...",
    "vertrouwelijkheidaanduiding": "openbaar"
  },
  "resourceUrl": "https://openzaak.mijnstad.nl/zaken/api/v1/statussen/..."
}
```

---

## Vereisten

- Het zaaktype moet op de whitelist staan (`ZGW_WHITELIST_ZAAKCREATE_IDS`)
- De `status.volgnummer` moet `1` zijn
- De burger moet contactgegevens hebben in OpenKlant
- Het template-ID moet zijn ingesteld (`NOTIFY_TEMPLATEID_EMAIL_ZAAKCREATE` en/of `NOTIFY_TEMPLATEID_SMS_ZAAKCREATE`)

---

## Beschikbare templateplaceholders

| Placeholder | Bron | Beschrijving |
|---|---|---|
| `((zaak.identificatie))` | zaak | Zaaknummer |
| `((zaak.omschrijving))` | zaak | Omschrijving van de zaak |
| `((zaak.startdatum))` | zaak | Startdatum van de zaak |
| `((status.omschrijving))` | statustype | Omschrijving van de huidige status |
| `((klant.voornaam))` | klant | Voornaam van de klant |
| `((klant.voorvoegselAchternaam))` | klant | Tussenvoegsel van de klant |
| `((klant.achternaam))` | klant | Achternaam van de klant |
| `((klant.emailadres))` | klant | E-mailadres van de klant |

---

## Relevante omgevingsvariabelen

| Variabele | Beschrijving |
|---|---|
| `ZGW_WHITELIST_ZAAKCREATE_IDS` | Kommagescheiden lijst van toegestane zaaktype-identificaties, of `*` |
| `NOTIFY_TEMPLATEID_EMAIL_ZAAKCREATE` | NotifyNL template-UUID voor e-mail |
| `NOTIFY_TEMPLATEID_SMS_ZAAKCREATE` | NotifyNL template-UUID voor sms |
| `NOTIFY_TEMPLATEID_LETTER_ZAAKCREATE` | NotifyNL template-UUID voor brief |
