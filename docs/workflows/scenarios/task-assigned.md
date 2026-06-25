# Task Assigned

Notifies the relevant party that a task (`taak`) has been assigned to them.

---

## Trigger event

```json
{
  "actie": "create",
  "kanaal": "objecten",
  "resource": "object",
  "kenmerken": {
    "objectType": "https://..."
  },
  "hoofdObject": "https://...",
  "resourceUrl": "https://...",
  "aanmaakdatum": "2000-01-01T10:00:00.000Z"
}
```

---

## Processing requirements

- Action (`actie`) is `"create"`
- Channel (`kanaal`) is `"objecten"`
- Resource (`resource`) is `"object"`
- The UUID from the object type URI (`objectType`) matches `ZGW_VARIABLE_OBJECTTYPE_TASKOBJECTTYPE_UUID`
- The task's case type is in `ZGW_WHITELIST_TASKASSIGNED_IDS` or the whitelist is `"*"`
- The task status (`status` in `record.data`) is `"open"`
- The task identification type (`type` in `record.data.identificatie`) is `"bsn"` (citizen) or `"kvk"` (organisation)
- The case type has `"informeren": true`
- All URIs are valid and authentication credentials are correct

---

## Required environment variables

```
ZGW_WHITELIST_TASKASSIGNED_IDS
ZGW_VARIABLE_OBJECTTYPE_TASKOBJECTTYPE_UUID

NOTIFY_TEMPLATEID_EMAIL_TASKASSIGNED
NOTIFY_TEMPLATEID_SMS_TASKASSIGNED
NOTIFY_TEMPLATEID_LETTER_TASKASSIGNED   (if using letter delivery)
```

---

## Template placeholders

| Placeholder | Description |
|---|---|
| `((klant.voornaam))` | First name |
| `((klant.voorvoegselAchternaam))` | Name infix |
| `((klant.achternaam))` | Last name |
| `((taak.verloopdatum))` | Task expiry date |
| `((taak.heeft_verloopdatum))` | Boolean — whether the task has an expiry date |
| `((taak.record.data.title))` | Task title |
| `((zaak.identificatie))` | Case reference number |
| `((zaak.omschrijving))` | Case description |
