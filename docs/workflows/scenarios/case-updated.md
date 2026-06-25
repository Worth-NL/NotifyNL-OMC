# Case Updated

Notifies the relevant party that the status of their case (`zaak`) has been updated.

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

> The trigger event is the same shape as Case Created. OMC distinguishes between the two by checking the number of statuses on the case and whether the latest status is final.

---

## Processing requirements

- Action (`actie`) is `"create"`
- Channel (`kanaal`) is `"zaken"`
- Resource (`resource`) is `"status"`
- The case has **2 or more statuses** (it has been updated at least once)
- The latest status is **not final** (`"isEindstatus": false`)
- The case type identifier is in `ZGW_WHITELIST_ZAAKUPDATE_IDS` or the whitelist is `"*"`
- The case type has `"informeren": true`
- All URIs are valid and authentication credentials are correct

---

## Required environment variables

```
ZGW_WHITELIST_ZAAKUPDATE_IDS

NOTIFY_TEMPLATEID_EMAIL_ZAAKUPDATE
NOTIFY_TEMPLATEID_SMS_ZAAKUPDATE
NOTIFY_TEMPLATEID_LETTER_ZAAKUPDATE   (if using letter delivery)
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
| `((status.omschrijving))` | Description of the new status |
