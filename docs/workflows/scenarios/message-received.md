# Message Received

Notifies the relevant party that a new message is available for them in the municipal message box (`berichtenbox`).

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
- The UUID from the object type URI matches `ZGW_VARIABLE_OBJECTTYPE_MESSAGEOBJECTTYPE_UUID`
- `ZGW_WHITELIST_MESSAGE_ALLOWED` is set to `"true"`
- All URIs are valid and authentication credentials are correct

---

## Required environment variables

```
ZGW_WHITELIST_MESSAGE_ALLOWED
ZGW_VARIABLE_OBJECTTYPE_MESSAGEOBJECTTYPE_UUID

NOTIFY_TEMPLATEID_EMAIL_MESSAGERECEIVED
NOTIFY_TEMPLATEID_SMS_MESSAGERECEIVED
NOTIFY_TEMPLATEID_LETTER_MESSAGERECEIVED   (if using letter delivery)
```

---

## Template placeholders

| Placeholder | Description |
|---|---|
| `((klant.voornaam))` | First name |
| `((klant.voorvoegselAchternaam))` | Name infix |
| `((klant.achternaam))` | Last name |
| `((message.onderwerp))` | Message subject |
| `((message.handelingsperspectief))` | Action the recipient should take |
