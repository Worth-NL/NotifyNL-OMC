# Step 2 — Configure ZGW API keys

OMC needs credentials to authenticate with your ZGW services. The exact credentials required depend on your [workflow version](../workflows/versions.md), but the following covers the full set.

---

## 2.1 OpenKlant API key

In the **OpenKlant** admin environment, go to **API Auth** and generate a token for OMC.

| Variable | Description |
|---|---|
| `ZGW_AUTH_KEY_OPENKLANT` | Required for workflow **v2 and above** |

![OpenKlant API key creation](../images/step%202%20image%201.png)

---

## 2.2 OpenZaak JWT credentials

OpenZaak uses JWT authentication. In the **OpenZaak** admin environment, create a new application/client with the following parameters and record them as environment variables:

| Variable | Description |
|---|---|
| `ZGW_AUTH_JWT_SECRET` | The secret key configured in OpenZaak |
| `ZGW_AUTH_JWT_ISSUER` | The client ID / issuer name configured in OpenZaak |
| `ZGW_AUTH_JWT_AUDIENCE` | The audience (optional) |
| `ZGW_AUTH_JWT_EXPIRESINMIN` | Token lifetime in minutes — set to `60` |
| `ZGW_AUTH_JWT_USERID` | The user ID associated with OMC (e.g. an email address) |
| `ZGW_AUTH_JWT_USERNAME` | A human-readable name for OMC (e.g. `"Municipality of Rotterdam"`) |

> The JWT secret and claims configured here **must match exactly** what is registered in the OpenZaak admin UI. OMC generates the JWT token internally using these values.

![OpenZaak JWT credentials](../images/step%202%20image%202.png)

---

## 2.3 Objecten API key

In the **Objecten** admin environment, generate a token for OMC.

| Variable | Description |
|---|---|
| `ZGW_AUTH_KEY_OBJECTEN` | API key for the Objecten service |

![Objecten API key creation](../images/step%202%20image%203.png)

---

## 2.4 ObjectTypen API key

In the **ObjectTypen** admin environment, generate a token for OMC.

| Variable | Description |
|---|---|
| `ZGW_AUTH_KEY_OBJECTTYPEN` | API key for the ObjectTypen service |

![ObjectTypen API key creation](../images/step%202%20image%204.png)

---

## 2.5 Service endpoints

Configure the base URLs for each ZGW service. All URLs must include the protocol (`https://`) and must **not** end with a trailing slash.

| Variable | Example value |
|---|---|
| `ZGW_ENDPOINT_OPENNOTIFICATIES` | `https://opennotificaties.mycity.nl/api/v1` |
| `ZGW_ENDPOINT_OPENZAAK` | `https://openzaak.mycity.nl/zaken/api/v1` |
| `ZGW_ENDPOINT_OPENKLANT` | `https://openklant.mycity.nl/klanten/api/v1` |
| `ZGW_ENDPOINT_BESLUITEN` | `https://openzaak.mycity.nl/besluiten/api/v1` |
| `ZGW_ENDPOINT_OBJECTEN` | `https://objecten.mycity.nl/api/v2` |
| `ZGW_ENDPOINT_OBJECTTYPEN` | `https://objecttypen.mycity.nl/api/v2` |
| `ZGW_ENDPOINT_CONTACTMOMENTEN` | `https://openklant.mycity.nl/contactmomenten/api/v1` |

> The exact paths depend on how your ZGW services are deployed. Check the admin UI or API documentation of each service to confirm the correct base path.
