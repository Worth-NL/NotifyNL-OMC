# Environment variables

Environment variables hold sensitive configuration (secrets, API keys, credentials) and values that differ between deployments. OMC reads them at startup and caches them internally.

> If a required variable is missing, OMC returns a readable error response naming the missing variable — this is the fastest way to identify what still needs to be set.

---

## .NET / ASP.NET Core

| Variable | Type | Required | Description |
|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | string | Yes | Determines which `appsettings[.xxx].json` is loaded. Accepted values: `Production`, `Development`, `Test` |

---

## OMC settings

| Variable | Type | Required | Description |
|---|---|---|---|
| `OMC_AUTH_JWT_SECRET` | string | Yes | JWT signing secret. Must be at least 64 bytes for security. |
| `OMC_AUTH_JWT_ISSUER` | string | Yes | JWT issuer claim — identifies OMC to Open Notificaties |
| `OMC_AUTH_JWT_AUDIENCE` | string | Yes | JWT audience claim |
| `OMC_AUTH_JWT_EXPIRESINMIN` | ushort | Yes | Token lifetime in minutes (e.g. `60`) |
| `OMC_AUTH_JWT_USERID` | string | Yes | User ID claim in the generated token |
| `OMC_AUTH_JWT_USERNAME` | string | Yes | Human-readable name claim in the generated token |
| `OMC_FEATURE_WORKFLOW_VERSION` | byte | Yes | Workflow version to use. `1` for OpenKlant v1, `2` for OpenKlant v2. OMC will not start without this. |
| `OMC_CONTEXT_PATH` | string | No | Base path when OMC is hosted behind a reverse proxy (e.g. `/omc`). Default: empty string. |
| `OMC_ACTOR_ID` | UUID | No | Unique identifier for this OMC instance. Used in contact moment registration. |

---

## ZGW settings

### JWT authentication (for OpenZaak and Besluiten)

| Variable | Type | Required | Description |
|---|---|---|---|
| `ZGW_AUTH_JWT_SECRET` | string | Yes | JWT secret matching the credential configured in OpenZaak |
| `ZGW_AUTH_JWT_ISSUER` | string | Yes | JWT issuer matching the client ID configured in OpenZaak |
| `ZGW_AUTH_JWT_AUDIENCE` | string | No | JWT audience (optional) |
| `ZGW_AUTH_JWT_EXPIRESINMIN` | ushort | Yes | Token lifetime in minutes (recommended: `60`) |
| `ZGW_AUTH_JWT_USERID` | string | Yes | User ID registered in OpenZaak |
| `ZGW_AUTH_JWT_USERNAME` | string | Yes | Username registered in OpenZaak |

### API key authentication

| Variable | Type | Required | Description |
|---|---|---|---|
| `ZGW_AUTH_KEY_OPENKLANT` | string | v2 only | API key generated in OpenKlant |
| `ZGW_AUTH_KEY_OBJECTEN` | string | Yes | API key generated in Objecten |
| `ZGW_AUTH_KEY_OBJECTTYPEN` | string | Yes | API key generated in ObjectTypen |

### Endpoints

All URLs must include the protocol (`https://`) and must **not** end with a trailing slash.

| Variable | Type | Required | Example |
|---|---|---|---|
| `ZGW_ENDPOINT_OPENNOTIFICATIES` | URI | Yes | `https://opennotificaties.mycity.nl/api/v1` |
| `ZGW_ENDPOINT_OPENZAAK` | URI | Yes | `https://openzaak.mycity.nl/zaken/api/v1` |
| `ZGW_ENDPOINT_OPENKLANT` | URI | Yes | `https://openklant.mycity.nl/klanten/api/v1` |
| `ZGW_ENDPOINT_BESLUITEN` | URI | Yes | `https://openzaak.mycity.nl/besluiten/api/v1` |
| `ZGW_ENDPOINT_OBJECTEN` | URI | Yes | `https://objecten.mycity.nl/api/v2` |
| `ZGW_ENDPOINT_OBJECTTYPEN` | URI | Yes | `https://objecttypen.mycity.nl/api/v2` |
| `ZGW_ENDPOINT_CONTACTMOMENTEN` | URI | Yes | `https://openklant.mycity.nl/contactmomenten/api/v1` |

### Whitelists

Whitelists control which case types trigger notifications. Set to specific `zaaktypeIdentificatie` values (comma-separated) or `"*"` to accept all types. Use `"*"` during initial testing; restrict to specific IDs in production.

| Variable | Type | Description |
|---|---|---|
| `ZGW_WHITELIST_ZAAKCREATE_IDS` | string[] | Case types that trigger Case Created notifications |
| `ZGW_WHITELIST_ZAAKUPDATE_IDS` | string[] | Case types that trigger Case Updated notifications |
| `ZGW_WHITELIST_ZAAKCLOSE_IDS` | string[] | Case types that trigger Case Closed notifications |
| `ZGW_WHITELIST_TASKASSIGNED_IDS` | string[] | Case types that trigger Task Assigned notifications |
| `ZGW_WHITELIST_DECISIONMADE_IDS` | string[] | Case types that trigger Decision Made notifications |
| `ZGW_WHITELIST_MESSAGE_ALLOWED` | bool | Whether Message Received notifications are enabled (`"true"` / `"false"`) |

### Object type UUIDs

| Variable | Type | Required | Description |
|---|---|---|---|
| `ZGW_VARIABLE_OBJECTTYPE_TASKOBJECTTYPE_UUID` | UUID | Yes | UUID of the task object type in ObjectTypen |
| `ZGW_VARIABLE_OBJECTTYPE_MESSAGEOBJECTTYPE_UUID` | UUID | Yes | UUID of the message object type in ObjectTypen |
| `ZGW_VARIABLE_OBJECTTYPE_MESSAGEOBJECTTYPE_VERSION` | ushort | Yes | Version number of the message object type |
| `ZGW_VARIABLE_OBJECTTYPE_DECISIONINFOOBJECTTYPE_UUIDS` | UUID[] | Yes | Comma-separated UUIDs of allowed decision info object types |
| `ZGW_VARIABLE_OBJECTTYPE_KTOBJECTTYPE_UUID` | UUID | No | UUID of the KTO object type (required only if KTO integration is enabled) |

---

## NotifyNL settings

| Variable | Type | Required | Description |
|---|---|---|---|
| `NOTIFY_API_BASEURL` | URI | Yes | NotifyNL API base URL (e.g. `https://api.notifynl.nl`) |
| `NOTIFY_API_KEY` | string | Yes | NotifyNL API key in format `name-UUID-UUID` |

### Email template IDs

| Variable | Type | Description |
|---|---|---|
| `NOTIFY_TEMPLATEID_EMAIL_ZAAKCREATE` | UUID | Email template for Case Created |
| `NOTIFY_TEMPLATEID_EMAIL_ZAAKUPDATE` | UUID | Email template for Case Updated |
| `NOTIFY_TEMPLATEID_EMAIL_ZAAKCLOSE` | UUID | Email template for Case Closed |
| `NOTIFY_TEMPLATEID_EMAIL_TASKASSIGNED` | UUID | Email template for Task Assigned |
| `NOTIFY_TEMPLATEID_EMAIL_DECISIONMADE` | UUID | Email template for Decision Made |
| `NOTIFY_TEMPLATEID_EMAIL_MESSAGERECEIVED` | UUID | Email template for Message Received |

### SMS template IDs

| Variable | Type | Description |
|---|---|---|
| `NOTIFY_TEMPLATEID_SMS_ZAAKCREATE` | UUID | SMS template for Case Created |
| `NOTIFY_TEMPLATEID_SMS_ZAAKUPDATE` | UUID | SMS template for Case Updated |
| `NOTIFY_TEMPLATEID_SMS_ZAAKCLOSE` | UUID | SMS template for Case Closed |
| `NOTIFY_TEMPLATEID_SMS_TASKASSIGNED` | UUID | SMS template for Task Assigned |
| `NOTIFY_TEMPLATEID_SMS_DECISIONMADE` | UUID | SMS template for Decision Made |
| `NOTIFY_TEMPLATEID_SMS_MESSAGERECEIVED` | UUID | SMS template for Message Received |

### Letter template IDs

| Variable | Type | Description |
|---|---|---|
| `NOTIFY_TEMPLATEID_LETTER_ZAAKCREATE` | UUID | Letter template for Case Created |
| `NOTIFY_TEMPLATEID_LETTER_ZAAKUPDATE` | UUID | Letter template for Case Updated |
| `NOTIFY_TEMPLATEID_LETTER_ZAAKCLOSE` | UUID | Letter template for Case Closed |
| `NOTIFY_TEMPLATEID_LETTER_TASKASSIGNED` | UUID | Letter template for Task Assigned |
| `NOTIFY_TEMPLATEID_LETTER_DECISIONMADE` | UUID | Letter template for Decision Made |
| `NOTIFY_TEMPLATEID_LETTER_MESSAGERECEIVED` | UUID | Letter template for Message Received |

> Not all scenarios require all three channel types. Configure only the templates for the channels your organisation uses.

---

## KTO / Expoints settings

Set unused KTO variables to `"-"` (a single dash) to explicitly disable the integration. Do **not** leave them empty.

| Variable | Type | Required | Description |
|---|---|---|---|
| `KTO_AUTH_JWT_CLIENTID` | string | KTO only | OAuth2 client ID for Expoints authentication |
| `KTO_AUTH_JWT_SECRET` | string | KTO only | JWT signing secret for Expoints |
| `KTO_AUTH_JWT_SCOPE` | string | KTO only | OAuth2 scope (e.g. `api://some/scope`) |
| `KTO_AUTH_JWT_ISSUER` | string | KTO only | Token issuer URL (e.g. `https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token`) |
| `KTO_URL` | URI | KTO only | Expoints API endpoint (e.g. `https://{subdomain}.expoints.nl`) |
| `KTO_CASETYPESETTINGS` | JSON string | KTO only | JSON mapping of case types to KTO survey configuration — see [KTO integration](../integrations/kto-expoints.md) |

---

## PostGuard settings

Required only when using the PostGuard encrypted PDF delivery feature (introduced in v2.0.0).

| Variable | Type | Required | Description |
|---|---|---|---|
| `POSTGUARD_API_KEY` | string | PostGuard only | PostGuard service API key |
| `POSTGUARD_API_PKGURL` | URI | PostGuard only | PostGuard package upload endpoint |
| `POSTGUARD_API_CRYPTIFYURL` | URI | PostGuard only | PostGuard encryption endpoint |
| `POSTGUARD_TEMPLATEID_SENDPOSTGUARDPDF` | UUID | PostGuard only | NotifyNL template ID for encrypted PDF delivery notifications |

See [PostGuard integration](../integrations/postguard.md) for details.

---

## BRP / Haal Centraal settings

Required when using letter delivery (OMC uses BRP to retrieve correspondence addresses).

| Variable | Type | Required | Description |
|---|---|---|---|
| `KEYCLOAK_AUTHSERVERURL` | URI | Letters only | Keycloak token endpoint URL |
| `KEYCLOAK_CLIENTID` | string | Letters only | Keycloak OAuth2 client ID for OMC |
| `KEYCLOAK_CLIENTSECRET` | string | Letters only | Keycloak OAuth2 client secret |
| `KEYCLOAK_TOKENEXCHANGEAUDIENCE` | string | Letters only | Audience for BRP token exchange (must match what the BRP API expects) |
| `BRP_BASEURL` | URI | Letters only | BRP / Haal Centraal API gateway URL |
| `BRP_CLIENTCERT_PEM_PATH` | path | Letters only | Absolute path to PEM-encoded mTLS client certificate |
| `BRP_CLIENTKEY_PEM_PATH` | path | Letters only | Absolute path to PEM-encoded mTLS private key |

See [BRP / Haal Centraal](../integrations/brp-haalcentraal.md) for details.

---

## Monitoring

| Variable | Type | Required | Description |
|---|---|---|---|
| `SENTRY_DSN` | URI | No | Sentry project DSN for error tracking and APM |
| `SENTRY_ENVIRONMENT` | string | No | Sentry environment tag (e.g. `"MyMunicipality-prod"`) |
