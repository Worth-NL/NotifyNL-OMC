# Zaak gewijzigd

Dit scenario wordt geactiveerd wanneer een bestaande zaak een tussentijdse statuswijziging ontvangt.

---

## Triggercondities

| Veld | Vereiste waarde |
|---|---|
| `kanaal` | `zaken` |
| `resource` | `status` |
| `actie` | `create` |
| Status `volgnummer` | Groter dan `1` |
| Status `isEindstatus` | `false` |

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

- Het zaaktype moet op de whitelist staan (`ZGW_WHITELIST_ZAAKUPDATE_IDS`)
- De `status.volgnummer` moet groter zijn dan `1`
- `statustype.isEindstatus` moet `false` zijn
- De burger moet contactgegevens hebben in OpenKlant
- Het template-ID moet zijn ingesteld

---

## Beschikbare templateplaceholders

| Placeholder | Bron | Beschrijving |
|---|---|---|
| `((zaak.identificatie))` | zaak | Zaaknummer |
| `((zaak.omschrijving))` | zaak | Omschrijving van de zaak |
| `((status.omschrijving))` | statustype | Omschrijving van de nieuwe status |
| `((status.datumStatusGezet))` | status | Datum waarop de status is gezet |
| `((klant.voornaam))` | klant | Voornaam van de klant |
| `((klant.voorvoegselAchternaam))` | klant | Tussenvoegsel van de klant |
| `((klant.achternaam))` | klant | Achternaam van de klant |

---

## Relevante omgevingsvariabelen

| Variabele | Beschrijving |
|---|---|
| `ZGW_WHITELIST_ZAAKUPDATE_IDS` | Kommagescheiden lijst van toegestane zaaktype-identificaties, of `*` |
| `NOTIFY_TEMPLATEID_EMAIL_ZAAKUPDATE` | NotifyNL template-UUID voor e-mail |
| `NOTIFY_TEMPLATEID_SMS_ZAAKUPDATE` | NotifyNL template-UUID voor sms |
| `NOTIFY_TEMPLATEID_LETTER_ZAAKUPDATE` | NotifyNL template-UUID voor brief |
