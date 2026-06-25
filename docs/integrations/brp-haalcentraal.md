# BRP / Haal Centraal

OMC supports querying the Basisregistratie Personen (BRP) via the [Haal Centraal API](https://vng-realisatie.github.io/Haal-Centraal-BRP-bevragen/) to retrieve verified citizen address data for letter delivery.

---

## What OMC retrieves from BRP

When letter delivery is required and the citizen's address cannot be sourced from OpenKlant, OMC queries BRP for:

| Data | Used for |
|---|---|
| Street name, house number, postal code, city | Correspondence address for letter delivery |
| First name(s), last name | Template personalisation |
| Gender | Template personalisation |
| BSN | Used internally for lookup only — never logged or persisted |

---

## Security model

BRP access is secured by two independent layers:

### Layer 1 — OAuth2 token exchange via Keycloak

OMC exchanges its internal service token for a BRP-scoped access token through Keycloak. The token exchange audience is restricted to the BRP API, so the token cannot be reused for other services.

### Layer 2 — Mutual TLS (mTLS)

Every request to the BRP gateway uses a client certificate for mutual authentication. Both the certificate and its private key are provided as PEM files via environment variables.

> **BRP data is highly sensitive.** OMC does not persist BRP responses and does not log personal data.

---

## Configuration

| Variable | Description |
|---|---|
| `KEYCLOAK_AUTHSERVERURL` | Keycloak token endpoint (e.g. `https://kc.city.nl/realms/myrealm/protocol/openid-connect/token`) |
| `KEYCLOAK_CLIENTID` | Client ID registered in Keycloak for OMC BRP access |
| `KEYCLOAK_CLIENTSECRET` | Client secret for the above client |
| `KEYCLOAK_TOKENEXCHANGEAUDIENCE` | Audience for the token exchange — must match what the BRP API expects (e.g. `haalcentraal-brp`) |
| `BRP_BASEURL` | BRP / Haal Centraal API gateway URL (e.g. `https://wsgateway.city.nl/haalcentraal/api`) |
| `BRP_CLIENTCERT_PEM_PATH` | Absolute path to PEM-encoded mTLS client certificate |
| `BRP_CLIENTKEY_PEM_PATH` | Absolute path to PEM-encoded mTLS private key |

Certificates must be accessible by the running OMC process. In Kubernetes, mount them as a secret volume and set the paths accordingly.

---

## Operational notes

- BRP endpoints are accessed **only when explicitly required** by a workflow (e.g. letter delivery with no address in OpenKlant)
- All BRP requests include correlation identifiers for auditability
- Any misconfiguration of the Keycloak token exchange or gateway certificates will cause the request to fail — BRP is not queried silently
- The section "To be finished" in the original docs referred to edge-case operational guidance that is still being defined
