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
