# Omgevingsvariabelen

Alle OMC-configuratie kan worden ingesteld via omgevingsvariabelen. Dit is de aanbevolen aanpak voor Docker- en Kubernetes-uitrollingen.

---

## AppSettings — Netwerk

Deze instellingen kunnen zowel via omgevingsvariabele als via `appsettings.json` (`Network`) worden ingesteld; de omgevingsvariabele wint als beide aanwezig zijn.

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `NETWORK_CONNECTIONLIFETIMEINSECONDS` | Nee | Levensduur van een HTTP-verbinding in seconden (standaard: `90`) |
| `NETWORK_HTTPREQUESTTIMEOUTINSECONDS` | Nee | Timeout voor uitgaande HTTP-requests in seconden (standaard: `60`) |
| `NETWORK_HTTPREQUESTSSIMULTANEOUSNUMBER` | Nee | Maximum aantal gelijktijdige uitgaande HTTP-requests (standaard: `20`) |

---

## AppSettings — Encryptie

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `ENCRYPTION_ISASYMMETRIC` | Nee | `true`/`false` — schakelt asymmetrische (i.p.v. symmetrische) versleuteling in (standaard: `false`) |

---

## AppSettings — Variabelen

Sturen de resolutie van de betrokken partij (burger/organisatie) en het bijbehorende kanaal aan.

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `VARIABLES_BETROKKENETYPE` | Nee | Subject-type-filter bij het opzoeken van de betrokkene op een zaak (standaard: `natuurlijk_persoon`) |
| `VARIABLES_OMSCHRIJVINGGENERIEK` | Nee | Rolomschrijving die als "initiator" geldt bij het bepalen van de betrokkene (standaard: `initiator`) |
| `VARIABLES_PARTIJIDENTIFICATOR` | Nee | Type partij-identificator voor de opzoeking in OpenKlant: `bsn` (burger) of `kvk` (organisatie) (standaard: `bsn`) |
| `VARIABLES_EMAILOMSCHRIJVINGGENERIEK` | Nee | OpenKlant digitaal-adres-typeomschrijving die als e-mailadres wordt herkend (standaard: `Email`) |
| `VARIABLES_TELEFOONOMSCHRIJVINGGENERIEK` | Nee | OpenKlant digitaal-adres-typeomschrijving die als telefoonnummer wordt herkend (standaard: `Telefoon`) |

### AppSettings — Variabelen — OpenKlant (onderwerpobject-koppeling / partij-aanmaak)

Constanten die worden ingevuld in de `onderwerpobject`/`bijlage`-koppeling van een klantcontact, en (de laatste twee) bij het aanmaken van een ontbrekende partij in OpenKlant.

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `VARIABLES_OPENKLANT_CODEOBJECTTYPE` | Nee | `codeObjecttype` voor de koppeling naar een zaak (standaard: `zaak`) |
| `VARIABLES_OPENKLANT_CODEREGISTER` | Nee | `codeRegister` voor de koppeling naar een zaak (standaard: `open-zaak`) |
| `VARIABLES_OPENKLANT_CODEOBJECTTYPEID` | Nee | `codeSoortObjectId` voor de koppeling naar een zaak (standaard: `uuid`) |
| `VARIABLES_OPENKLANT_CODEOBJECTTYPE_BERICHT` | Nee | `codeObjecttype` voor de MOBB/Berichtenbox-koppeling (standaard: `bericht` — placeholder, nog niet bevestigd in OpenKlant) |
| `VARIABLES_OPENKLANT_CODEREGISTER_BERICHT` | Nee | `codeRegister` voor de MOBB/Berichtenbox-koppeling (standaard: `open-vtb`) |
| `VARIABLES_OPENKLANT_CODEOBJECTTYPE_BIJLAGE` | Nee | `codeObjecttype` voor een klantcontact-bijlage die naar een informatieobject in de Documenten-API verwijst (standaard: `enkelvoudiginformatieobject`) |
| `VARIABLES_OPENKLANT_CODEREGISTER_BIJLAGE` | Nee | `codeRegister` voor een klantcontact-bijlage (standaard: `open-zaak`) — pas dit aan wanneer het documentregister afwijkt, bijvoorbeeld `nld:denhaag:zaken-main:drc` |
| `VARIABLES_OPENKLANT_CODEOBJECTTYPE_PARTIJ` | Nee | `codeObjecttype` gebruikt bij het aanmaken van een ontbrekende partij (burger) in OpenKlant voor de Print/MOBB-flows (standaard: `natuurlijk_persoon`) |
| `VARIABLES_OPENKLANT_CODEREGISTER_PARTIJ` | Nee | `codeRegister` gebruikt bij het aanmaken van een ontbrekende partij in OpenKlant (standaard: `brp`) |

### AppSettings — Variabelen — UxMessages

Burgergerichte fallback-tekst per kanaal en uitkomst, gebruikt wanneer de daadwerkelijke template-subject/body niet bij NotifyNL kon worden opgehaald.

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `VARIABLES_UXMESSAGES_SMS_SUCCESS_SUBJECT` | Nee | Fallback-onderwerp bij een geslaagde sms (standaardwaarde aanwezig in `appsettings.json`) |
| `VARIABLES_UXMESSAGES_SMS_SUCCESS_BODY` | Nee | Fallback-inhoud bij een geslaagde sms (standaardwaarde aanwezig) |
| `VARIABLES_UXMESSAGES_SMS_FAILURE_SUBJECT` | Nee | Fallback-onderwerp bij een mislukte sms (standaardwaarde aanwezig) |
| `VARIABLES_UXMESSAGES_SMS_FAILURE_BODY` | Nee | Fallback-inhoud bij een mislukte sms (standaardwaarde aanwezig) |
| `VARIABLES_UXMESSAGES_EMAIL_SUCCESS_SUBJECT` | Nee | Fallback-onderwerp bij een geslaagde e-mail (standaardwaarde aanwezig) |
| `VARIABLES_UXMESSAGES_EMAIL_SUCCESS_BODY` | Nee | Fallback-inhoud bij een geslaagde e-mail (standaardwaarde aanwezig) |
| `VARIABLES_UXMESSAGES_EMAIL_FAILURE_SUBJECT` | Nee | Fallback-onderwerp bij een mislukte e-mail (standaardwaarde aanwezig) |
| `VARIABLES_UXMESSAGES_EMAIL_FAILURE_BODY` | Nee | Fallback-inhoud bij een mislukte e-mail (standaardwaarde aanwezig) |
| `VARIABLES_UXMESSAGES_LETTER_SUCCESS_SUBJECT` | Ja, indien Brief-kanaal gebruikt wordt | Fallback-onderwerp bij een geslaagde brief — **geen standaardwaarde in `appsettings.json`**, ontbreken veroorzaakt een fout bij eerste gebruik |
| `VARIABLES_UXMESSAGES_LETTER_SUCCESS_BODY` | Ja, indien Brief-kanaal gebruikt wordt | Fallback-inhoud bij een geslaagde brief — geen standaardwaarde aanwezig |
| `VARIABLES_UXMESSAGES_LETTER_FAILURE_SUBJECT` | Ja, indien Brief-kanaal gebruikt wordt | Fallback-onderwerp bij een mislukte brief — geen standaardwaarde aanwezig |
| `VARIABLES_UXMESSAGES_LETTER_FAILURE_BODY` | Ja, indien Brief-kanaal gebruikt wordt | Fallback-inhoud bij een mislukte brief — geen standaardwaarde aanwezig |

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

## OMC — Context

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `OMC_CONTEXT_PATH` | Nee | ASP.NET `PathBase` waaronder de applicatie wordt gehost; leeg = geen path-base |

---

## OMC — Actor

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `OMC_ACTOR_ID` | Ja | UUID van het OMC-actorprofiel dat wordt gekoppeld aan elk geregistreerd contactmoment |

---

## ZGW — Algemeen

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `ZGW_URN` | Nee | ZGW URN (OIN/RSIN) identifier |

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
| `ZGW_AUTH_KEY_OPENVTB` | Ja | API-sleutel voor OpenVTB (Berichtenbox/MOBB-brontrigger) — `HttpNetworkService` bouwt bij het opstarten een HttpClient voor elke `HttpClientTypes`-waarde, dus dit is ook vereist wanneer MOBB niet gebruikt wordt |

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
| `ZGW_ENDPOINT_DOCUMENTEN` | Ja | Basis-URL van de Documenten API (OpenZaak) — gebruikt door de printstraat-flow om de PDF op te halen; de `pdfurl` uit het printobject moet dezelfde origin hebben (SSRF-check) |
| `ZGW_ENDPOINT_OPENVTB` | Ja | Basis-URL van OpenVTB (Berichtenbox/MOBB-brontrigger) — zie `ZGW_AUTH_KEY_OPENVTB` hierboven voor waarom dit ook vereist is wanneer MOBB niet gebruikt wordt |

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
| `ZGW_WHITELIST_VTBMESSAGE_TYPES` | Nee | Toegestane berichttypes voor de MOBB/Berichtenbox-flow |
| `ZGW_WHITELIST_MESSAGE_ALLOWED` | Ja | `true` of `false` — schakel het scenario Bericht ontvangen in of uit |
| `ZGW_WHITELIST_PRINT_ALLOWED` | Ja | `true` of `false` — schakel het scenario Printen (printstraat) in of uit |

---

## ZGW — Objecttype UUID's

Deze UUID's verwijzen naar objecttype-definities in de ObjectTypen-API en zijn omgevingsspecifiek.

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `ZGW_VARIABLE_OBJECTTYPE_TASKOBJECTTYPE_UUID` | Ja | UUID van het taakobjecttype in ObjectTypen |
| `ZGW_VARIABLE_OBJECTTYPE_MESSAGEOBJECTTYPE_UUID` | Ja | UUID van het berichtobjecttype in ObjectTypen |
| `ZGW_VARIABLE_OBJECTTYPE_MESSAGEOBJECTTYPE_VERSION` | Nee | Versie van het berichtobjecttype (standaard: `1`) |
| `ZGW_VARIABLE_OBJECTTYPE_KTOOBJECTTYPE_UUID` | Nee | UUID van het KTO-objecttype in ObjectTypen |
| `ZGW_VARIABLE_OBJECTTYPE_PRINTOBJECTTYPE_UUID` | Ja | UUID van het printobjecttype in ObjectTypen — objecten van dit type starten de printstraat-flow |
| `ZGW_VARIABLE_OBJECTTYPE_DECISIONINFOOBJECTTYPE_UUIDS` | Nee | Kommagescheiden lijst van UUID's — welke informatieobjecttypes een geldige besluit-bijlage mogen zijn (scenario Besluit genomen) |

---

## NotifyNL — API

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `NOTIFY_API_BASEURL` | Ja | NotifyNL API basis-URL — `https://api.notifynl.nl` |
| `NOTIFY_API_KEY` | Ja | NotifyNL API-sleutel (formaat: `naam-UUID-UUID`) |

---

## NotifyNL — Besluit-template (alle kanalen)

Het scenario Besluit genomen genereert alleen een template-preview (voor de Objecten-API) en verstuurt niet via NotifyNL zelf; er is daarom één gedeelde template-UUID voor alle kanalen, in plaats van een template per kanaal zoals bij de andere scenario's.

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `NOTIFY_TEMPLATEID_DECISIONMADE` | Nee* | Gedeelde template-UUID voor het scenario Besluit genomen (alle kanalen) |

> \* Zie de voetnoot onder "NotifyNL — Brieftemplates" hieronder voor het algemene gedrag wanneer geen template-ID is ingesteld.

---

## NotifyNL — E-mailtemplates

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `NOTIFY_TEMPLATEID_EMAIL_ZAAKCREATE` | Nee* | Template-UUID voor zaak aangemaakt via e-mail |
| `NOTIFY_TEMPLATEID_EMAIL_ZAAKUPDATE` | Nee* | Template-UUID voor zaak gewijzigd via e-mail |
| `NOTIFY_TEMPLATEID_EMAIL_ZAAKCLOSE` | Nee* | Template-UUID voor zaak afgesloten via e-mail |
| `NOTIFY_TEMPLATEID_EMAIL_TASKASSIGNED` | Nee* | Template-UUID voor taak toegewezen via e-mail |
| `NOTIFY_TEMPLATEID_EMAIL_MESSAGERECEIVED` | Nee* | Template-UUID voor bericht ontvangen via e-mail |
| `NOTIFY_TEMPLATEID_EMAIL_MESSAGEBOX` | Nee* | Template-UUID voor de MOBB/Berichtenbox-fallback via e-mail |

---

## NotifyNL — Sms-templates

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `NOTIFY_TEMPLATEID_SMS_ZAAKCREATE` | Nee* | Template-UUID voor zaak aangemaakt via sms |
| `NOTIFY_TEMPLATEID_SMS_ZAAKUPDATE` | Nee* | Template-UUID voor zaak gewijzigd via sms |
| `NOTIFY_TEMPLATEID_SMS_ZAAKCLOSE` | Nee* | Template-UUID voor zaak afgesloten via sms |
| `NOTIFY_TEMPLATEID_SMS_TASKASSIGNED` | Nee* | Template-UUID voor taak toegewezen via sms |
| `NOTIFY_TEMPLATEID_SMS_MESSAGERECEIVED` | Nee* | Template-UUID voor bericht ontvangen via sms |

---

## NotifyNL — Brieftemplates

> ⚠️ **Bekende beperking:** door een bug in de configuratielaag (`OmcConfiguration`, `LetterComponent`) worden `NOTIFY_TEMPLATEID_LETTER_ZAAKCREATE`, `..._ZAAKUPDATE`, `..._ZAAKCLOSE`, `..._TASKASSIGNED` en `..._MESSAGERECEIVED` momenteel **niet** gebruikt. Het Brief-kanaal hergebruikt voor deze scenario's stilzwijgend de bijbehorende `NOTIFY_TEMPLATEID_SMS_*`-waarde. Stel dus voorlopig de gewenste brief-template in via de sms-variabele van hetzelfde scenario. Alleen `NOTIFY_TEMPLATEID_LETTER_MESSAGEBOX` werkt zoals de naam doet vermoeden. Dit is een bekend, bewust nog niet opgelost probleem (het fixen ervan raakt 5 live scenario's en vraagt afstemming over omgevingen heen).

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `NOTIFY_TEMPLATEID_LETTER_ZAAKCREATE` | Nee* | **Genegeerd — zie waarschuwing hierboven.** Gebruik in plaats daarvan `NOTIFY_TEMPLATEID_SMS_ZAAKCREATE` |
| `NOTIFY_TEMPLATEID_LETTER_ZAAKUPDATE` | Nee* | **Genegeerd — zie waarschuwing hierboven.** Gebruik in plaats daarvan `NOTIFY_TEMPLATEID_SMS_ZAAKUPDATE` |
| `NOTIFY_TEMPLATEID_LETTER_ZAAKCLOSE` | Nee* | **Genegeerd — zie waarschuwing hierboven.** Gebruik in plaats daarvan `NOTIFY_TEMPLATEID_SMS_ZAAKCLOSE` |
| `NOTIFY_TEMPLATEID_LETTER_TASKASSIGNED` | Nee* | **Genegeerd — zie waarschuwing hierboven.** Gebruik in plaats daarvan `NOTIFY_TEMPLATEID_SMS_TASKASSIGNED` |
| `NOTIFY_TEMPLATEID_LETTER_MESSAGERECEIVED` | Nee* | **Genegeerd — zie waarschuwing hierboven.** Gebruik in plaats daarvan `NOTIFY_TEMPLATEID_SMS_MESSAGERECEIVED` |
| `NOTIFY_TEMPLATEID_LETTER_MESSAGEBOX` | Nee* | Template-UUID voor de MOBB/Berichtenbox-fallback via brief — **werkt correct, niet door de bug getroffen** |

> \* Als er geen template-ID is ingesteld voor een bepaald kanaal/scenario-combinatie, slaat het OMC dat kanaal over voor dat scenario. Stel minimaal één kanaal in per scenario dat je wilt activeren.

---

## MijnOverheid

Zie [MijnOverheid](../integraties/mijnoverheid.md) voor uitleg over de integratie.

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `MIJNOVERHEID_WEBHOOK_URL` | Ja | URL van de MijnOverheid "zaak-muteren"-webhook |
| `MIJNOVERHEID_AUTH_CLIENTID` | Ja | OAuth2 client-ID voor de MijnOverheid-tokenuitwisseling |
| `MIJNOVERHEID_AUTH_SECRET` | Ja | OAuth2 clientgeheim voor MijnOverheid |
| `MIJNOVERHEID_AUTH_TOKEN_ENDPOINT` | Ja | OAuth2 token-eindpunt van MijnOverheid |

---

## KTO / Expoints

Stel alle KTO-variabelen in op `-` als de integratie niet wordt gebruikt.

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `KTO_URL` | Nee | Basis-URL van de KTO Expoints-dienst |
| `KTO_AUTH_JWT_SECRET` | Nee | JWT geheime sleutel voor de KTO-dienst |
| `KTO_AUTH_JWT_ISSUER` | Nee | JWT-uitgever voor de KTO-dienst |
| `KTO_AUTH_JWT_SCOPE` | Nee | JWT-scope voor de KTO-dienst |
| `KTO_AUTH_JWT_CLIENTID` | Nee | JWT client-ID voor de KTO-dienst |

---

## PostGuard

Stel alle PostGuard-variabelen in op `-` als de integratie niet wordt gebruikt.

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `POSTGUARD_API_KEY` | Nee | API-sleutel voor de PostGuard-dienst |
| `POSTGUARD_API_PKGURL` | Nee | URL van de PostGuard "pkg"-dienst |
| `POSTGUARD_API_CRYPTIFYURL` | Nee | URL van de PostGuard "cryptify"-dienst |
| `POSTGUARD_TEMPLATEID_SENDPOSTGUARDPDF` | Nee | Template-UUID voor het versturen van een vooraf samengestelde PDF via PostGuard |

---

## BRP / Haal Centraal — Keycloak

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `KEYCLOAK_AUTHSERVERURL` | Nee | Basis-URL van de Keycloak-autorisatieserver |
| `KEYCLOAK_CLIENTID` | Nee | OAuth2 client-ID voor Keycloak-tokenuitwisseling |
| `KEYCLOAK_CLIENTSECRET` | Nee | OAuth2 clientgeheim voor Keycloak |
| `KEYCLOAK_TOKENEXCHANGEAUDIENCE` | Nee | Doelgroep (audience) voor de Keycloak token-exchange (standaard: `haalcentraal`) |
| `BRP_BASEURL` | Nee | Basis-URL van de BRP (Basisregistratie Personen) / WS Gateway-dienst — vereist wanneer de BRP-integratie gebruikt wordt |

---

## BRP / Haal Centraal — mTLS

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `BRP_CLIENTCERT_PEM_PATH` | Nee | Bestandspad naar het PEM-gecodeerde clientcertificaat |
| `BRP_CLIENTKEY_PEM_PATH` | Nee | Bestandspad naar de PEM-gecodeerde private key |

---

## Dashboard

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `DASHBOARD_ORIGINS` | Nee | Toegestane CORS-origins voor het dashboard (standaard: `http://localhost:3000`) |
| `DASHBOARD_URL` | Nee | Redirect-doel voor de root (`/`) |

---

## Monitoring

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `SENTRY_DSN` | Nee | Sentry DSN voor centrale foutregistratie en logging |
| `SENTRY_ENVIRONMENT` | Nee | Omgevingsnaam die aan Sentry wordt doorgegeven — valt terug op `ASPNETCORE_ENVIRONMENT` indien niet ingesteld |
