# Stap 2 — Configureer ZGW API sleutels

Het OMC heeft referenties nodig om te authenticeren met je ZGW-diensten. De exacte referenties die nodig zijn, hangen af van je [werkwijze versie](../werkwijzen/versies.md), maar het volgende dekt de volledige set.

---

## 2.1 OpenKlant API-sleutel

Ga in de **OpenKlant**-beheeromgeving naar **API Auth** en genereer een token voor het OMC.

| Variabele | Beschrijving |
|---|---|
| `ZGW_AUTH_KEY_OPENKLANT` | Vereist voor werkwijze **v2 en hoger** |

![OpenKlant API-sleutel aanmaken](../images/step%202%20image%201.png)

---

## 2.2 OpenZaak JWT-referenties

OpenZaak gebruikt JWT-authenticatie. Maak in de **OpenZaak**-beheeromgeving een nieuwe applicatie/client aan met de volgende parameters en sla ze op als omgevingsvariabelen:

| Variabele | Beschrijving |
|---|---|
| `ZGW_AUTH_JWT_SECRET` | De geheime sleutel geconfigureerd in OpenZaak |
| `ZGW_AUTH_JWT_ISSUER` | De client ID / uitgevernaam geconfigureerd in OpenZaak |
| `ZGW_AUTH_JWT_AUDIENCE` | De doelgroep (optioneel) |
| `ZGW_AUTH_JWT_EXPIRESINMIN` | Tokenlevensduur in minuten — stel in op `60` |
| `ZGW_AUTH_JWT_USERID` | De gebruikers-ID gekoppeld aan het OMC (bijv. een e-mailadres) |
| `ZGW_AUTH_JWT_USERNAME` | Een leesbare naam voor het OMC (bijv. `"Gemeente Rotterdam"`) |

> De JWT-geheime sleutel en claims die hier worden geconfigureerd, **moeten exact overeenkomen** met wat is geregistreerd in de OpenZaak-beheerinterface. Het OMC genereert het JWT-token intern met behulp van deze waarden.

![OpenZaak JWT-referenties](../images/step%202%20image%202.png)

---

## 2.3 Objecten API-sleutel

Genereer in de **Objecten**-beheeromgeving een token voor het OMC.

| Variabele | Beschrijving |
|---|---|
| `ZGW_AUTH_KEY_OBJECTEN` | API-sleutel voor de Objecten-dienst |

![Objecten API-sleutel aanmaken](../images/step%202%20image%203.png)

---

## 2.4 ObjectTypen API-sleutel

Genereer in de **ObjectTypen**-beheeromgeving een token voor het OMC.

| Variabele | Beschrijving |
|---|---|
| `ZGW_AUTH_KEY_OBJECTTYPEN` | API-sleutel voor de ObjectTypen-dienst |

![ObjectTypen API-sleutel aanmaken](../images/step%202%20image%204.png)

---

## 2.5 Service-eindpunten

Configureer de basis-URL's voor elke ZGW-dienst. Alle URL's moeten het protocol bevatten (`https://`) en mogen **niet** eindigen met een schuine streep.

| Variabele | Voorbeeldwaarde |
|---|---|
| `ZGW_ENDPOINT_OPENNOTIFICATIES` | `https://opennotificaties.mijnstad.nl/api/v1` |
| `ZGW_ENDPOINT_OPENZAAK` | `https://openzaak.mijnstad.nl/zaken/api/v1` |
| `ZGW_ENDPOINT_OPENKLANT` | `https://openklant.mijnstad.nl/klanten/api/v1` |
| `ZGW_ENDPOINT_BESLUITEN` | `https://openzaak.mijnstad.nl/besluiten/api/v1` |
| `ZGW_ENDPOINT_OBJECTEN` | `https://objecten.mijnstad.nl/api/v2` |
| `ZGW_ENDPOINT_OBJECTTYPEN` | `https://objecttypen.mijnstad.nl/api/v2` |
| `ZGW_ENDPOINT_CONTACTMOMENTEN` | `https://openklant.mijnstad.nl/contactmomenten/api/v1` |

> De exacte paden zijn afhankelijk van hoe je ZGW-diensten zijn uitgerold. Controleer de beheerinterface of API-documentatie van elke dienst om het juiste basispad te bevestigen.
