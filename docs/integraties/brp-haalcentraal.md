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

## Endpoint

| Variabele | Beschrijving |
|---|---|
| `BRP_BASEURL` | Basis-URL van de BRP / WS Gateway-dienst — vereist wanneer de BRP-integratie gebruikt wordt |

---

## Tweelaagse beveiliging

De BRP-verbinding vereist twee beveiligingslagen:

### Laag 1 — Keycloak OAuth2-tokenuitwisseling

Het OMC haalt eerst met `client_credentials` een service-token op bij Keycloak en wisselt dat vervolgens in voor een token dat geldig is voor de BRP-audience (`urn:ietf:params:oauth:grant-type:token-exchange`).

| Variabele | Beschrijving |
|---|---|
| `KEYCLOAK_AUTHSERVERURL` | Basis-URL van de Keycloak-autorisatieserver — het OMC voegt hier zelf `/token` aan toe |
| `KEYCLOAK_CLIENTID` | OAuth2 client ID geregistreerd in Keycloak |
| `KEYCLOAK_CLIENTSECRET` | OAuth2 clientgeheim |
| `KEYCLOAK_TOKENEXCHANGEAUDIENCE` | Doelgroep (audience) van het uitgewisselde token (standaard: `haalcentraal`) |

### Laag 2 — Mutuele TLS (mTLS)

De BRP API vereist dat het OMC een clientcertificaat presenteert bij elke aanroep.

| Variabele | Beschrijving |
|---|---|
| `BRP_CLIENTCERT_PEM_PATH` | Bestandspad naar het PEM-gecodeerde clientcertificaat |
| `BRP_CLIENTKEY_PEM_PATH` | Bestandspad naar de PEM-gecodeerde private key |

> Dit zijn **bestandspaden**, geen base64-blobs: het OMC leest beide bestanden van schijf en combineert ze met `X509Certificate2.CreateFromPem`. Ontbreken de variabelen of de bestanden, dan start het OMC gewoon op — maar zijn BRP-aanroepen niet beschikbaar.

Zie [Omgevingsvariabelen](../configuratie/omgevingsvariabelen.md) voor de volledige lijst.

---

## Operationele notities

- BRP-gegevens worden uitsluitend in het geheugen verwerkt — nooit opgeslagen of gelogd
- Als BRP niet beschikbaar is, valt het OMC terug op de gegevens uit OpenKlant
- Clientcertificaten moeten periodiek worden geroteerd conform de vereisten van de BRP-aanbieder
- Neem contact op met de BRP-aanbieder (RvIG of gemeente) voor certificaatuitgifte en Keycloak-registratie
