## 1.17.3

- Fixes the functionality for the Case Created scenario to look at the triggering status's type for serialnumber (volgnummer) to be 1. If it is, the scenario "case created" will be triggered

## 1.17.2

- Makes preffered address ("voorkeursAdres") optional - if not filled this will require a digital reference to the zaak to allow notifications being sent.

## 1.17.1

- Changes Bsn to bsn because the queryparam doesnt allow for capitals.

## 1.17.0

- BREAKING CHANGE. Appsettings have changed because of unannounced change in open klant changing PartijIdentificator from a string to an Enum. This version will require openklant v2.12.0 or higher.  

## 1.16.0

- BREAKING CHANGE. ZGW_ENDPOINTS_ need to include Http protocol. E.g. "https://openzaak.test.nl/zaken/api/v1" 

## 1.15.8

- Adds environment variable OMC_CONTEXT_PATH to set a context path. Default empty string "".

## 1.15.7

- Adds contactmoment callback to documentation

## 1.15.6 

- Adds documentation for Case Created scenario

## 1.15.5

- Bugfix Handle multiple roles some without inpBsn.

## 1.15.4

- Bugfix wrongful setting of Distribution Channel sometimes causing errors in Notify NL.

## 1.15.3

- Update DetermineDistributionChannel to check against "Telefoon" and "telefoonnummer" as digitalAddressType since OpenKlant v2.4.0.

## 1.15.2

- Update some documentation. 

## 1.15.1

- Add more personal data to KTO call to Expoint. Makes breaking changes to Launchsettings. 
  See OMC - Documentation 1.1.2.1. Customizing profile. And mind the KTO_ section.

## 1.15.0

- Introduce Customer Satisfaction Survey by Expoint. When configured sends survey on callback. Makes breaking changes to Launchsettings. 
  See OMC - Documentation 1.1.2.1. Customizing profile. And mind the KTO_ section. 

## 1.14.6

- Make digital address type comparison to case insensitive. To i.e. accept "Email" and "email" alike.

## 1.14.5

- Missing .image.tag update on chart added.

## 1.14.4

- Base64 decoding corrected for post-merge deployment.

## 1.14.3

- Further updates to test and build automation.

## 1.14.2

- Patches CVE-2024-21907 and consolidates dependencies

## 1.14.1

- Version numbering patch

## 1.14.0

- Adds the option to override someone's preferred digital address based on case number

## 1.13.2

- Updates to test and build automation.

## 1.13.1

- Update documentation (old paths).
- Cleaning up code (Generic naming of method, streamlining parameters).
