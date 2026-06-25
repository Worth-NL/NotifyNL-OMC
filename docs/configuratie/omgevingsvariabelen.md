# Omgevingsvariabelen

Alle OMC-configuratie kan worden ingesteld via omgevingsvariabelen. Dit is de aanbevolen aanpak voor Docker- en Kubernetes-uitrollingen.

---

## OMC — Authenticatie

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `OMC_AUTH_JWT_SECRET` | Ja | Geheime sleutel voor het valideren van inkomende JWT Bearer-tokens |
| `OMC_AUTH_JWT_ISSUER` | Ja | Verwachte JWT-uitgever |
| `OMC_AUTH_JWT_AUDIENCE` | Nee | Verwachte JWT-doelgroep (optioneel) |
| `OMC_AUTH_JWT_EXPIRESINMIN` | Ja | Tokenlevensduur in minuten |
| `OMC_AUTH_JWT_USERID` | Ja | Gebruikers-ID die wordt ingesloten in OMC-tokens |
| `OMC_AUTH_JWT_USERNAME` | Ja | Gebruikersnaam die wordt ingesloten in OMC-tokens |

---

## OMC — Functies

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `OMC_FEATURE_WORKFLOW_VERSION` | Ja | Werkwijze versie: `1` (OpenKlant v1, alleen BSN) of `2` (OpenKlant v2, BSN + KVK) |

---

## OMC — Actor

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `OMC_ACTOR_NAME` | Nee | Naam van het OMC-actorprofiel dat wordt geschreven bij contactmomenten (standaard: `"OMC"`) |

---

## ZGW — JWT-authenticatie (OpenZaak / Besluiten)

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `ZGW_AUTH_JWT_SECRET` | Ja | JWT geheime sleutel geconfigureerd in OpenZaak |
| `ZGW_AUTH_JWT_ISSUER` | Ja | JWT-uitgever geconfigureerd in OpenZaak |
| `ZGW_AUTH_JWT_AUDIENCE` | Nee | JWT-doelgroep (optioneel) |
| `ZGW_AUTH_JWT_EXPIRESINMIN` | Ja | Tokenlevensduur in minuten — aanbevolen: `60` |
| `ZGW_AUTH_JWT_USERID` | Ja | Gebruikers-ID geregistreerd in OpenZaak voor het OMC |
| `ZGW_AUTH_JWT_USERNAME` | Ja | Naam van het OMC geconfigureerd in OpenZaak |

---

## ZGW — API-sleutels

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `ZGW_AUTH_KEY_OPENKLANT` | Ja (v2) | API-sleutel voor OpenKlant (vereist voor werkwijze v2) |
| `ZGW_AUTH_KEY_OBJECTEN` | Ja | API-sleutel voor de Objecten-API |
| `ZGW_AUTH_KEY_OBJECTTYPEN` | Ja | API-sleutel voor de ObjectTypen-API |

---

## ZGW — Service-eindpunten

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `ZGW_ENDPOINT_OPENNOTIFICATIES` | Ja | Basis-URL van de Open Notificaties API |
| `ZGW_ENDPOINT_OPENZAAK` | Ja | Basis-URL van de Open Zaak (Zaken) API |
| `ZGW_ENDPOINT_OPENKLANT` | Ja | Basis-URL van de Open Klant API |
| `ZGW_ENDPOINT_BESLUITEN` | Ja | Basis-URL van de Besluiten API |
| `ZGW_ENDPOINT_OBJECTEN` | Ja | Basis-URL van de Objecten API |
| `ZGW_ENDPOINT_OBJECTTYPEN` | Ja | Basis-URL van de ObjectTypen API |
| `ZGW_ENDPOINT_CONTACTMOMENTEN` | Ja | Basis-URL van de Contactmomenten API |

---

## ZGW — Whitelists

Alleen zaken waarvan het zaaktype overeenkomt met de whitelist worden verwerkt. Gebruik `*` om alle zaaktypen toe te staan, of geef een kommagescheiden lijst van `zaaktypeIdentificatie`-waarden op.

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `ZGW_WHITELIST_ZAAKCREATE_IDS` | Ja | Toegestane zaaktype-identificaties voor het scenario Zaak aangemaakt |
| `ZGW_WHITELIST_ZAAKUPDATE_IDS` | Ja | Toegestane zaaktype-identificaties voor het scenario Zaak gewijzigd |
| `ZGW_WHITELIST_ZAAKCLOSE_IDS` | Ja | Toegestane zaaktype-identificaties voor het scenario Zaak afgesloten |
| `ZGW_WHITELIST_TASKASSIGNED_IDS` | Ja | Toegestane objecttype-identificaties voor het scenario Taak toegewezen |
| `ZGW_WHITELIST_DECISIONMADE_IDS` | Ja | Toegestane besluittype-identificaties voor het scenario Besluit genomen |
| `ZGW_WHITELIST_MESSAGE_ALLOWED` | Ja | `true` of `false` — schakel het scenario Bericht ontvangen in of uit |

---

## ZGW — Objecttype UUID's

Deze UUID's verwijzen naar objecttype-definities in de ObjectTypen-API en zijn omgevingsspecifiek.

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `ZGW_VARIABLE_OBJECTTYPE_TAAK_UUID` | Ja | UUID van het taakobjecttype in ObjectTypen |
| `ZGW_VARIABLE_OBJECTTYPE_MESSAGE_UUID` | Ja | UUID van het berichtobjecttype in ObjectTypen |
| `ZGW_VARIABLE_OBJECTTYPE_PRODUCTREQUEST_UUID` | Nee | UUID van het productaanvraagobjecttype (toekomstig gebruik) |
| `ZGW_VARIABLE_OBJECTTYPE_KTOBJECTTYPE_UUID` | Nee | UUID van het KTO-objecttype in ObjectTypen |
| `ZGW_VARIABLE_OBJECTEN_MESSAGEOBJECTTYPE_VERSION` | Nee | Versie van het berichtobjecttype (standaard: `1`) |
| `ZGW_VARIABLE_OBJECTEN_TAAKOBJECTTYPE_VERSION` | Nee | Versie van het taakobjecttype (standaard: `1`) |

---

## NotifyNL — API

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `NOTIFY_API_BASEURL` | Ja | NotifyNL API basis-URL — `https://api.notifynl.nl` |
| `NOTIFY_API_KEY` | Ja | NotifyNL API-sleutel (formaat: `naam-UUID-UUID`) |

---

## NotifyNL — E-mailtemplates

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `NOTIFY_TEMPLATEID_EMAIL_ZAAKCREATE` | Nee* | Template-UUID voor zaak aangemaakt via e-mail |
| `NOTIFY_TEMPLATEID_EMAIL_ZAAKUPDATE` | Nee* | Template-UUID voor zaak gewijzigd via e-mail |
| `NOTIFY_TEMPLATEID_EMAIL_ZAAKCLOSE` | Nee* | Template-UUID voor zaak afgesloten via e-mail |
| `NOTIFY_TEMPLATEID_EMAIL_TASKASSIGNED` | Nee* | Template-UUID voor taak toegewezen via e-mail |
| `NOTIFY_TEMPLATEID_EMAIL_DECISIONMADE` | Nee* | Template-UUID voor besluit genomen via e-mail |
| `NOTIFY_TEMPLATEID_EMAIL_MESSAGERECEIVED` | Nee* | Template-UUID voor bericht ontvangen via e-mail |

---

## NotifyNL — Sms-templates

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `NOTIFY_TEMPLATEID_SMS_ZAAKCREATE` | Nee* | Template-UUID voor zaak aangemaakt via sms |
| `NOTIFY_TEMPLATEID_SMS_ZAAKUPDATE` | Nee* | Template-UUID voor zaak gewijzigd via sms |
| `NOTIFY_TEMPLATEID_SMS_ZAAKCLOSE` | Nee* | Template-UUID voor zaak afgesloten via sms |
| `NOTIFY_TEMPLATEID_SMS_TASKASSIGNED` | Nee* | Template-UUID voor taak toegewezen via sms |
| `NOTIFY_TEMPLATEID_SMS_DECISIONMADE` | Nee* | Template-UUID voor besluit genomen via sms |
| `NOTIFY_TEMPLATEID_SMS_MESSAGERECEIVED` | Nee* | Template-UUID voor bericht ontvangen via sms |

---

## NotifyNL — Brieftemplates

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `NOTIFY_TEMPLATEID_LETTER_ZAAKCREATE` | Nee* | Template-UUID voor zaak aangemaakt via brief |
| `NOTIFY_TEMPLATEID_LETTER_ZAAKUPDATE` | Nee* | Template-UUID voor zaak gewijzigd via brief |
| `NOTIFY_TEMPLATEID_LETTER_ZAAKCLOSE` | Nee* | Template-UUID voor zaak afgesloten via brief |
| `NOTIFY_TEMPLATEID_LETTER_TASKASSIGNED` | Nee* | Template-UUID voor taak toegewezen via brief |
| `NOTIFY_TEMPLATEID_LETTER_DECISIONMADE` | Nee* | Template-UUID voor besluit genomen via brief |
| `NOTIFY_TEMPLATEID_LETTER_MESSAGERECEIVED` | Nee* | Template-UUID voor bericht ontvangen via brief |

> \* Als er geen template-ID is ingesteld voor een bepaald kanaal/scenario-combinatie, slaat het OMC dat kanaal over voor dat scenario. Stel minimaal één kanaal in per scenario dat je wilt activeren.

---

## KTO / Expoints

Stel alle KTO-variabelen in op `-` als de integratie niet wordt gebruikt.

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `KTO_BASEURL` | Nee | Basis-URL van de KTO Expoints-dienst |
| `KTO_APIKEY` | Nee | API-sleutel voor de KTO-dienst |
| `KTO_CASETYPESETTINGS` | Nee | JSON-object dat zaaktype-UUID's koppelt aan KTO-enquête-instellingen |

---

## PostGuard

Stel alle PostGuard-variabelen in op `-` als de integratie niet wordt gebruikt.

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `POSTGUARD_BASE_URL` | Nee | Basis-URL van de PostGuard-dienst |
| `POSTGUARD_ACCESS_TOKEN` | Nee | Toegangstoken voor de PostGuard API |
| `POSTGUARD_SENDER_EMAIL` | Nee | E-mailadres van de afzender voor PostGuard-berichten |
| `POSTGUARD_SENDER_NAME` | Nee | Naam van de afzender voor PostGuard-berichten |

---

## BRP / Haal Centraal — Keycloak

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `BRP_AUTH_CLIENTID` | Nee | OAuth2 client ID voor Keycloak-tokenuitwisseling |
| `BRP_AUTH_CLIENTSECRET` | Nee | OAuth2 clientgeheim voor Keycloak |
| `BRP_AUTH_SCOPE` | Nee | OAuth2 scope(s) voor het BRP-toegangstoken |
| `BRP_AUTH_REDIRECTURI` | Nee | Redirect-URI geregistreerd in Keycloak |
| `BRP_AUTH_TOKENENDPOINT` | Nee | Keycloak token-eindpunt URL |

---

## BRP / Haal Centraal — mTLS

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `BRP_MTLS_CERTIFICATE` | Nee | Base64-gecodeerd PEM-clientcertificaat |
| `BRP_MTLS_KEY` | Nee | Base64-gecodeerde PEM-privésleutel |

---

## Monitoring

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `OMC_LOGGING_LEVEL` | Nee | Logniveau: `Debug`, `Information`, `Warning`, `Error` (standaard: `Information`) |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Nee | Azure Application Insights verbindingsreeks voor gecentraliseerde monitoring |
