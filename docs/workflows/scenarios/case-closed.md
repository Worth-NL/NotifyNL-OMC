# Case Closed

Notifies the relevant party that their case (`zaak`) has been closed or resolved.

---

## Trigger event

```json
{
  "actie": "create",
  "kanaal": "zaken",
  "resource": "status",
  "kenmerken": {
    "zaaktype": "https://...",
    "bronorganisatie": "000000000",
    "vertrouwelijkheidaanduiding": "openbaar"
  },
  "hoofdObject": "https://...",
  "resourceUrl": "https://...",
  "aanmaakdatum": "2000-01-01T10:00:00.000Z"
}
```

---

## Processing requirements

- Action (`actie`) is `"create"`
- Channel (`kanaal`) is `"zaken"`
- Resource (`resource`) is `"status"`
- The case has **2 or more statuses**
- The latest status **is final** (`"isEindstatus": true`)
- The case type identifier is in `ZGW_WHITELIST_ZAAKCLOSE_IDS` or the whitelist is `"*"`
- The case type has `"informeren": true`
- All URIs are valid and authentication credentials are correct

---

## Required environment variables

```
ZGW_WHITELIST_ZAAKCLOSE_IDS

NOTIFY_TEMPLATEID_EMAIL_ZAAKCLOSE
NOTIFY_TEMPLATEID_SMS_ZAAKCLOSE
NOTIFY_TEMPLATEID_LETTER_ZAAKCLOSE   (if using letter delivery)
```

---

## Template placeholders

| Placeholder | Description |
|---|---|
| `((klant.voornaam))` | First name |
| `((klant.voorvoegselAchternaam))` | Name infix |
| `((klant.achternaam))` | Last name |
| `((zaak.identificatie))` | Case reference number |
| `((zaak.omschrijving))` | Case description |
| `((status.omschrijving))` | Description of the final status |
