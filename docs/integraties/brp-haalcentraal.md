# BRP / Haal Centraal

Het OMC kan optioneel integreren met de BRP (Basisregistratie Personen) via de Haal Centraal API om aanvullende persoonsgegevens op te halen voor notificatietemplate-verrijking.

---

## Opgehaalde BRP-gegevens

Als de BRP-integratie is ingeschakeld, haalt het OMC de volgende gegevens op:

| Gegeven | ZGW-veld | Template-placeholder |
|---|---|---|
| Voornaam | `voornamen` | `((klant.voornaam))` |
| Tussenvoegsel | `voorvoegselGeslachtsnaam` | `((klant.voorvoegselAchternaam))` |
| Achternaam | `geslachtsnaam` | `((klant.achternaam))` |

> Zonder BRP-integratie haalt het OMC contactgegevens op via OpenKlant. BRP biedt aanvullende naamgegevens wanneer die niet beschikbaar zijn in OpenKlant.

---

## Tweelaagse beveiliging

De BRP-verbinding vereist twee beveiligingslagen:

### Laag 1 — Keycloak OAuth2-tokenuitwisseling

Het OMC wisselt zijn credentials in bij Keycloak voor een toegangstoken dat geldig is voor de BRP API.

| Variabele | Beschrijving |
|---|---|
| `BRP_AUTH_CLIENTID` | OAuth2 client ID geregistreerd in Keycloak |
| `BRP_AUTH_CLIENTSECRET` | OAuth2 clientgeheim |
| `BRP_AUTH_SCOPE` | OAuth2 scope(s) voor BRP-toegang |
| `BRP_AUTH_REDIRECTURI` | Redirect-URI geregistreerd in Keycloak |
| `BRP_AUTH_TOKENENDPOINT` | Keycloak token-eindpunt URL |

### Laag 2 — Mutuele TLS (mTLS)

De BRP API vereist dat het OMC een clientcertificaat presenteert bij elke aanroep.

| Variabele | Beschrijving |
|---|---|
| `BRP_MTLS_CERTIFICATE` | Base64-gecodeerd PEM-clientcertificaat |
| `BRP_MTLS_KEY` | Base64-gecodeerde PEM-privésleutel |

---

## Operationele notities

- BRP-gegevens worden uitsluitend in het geheugen verwerkt — nooit opgeslagen of gelogd
- Als BRP niet beschikbaar is, valt het OMC terug op de gegevens uit OpenKlant
- Clientcertificaten moeten periodiek worden geroteerd conform de vereisten van de BRP-aanbieder
- Neem contact op met de BRP-aanbieder (RvIG of gemeente) voor certificaatuitgifte en Keycloak-registratie
