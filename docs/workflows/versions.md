# Workflow versions

OMC supports multiple workflow versions to handle different versions of the ZGW APIs. The active version is set with the `OMC_FEATURE_WORKFLOW_VERSION` environment variable.

---

## Choosing a version

| Version | `OMC_FEATURE_WORKFLOW_VERSION` | Supported parties | OpenKlant version |
|---|---|---|---|
| **v1** | `1` | Citizens (BSN only) | v1.0.0 |
| **v2** | `2` | Citizens (BSN) + Organisations (KVK) | v2.0.0 |

Use **v2** for new deployments. v1 is maintained for backwards compatibility with environments still on OpenKlant v1.

---

## v1 — Service dependencies

| Service | Version |
|---|---|
| Open Notificaties | v1.6.0 |
| Open Zaak | v1.12.1 |
| **Open Klant** | **v1.0.0** |
| Besluiten | v1.1.0 |
| Objecten | v2.3.1 |
| ObjectTypen | v2.2.0 |
| Contactmomenten | v1.0.0 |

> `ZGW_AUTH_KEY_OPENKLANT` is not required for v1.

---

## v2 — Service dependencies

| Service | Version |
|---|---|
| Open Notificaties | v1.6.0 |
| Open Zaak | v1.12.1 |
| **Open Klant** | **v2.0.0** |
| Besluiten | v1.1.0 |
| Objecten | v2.3.1 |
| ObjectTypen | v2.2.0 |
| **Klantcontacten** | **v2.0.0** |

> v2 uses `ZGW_AUTH_KEY_OPENKLANT` (API key) instead of the shared JWT for OpenKlant. The `ZGW_ENDPOINT_CONTACTMOMENTEN` endpoint path also differs between v1 and v2 OpenKlant deployments.

---

## Breaking changes between versions

See the [Changelog](../project/changelog.md) for a full history. Notable breaking changes:

- **v1.17.0** — OpenKlant changed `PartijIdentificator` from a string to an enum. Requires OpenKlant v2.12.0+.
- **v1.16.0** — `ZGW_ENDPOINT_*` variables must now include the HTTP protocol (e.g. `https://...`).
- **v1.15.0** — KTO/Expoints integration introduced. Requires new `KTO_*` environment variables.
