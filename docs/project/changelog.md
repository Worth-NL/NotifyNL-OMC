# Changelog

## 2.0.0

- Upgrades .NET 8 → .NET 10
- Implements PostGuard integration for sending encrypted PDFs unlockable with Yivi wallets
- Renames all `Zhv` endpoint references to `Zgw`

## 1.17.19

- Adds custom GovUkNotify client to accept extras when sending letters; returns `202` on confirm without reference

## 1.17.18

- Adds Keycloak logic and BRP / Haal Centraal integration with verbose logging for testing

## 1.17.17

- Adds test endpoint for sending letters through NotifyNL

## 1.17.16

- Remaps Object to Expoints-specific payload

## 1.17.15

- New KTO implementation treating KTO as a notification

## 1.17.14

- First clean-up: logs outgoing API calls to ZGW and their responses in Sentry; no longer fetches case status and its type twice; checks notification expectation earlier in case scenarios

## 1.17.13

- Bugfix: catches errors from OpenKlant and surfaces them

## 1.17.12

- Bugfix: sets relevant services to scoped scope to prevent race conditions

## 1.17.11

- Prevents possible race conditions by not using QueryBase

## 1.17.10

- Adds `CaseResultType` and includes it in `NotifyData` for the Case Closed scenario

## 1.17.9

- Bugfix: if the initiator role has no BSN, does not attempt to query parties (OpenKlant returns a list in this case)

## 1.17.8

- Bugfix: removes KTO execution from `ITelemetryService`

## 1.17.7

- Bugfix: comparison was happening on description instead of reference; `ActorId` added

## 1.17.6

- Changes OpenKlant variables in `appsettings.json` to: `"CodeObjectType": "zaak"`, `"CodeRegister": "open-zaak"`, `"CodeObjectTypeId": "uuid"` per ZGW standards

## 1.17.5

- Bugfix: adds JSON escape logic when building `ContactMomentenJsonBody`

## 1.17.4

- Adds notification subject and body to the contact moment

## 1.17.3

- Fixes Case Created scenario to check the triggering status's `volgnummer` equals `1`

## 1.17.2

- Makes `voorkeursAdres` (preferred address) optional — if not set, a digital reference to the zaak is required for notifications to be sent

## 1.17.1

- Changes `Bsn` to `bsn` in query parameters (OpenKlant does not accept capitals)

## 1.17.0 ⚠️ Breaking change

- `appsettings.json` changed because OpenKlant changed `PartijIdentificator` from a string to an enum. Requires OpenKlant **v2.12.0 or higher**.

## 1.16.0 ⚠️ Breaking change

- `ZGW_ENDPOINT_*` variables must now include the HTTP protocol prefix (e.g. `https://openzaak.mycity.nl/...`)

## 1.15.8

- Adds `OMC_CONTEXT_PATH` environment variable for reverse proxy path prefix support. Default: empty string.

## 1.15.7

- Adds contact moment callback to documentation

## 1.15.6

- Adds documentation for Case Created scenario

## 1.15.5

- Bugfix: handles multiple roles where some have no `inpBsn`

## 1.15.4

- Bugfix: corrects wrongful setting of distribution channel that sometimes caused errors in NotifyNL

## 1.15.3

- Updates `DetermineDistributionChannel` to check against both `"Telefoon"` and `"telefoonnummer"` as digital address types (OpenKlant v2.4.0 changed the value)

## 1.15.2

- Documentation updates

## 1.15.1 ⚠️ Breaking change (launchSettings)

- Adds more personal data to KTO call to Expoints. Breaking changes to `launchSettings.json` — see `KTO_*` section in [environment variables](../configuration/environment-variables.md).

## 1.15.0 ⚠️ Breaking change (launchSettings)

- Introduces Customer Satisfaction Survey (KTO) integration via Expoints. Breaking changes to `launchSettings.json` — see `KTO_*` section.

## 1.14.6

- Makes digital address type comparison case-insensitive (accepts both `"Email"` and `"email"`)

## 1.14.5

- Missing `.image.tag` update on chart

## 1.14.4

- Corrects Base64 decoding for post-merge deployment

## 1.14.3

- Updates to test and build automation

## 1.14.2

- Patches CVE-2024-21907 and consolidates dependencies

## 1.14.1

- Version numbering patch

## 1.14.0

- Adds option to override a citizen's preferred digital address based on case number

## 1.13.2

- Updates to test and build automation

## 1.13.1

- Updates documentation (old paths)
- Code cleanup: generic method naming, streamlined parameters
