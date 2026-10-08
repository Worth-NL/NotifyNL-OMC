## 2.3.1

- Redesigns the `/status/flow` dashboard after the OMC3 "Stroom" view: fixed columns (Invoer → OMC → Uitvoer → Afleverbevestiging) with the registers in one row underneath, instead of an auto-laid-out graph whose lines crossed more with every scenario
- Merges Output Patronen and the checks into one OMC block: pick a flow and its row of checks lights up, in the order the scenario runs them, while registers and outputs it doesn't use are dimmed
- Clicking a check shows what it checks, where it is configured, what happens when it fails (206, 412 or skipped) and where it lives in the code, above its log
- Adds the Printstraat and Berichtenbox (MOBB) flows to the dashboard, with Printstraat and Logius Berichtenbox as outputs and the BerichtenAPI (OpenVTB) and DocumentenAPI as registers
- Traces the Berichtenbox (MOBB) flow: every check and every fallback (Berichtenbox → e-mail → letter via BRP) now shows up in the live trace. Previously a Bericht left no trace at all
- Calmer live trace: one dot per message that runs each line once. Steps that don't move the dot no longer take a full second, and the dot no longer restarts at every step, which made one message look like several
- Integrations that don't exist yet are hidden behind "Toon geplande koppelingen"; the Dagre layout dependency is removed
- The flow page is just the diagram with the live trace log beside it: the trace runs as soon as the page opens, and the introduction, the metrics bar (environment toggle, load, handling time, sparkline) and the trace/reset buttons are gone
- **The status dashboard is now opt-in.** Its pages and APIs (`/status`, `/status/flow`, `/status/scenarios`, `/status/stream`, `/status/trace/stream`) have no authentication, so they are only served with `DASHBOARD_ENABLED=true` and answer 404 otherwise. The Docker image only contains the dashboard when built with `--build-arg BUILD_DASHBOARD=true`. Environments that use the dashboard need both
- Keeps personal data out of the live trace stream: a failed register call's message carries the request URI (for an OpenKlant party search, with the BSN or KVK number in it) and the response body. Every trace detail is now scrubbed on its way out: no URI or response body, no query strings, long digit runs masked, length capped
- Updates the dashboard to Next.js 16.4.0 and pins KaTeX (pulled in by Mermaid) to 0.18.2 or later, clearing the critical and low npm audit findings in its runtime dependencies

## 2.3.0

- Adds the "product aangemaakt" scenario: a `producten`/`product`/`create` event from Open Product is fetched, checked against a product type whitelist, and e-mailed to every eigenaar of the product
- Organisations are now addressed by their name: when a partij has no surname, the organisation name fills `((klant.achternaam))`, so "Beste ((klant.voornaam)) ((klant.voorvoegselAchternaam)) ((klant.achternaam))" reads "Beste <bedrijfsnaam>" instead of "Beste " in every scenario
- An eigenaar with a `vestigingsnummer` is resolved to that vestiging in OpenKlant (`vestigingsnummer` scoped under its `kvk_nummer` via `subIdentificatorVan`), instead of to the organisation. Requires OpenKlant 2.16.0 or later
- Adds an Open Producten client (`ZGW_ENDPOINT_OPENPRODUCTEN`, `ZGW_AUTH_KEY_OPENPRODUCTEN`), including its masked-key, endpoint and connectivity rows on the status page
- Open Product is optional: every new variable may be left unset, and an empty `ZGW_ENDPOINT_OPENPRODUCTEN` switches the scenario off (status page shows it as disabled, product events are dropped with a 206). Existing deployments need no configuration changes to upgrade
- Adds `ZGW_WHITELIST_PRODUCTCREATE_IDS`, matched on the product type's `code` — Open Product has no `identificatie` field to whitelist on
- Resolves each eigenaar to an OpenKlant partij on its BSN or KVK number; an eigenaar's own UUID is Open Product's primary key and has no relation to OpenKlant
- Prefers the digital address a partij marked `portaalvoorkeur`, and restricts the address search to e-mail, so a partij whose preferred address is a phone number is no longer read as having no e-mail at all
- States the reason in the failed contactmoment (`Reden: …`): no e-mail address on file, a refusal or error from Notify NL, or the delivery status of a failed receipt. A failed contactmoment needs a partij, so an owner without one still ends the whole notification with a 206
- Registers a failed contactmoment per eigenaar who could not be notified; successful ones are written from the Notify NL afleverstatus callback, as with the other channels
- A partij that OpenKlant reports as not existing - for a BSN, KVK number or partij id - now aborts the notification with a 206 in every scenario, with a reason naming the kind of identifier (never its value). Previously the zaak scenarios answered 412, so Open Notificaties kept redelivering an event that could never succeed. OpenKlant being unreachable or erroring still answers 412 and is retried
- Notifies every eigenaar only once the product, its type, its publication state and all of its eigenaren have been validated — a failure in any of those notifies nobody

## 2.2.2

- Fixes letter notifications being sent with the SMS template: `LetterComponent`
- A letter notification whose template variable is unset now returns 412 and is redelivered by Open Notificaties, The service still starts without the letter templates, and e-mail, SMS, Berichtenbox and printstraat are unaffected
- Corrects environment variable names throughout the documentation: the BRP/Haal Centraal, KTO/Expoints and PostGuard integration pages documented variables the code never reads, so provisioning from them produced a silently degraded deployment. Scenario and architecture pages had drifted on objecttype, template and HTTP-pooling names
- Builds warning-free again: VSTHRD200 is disabled for test code, where the test naming convention makes an "Async" suffix noise, and two missing XML param tags were added to the `NotifyCallbackResponder` constructor

## 2.2.1

- Fixes the MijnZaken "zaak gemuteerd" outgoing CloudEvent to use the status's datumStatusGezet as its time, instead of the OMC's own processing time (or the case's laatstGemuteerd)

## 2.2.0

- Adds the print (printstraat) scenario: sends a pre-generated PDF letter triggered from an Objecten API object, registers the contactmoment from the delivery callback, and deletes the object afterwards
- Adds precompiled-letter sending to the Notify client, and records letters on the klantcontact as kanaal "brief"
- Treats Notify's validation-failed letter status as a delivery failure rather than a success
- Creates a missing OpenKlant partij on the fly instead of failing with a hard 412
- Threads the print request's originating object URL and the onderwerpobject's zaaktype onto its klantcontact
- Fixes a semaphore leak in HttpNetworkService: a failed HTTP call never released its permit, so repeated failures could exhaust the shared throttle and hang every subsequent outbound call until restart
- Fixes the MijnZaken "zaak geopend" filter so the natural-person check applies to a case's first opening too, instead of only once laatstGeopend is populated
- Rejects a MijnZaken hoofdObject whose last path segment is not a UUID, instead of deriving a "/" subject that was silently dropped further downstream
- Reports the MOBB scenario's permanent drops (empty message text, recipient without a BSN, message type not whitelisted) as a no-op success rather than a failure, so Open VTB no longer redelivers a Bericht that can never succeed
- Registers MessageBoxScenario in DI; a "berichten" notification threw instead of being handled

## 2.1.0

- Adds a "MijnZaken" endpoint that normalizes incoming ZGW CloudEvents/NotificationEvents and forwards them to MijnOverheid, with whitelist and natural-person filtering
- Adds support for zaak-geopend and zaak-verwijderd events (previously only zaak-gemuteerd was recognized from NotificationEvent-shaped input)
- Fixes MijnZaken payload binding, NotificationEvent field mapping, dataref format, and event timestamp accuracy so the endpoint works end-to-end
- Adds the configuration status dashboard (statically served React/Next.js SPA) with a live scenario-trace visualization
- Moves REST API documentation to the notifynl-api repository; page now redirects there
- Add MOBB (MijnOverheid Berichtenbox) scenario, with fallback to digitale post (email) and postal letter when Berichtenbox delivery isn't available or fails
- Fetch real attachment content from the Documenten API for MOBB/letter attachments, instead of forwarding the raw download URI
- Add letter fallback so a recipient with no digital address on file is no longer a hard failure
- Add letter template personalization and template ID resolution to match the real Notify NL MessageBoxLetter template

## 2.0.2

- Remove v1 workflow versioning: drop OpenKlant v1, OpenZaak v1/v2 distinction, and OMC_FEATURE_WORKFLOW_VERSION configuration
- Add integration test framework using WebApplicationFactory with launchSettings-based configuration and JWT token generation

## 2.0.1

* Adds documentation to a /docs folder to sync to gitbook
* Fix multiline changelog output in CI workflow

## 2.0.0

* Upgrades .net8.0 => to .net10.0
* Implements PostGuard endpoint to send encrypted pdfs unlockable with yivi wallets
* Renames all "Zhv" endpoints to "Zgw"

## 1.17.19

* Add custom GovUkNotify Client to accept extras when sending letters, and return 202 when confirm without reference.

## 1.17.18

* Add Keycloak logic and Brp/Haalcentraal logic with verbose logging for testing

## 1.17.17

* Add Test Endpoint for sending Letters through NotifyNL

## 1.17.16

* Remap Object to Expoints specific Payload

## 1.17.15

* Add New KTO implementation treating Kto as a notification

## 1.17.14

* First Clean-Up, Logs outgoing api calls to ZGW and there responses in Sentry, No longer fetches CaseStatus and it's Type twice, For Case Scenarios it checks if notification is expected earlier in the process

## 1.17.13

* Bugfix. Catches bugs in openklant and throws them

## 1.17.12

* Bugfix. Set relevant services to scoped to prevent race conditions

## 1.17.11

* Prevent possible race conditions by not using QueryBase

## 1.17.10

* Adds CaseResultType, And adds it to NotifyData in CaseClosedScenario

## 1.17.9

* Bugfix if the initiator role has no BSN, dont try to query the parties because openklant returns a list for some reason

## 1.17.8

* Bugfix Remove KTO Execution from ITelemetryService

## 1.17.7

* Bugfix Comparison was happening on Description instead of Reference, ActorId added

## 1.17.6

* Changes OpenKlant Variables in AppSettings to: "CodeObjectType": "zaak","CodeRegister": "open-zaak","CodeObjectTypeId": "uuid". According to ZGW standards.

## 1.17.5

* BUGFIX. Adds Escape Json Logic to building ContactMomentenJsonBody.

## 1.17.4

* Adds the body and subject of the notification to the ContactMoment.

## 1.17.3

* Fixes the functionality for the Case Created scenario to look at the triggering status's type for serialnumber (volgnummer) to be 1. If it is, the scenario "case created" will be triggered

## 1.17.2

* Makes preffered address ("voorkeursAdres") optional - if not filled this will require a digital reference to the zaak to allow notifications being sent.

## 1.17.1

* Changes Bsn to bsn because the queryparam doesnt allow for capitals.

## 1.17.0

* BREAKING CHANGE. Appsettings have changed because of unannounced change in open klant changing PartijIdentificator from a string to an Enum. This version will require openklant v2.12.0 or higher.

## 1.16.0

* BREAKING CHANGE. ZGW\_ENDPOINTS\_ need to include Http protocol. E.g. "https://openzaak.test.nl/zaken/api/v1"

## 1.15.8

* Adds environment variable OMC\_CONTEXT\_PATH to set a context path. Default empty string "".

## 1.15.7

* Adds contactmoment callback to documentation

## 1.15.6

* Adds documentation for Case Created scenario

## 1.15.5

* Bugfix Handle multiple roles some without inpBsn.

## 1.15.4

* Bugfix wrongful setting of Distribution Channel sometimes causing errors in Notify NL.

## 1.15.3

* Update DetermineDistributionChannel to check against "Telefoon" and "telefoonnummer" as digitalAddressType since OpenKlant v2.4.0.

## 1.15.2

* Update some documentation.

## 1.15.1

* Add more personal data to KTO call to Expoint. Makes breaking changes to Launchsettings.
See OMC - Documentation 1.1.2.1. Customizing profile. And mind the KTO\_ section.

## 1.15.0

* Introduce Customer Satisfaction Survey by Expoint. When configured sends survey on callback. Makes breaking changes to Launchsettings.
See OMC - Documentation 1.1.2.1. Customizing profile. And mind the KTO\_ section.

## 1.14.6

* Make digital address type comparison to case insensitive. To i.e. accept "Email" and "email" alike.

## 1.14.5

* Missing .image.tag update on chart added.

## 1.14.4

* Base64 decoding corrected for post-merge deployment.

## 1.14.3

* Further updates to test and build automation.

## 1.14.2

* Patches CVE-2024-21907 and consolidates dependencies

## 1.14.1

* Version numbering patch

## 1.14.0

* Adds the option to override someone's preferred digital address based on case number

## 1.13.2

* Updates to test and build automation.

## 1.13.1

* Update documentation (old paths).
* Cleaning up code (Generic naming of method, streamlining parameters).

