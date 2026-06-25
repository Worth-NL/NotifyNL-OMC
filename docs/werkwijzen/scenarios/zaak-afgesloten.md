# Zaak afgesloten

Dit scenario wordt geactiveerd wanneer een zaak de eindstatus bereikt.

---

## Triggercondities

| Veld | Vereiste waarde |
|---|---|
| `kanaal` | `zaken` |
| `resource` | `status` |
| `actie` | `create` |
| Status `isEindstatus` | `true` |

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

- Het zaaktype moet op de whitelist staan (`ZGW_WHITELIST_ZAAKCLOSE_IDS`)
- `statustype.isEindstatus` moet `true` zijn
- De burger moet contactgegevens hebben in OpenKlant
- Het template-ID moet zijn ingesteld

---

## Beschikbare templateplaceholders

| Placeholder | Bron | Beschrijving |
|---|---|---|
| `((zaak.identificatie))` | zaak | Zaaknummer |
| `((zaak.omschrijving))` | zaak | Omschrijving van de zaak |
| `((zaak.einddatum))` | zaak | Einddatum van de zaak |
| `((status.omschrijving))` | statustype | Omschrijving van de eindstatus |
| `((klant.voornaam))` | klant | Voornaam van de klant |
| `((klant.voorvoegselAchternaam))` | klant | Tussenvoegsel van de klant |
| `((klant.achternaam))` | klant | Achternaam van de klant |

---

## Relevante omgevingsvariabelen

| Variabele | Beschrijving |
|---|---|
| `ZGW_WHITELIST_ZAAKCLOSE_IDS` | Kommagescheiden lijst van toegestane zaaktype-identificaties, of `*` |
| `NOTIFY_TEMPLATEID_EMAIL_ZAAKCLOSE` | NotifyNL template-UUID voor e-mail |
| `NOTIFY_TEMPLATEID_SMS_ZAAKCLOSE` | NotifyNL template-UUID voor sms |
| `NOTIFY_TEMPLATEID_LETTER_ZAAKCLOSE` | NotifyNL template-UUID voor brief |
