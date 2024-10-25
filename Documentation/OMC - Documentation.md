<h1 id="start">OMC Documentation</h1>

v.1.11.3

© 2023-2024, Worth Systems.

---

<h1 id="table">Table of contents</h1>

1. [Introduction](#introduction)

   * [Open services](#openServices_list)
   * [Notify](#notify_list)

   - 1.1. [Swagger UI](#swagger_ui)

      - 1.1.1. [Using web browser](#web_browser)

      - 1.1.2. [Using IDE (Visual Studio)](#visual_studio)

         - 1.1.2.1. [Customizing profile](#custom_lanunchSettings_profile)
    
         - 1.1.2.2. [Running profile](#running_profile)

    - 1.2. [Docker](#docker)

2. [Architecture](#architecture)

3. [Setup](#setup)

   - 3.1. [Configurations](#configurations)

      - 3.1.1. [appsettings.json](#appsettings)

         - 3.1.1.1. [Example](#appsettings_example)

      - 3.1.2. [Environment variables](#environment_variables)

         - 3.1.2.1. [Example](#environment_variables_example)

         - 3.1.2.2. [Get environment variables](#get_environment_variables)

         - 3.1.2.3. [Set environment variables](#set_environment_variables)

         - 3.1.2.4. [Using HELM Charts](#helm_charts)

4. [Authorization and authentication](#authorization)

   - 4.1. [JSON Web Tokens](#jwt_tokens)

      - 4.1.1. [Required components](#jwt_required_components)

         - 4.1.1.1. [Header (algorithm + type)](#jwt_header)

         - 4.1.1.2. [Payload (claims)](#jwt_claims)

         - 4.1.1.3. [Signature (secret)](#jwt_secret)

      - 4.1.2. [Mapping of JWT claims from environment variables](#jwt_mapping_environment_variables)

      - 4.1.3. [Using generated JSON Web Token (JWT)](#jwt_generating)

         - 4.1.3.1. [Postman (authorization)](#postman_authorization)

         - 4.1.3.2. [Swagger UI (authorization)](#swagger_ui_authorization)

5. [OMC Workflow](#omc_workflow)

   - 5.1. [Versions](#workflow_versions)

      - 5.1.1. [Dependencies](#workflow_dependencies)

         - 5.1.1.1. [OMC workflow v1 `(default)`](#omc_workflow_v1)

         - 5.1.1.1. [OMC workflow v2](#omc_workflow_v2)

   - 5.2. [Scenarios](#scenarios)

      - 5.2.1. [General introduction](#scenarios_general_introduction)

         - 5.2.1.1. [Notification](#scenarios_general_notification)

         - 5.2.1.2. [Environment variables](#scenarios_general_environment_variables)

         - 5.2.1.3. [Requirements](#scenarios_general_requirements)

         - 5.2.1.4. [Template placeholders](#scenarios_general_template_placeholders)

      - [Examples](#scenarios_examples)

      - 5.2.2. [Case Created](#case_created)

         - 5.2.2.1. [Notification](#case_created_notification)

         - 5.2.2.2. [Environment variables](#case_created_environment_variables)

         - 5.2.2.3. [Requirements](#case_created_requirements)

         - 5.2.2.4. [Template placeholders](#case_created_template_placeholders)

      - 5.2.3. [Case Updated](#case_updated)

         - 5.2.3.1. [Notification](#case_updated_notification)

         - 5.2.3.2. [Environment variables](#case_updated_environment_variables)

         - 5.2.3.3. [Requirements](#case_updated_requirements)

         - 5.2.3.4. [Template placeholders](#case_updated_template_placeholders)

      - 5.2.4. [Case Closed](#case_closed)

         - 5.2.4.1. [Notification](#case_closed_notification)

         - 5.2.4.2. [Environment variables](#case_closed_environment_variables)

         - 5.2.4.3. [Requirements](#case_closed_requirements)

         - 5.2.4.4. [Template placeholders](#case_closed_template_placeholders)

      - 5.2.5. [Task Assigned](#task_assigned)

         - 5.2.5.1. [Notification](#task_assigned_notification)

         - 5.2.5.2. [Environment variables](#task_assigned_environment_variables)

         - 5.2.5.3. [Requirements](#task_assigned_requirements)

         - 5.2.5.4. [Template placeholders](#task_assigned_template_placeholders)

      - 5.2.6. [Decision Made](#decision_made)

         - 5.2.6.1. [Notification](#decision_made_notification)

         - 5.2.6.2. [Environment variables](#decision_made_environment_variables)

         - 5.2.6.3. [Requirements](#decision_made_requirements)

         - 5.2.6.4. [Template placeholders](#decision_made_template_placeholders)

      - 5.2.7. [Message Received](#message_received)

         - 5.2.7.1. [Notification](#message_received_notification)

         - 5.2.7.2. [Environment variables](#message_received_environment_variables)

         - 5.2.7.3. [Requirements](#message_received_requirements)

         - 5.2.7.4. [Template placeholders](#message_received_template_placeholders)

      - 5.2.99. [Not Implemented](#not_implemented_scenario)

6. [Errors](#errors)

   - 6.1. [Events Controller](#errors_events_controller)

      - 6.1.1. [Possible errors](#errors_events_controller_possible_errors)

   - 6.2. [Notify Controller](#errors_notify_controller)

      - 6.2.1. [Possible errors](#errors_notify_controller_possible_errors)

   - 6.3. [Test Controller](#errors_test_controller)

      - 6.3.1. [Testing Notify](#errors_test_controller_notify)

         - 6.3.1.1. [Possible errors](#errors_test_controller_notify_possible_errors)
         
            a) [Common for SendEmail + SendSms](#errors_test_controller_notify_common)

            b) [SendEmail](#errors_test_controller_notify_common_sendEmail)

            c) [SendSms](#errors_test_controller_notify_common_sendSms)

      - 6.3.2. [Testing Open services](#errors_test_controller_open)

         - 6.3.2.1. [Possible errors](#errors_test_controller_open_possible_errors)

---
<h1 id="introduction">1. Introduction</h1>

<sup>[Go back](#start)</sup>

**OMC (Output Management Component)** is a central point and the common hub of the communication workflow between third parties software such as:

<h4 id="openServices_list">Open services (repositories)</h4>

- [**Open Notificaties**](https://github.com/open-zaak/open-notificaties) (Web API service)
- [**Open Zaak**](https://github.com/open-zaak/open-zaak) (Web API service)
- [**Open Klant**](https://github.com/maykinmedia/open-klant) (Web API service)
- [**Besluiten**](https://github.com/open-zaak/open-zaak) (Web API service)  `NOTE: It's part of Open Zaak repository`
- [**Objecten**](https://github.com/maykinmedia/objects-api) (Web API service)
- [**ObjectTypen**](https://github.com/maykinmedia/objecttypes-api) (Web API service)
- [**Klantinteracties**](https://vng-realisatie.github.io/klantinteracties/) (Web API service)

<h4 id="notify_list">Notify</h4>

- [**Notify NL**](https://github.com/Worth-NL/notifications-api) (Web API service) => based on [**Notify UK**](https://www.notifications.service.gov.uk/)
    
    \- Web API service (Python)

    \- Language-specific clients (e.g., C#, JavaScript, PHP)
    > **OMC** is written in C# and using .NET Client for Notify.

    \- Webpage: admin portal

> **NOTE:** Different versions of these external API services are handled by, so-called "[OMC Workflows](#workflow_versions)".

<h2 id="swagger_ui">1.1. Swagger UI</h2>

Since the **OMC** project is just an API, it would not have any user-friendly graphic representation if used as a standalone RESTful ASP.NET Web API project.

That's why **ASP.NET** projects are usually exposing a UI presentation layer for the convenience of future users (usually developers). To achieve this effect, we are using so called [Swagger UI](https://swagger.io/tools/swagger_ui/), a standardized **HTML**/**CSS**/**JavaScript**-based suite of tools and assets made to generate visualized API endpoints, API documentation, data models schema, data validation, interaction with user (API responses), and other helpful hints on how to use the certain API.

**Swagger UI** can be accessed just like a regular webpage, or when you are starting your project in your IDE (preferably **Visual Studio**).

![Invalid base URL - Error](images/swagger_ui_example.png)

**NOTE**: Check the section dedicated to [requests authorization](#swagger_ui_authorization) when using **Swagger UI**.

<h3 id="web_browser">1.1.1. Using web browser</h3>

The URL to **Swagger UI** can be recreated in the following way:

- [Protocol*] + [Domain**] + `/swagger/index.html`

For example: https://omc.acc.notifynl.nl/swagger/index.html

\* Usually https
\** Where your **OMC** Web API application is deployed

<h3 id="visual_studio">1.1.2. Using IDE (Visual Studio)</h3>

To run the application locally (using **Visual Studio**) select one of the `launchSettings.json` **profiles** to start **Swagger UI** page in your browser (which will be using `/localhost:...` address).

By default these **profiles** are already defined:

- `http`
- `https`
- `IIS Express`

And all of them have **Swagger UI** specified as the default start option.

![Invalid base URL - Error](images/swagger_ui_launch_settings.png)

> **NOTE:** In this example application will start in "Development" mode.

<h4 id="custom_lanunchSettings_profile">1.1.2.1. Customizing profile</h4>

> Full content of `launchSettings.json` file.

```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "dotnetRunMessages": true,
      "applicationUrl": "http://localhost:0000"
    },
    "https": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "dotnetRunMessages": true,
      "applicationUrl": "https://localhost:0001;http://localhost:0000"
    },
    "IIS Express (Development)": {  // NOTE: Name of the profile can be changed
      "commandName": "IISExpress",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",  // NOTE: Application mode can be changed
		 
        "OMC_AUTHORIZATION_JWT_SECRET": "",
        "OMC_AUTHORIZATION_JWT_ISSUER": "",
        "OMC_AUTHORIZATION_JWT_AUDIENCE": "",
        "OMC_AUTHORIZATION_JWT_EXPIRESINMIN": "",
        "OMC_AUTHORIZATION_JWT_USERID": "OMC (Development)",  // NOTE: Optional place to reflect application mode
        "OMC_AUTHORIZATION_JWT_USERNAME": "OMC (Development)",  // NOTE: Optional place to reflect application mode

        "OMC_FEATURES_WORKFLOW_VERSION": "",
        
        "ZGW_AUTHORIZATION_JWT_SECRET": "",
        "ZGW_AUTHORIZATION_JWT_ISSUER": "",
        "ZGW_AUTHORIZATION_JWT_AUDIENCE": "",
        "ZGW_AUTHORIZATION_JWT_EXPIRESINMIN": "",
        "ZGW_AUTHORIZATION_JWT_USERID": "",
        "ZGW_AUTHORIZATION_JWT_USERNAME": "",
        
        "ZGW_API_KEY_OPENKLANT": "", // NOTE: Not required if OMC Workflow v1 is used
        "ZGW_API_KEY_OBJECTEN": "",
        "ZGW_API_KEY_OBJECTTYPEN": "",
        "ZGW_API_KEY_NOTIFYNL": "",
        
        "ZGW_DOMAIN_OPENNOTIFICATIES": "",
        "ZGW_DOMAIN_OPENZAAK": "",
        "ZGW_DOMAIN_OPENKLANT": "",
        "ZGW_DOMAIN_OBJECTEN": "",
        "ZGW_DOMAIN_OBJECTTYPEN": "",
        "ZGW_DOMAIN_CONTACTMOMENTEN": "",
        
        "ZGW_TEMPLATEIDS_EMAIL_ZAAKCREATE": "",
        "ZGW_TEMPLATEIDS_EMAIL_ZAAKUPDATE": "",
        "ZGW_TEMPLATEIDS_EMAIL_ZAAKCLOSE": "",
        "ZGW_TEMPLATEIDS_EMAIL_TASKASSIGNED": "",
        "ZGW_TEMPLATEIDS_EMAIL_MESSAGERECEIVED": "",
        
        "ZGW_TEMPLATEIDS_SMS_ZAAKCREATE": "",
        "ZGW_TEMPLATEIDS_SMS_ZAAKUPDATE": "",
        "ZGW_TEMPLATEIDS_SMS_ZAAKCLOSE": "",
        "ZGW_TEMPLATEIDS_SMS_TASKASSIGNED": "",
        "ZGW_TEMPLATEIDS_SMS_MESSAGERECEIVED": "",

        "ZGW_TEMPLATEIDS_DECISIONMADE": "",

        "ZGW_WHITELIST_ZAAKCREATE_IDS": "",
        "ZGW_WHITELIST_ZAAKUPDATE_IDS": "",
        "ZGW_WHITELIST_ZAAKCLOSE_IDS": "",
        "ZGW_WHITELIST_TASKASSIGNED_IDS": "",
        "ZGW_WHITELIST_DECISIONMADE_IDS": "",
        "ZGW_WHITELIST_MESSAGE_ALLOWED": "false",
        "ZGW_WHITELIST_TASKOBJECTTYPE_UUID": "",
        "ZGW_WHITELIST_MESSAGEOBJECTTYPE_UUID": "",
        "ZGW_WHITELIST_DECISIONINFOOBJECTTYPE_UUIDS": "",

        "ZGW_VARIABLES_OBJECTEN_MESSAGEOBJECTTYPE_VERSION" : "",
        
        "NOTIFY_API_BASEURL": "",
        
        "SENTRY_DSN": "",
        "SENTRY_ENVIRONMENT": "Worth Systems (Development)"  // NOTE: Optional place to reflect application instance and mode
      }
    },
    "Docker": {
      "commandName": "Docker",
      "launchBrowser": true,
      "launchUrl": "{Scheme}://{ServiceHost}:{ServicePort}/swagger",
      "publishAllPorts": true,
      "useSSL": true
    }
  },
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:00002",
      "sslPort": 00003
    }
  }
}
```

> **NOTE:** An example of customized "IIS Express (Development)" profile (with environment variables overruling those defined directly in Windows OS).

The developer can create more than one launch profile:
> e.g., for testing **OMC Workflow v1** (pointing to older domains) and **OMC Workflow vXYZ** (pointing to newer domains). Both using different credentials, template IDs, application modes (_Production_, _Development_, _Test_), names, logging identifiers (Sentry.io), etc.

![Multiple custom launch profiles - Visual Studio](images/launchProfiles_many_custom.png)

![Multiple custom launch profiles - launchSettings.json](images/launchSettings_many_custom.png)

<h4 id="running_profile">1.1.2.2. Running profile</h4>

![Invalid base URL - Error](images/launchProfiles_full.png)

<h2 id="docker">1.2. Docker</h2>

- After cloning **OMC** Git repository:
> git@github.com:Worth-NL/NotifyNL-OMC.git

- Go to the root catalog:
> .../NotifyNL-OMC

- And run the following **docker** command:
> docker build -f EventsHandler/Api/EventsHandler/Dockerfile --force-rm -t `omc` .
>
> **NOTE:** `omc` is just a name of your **docker image** and it can be anything you want.

The command from above is addressing the issue with building **docker image** from the `Dockerfile` location:
`ERROR: failed to solve: failed to compute cache key: failed to calculate checksum of ref`

![Docker - Failed to compute cache key](images/docker_error_failed_to_compute_cache_key.png)

- From this moment follow the **HELM Chart** documentation to set up _environment variables_
in order to run an already created **docker container**.
 
---
<h1 id="architecture">2. Architecture</h1>

<sup>[Go back](#start)</sup>

[Scenarios](#scenarios) implemented in **OMC** are following _Strategy Design Pattern_, and they are using JSON data deserialized into _POCO (Plain Old CLR Object)_ models, and passed as _DTO (Data Transfer Object)_ models to query services (reflecting the external micro-services architecture of third-party "Open Services"). Query services are aggregated under _IQueryContext_ and its implementation _QueryContext_ - following _Adapter Design Pattern_ thanks to which queries can be agnostic (dependencies resolved internally) and organized within a single testable abstraction, giving the developers access to all available API query methods.

---
<h1 id="setup">3. Setup</h1>

<sup>[Go back](#start)</sup>

<h2 id="configurations">3.1. Configurations</h2>

**OMC API** and related sub-systems (e.g., **Secrets Manager**) are using two types of configurations:

> - public (`appsettings.json`)
> - private (`environment variables`)

Which can also be divided into other two categories:

> - universal settings (not changing very often; basic/default behavior of **OMC**)
> - customizable settings (which may vary between **OMC** instances; business)

Easier to monitor, test, modify, and maintain by developers are `appsettings.json`,
but `environment variables` are easier to be adjusted by the end users of **OMC**.

<h3 id="appsettings">3.1.1. `appsettings.json`</h3>

> Made for public configurations (can be preserved in the code). They are not meant to be changed very often.

![Invalid base URL - Error](images/appsettings.png)

> **NOTE:** Here are defined settings related to HTTP connection, encryption used for JWT tokens to authorize HTTP requests to / from other Web API services, or default variables defining **OMC** domain setup - adjusting how the generic and agnostic [**Open Services**](#openServices_list) will be utilized.

<h4 id="appsettings_example">3.1.1.1. Example</h4>

> Full content of `appsettings.json` file.

```JSON
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

  // Predefined variables based on which the application workflow currently relies.
  // NOTE: Their default values should not be changed, unless they are also adopted on
  //       the OpenZaak and OpenKlant API sides (which are more dynamic than OMC API).
  "Variables": {
    // ENG: Subject type (e.g., person or organization)
    "BetrokkeneType": "natuurlijk_persoon",
    // ENG: General description => "initiator role"
    "OmschrijvingGeneriek": "initiator",
    // ENG: Party identifier => e.g., "citizen identifier"
    "PartijIdentificator": "Burgerservicenummer",
    // ENG: Email general description (e.g., "email", "e-mail", "Email"...)
    "EmailOmschrijvingGeneriek": "Email",
    // ENG: Phone general description (e.g., "phone", "mobile", "nummer"...)
    "TelefoonOmschrijvingGeneriek": "Telefoon",

    "OpenKlant": {
      "CodeObjectType": "Zaak",
      "CodeRegister": "ZRC",
      "CodeObjectTypeId": "identificatie"
    },

    // User communication: Messages to be put into register exposed to citizens
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

You can determine which _appsettings[...].json_ configuration will be used by setting a respective value of `ASPNETCORE_ENVIRONMENT` property in `environmentVariables` in your _launch profile_ defined in `launchSettings.json`. The supported values are:

- "Production"
- "Development"
- "Test"

![Configuration in appsettings.json](images/environment_varibles_aspnetcore.png)

During the start of the **OMC** application the content of `appsettings.[ASPNETCORE_ENVIRONMENT].json` file will be loaded.

> **NOTE:** Sometimes, in the documentation or in the code, when referring to this value a name "application mode(s)" might be used - because this _environment variable_ is usually defining the global setup / behavior of any **.NET** application.

<h3 id="environment_variables">3.1.2. Environment variables</h3>

> Meant to store sensitive configurations and / or customizable per instances of the **OMC** application).

<h4 id="environment_variables_example">3.1.2.1. Example</h4>

| Name*                                               | .NET Type | Example                                       | Is sensitive | Validation                                                                                                                                 | Notes                                                                                                                                                                                                        |
| --------------------------------------------------- | --------- | --------------------------------------------- | ------------ | ------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **.NET**                                            |           |                                               |              |                                                                                                                                            | ---                                                                                                                                                                                                          |
| ASPNETCORE_ENVIRONMENT                              | string    | "Development", "Production", "Test"           | false        | Cannot be missing and have null or empty value                                                                                             | Defines in which mode (environment) the OMC applicatio is running                                                                                                                                            |
| ---                                                 | ---       | ---                                           | ---          | ---                                                                                                                                        | ---                                                                                                                                                                                                          |
| **OMC**                                             |           |                                               |              |                                                                                                                                            |                                                                                                                                                                                                              |
| OMC_AUTHORIZATION_JWT_SECRET                        | string    | "abcd123t2gw3r8192dewEg%wdlsa3e!"             | true         | Cannot be missing and have null or empty value                                                                                             | For security reasons it should be at least 64 bytes long                                                                                                                                                     |
| OMC_AUTHORIZATION_JWT_ISSUER                        | string    | "OMC"                                         | true         | Cannot be missing and have null or empty value                                                                                             | Something identifying Notify NL (OMC Web API) service (it will be used internally) - The OMC is the issuer                                                                                                   |
| OMC_AUTHORIZATION_JWT_AUDIENCE                      | string    | "OMC"                                         | true         | Cannot be missing                                                                                                                          | Something identifying Notify NL (OMC Web API) service (it will be used internally) - The OMC is the audience                                                                                                 |
| OMC_AUTHORIZATION_JWT_EXPIRESINMIN                  | ushort    | "60"                                          | true         | Cannot be missing and have null or empty value                                                                                             | The OMC JWT tokens are generated by OMC and authorized by Open services. New JWT token has to be generated manually, using OMC dedicated library, if the token validity expire (by default it is 60 minutes) |
| OMC_AUTHORIZATION_JWT_USERID                        | string    | "tester"                                      | false        | Cannot be missing and have null or empty value                                                                                             | The OMC JWT tokens are generated by OMC and authorized by Open services. New JWT token has to be generated manually, using OMC dedicated library, if the token validity expire (by default it is 60 minutes) |
| OMC_AUTHORIZATION_JWT_USERNAME                      | string    | "Charlotte Sanders"                           | false        | Cannot be missing and have null or empty value                                                                                             | The OMC JWT tokens are generated by OMC and authorized by Open services. New JWT token has to be generated manually, using OMC dedicated library, if the token validity expire (by default it is 60 minutes) |
| ---                                                 | ---       | ---                                           | ---          | ---                                                                                                                                        | ---                                                                                                                                                                                                          |
| **Features:** of the "OMC" Web API                  |           |                                               |              |                                                                                                                                            |                                                                                                                                                                                                              |
| OMC_FEATURES_WORKFLOW_VERSION                       | byte      | "1"                                           | false        | Cannot be missing and have null or empty value. It also needs to correspond to already supported [OMC Workflows](#workflow_versions)       | The list of supported OMC workflows can be found [here](#workflow_dependencies)                                                                                                                              |
| ---                                                 | ---       | ---                                           | ---          | ---                                                                                                                                        | ---                                                                                                                                                                                                          |
| **ZGW**                                             |           |                                               |              |                                                                                                                                            |                                                                                                                                                                                                              |
| ZGW_AUTHORIZATION_JWT_SECRET                        | string    | "abcd123t2gw3r8192dewEg%wdlsa3e!"             | true         | Cannot be missing and have null or empty value                                                                                             | Internal implementation of Open services is regulating this, however it's better to use something longer as well                                                                                             |
| ZGW_AUTHORIZATION_JWT_ISSUER                        | string    | "Open Services"                               | true         | Cannot be missing and have null or empty value                                                                                             | Something identifying "OpenZaak" / "OpenKlant" / "OpenNotificatie" Web API services (token is shared between of them)                                                                                        |
| ZGW_AUTHORIZATION_JWT_AUDIENCE                      | string    | "OMC"                                         | true         | Cannot be missing                                                                                                                          | Something identifying OMC Web API service (it will be used internally) - The OMC is the audience                                                                                                             |
| ZGW_AUTHORIZATION_JWT_EXPIRESINMIN                  | ushort    | "60"                                          | true         | Cannot be missing and have null or empty value                                                                                             | This JWT token will be generated from secret, and other JWT claims, configured from UI of OpenZaak Web API service. Identical details (secret, iss, aud, exp, etc) as in Open services needs to be used here |
| ZGW_AUTHORIZATION_JWT_USERID                        | string    | "admin"                                       | false        | Cannot be missing and have null or empty value                                                                                             | This JWT token will be generated from secret, and other JWT claims, configured from UI of OpenZaak Web API service. Identical details (secret, iss, aud, exp, etc) as in Open services needs to be used here |
| ZGW_AUTHORIZATION_JWT_USERNAME                      | string    | "Municipality of Rotterdam"                   | false        | Cannot be missing and have null or empty value                                                                                             | This JWT token will be generated from secret, and other JWT claims, configured from UI of OpenZaak Web API service. Identical details (secret, iss, aud, exp, etc) as in Open services needs to be used here |
| ZGW_API_KEY_OPENKLANT                               | string    | "43dcba52d312d1e00bc..."                      | true         | Cannot be missing and have null or empty value (if you are using OMC Workflow v2 and above; otherwise, it's not mandatory)                 | It needs to be generated for OMC Workflow v2 and above from "OpenKlant" 2.0 Web API service UI                                                                                                               |
| ZGW_API_KEY_OBJECTEN                                | string    | "56abcd24e75c02d44ee..."                      | true         | Cannot be missing and have null or empty value                                                                                             | It needs to be generated from "Objecten" Web API service UI                                                                                                                                                  |
| ZGW_API_KEY_OBJECTTYPEN                             | string    | "647c4eg120f98ed5f5a..."                      | true         | Cannot be missing and have null or empty value                                                                                             | It needs to be generated from "ObjectTypen" Web API service UI                                                                                                                                               |
| ZGW_API_KEY_NOTIFYNL                                | string    | "name-8-4-4-4-12-8-4-4-4-12" (ID + UUID x2)   | true         | Cannot be missing and have null or empty value + must be in name-UUID-UUID format + must pass Notify NL validation                         | It needs to be generated from "Notify NL" Admin Portal                                                                                                                                                       |
| ---                                                 | ---       | ---                                           | ---          | ---                                                                                                                                        | ---                                                                                                                                                                                                          |
| ZGW_DOMAIN_OPENNOTIFICATIES                         | string    | "opennotificaties.mycity.nl/api/v1"           | false        | Cannot be missing and have null or empty value + only domain should be used: without protocol (http / https). Without slash at the end     | You have to use the domain part from URLs where you are hosting the dedicated Open services                                                                                                                  |
| ZGW_DOMAIN_OPENZAAK                                 | string    | "openzaak.mycity.nl/zaken/api/v1"             | false        | Cannot be missing and have null or empty value + only domain should be used: without protocol (http / https). Without slash at the end     | You have to use the domain part from URLs where you are hosting the dedicated Open services                                                                                                                  |
| ZGW_DOMAIN_OPENKLANT                                | string    | "openklant.mycity.nl/klanten/api/v1"          | false        | Cannot be missing and have null or empty value + only domain should be used: without protocol (http / https). Without slash at the end     | You have to use the domain part from URLs where you are hosting the dedicated Open services                                                                                                                  |
| ZGW_DOMAIN_BESLUITEN                                | string    | "besluiten.mycity.nl/besluiten/api/v1"        | false        | Cannot be missing and have null or empty value + only domain should be used: without protocol (http / https). Without slash at the end     | You have to use the domain part from URLs where you are hosting the dedicated Open services                                                                                                                  |
| ZGW_DOMAIN_OBJECTEN                                 | string    | "objecten.mycity.nl/api/v2"                   | false        | Cannot be missing and have null or empty value + only domain should be used: without protocol (http / https). Without slash at the end     | You have to use the domain part from URLs where you are hosting the dedicated Open services                                                                                                                  |
| ZGW_DOMAIN_OBJECTTYPEN                              | string    | "objecttypen.mycity.nl/api/v2"                | false        | Cannot be missing and have null or empty value + only domain should be used: without protocol (http / https). Without slash at the end     | You have to use the domain part from URLs where you are hosting the dedicated Open services                                                                                                                  |
| ZGW_DOMAIN_CONTACTMOMENTEN                          | string    | "openklant.mycity.nl/contactmomenten/api/v1"  | false        | Cannot be missing and have null or empty value + only domain should be used: without protocol (http / https). Without slash at the end     | You have to use the domain part from URLs where you are hosting the dedicated Open services                                                                                                                  |
| ---                                                 | ---       | ---                                           | ---          | ---                                                                                                                                        | ---                                                                                                                                                                                                          |
| ZGW_TEMPLATEIDS_EMAIL_ZAAKCREATE                    | GUID**    | "00000000-0000-0000-0000-000000000000"        | false        | Cannot be missing and have null or empty value + must be in UUID format                                                                    | Should be generated per specific business use case from "Notify NL" Admin Portal                                                                                                                             |
| ZGW_TEMPLATEIDS_EMAIL_ZAAKUPDATE                    | GUID      | "00000000-0000-0000-0000-000000000000"        | false        | Cannot be missing and have null or empty value + must be in UUID format                                                                    | Should be generated per specific business use case from "Notify NL" Admin Portal                                                                                                                             |
| ZGW_TEMPLATEIDS_EMAIL_ZAAKCLOSE                     | GUID      | "00000000-0000-0000-0000-000000000000"        | false        | Cannot be missing and have null or empty value + must be in UUID format                                                                    | Should be generated per specific business use case from "Notify NL" Admin Portal                                                                                                                             |
| ZGW_TEMPLATEIDS_EMAIL_TASKASSIGNED                  | GUID      | "00000000-0000-0000-0000-000000000000"        | false        | Cannot be missing and have null or empty value + must be in UUID format                                                                    | Should be generated per specific business use case from "Notify NL" Admin Portal                                                                                                                             |
| ZGW_TEMPLATEIDS_EMAIL_MESSAGE                       | GUID      | "00000000-0000-0000-0000-000000000000"        | false        | Cannot be missing and have null or empty value + must be in UUID format                                                                    | Should be generated per specific business use case from "Notify NL" Admin Portal                                                                                                                             |
| ZGW_TEMPLATEIDS_SMS_ZAAKCREATE                      | GUID      | "00000000-0000-0000-0000-000000000000"        | false        | Cannot be missing and have null or empty value + must be in UUID format                                                                    | Should be generated per specific business use case from "Notify NL" Admin Portal                                                                                                                             |
| ZGW_TEMPLATEIDS_SMS_ZAAKUPDATE                      | GUID      | "00000000-0000-0000-0000-000000000000"        | false        | Cannot be missing and have null or empty value + must be in UUID format                                                                    | Should be generated per specific business use case from "Notify NL" Admin Portal                                                                                                                             |
| ZGW_TEMPLATEIDS_SMS_ZAAKCLOSE                       | GUID      | "00000000-0000-0000-0000-000000000000"        | false        | Cannot be missing and have null or empty value + must be in UUID format                                                                    | Should be generated per specific business use case from "Notify NL" Admin Portal                                                                                                                             |
| ZGW_TEMPLATEIDS_SMS_TASKASSIGNED                    | GUID      | "00000000-0000-0000-0000-000000000000"        | false        | Cannot be missing and have null or empty value + must be in UUID format                                                                    | Should be generated per specific business use case from "Notify NL" Admin Portal                                                                                                                             |
| ZGW_TEMPLATEIDS_SMS_MESSAGE                         | GUID      | "00000000-0000-0000-0000-000000000000"        | false        | Cannot be missing and have null or empty value + must be in UUID format                                                                    | Should be generated per specific business use case from "Notify NL" Admin Portal                                                                                                                             |
| ZGW_TEMPLATEIDS_DECISIONMADE                        | GUID      | "00000000-0000-0000-0000-000000000000"        | false        | Cannot be missing and have null or empty value + must be in UUID format                                                                    | Should be generated per specific business use case from "Notify NL" Admin Portal                                                                                                                             |
| ---                                                 | ---       | ---                                           | ---          | ---                                                                                                                                        | ---                                                                                                                                                                                                          |
| ZGW_WHITELIST_ZAAKCREATE_IDS                        | string[]  | "1, 2, 3, 4" or "*" (allow everything)        | false        |                                                                                                                                            | Is provided by the user based on "Identificatie" property of case type retrieved from case URI ("zaak") from "OpenZaak" Web API service                                                                      |
| ZGW_WHITELIST_ZAAKUPDATE_IDS                        | string[]  | "1, 2, 3, 4" or "*" (allow everything)        | false        |                                                                                                                                            | Is provided by the user based on "Identificatie" property of case type retrieved from case URI ("zaak") from "OpenZaak" Web API service                                                                      |
| ZGW_WHITELIST_ZAAKCLOSE_IDS                         | string[]  | "1, 2, 3, 4" or "*" (allow everything)        | false        |                                                                                                                                            | Is provided by the user based on "Identificatie" property of case type retrieved from case URI ("zaak") from "OpenZaak" Web API service                                                                      |
| ZGW_WHITELIST_TASKASSIGNED_IDS                      | string[]  | "1, 2, 3, 4" or "*" (allow everything)        | false        |                                                                                                                                            | Is provided by the user based on "Identificatie" property of case type retrieved from case URI ("zaak") from "OpenZaak" Web API service                                                                      |
| ZGW_WHITELIST_DECISIONMADE_IDS                      | string[]  | "1, 2, 3, 4" or "*" (allow everything)        | false        |                                                                                                                                            | Is provided by the user based on "Identificatie" property of case type retrieved from case URI ("zaak") from "OpenZaak" Web API service                                                                      |
| ZGW_WHITELIST_MESSAGE_ALLOWED                       | bool      | "true" or "false"                             | false        | Cannot be missing and have null or empty value                                                                                             | Is provided by the user                                                                                                                                                                                      |
| ZGW_WHITELIST_TASKOBJECTTYPE_UUID                   | GUID      | "00000000-0000-0000-0000-000000000000"        | false        | Cannot be missing and have null or empty value + must be in UUID format                                                                    | Is provided by the user based on "objectType" from "kenmerken" from the initial notification received from "Notificaties" Web API service                                                                    |
| ZGW_WHITELIST_MESSAGEOBJECTTYPE_UUID                | GUID      | "00000000-0000-0000-0000-000000000000"        | false        | Cannot be missing and have null or empty value + must be in UUID format                                                                    | Is provided by the user based on "objectType" from "kenmerken" from the initial notification received from "Notificaties" Web API service                                                                    |
| ZGW_WHITELIST_DECISIONINFOOBJECTTYPE_UUIDS          | GUID[]    | "00000000-0000-..., 00000000-0000-..."        | false        | Cannot be missing and have null or empty value + must be in UUID format                                                                    | Is provided by the user based on "informatieobjecttype" from "informatieobject" retrieved from "OpenZaak" Web API service when querying "besluiten"                                                          |
| ---                                                 | ---       | ---                                           | ---          | ---                                                                                                                                        | ---                                                                                                                                                                                                          |
| ZGW_VARIABLES_OBJECTEN_MESSAGEOBJECTTYPE_VERSION    | ushort    | "1"                                           | false        | Cannot be missing and have null or empty value                                                                                             | It can be taken from "version" value set in "ObjectTypen" Web API service                                                                                                                                    |
| ---                                                 | ---       | ---                                           | ---          | ---                                                                                                                                        | ---                                                                                                                                                                                                          |
| **Notify**                                          |           |                                               |              |                                                                                                                                            |                                                                                                                                                                                                              |
| NOTIFY_API_BASEURL                                  | URI       | "https://api.notify.nl"                       | false        | Cannot be missing and have null or empty value                                                                                             | The domain where your Notify API instance is listening (e.g.: "https://api.notifynl.nl")                                                                                                                     |
| ---                                                 | ---       | ---                                           | ---          | ---                                                                                                                                        | ---                                                                                                                                                                                                          |
| **Monitoring**                                      |           |                                               |              |                                                                                                                                            |                                                                                                                                                                                                              |
| SENTRY_DSN                                          | URI       | "https://1abxxx@o1xxx.sentry.io/xxx"          | false        | Validated internally by Sentry.SDK                                                                                                         | It points out to the Sentry project configured to store captured events from the app (messages, exceptions)                                                                                                  |
| SENTRY_ENVIRONMENT                                  | string    | "MyCompany-prod"                              | false        | Validated internally by Sentry.SDK                                                                                                         | It's the identifier used by Sentry external logging system to distinguish instance and mode of the application (it can contains name of the company, or specific environment: prod, acc, dev, test...)       |

\* Copy-paste the *environment variable* name and set the value of respective type like showed in the **Example** column from the above.
\** GUID and UUID are representing the same data type in the following format: 8-4-4-4-12 and using Hexadecimal values (0-f). The difference is that UUID is used in cross-platform context, while GUID is the data type used in .NET

<h4 id="get_environment_variables">3.1.2.2. Get environment variables</h4>

`OMC_AUTHORIZATION_JWT_SECRET` - To be generated from any passwords manager. Like other **OMC_AUTHORIZATION_[...]** configurations it's meant to be set by the user.

`ZGW_AUTHORIZATION_JWT_SECRET` - Like other **ZGW_AUTHORIZATION_[...]** configurations to be configured and set by the user after logging to **OpenZaak** Web API service.

`ZGW_API_KEY_NOTIFYNL` - To be generated from **NotifyNL** Admin Portal => **API Integration** section.

`ZGW_TEMPLATEIDS_SMS_ZAAKCREATE` - All **Template IDs** (SMS and Email) will be generated (and then you can copy-paste them into environment variables) when the user create (one-by-one) new templates from **NotifyNL** Admin Portal => **Templates** section.

<h4 id="set_environment_variables">3.1.2.3. Set environment variables</h4>

1. On Windows:

![Invalid base URL - Error](images/environment_varibles_windows.png)

Additionally, environment variables can be also defined in **Visual Studio**'s `launchSettings.json` file. Check the example [here](#custom_lanunchSettings_profile).

2. On Linux:

> To be finished...

3. On Mac:

> To be finished...

<h4 id="helm_charts">3.1.2.4. Using HELM Charts</h4>

**NotifyNL** and **OMC** are meant to be used with [HELM Charts](https://helm.sh/) (helping to install them on your local machine / server).

- [NotifyNL HELM Charts (GitHub)](https://github.com/Worth-NL/helm_charts)

- [OMC HELM Charts (GitHub)](https://github.com/Worth-NL/helm_charts/tree/main/notifynl-omc)

---
<h1 id="authorization">4. Authorization and authentication</h1>

<sup>[Go back](#start)</sup>

All of the API services involved in the notifying process (**OpenServices**, **OMC**, **Notify**) requires some type of authorization and authentication procedure.

> **NOTE:** some external Web API services (e.g. **Open Klant** v2.0 or **Objecten** APIs) are using prefedined _API keys_ to authenticate users and authorize them to access the service. In other cases, JWT have to be generated (and refreshed if needed).

> The user of **OMC** doesn't have to worry which authorization method will be used behind the hood, as long as you provide valid credentials and specify which version of "OpenServices" [workflow](#workflow_versions) is used.

<h2 id="jwt_tokens">4.1. JSON Web Tokens</h2>

In the normal business workflow **OMC** API will ensure that valid _JWT tokens_ would be used internally (based on the provided credentials (_environment variables_). However, developers testing or maintaining the solution need to generate their own JWT tokens (e.g., to access the **OMC** API endpoints from **Swagger UI** or **Postman**) using one of the following approaches.

**JSON Web Token (JWT)** can be generated using:

- **SecretsManager.exe** from `CLI (Command Line Interface)` externally (e.g., `CMD.exe on Windows`, using valid credentials defined in _environment variables_)

> The commands are defined in the [Secrets Manager](https://github.com/Worth-NL/NotifyNL-OMC/blob/update/Documentation/EventsHandler/Logic/SecretsManager/Readme.md)'s documentation.
> **NOTE:** Do not use `PowerShell`.

An example of a simple `.cmd` script using one of the commands responsible for creating _JWT token_ valid for 24 hours:

<code>
"C:\[...]\NotifyNL-OMC\EventsHandler\Logic\SecretsManager\bin\Debug\net7.0\NotifyNL.SecretsManager.exe" 1440

pause
</code>

Users can also execute their commands directly in the catalog where **SecretsManager.exe** is located.

- **SecretsManager.dll** (after referencing and importing the library) from the code (using valid credentials defined in _environment variables_ or overruled from launch profile in _launchSettings.json_)

> To learn more, read the documentation dedicated to [Secrets Manager](https://github.com/Worth-NL/NotifyNL-OMC/blob/update/Documentation/EventsHandler/Logic/SecretsManager/Readme.md).

- By running **Secrets Manager** project in _Visual Studio_ (after selecting "Set as Startup Project" option in Solution Explorer and using valid credentials defined in _environment variables_ or overruled from launch profile in _launchSettings.json_)

![Configuration in appsettings.json](images/launchProfiles_secrets_manager.png)

- Through the external **https://jwt.io** webpage (using the same credentials as those defined in _environment variables_).

<h3 id="jwt_required_components">4.1.1. Required components</h3>

> Knowing all required *environment variables* you can fill these claims manually and generate your own JWT tokens without using **Secrets Manager**. This approach might be helpful if you are using **OMC** Web API service only as a Web API service (**Swagger UI**), during testing its functionality from **Postman**, or when using only the **Docker Image**.

<h4 id="jwt_header">4.1.1.1. Header (algorithm + type)</h4>

> {
  "alg": "HS256",
  "typ": "JWT"
}

<h4 id="jwt_claims">4.1.1.2. Payload (claims)</h4>

> {
  "client_id": "",
  "user_id": "",
  "user_representation": "",
  "iss": "",
  "aud": "",
  "iat": 0000000000,
  "exp": 0000000000
}

<h4 id="jwt_secret">4.1.1.3. Signature (secret)</h4>

![JWT Signature](images/jwt_signature.png)

> **NOTE:** To be filled in **https://jwt.io**.

<h3 id="jwt_mapping_environment_variables">4.1.2. Mapping of JWT claims from environment variables</h3>

| JWT claims            | **OMC** Environment Variables                |
| --------------------- | -------------------------------------------- |
| `client_id`           | `OMC_AUTHORIZATION_JWT_ISSUER`               |
| `user_id`             | `OMC_AUTHORIZATION_JWT_USERID`               |
| `user_representation` | `OMC_AUTHORIZATION_JWT_USERNAME`             |
| `iss`                 | `OMC_AUTHORIZATION_JWT_ISSUER`               |
| `aud`                 | `OMC_AUTHORIZATION_JWT_AUDIENCE`             |
| `iat`                 | To be filled manually using current time     |
| `exp`                 | `iat` + `OMC_AUTHORIZATION_JWT_EXPIRESINMIN` |
| `secret`              | `OMC_AUTHORIZATION_JWT_SECRET`               |

> **NOTE:** "iat" and "exp" times requires Unix formats of timestamps.
The Unix timestamp can be generated using [Unix converter](https://www.unixtimestamp.com/).

<h3 id="jwt_generating">4.1.3. Using generated JSON Web Token (JWT)</h3>

<h4 id="postman_authorization">4.1.3.1. Postman (authorization)</h4>

> After generating the JWT token you can copy-paste it in **Postman** to authorize your HTTP requests.

![Postman - Authorization](images/postman_authorization.png)

---
<h4 id="swagger_ui_authorization">4.1.3.2. Swagger UI (authorization)</h4>

> If you are using **OMC** **Swagger UI** from browser (graphic interface for **OMC** Web API service) then you need to copy the generated token in the following way:

![Swagger UI - Authorization](images/swagger_ui_authorization.png)

And then click "Authorize".

---
<h1 id="omc_workflow">5. OMC Workflow</h1>

<sup>[Go back](#start)</sup>

![Invalid base URL - Error](images/OMC_Sequence_Chart.png)

> Version of **OMC** <= 1.7.4 (using "[OMC workflow v1](#workflow_dependencies)").

<h2 id="workflow_versions">5.1. Versions</h2>

The **OMC** API is using different configurations and setups to handle multiple complex business cases. Sometimes, it is even required to support multiple versions of the same external API services (which might be very different from each other).

> **NOTE:** The OMC workflow can be changed using a respective _environment variable_ in the section of features.

<h3 id="workflow_dependencies">5.1.1. Dependencies</h3>

Here are the details which _workflows_ are using which versions of the external API services:

<h4 id="omc_workflow_v1">OMC workflow v1 <kbd>(default)</kbd></h4>

- "OpenNotificaties" v1.6.0
- "OpenZaak" v1.12.1
- "OpenKlant" v1.0.0
- "Besluiten" v1.1.0
- "Objecten" v2.3.1
- "ObjectTypen" v2.2.0
- "Contactmomenten" v1.0.0

> **NOTE:** This workflow is supporting only _citizens_ (using **BSN** numbers).

<h4 id="omc_workflow_v2">OMC workflow v2</h4>

- "OpenNotificaties" v1.6.0
- "OpenZaak" v1.12.1
- <code>new</code> "OpenKlant" v2.0.0
- "Besluiten" v1.1.0
- "Objecten" v2.3.1
- "ObjectTypen" v2.2.0
- <code>new</code> "Klantcontacten" v2.0.0

> **NOTE:** This workflow is supporting both _citizens_ (using **BSN** numbers) and _organizations_ (using **KVK** numbers). The term used to describe such different entities is "party".

<h2 id="scenarios">5.2. Scenarios</h2>

List of scenarios and the details how to use them with **OMC** (configurations, template personalizations, environment variables, business logic conditions, etc.).

<h3 id="scenarios_general_introduction">5.2.1. General introduction</h3>

**OMC** "Scenarios" are specific processing workflows, set up in the code to handle certain business requirements: _what_, _when_, _how_, and _which_ to process the "initial notification" received from a subscribed channel from a _message queue_ implemented by **Open Notificaties** Web API service.

Using _environment variables_ such as "Domains", "Whitelists", "TemplateIds", and most importantly "OMC Workflow version", the user of **OMC** can have some control:
- _what_ external third-party Web API services will be used (domains)
- _when_ the specific notification will be processed (whitelists)
- _how_ the recipient will see the notification (template IDs), and
- _which_ internal implementation will be used ([OMC Workflow](#workflow_versions) version)

Implementation of new business cases, obviously(!), requires ingeretion in the code (C#) and preparing new **OMC** scenarios and the processing logic (validation, conditioning, settings...).

Currently, the following business **scenarios** are implemented:

- Opening a _case_
- Updating a _case_ status
- Closing a _case_
- Assignment of a _task_
- Receiving a _decision_
- Receiving a _message_
 
<h4 id="scenarios_general_notification">5.2.1.1. Notification</h4>

Any **OMC** workflow relies on receiving the (initial) notification event from **Open Notificaties** Web API service to trigger the processing business logic.

This notification is in _JSON_ format.

Except of being awaited by **OMC** callback (`[OMC]/events/listen` endpoint) it can also be passed from outside to HTTP Requests while using **Swagger UI** or **Postman** - to test or simulate the desired **OMC** behavior. This way the initial event from **Open Notificaties** Web API service can be stubbed.

Using **Swagger UI** is recommended solution, because of its user-friendly User Interface, documentation of endpoints, parameters, remarks, JSON examples, model schemas, and validation; formatting of API responses is also better than in **Postman**.

<h4 id="scenarios_general_environment_variables">5.2.1.2. Environment variables</h4>

To work properly **OMC** always requires these mandatory _environment variables_ to be set:

> **NOTE:** If some environment variable is missing but required by one of the countless scenarios, conditions, or workflows, the **OMC** application will return a readable and user-friendly API response with the name of the missing environment variable. This is the easiest way to figure out what else is required.

</br>

`ASPNETCORE_ENVIRONMENT`

> Used by `[OMC]/events/version` endpoint and to determine which `appsettings[.xxx].json` will be used.

</br>

`OMC_AUTHORIZATION_JWT_SECRET`

`OMC_AUTHORIZATION_JWT_ISSUER`

`OMC_AUTHORIZATION_JWT_AUDIENCE`

`OMC_AUTHORIZATION_JWT_EXPIRESINMIN`

`OMC_AUTHORIZATION_JWT_USERID`

`OMC_AUTHORIZATION_JWT_USERNAME`

> Required to get access to **OMC** and be able to use it. Moreover, **Open Notificaties** Web API service will use this method to make an authorized requests while sending notification events to **OMC**.

</br>

`NOTIFY_API_BASEURL`

> Without this URL notifications would not work.

</br>

`OMC_FEATURES_WORKFLOW_VERSION`

> Without this setting (the version needs to be supported) the **OMC** Web API will not even run and specific implementations of underlying services will not be resolved by _Dependency Injection_ mechanism. By default you can always use `"1"` if you don't know yet which other [OMC Workflow](#workflow_versions) version you should use.

</br>

`ZGW_AUTHORIZATION_JWT_SECRET`

`ZGW_AUTHORIZATION_JWT_ISSUER`

`ZGW_AUTHORIZATION_JWT_AUDIENCE`

`ZGW_AUTHORIZATION_JWT_EXPIRESINMIN`

`ZGW_AUTHORIZATION_JWT_USERID`

`ZGW_AUTHORIZATION_JWT_USERNAME`

> **JWT authorization** is required by some versions of external API services used in certain [OMC Workflow](#workflow_versions) versions.

</br>

`ZGW_API_KEY_OPENKLANT`  => Required only in certain [OMC Workflow](#workflow_versions) versions

`ZGW_API_KEY_OBJECTEN`

`ZGW_API_KEY_OBJECTTYPEN`

`ZGW_API_KEY_NOTIFYNL`

> **API key authorization** is required by some versions of external API services used in certain [OMC Workflow](#workflow_versions) versions.

</br>

`ZGW_DOMAIN_OPENZAAK`

`ZGW_DOMAIN_OPENKLANT`

`ZGW_DOMAIN_BESLUITEN`

`ZGW_DOMAIN_OBJECTEN`

`ZGW_DOMAIN_OBJECTTYPEN`

`ZGW_DOMAIN_CONTACTMOMENTEN`

> **Domains** might have different _paths_ (e.g., `domain/something/v1/`) depends on version of external API service used in certain [OMC Workflow](#workflow_versions). For example domains for OpenKlant and ContactMomenten depends on version of **Open Klant** Web API service. Moreover, domains and paths depends on the place where your version of Web API service was deployed (domain) and the way how it is internally structured (paths).

</br>

These _environment variables_ are optional:

`SENTRY_DSN`

`SENTRY_ENVIRONMENT`

> Logging and analytics in third-party service ([Sentry.io](https://sentry.io)).

<h4 id="scenarios_general_requirements">5.2.1.3. Requirements</h4>

To process certain notification the specific internal criteria must be met. Usually, they are some pre-validation (analyzing the "initial notification" received from **Open Notificaties** Web API service), post-validation (to determine the scenario suited for this type of the notification), and whitelisting steps (to ensure that **OMC** should continue processing this type of notification). Sometimes, additional checks have to be performed - which depends on the specific **OMC** scenario.

<h4 id="scenarios_general_template_placeholders">5.2.1.4. Template placeholders</h4>

When everything is already validated, prepared, and processed, the **Notify NL** Web API service needs to receive instruction how to format the upcoming notification. The way how to achieve this is to set up so called "template" (using **Notify NL Admin portal** webpage), define `((placeholders))` in the text (_subject_ and/or _body_) - matching to the ones defined by the specific **OMC** scenario, and then use the `ID` of this freshly generated "template" in respective _environment variable_ for **OMC**.

---

<h1 id="scenarios_examples">Examples</h1>

<h3 id="case_created">5.2.2. Case Created</h3>

Notifies the respective party (e.g., a citizen or an organization) about the case being open for them. For the residents of The Netherlands the case is related to their unique personal identification number **BSN** (_Burgerservicenummer_), thanks to which their contact details and contact preferrences can be retrieved (whether they want to be notified and which notification method they prefer, e.g. by Email, SMS, etc.).

<h4 id="case_created_notification">5.2.2.1. Notification</h4>

Example of JSON schema:

```json
{
  "actie": "create",
  "kanaal": "zaken",
  "resource": "status",
  "kenmerken": {
    "zaaktype": "https://...",
    "bronorganisatie": "000000000",
    "vertrouwelijkheidaanduiding": "openbaar" // Or "vertrouwelijk"
  },
  "hoofdObject": "https://...",
  "resourceUrl": "https://...",
  "aanmaakdatum": "2000-01-01T10:00:00.000Z"
}
```

<h4 id="case_created_environment_variables">5.2.2.2. Environment variables</h4>

Required to be set:

`ZGW_TEMPLATEIDS_EMAIL_ZAAKCREATE`

`ZGW_TEMPLATEIDS_SMS_ZAAKCREATE`

</br>

`ZGW_WHITELIST_ZAAKCREATE_IDS`

<h4 id="case_created_requirements">5.2.2.3. Requirements</h4>

- The _initial notification_ has:
  -- **Action:** Create (`"create"`)
  -- **Channel:** Cases (`"zaken"`)
  -- **Resource:** Status (`"status"`)

- The _case_ has 1 _status_ (it was never updated) => this is a new _case_

- The _case type identifier_ (`"zaaktypeIdentificatie"`) has to be **whitelisted** or `"*"` wildcard used (to accept all case types) in respective whitelist _environment variable_

- The notification indication property (`"informeren"`) in _case type_ is set to _true_

- All **URI**s are valid, source data complete, and **JWT token** or **API keys** correct

The notification will be processed and sent!

> Otherwise, user will get a meaningful API feedback from **OMC** application explaining what exactly is missing.

<h4 id="case_created_template_placeholders">5.2.2.4. Template placeholders</h4>

Required placeholders names in the **Notify NL** template:

`((klant.voornaam))`

`((klant.voorvoegselAchternaam))`

`((klant.achternaam))`

</br>

`((zaak.identificatie))`

`((zaak.omschrijving))`

---

<h3 id="case_updated">5.2.3. Case Status Updated</h3>

Notifies the respective party (e.g., a citizen or an organization) that the status of their case was updated.

<h4 id="case_updated_notification">5.2.3.1. Notification</h4>

Example of JSON schema:

```json
{
  "actie": "create",
  "kanaal": "zaken",
  "resource": "status",
  "kenmerken": {
    "zaaktype": "https://...",
    "bronorganisatie": "000000000",
    "vertrouwelijkheidaanduiding": "openbaar" // Or "vertrouwelijk"
  },
  "hoofdObject": "https://...",
  "resourceUrl": "https://...",
  "aanmaakdatum": "2000-01-01T10:00:00.000Z"
}
```

<h4 id="case_updated_environment_variables">5.2.3.2. Environment variables</h4>

Required to be set:

`ZGW_TEMPLATEIDS_EMAIL_ZAAKUPDATE`

`ZGW_TEMPLATEIDS_SMS_ZAAKUPDATE`

</br>

`ZGW_WHITELIST_ZAAKUPDATE_IDS`

<h4 id="case_updated_requirements">5.2.3.3. Requirements</h4>

- The _initial notification_ has:
  -- **Action:** Create (`"create"`)
  -- **Channel:** Cases (`"zaken"`)
  -- **Resource:** Status (`"status"`)

- The _case_ has 2+ _statuses_ (it was updated at least once)

- The last _case status_ is not set to final (`"isEindstatus" : false`) => the _status_ of a _case_ is just updated and not yet finalized

- The _case type identifier_ (`"zaaktypeIdentificatie"`) has to be **whitelisted** or `"*"` wildcard used (to accept all case types) in respective whitelist _environment variable_

- The notification indication property (`"informeren"`) in _case type_ is set to _true_

- All **URI**s are valid, source data complete, and **JWT token** or **API keys** correct

The notification will be processed and sent!

> Otherwise, user will get a meaningful API feedback from **OMC** application explaining what exactly is missing.

<h4 id="case_updated_template_placeholders">5.2.3.4. Template placeholders</h4>

Required placeholders names in the **Notify NL** template:

`((klant.voornaam))`

`((klant.voorvoegselAchternaam))`

`((klant.achternaam))`

</br>

`((zaak.identificatie))`

`((zaak.omschrijving))`

</br>

`((status.omschrijving))`

---

<h3 id="case_closed">5.2.4. Case Closed</h3>

Notifies the respective party (e.g., a citizen or an organization) that their case was closed (e.g., resolved).

<h4 id="case_closed_notification">5.2.4.1. Notification</h4>

Example of JSON schema:

```json
{
  "actie": "create",
  "kanaal": "zaken",
  "resource": "status",
  "kenmerken": {
    "zaaktype": "https://...",
    "bronorganisatie": "000000000",
    "vertrouwelijkheidaanduiding": "openbaar" // Or "vertrouwelijk"
  },
  "hoofdObject": "https://...",
  "resourceUrl": "https://...",
  "aanmaakdatum": "2000-01-01T10:00:00.000Z"
}
```

<h4 id="case_closed_environment_variables">5.2.4.2. Environment variables</h4>

Required to be set:

`ZGW_TEMPLATEIDS_EMAIL_ZAAKCLOSE`

`ZGW_TEMPLATEIDS_SMS_ZAAKCLOSE`

</br>

`ZGW_WHITELIST_ZAAKCLOSE_IDS`

<h4 id="case_closed_requirements">5.2.4.3. Requirements</h4>

- The _initial notification_ has:
  -- **Action:** Create (`"create"`)
  -- **Channel:** Cases (`"zaken"`)
  -- **Resource:** Status (`"status"`)

- The _case_ has 2+ _statuses_ (it was updated at least once)

- The last _case status_ is set to final (`"isEindstatus" : true`) => the _case_ is closed

- The _case type identifier_ (`"zaaktypeIdentificatie"`) has to be **whitelisted** or `"*"` wildcard used (to accept all case types) in respective whitelist _environment variable_

- The notification indication property (`"informeren"`) in _case type_ is set to _true_

- All **URI**s are valid, source data complete, and **JWT token** or **API keys** correct

The notification will be processed and sent!

> Otherwise, user will get a meaningful API feedback from **OMC** application explaining what exactly is missing.

<h4 id="case_closed_template_placeholders">5.2.4.4. Template placeholders</h4>

Required placeholders names in the **Notify NL** template:

`((klant.voornaam))`

`((klant.voorvoegselAchternaam))`

`((klant.achternaam))`

</br>

`((zaak.identificatie))`

`((zaak.omschrijving))`

</br>

`((status.omschrijving))`

---

<h3 id="task_assigned">5.2.5. Task Assigned</h3>

Notifies the respective party (e.g., a citizen or an organization) that the new task was assigned to them.

<h4 id="task_assigned_notification">5.2.5.1. Notification</h4>

Example of JSON schema:

```json
{
  "actie": "create",
  "kanaal": "objecten",
  "resource": "object",
  "kenmerken": {
    "objectType": "https://..."
  },
  "hoofdObject": "https://...",
  "resourceUrl": "https://...",
  "aanmaakdatum": "2000-01-01T10:00:00.000Z"
}
```

<h4 id="task_assigned_environment_variables">5.2.5.2. Environment variables</h4>

Required to be set:

`ZGW_TEMPLATEIDS_EMAIL_TASKASSIGNED`

`ZGW_TEMPLATEIDS_SMS_TASKASSIGNED`

</br>

`ZGW_WHITELIST_TASKASSIGNED_IDS`

`ZGW_WHITELIST_TASKOBJECTTYPE_UUID`

<h4 id="task_assigned_requirements">5.2.5.3. Requirements</h4>

- The _initial notification_ has:
  -- **Action:** Create (`"create"`)
  -- **Channel:** Objects (`"objecten"`)
  -- **Resource:** Object (`"object"`)

- The **GUID** from _object type URI_ (`"objectType"`) in the _initial notification_ has to be **whitelisted** or `"*"` wildcard used (to accept all object types) in respective whitelist _environment variable_. This step will distinguish for which object type the notification is desired (e.g., tasks, messages, etc.)

- The _task_ status (`"status"`) from `record.data` nested object is set to open (`"open"`)

- The _task_ identification type (`"type"`) from `record.data.identificatie` is set to:
  -- private person (`"bsn"`)
  -- or company (`"kvk"`)

- The _case type identifier_ (`"zaaktypeIdentificatie"`) has to be **whitelisted** or `"*"` wildcard used (to accept all case types) in respective whitelist _environment variable_

- The notification indication property (`"informeren"`) in _case type_ is set to _true_

- All **URI**s are valid, source data complete, and **JWT token** or **API keys** correct

The notification will be processed and sent!

> Otherwise, user will get a meaningful API feedback from **OMC** application explaining what exactly is missing.

<h4 id="task_assigned_template_placeholders">5.2.5.4. Template placeholders</h4>

Required placeholders names in the **Notify NL** template:

`((klant.voornaam))`

`((klant.voorvoegselAchternaam))`

`((klant.achternaam))`

</br>

`((taak.verloopdatum))`

`((taak.heeft_verloopdatum))`

`((taak.record.data.title))`

</br>

`((zaak.identificatie))`

`((zaak.omschrijving))`

---

<h3 id="decision_made">5.2.6. Decision Made</h3>

Notifies the respective party (e.g., a citizen or an organization) that the decision was made in their case.

<h4 id="decision_made_notification">5.2.6.1. Notification</h4>

Example of JSON schema:

```json
{
  "actie": "create",
  "kanaal": "besluiten",
  "resource": "besluitinformatieobject",
  "kenmerken": {
    "besluittype": "https://...",
    "verantwoordelijkeOrganisatie": "000000000"
  },
  "hoofdObject": "https://...",
  "resourceUrl": "https://...",
  "aanmaakdatum": "2000-01-01T10:00:00.000Z"
}
```

<h4 id="decision_made_environment_variables">5.2.6.2. Environment variables</h4>

Required to be set:

`ZGW_TEMPLATEIDS_DECISIONMADE`

</br>

`ZGW_WHITELIST_DECISIONMADE_IDS`

`ZGW_WHITELIST_DECISIONINFOOBJECTTYPE_UUIDS`

`ZGW_WHITELIST_MESSAGEOBJECTTYPE_UUID`

</br>

`ZGW_VARIABLES_OBJECTEN_MESSAGEOBJECTTYPE_VERSION`

<h4 id="decision_made_requirements">5.2.6.3. Requirements</h4>

- The _initial notification_ has:
  -- **Action:** Create (`"create"`)
  -- **Channel:** Objects (`"besluiten"`)
  -- **Resource:** Object (`"besluitinformatieobject"`)

- The **GUID** from _info object type URI_ (`"informatieobjecttype"`) linked to the _decision_ has to be **whitelisted** or `"*"` wildcard used (to accept all info object types) in respective whitelist _environment variable_

- The _info object status_ (`"status"`) is set to definitive (`"definitief"`)

- The _info object confidentiality_ (`"vertrouwelijkheidaanduiding"`) is set to non-confidential (`"openbaar"`)

- The _case type identifier_ (`"zaaktypeIdentificatie"`) has to be **whitelisted** or `"*"` wildcard used (to accept all case types) in respective whitelist _environment variable_

- The notification indication property (`"informeren"`) in _case type_ is set to _true_

- All **URI**s are valid, source data complete, and **JWT token** or **API keys** correct

The notification will be processed and sent!

> Otherwise, user will get a meaningful API feedback from **OMC** application explaining what exactly is missing.

<h4 id="decision_made_template_placeholders">5.2.6.4. Template placeholders</h4>

Required placeholders names in the **Notify NL** template:

`((klant.voornaam))`

`((klant.voorvoegselAchternaam))`

`((klant.achternaam))`

</br>

`((besluit.identificatie))`

`((besluit.datum))`

`((besluit.toelichting))`

`((besluit.bestuursorgaan))`

`((besluit.ingangsdatum))`

`((besluit.vervaldatum))`

`((besluit.vervalreden))`

`((besluit.publicatiedatum))`

`((besluit.verzenddatum))`

`((besluit.uiterlijkereactiedatum))`

</br>

`((besluittype.omschrijving))`

`((besluittype.omschrijvingGeneriek))`

`((besluittype.besluitcategorie))`

`((besluittype.publicatieindicatie))`

`((besluittype.publicatietekst))`

`((besluittype.toelichting))`

</br>

`((zaak.identificatie))`

`((zaak.omschrijving))`

`((zaak.registratiedatum))`

</br>

`((zaaktype.omschrijving))`

`((zaaktype.omschrijvingGeneriek))`

---

<h3 id="message_received">5.2.7. Message Received</h3>

Notifies the respective party (e.g., a citizen or an organization) that the message with decision is available on their mailbox.

<h4 id="message_received_notification">5.2.7.1. Notification</h4>

Example of JSON schema:

```json
{
  "actie": "create",
  "kanaal": "objecten",
  "resource": "object",
  "kenmerken": {
    "objectType": "https://..."
  },
  "hoofdObject": "https://...",
  "resourceUrl": "https://...",
  "aanmaakdatum": "2000-01-01T10:00:00.000Z"
}
```

<h4 id="message_received_environment_variables">5.2.7.2. Environment variables</h4>

Required to be set:

`ZGW_TEMPLATEIDS_EMAIL_MESSAGERECEIVED`

`ZGW_TEMPLATEIDS_SMS_MESSAGERECEIVED`

</br>

`ZGW_WHITELIST_MESSAGE_ALLOWED`

`ZGW_WHITELIST_MESSAGEOBJECTTYPE_UUID`

<h4 id="message_received_requirements">5.2.7.3. Requirements</h4>

- The _initial notification_ has:
  -- **Action:** Create (`"create"`)
  -- **Channel:** Objects (`"objecten"`)
  -- **Resource:** Object (`"object"`)

- The **GUID** from _object type URI_ (`"objectType"`) in the _initial notification_ has to be **whitelisted** or `"*"` wildcard used (to accept all object types) in respective whitelist _environment variable_. This step will distinguish for which object type the notification is desired (e.g., tasks, messages, etc.)

- Sending of messages is allowed in respective _environment variable_

- All **URI**s are valid, source data complete, and **JWT token** or **API keys** correct

The notification will be processed and sent!

> Otherwise, user will get a meaningful API feedback from **OMC** application explaining what exactly is missing.

<h4 id="message_received_template_placeholders">5.2.7.4. Template placeholders</h4>

Required placeholders names in the **Notify NL** template:

`((klant.voornaam))`

`((klant.voorvoegselAchternaam))`

`((klant.achternaam))`

</br>

`((message.onderwerp))`

`((message.handelingsperspectief))`

---

<h3 id="not_implemented_scenario">5.2.99. Not Implemented</h3>

A special fallback scenario which only role is to report that the provided "initial notification" or conditions are not sufficient to determine a proper **OMC** scenario - to be resolved and used for processing the business logic.

User can expect meaningful API response from **OMC**. This response will have _HTTP Status Code_ (206) that will not trigger **Open Notificaties** Web API service to retry sending the same type of "initial notification" again (which would be pointless and fail again since the **OMC** scenario or new condition are not yet implemented).

---
<h1 id="errors">6. Errors</h1>

<sup>[Go back](#start)</sup>

List of **validation** (format, requirements), **connectivity** or business logic **processing** errors that you might encounter during accessing **OMC** API endpoints.

**General errors:**

> **HTTP Status Code: 401 Unauthorized**

- Invalid JWT token:

![Invalid JWT Token - Error](images/general_jwt_invalid.png)

- Invalid JWT secret:

![Invalid JWT secret - Error](images/general_jwt_secret_wrong.png)

<h2 id="errors_events_controller">6.1. Events Controller</h2>

Endpoints:

- `POST` .../Events/Listen
- `GET` .../Events/Version

<h3 id="errors_events_controller_possible_errors">6.1.1. Possible errors</h3>

> HTTP Status Code: 206 Partial Content

- Test notification received:

![Test notification - Input](images/events_listen_testNotificationInput.png)

![Test notification - Warning](images/events_listen_testNotificationWarning.png)

**NOTE**: Open Notificaties API is sending test notifications to ensure whether **OMC** is able to receive incoming notifications.

- Not implemented scenario:

![Not implemented scenario - Warning](images/events_listen_notImplementedScenario.png)

> HTTP Status Code: 422 Unprocessable Entity

- Invalid JSON payload (syntax error):

![Invalid JSON payload - Error](images/events_listen_jsonError.png)

- Invalid data model (missing required fields):

![Invalid required data - Error](images/events_listen_modelMissingRequiredFields.png)

**NOTE:** Multiple propertis are supported (comma-separated).

- Invalid data model (unexpected fields):

![Invalid unexpected data - Error](images/events_listen_modelUnexpectedFields.png)

**NOTE:** Multiple propertis are supported (comma-separated).

> HTTP Status Code: 500 Internal Server Error

Any eventual (however unlike) unhandled exceptions will be reported as 500.

> HTTP Status Code: 501 Not Implemented

Other cases (than not implemented business case scenarios) may raise 501 errors.
This is however highly unlikely and might occur mainly in the development phase.

---
<h2 id="errors_notify_controller">6.2. Notify Controller</h2>

Endpoints:

- `POST` .../Notify/Confirm

<h3 id="errors_notify_controller_possible_errors">6.2.1. Possible errors</h3>

> HTTP Status Code: 400 Bad Request

- HTTP Request error

![HTTP Request - Error](images/events_listen_httpRequestError.png)

Something went wrong when calling external API services: OpenZaak, OpenKlant, contactmomenten...

You woull get the following outcome (separated by pipes):
`OMC` | `Log severity (Error / Warning / Info / Debug)` | `The first possible error message`* | `Full URL to which request was tried to be send` | `The original JSON response from the called service`** | `Notification: The initial notification`

\*  That interrupted the happy path workflow due to connectivity issues, invalid configuration values, or service being down. Unfortunately, due to complexity of the system, the variety of potential errors is quite broad.

\** **WARNING**: For some mysterious reasons, authors of the third-party software (used in **OMC-NotifyNL** workflow) decided to communicate back with the user of their API (Application **Public** Interface - through **publicly** accessible **World Wide Web** network) by using one of the local languages. You might need to translate those received _JSON Response_ messages into English.

> **NOTE**: Unfortunately, **OMC** Development Team cannot provide meaningful guidance how the external services were developed or configured.

---
<h2 id="errors_test_controller">6.3. Test Controller</h2>

<h3 id="errors_test_controller_notify">6.3.1. Testing Notify</h3>

Endpoints:

- `GET` .../Test/Notify/HealthCheck
- `POST` .../Test/Notify/SendEmail
- `POST` .../Test/Notify/SendSms

<h4 id="errors_test_controller_notify_possible_errors">6.3.1.1. Possible errors</h4>

<h5 id="errors_test_controller_notify_common">a) Common for SendEmail + SendSms</h5>

> **HTTP Status Code: 403 Forbidden**

- Invalid base URL (**NotifyNL** API service):

![Invalid base URL - Error](images/test_notify_baseUrl.png)

- Invalid format of API key or it is missing (**NotifyNL** API service):

![Invalid format of API key - Error](images/test_notify_apiKeyMissing.png)

- Invalid API key - it was not registered for this **NotifyNL** API service:

![Invalid API key - Error](images/test_notify_apiKeyInvalid.png)

> **HTTP Status Code: 400 Bad Request**

- Template UUID is invalid:

![Invalid template ID format - Input](images/test_notify_templateIdFormatInput.png)

![Invalid template ID format - Error](images/test_notify_templateIdFormatError.png)

- Template not found:

![Not found template ID - Input](images/test_notify_templateIdNotFoundInput.png)

![Not found template ID - Error](images/test_notify_templateIdNotFoundError.png)

- Missing required personalization (or the default example was used):

![Missing required personalization - Input](images/test_notify_personalizationMissingInput.png)

![Missing required personalization - Error](images/test_notify_personalizationMissingError.png)

<h5 id="errors_test_controller_notify_common_sendEmail">b) SendEmail</h5>

- Missing required parameters:

![Missing required email address - Error](images/test_notify_emailMissing.png)

![Swagger UI validation - Error](images/test_notify_swaggerValidation.png)

> **HTTP Status Code: 400 Bad Request**

- Email is empty (only whitespaces):

![Missing email - Input](images/test_notify_emailEmptyInput.png)

![Missing email - Error](images/test_notify_emailEmptyError.png)

- Email is invalid (missing @, dot, domain, etc.):

![Invalid email - Input](images/test_notify_emailInvalidInput.png)

![Invalid email - Error](images/test_notify_emailInvalidError.png)

<h5 id="errors_test_controller_notify_common_sendSms">c) SendSms</h5>

- Missing required parameters:

![Missing required phone number - Error](images/test_notify_phoneMissing.png)

![Swagger UI validation - Error](images/test_notify_swaggerValidation.png)

> **HTTP Status Code: 400 Bad Request**

- Phone number is empty (only whitespaces):

![Missing phone - Input](images/test_notify_phoneEmptyInput.png)

![Missing phone - Error](images/test_notify_phoneEmptyError.png)

- Phone number contains letters or symbols:

![Invalid phone - Input](images/test_notify_phoneLettersInput.png)

![Invalid phone - Error](images/test_notify_phoneLettersError.png)

- Phone number contains not enough digits:

![Short phone - Input](images/test_notify_phoneShortInput.png)

![Short phone - Error](images/test_notify_phoneShortError.png)

- Phone number contains too many digits:

![Long phone - Input](images/test_notify_phoneLongInput.png)

![Long phone - Error](images/test_notify_phoneLongError.png)

- Phone number has incorrect format (e.g., country code is not supported):

![Invalid phone format - Input](images/test_notify_phoneFormatInput.png)

![Invalid phone format - Error](images/test_notify_phoneFormatError.png)

<h3 id="errors_test_controller_open">6.3.2. Testing Open services</h3>

Endpoints:

- `POST` .../Test/Open/ContactRegistration

<h4 id="errors_test_controller_open_possible_errors">6.3.2.1. Possible errors</h4>

> To be finished...
