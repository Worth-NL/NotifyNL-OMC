# appsettings.json

`appsettings.json` holds public, rarely-changing configuration that is safe to check into source control. Sensitive values (secrets, API keys, credentials) belong in [environment variables](environment-variables.md) instead.

---

## Which file is loaded

OMC loads a configuration file based on the `ASPNETCORE_ENVIRONMENT` environment variable:

| Value | File loaded |
|---|---|
| `Production` | `appsettings.json` |
| `Development` | `appsettings.Development.json` |
| `Test` | `appsettings.Test.json` |

---

## Full example — Events Handler

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "Network": {
    "ConnectionLifetimeInSeconds": 90,
    "HttpRequestTimeoutInSeconds": 60,
    "HttpRequestsSimultaneousNumber": 20
  },
  "Encryption": {
    "IsAsymmetric": false
  },
  "Variables": {
    "BetrokkeneType": "natuurlijk_persoon",
    "OmschrijvingGeneriek": "initiator",
    "PartijIdentificator": "Burgerservicenummer",
    "EmailOmschrijvingGeneriek": "Email",
    "TelefoonOmschrijvingGeneriek": "Telefoon",
    "OpenKlant": {
      "CodeObjectType": "zaak",
      "CodeRegister": "open-zaak",
      "CodeObjectTypeId": "uuid"
    },
    "UxMessages": {
      "SMS_Success_Subject": "Notificatie verzonden",
      "SMS_Success_Body": "SMS notificatie succesvol verzonden.",
      "SMS_Failure_Subject": "We konden uw notificatie niet afleveren.",
      "SMS_Failure_Body": "Het afleveren van een SMS bericht is niet gelukt. Controleer het telefoonnumer in uw profiel.",
      "Email_Success_Subject": "Notificatie verzonden",
      "Email_Success_Body": "E-mail notificatie succesvol verzonden.",
      "Email_Failure_Subject": "We konden uw notificatie niet afleveren.",
      "Email_Failure_Body": "Het afleveren van een email bericht is niet gelukt. Controleer het emailadres in uw profiel."
    }
  },
  "AllowedHosts": "*"
}
```

---

## Settings reference

### Network

| Setting | Default | Description |
|---|---|---|
| `ConnectionLifetimeInSeconds` | `90` | HTTP connection pool lifetime |
| `HttpRequestTimeoutInSeconds` | `60` | Per-request timeout |
| `HttpRequestsSimultaneousNumber` | `20` | Max concurrent outbound HTTP requests |

### Encryption

| Setting | Default | Description |
|---|---|---|
| `IsAsymmetric` | `false` | Use RSA asymmetric JWT signing instead of symmetric (HMAC) |

### Variables

These values map to ZGW API field names. **Do not change them** unless the corresponding values have also changed in your OpenZaak/OpenKlant configuration.

| Setting | Default | Description |
|---|---|---|
| `BetrokkeneType` | `"natuurlijk_persoon"` | Subject type for party lookup |
| `OmschrijvingGeneriek` | `"initiator"` | Party role used in ZGW queries |
| `PartijIdentificator` | `"Burgerservicenummer"` | Party identifier type (BSN). Note: OpenKlant v2.12.0+ requires exactly this value. |
| `EmailOmschrijvingGeneriek` | `"Email"` | Digital address type label for email |
| `TelefoonOmschrijvingGeneriek` | `"Telefoon"` | Digital address type label for phone. OpenKlant v2.4.0+ also accepts `"telefoonnummer"` — both are handled in code. |
| `OpenKlant.CodeObjectType` | `"zaak"` | Object type code for contact moments |
| `OpenKlant.CodeRegister` | `"open-zaak"` | Register code for contact moments |
| `OpenKlant.CodeObjectTypeId` | `"uuid"` | ID format for contact moment object references |

### UxMessages

Dutch-language messages written to contact moments on notification success or failure. These are what citizens see in their portal history.

---

## Overriding appsettings values with environment variables

Any `appsettings.json` value can be overridden at runtime with an environment variable. OMC checks for the environment variable first before falling back to the file.

**Naming convention:** flatten the JSON path, replace `:` and `.` with `_`, and capitalise everything.

**Example:** to override `Variables.UxMessages.SMS_Success_Subject`:

1. Flatten: `Variables:UxMessages:SMS_Success_Subject`
2. Replace separators: `Variables_UxMessages_SMS_Success_Subject`
3. Capitalise: `VARIABLES_UXMESSAGES_SMS_SUCCESS_SUBJECT`

Set this as an environment variable and restart OMC. The override only works in one direction — environment variables can override `appsettings.json`, not the reverse.

> Changes to `appsettings.json` require rebuilding the Docker image and redeploying. For values that change between environments or deployments, use environment variables instead.
