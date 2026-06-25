# Decision Made

Notifies the relevant party that a decision (`besluit`) has been made that affects their case.

---

## Trigger event

```json
{
  "actie": "create",
  "kanaal": "besluiten",
  "resource": "besluitinformatieobject",
  "kenmerken": {
    "besluittype": "https://...",
    "verantwoordelijkeOrganisatie": "000000000"
  },
  "hoofdObject": "https://...",
  "resourceUrl": "https://...",
  "aanmaakdatum": "2000-01-01T10:00:00.000Z"
}
```

---

## Processing requirements

- Action (`actie`) is `"create"`
- Channel (`kanaal`) is `"besluiten"`
- Resource (`resource`) is `"besluitinformatieobject"`
- The UUID of the information object type (`informatieobjecttype`) linked to the decision is in `ZGW_VARIABLE_OBJECTTYPE_DECISIONINFOOBJECTTYPE_UUIDS`
- The information object status (`status`) is `"definitief"`
- The information object confidentiality (`vertrouwelijkheidaanduiding`) is `"openbaar"`
- The case type is in `ZGW_WHITELIST_DECISIONMADE_IDS` or the whitelist is `"*"`
- The case type has `"informeren": true`
- All URIs are valid and authentication credentials are correct

---

## Required environment variables

```
ZGW_WHITELIST_DECISIONMADE_IDS
ZGW_VARIABLE_OBJECTTYPE_MESSAGEOBJECTTYPE_UUID
ZGW_VARIABLE_OBJECTTYPE_MESSAGEOBJECTTYPE_VERSION
ZGW_VARIABLE_OBJECTTYPE_DECISIONINFOOBJECTTYPE_UUIDS

NOTIFY_TEMPLATEID_EMAIL_DECISIONMADE
NOTIFY_TEMPLATEID_SMS_DECISIONMADE
NOTIFY_TEMPLATEID_LETTER_DECISIONMADE   (if using letter delivery)
```

> Note: unlike other scenarios, the Decision Made template ID variables do not have a separate legacy `NOTIFY_TEMPLATEID_DECISIONMADE` fallback. Use the channel-specific variants above.

---

## Template placeholders

### Customer

| Placeholder | Description |
|---|---|
| `((klant.voornaam))` | First name |
| `((klant.voorvoegselAchternaam))` | Name infix |
| `((klant.achternaam))` | Last name |

### Decision (`besluit`)

| Placeholder | Description |
|---|---|
| `((besluit.identificatie))` | Decision reference |
| `((besluit.datum))` | Decision date |
| `((besluit.toelichting))` | Decision explanation |
| `((besluit.bestuursorgaan))` | Governing body that made the decision |
| `((besluit.ingangsdatum))` | Effective date |
| `((besluit.vervaldatum))` | Expiry date |
| `((besluit.vervalreden))` | Reason for expiry |
| `((besluit.publicatiedatum))` | Publication date |
| `((besluit.verzenddatum))` | Dispatch date |
| `((besluit.uiterlijkereactiedatum))` | Deadline for response |

### Decision type (`besluittype`)

| Placeholder | Description |
|---|---|
| `((besluittype.omschrijving))` | Decision type description |
| `((besluittype.omschrijvingGeneriek))` | Generic decision type description |
| `((besluittype.besluitcategorie))` | Decision category |
| `((besluittype.publicatieindicatie))` | Whether the decision is published |
| `((besluittype.publicatietekst))` | Publication text |
| `((besluittype.toelichting))` | Decision type explanation |

### Case (`zaak`)

| Placeholder | Description |
|---|---|
| `((zaak.identificatie))` | Case reference number |
| `((zaak.omschrijving))` | Case description |
| `((zaak.registratiedatum))` | Case registration date |
| `((zaaktype.omschrijving))` | Case type description |
| `((zaaktype.omschrijvingGeneriek))` | Generic case type description |
