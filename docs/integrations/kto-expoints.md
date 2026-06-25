# KTO / Expoints

OMC supports an optional integration with [Expoints](https://expoints.nl/) — a customer satisfaction survey (KTO — *Klanttevredenheidsonderzoek*) platform. When configured, OMC triggers a survey submission to Expoints after a notification is delivered.

---

## How it works

After a successful notification delivery callback from NotifyNL, OMC can submit case data to the Expoints API. Expoints then invites the citizen to participate in a satisfaction survey for that case type.

The KTO integration is triggered as part of the notification callback flow and is treated as a notification in its own right internally.

---

## Configuration

Set the following environment variables. If KTO is not used, set all of them to `"-"` (a single dash) — do **not** leave them empty.

| Variable | Description |
|---|---|
| `KTO_AUTH_JWT_CLIENTID` | OAuth2 client ID for Expoints authentication |
| `KTO_AUTH_JWT_SECRET` | JWT signing secret |
| `KTO_AUTH_JWT_SCOPE` | OAuth2 scope (e.g. `api://some/scope`) |
| `KTO_AUTH_JWT_ISSUER` | Token issuer URL (e.g. `https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token`) |
| `KTO_URL` | Expoints API endpoint (e.g. `https://{subdomain}.expoints.nl`) |
| `KTO_CASETYPESETTINGS` | JSON mapping of case types to survey configuration (see below) |

---

## Case type settings

`KTO_CASETYPESETTINGS` is a JSON string that maps case type identifiers to the survey parameters Expoints expects:

```json
{
  "ApproveAutomatically": true,
  "IsTest": false,
  "caseTypeSettings": [
    {
      "caseTypeId": "ZAAKTYPE-IDENTIFICATIE-1",
      "Vragenlijst_naam": "Brede intake",
      "Dienst_naam": "SCW",
      "Type_meting": "Product KTO"
    },
    {
      "caseTypeId": "ZAAKTYPE-IDENTIFICATIE-2",
      "Vragenlijst_naam": "Brede intake",
      "Dienst_naam": "SCW",
      "Type_meting": "Product KTO"
    }
  ]
}
```

| Field | Description |
|---|---|
| `ApproveAutomatically` | Whether to auto-approve survey invitations |
| `IsTest` | Set to `true` in non-production environments |
| `caseTypeId` | The `zaaktypeIdentificatie` value to match |
| `Vragenlijst_naam` | Survey questionnaire name in Expoints |
| `Dienst_naam` | Service name in Expoints |
| `Type_meting` | Measurement type in Expoints |

<!-- image: Documentation/images/example_kto_settings.png -->

---

## ZGW object type

If KTO uses a dedicated object type in the Objecten API, configure:

```
ZGW_VARIABLE_OBJECTTYPE_KTOBJECTTYPE_UUID
```

This UUID should match the KTO object type registered in ObjectTypen.
