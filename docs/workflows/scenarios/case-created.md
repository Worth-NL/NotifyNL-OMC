# Case Created

Notifies the relevant party (citizen or organisation) that a new case (`zaak`) has been opened for them.

---

## Trigger event

OMC listens for this JSON event from Open Notificaties:

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

All of the following must be true for the notification to be sent:

- Action (`actie`) is `"create"`
- Channel (`kanaal`) is `"zaken"`
- Resource (`resource`) is `"status"`
- The case has **exactly 1 status** (i.e. it is newly created — the triggering status's `volgnummer` is `1`)
- The case type identifier (`zaaktypeIdentificatie`) is in `ZGW_WHITELIST_ZAAKCREATE_IDS` or the whitelist is set to `"*"`
- The case type has `"informeren": true`
- All URIs are valid and authentication credentials are correct

---

## Required environment variables

```
ZGW_WHITELIST_ZAAKCREATE_IDS

NOTIFY_TEMPLATEID_EMAIL_ZAAKCREATE
NOTIFY_TEMPLATEID_SMS_ZAAKCREATE
NOTIFY_TEMPLATEID_LETTER_ZAAKCREATE   (if using letter delivery)
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
