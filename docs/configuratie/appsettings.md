# appsettings.json

Het OMC laadt zijn configuratie vanuit `appsettings.json`, dat kan worden overschreven via omgevingsvariabelen. In Kubernetes/Docker-uitrollingen zijn omgevingsvariabelen de aanbevolen methode.

---

## Configuratiestructuur

Alleen de `Network`-, `Encryption`- en `Variables`-secties worden daadwerkelijk vanuit `appsettings.json` gelezen (met omgevingsvariabele-overschrijving). Dit is de volledige, actuele structuur:

```json
{
  "Network": {
    "ConnectionLifetimeInSeconds": 90,
    "HttpRequestTimeoutInSeconds": 60,
    "HttpRequestsSimultaneousNumber": 20
  },
  "Encryption": {
    "IsAsymmetric": false
  },
  "Variables": {
    "BetrokkeneType": "natuurlijk_persoon",
    "OmschrijvingGeneriek": "initiator",
    "PartijIdentificator": "bsn",
    "EmailOmschrijvingGeneriek": "Email",
    "TelefoonOmschrijvingGeneriek": "Telefoon",
    "OpenKlant": {
      "CodeObjectType": "zaak",
      "CodeRegister": "open-zaak",
      "CodeObjectTypeId": "uuid",
      "CodeObjectType_Bericht": "bericht",
      "CodeRegister_Bericht": "open-vtb",
      "CodeObjectType_Bijlage": "enkelvoudiginformatieobject",
      "CodeRegister_Bijlage": "open-zaak",
      "CodeObjectType_Partij": "natuurlijk_persoon",
      "CodeRegister_Partij": "brp"
    },
    "UxMessages": {
      "SMS_Success_Subject": "",
      "SMS_Success_Body": "",
      "SMS_Failure_Subject": "",
      "SMS_Failure_Body": "",
      "Email_Success_Subject": "",
      "Email_Success_Body": "",
      "Email_Failure_Subject": "",
      "Email_Failure_Body": ""
    }
  },
  "AllowedHosts": "*"
}
```

> **Let op:** `Variables.UxMessages` heeft in `appsettings.json` alleen standaardwaarden voor de `SMS_*`- en `Email_*`-sleutels. De vier `Letter_*`-sleutels (`Letter_Success_Subject`, `Letter_Success_Body`, `Letter_Failure_Subject`, `Letter_Failure_Body`) hebben **geen** standaardwaarde in `appsettings.json` en moeten via omgevingsvariabele worden aangeleverd zodra het Brief-kanaal gebruikt wordt — anders faalt de configuratielookup bij het eerste gebruik. Zie [Omgevingsvariabelen](omgevingsvariabelen.md) voor de volledige lijst.

`Logging` en `AllowedHosts` staan in `appsettings.json` om ASP.NET Core-conventies te volgen, maar worden niet door de OMC-configuratielaag (`OmcConfiguration`) gelezen.

---

## OMC, ZGW, Notify, KTO en PostGuard — alleen via omgevingsvariabele

De `OMC`-, `ZGW`-, `Notify`-, `KTO`- en `PostGuard`-secties (JWT-secrets, API-eindpunten, whitelists, objecttype-UUID's, template-ID's, enzovoort) worden in code uitsluitend geladen via omgevingsvariabelen — er bestaat voor deze secties **geen** `appsettings.json`-fallback. Het opnemen van bijvoorbeeld een `"ZGW": { "Endpoint": { ... } }`-blok in `appsettings.json` heeft dus geen effect; deze instellingen moeten als omgevingsvariabele worden aangeleverd.

Zie [Omgevingsvariabelen](omgevingsvariabelen.md) voor de volledige, actuele referentie van al deze instellingen.

---

## Omgevingsvariabelen als overschrijving

Alle instellingen in `appsettings.json` kunnen worden overschreven via omgevingsvariabelen. De omgevingsvariabelenaam wordt afgeleid van het configuratiepad door dubbele punten te vervangen door underscores en de naam te kapitaliseren.

Bijvoorbeeld:

| JSON pad | Omgevingsvariabele |
|---|---|
| `Network:ConnectionLifetimeInSeconds` | `NETWORK_CONNECTIONLIFETIMEINSECONDS` |
| `Variables:PartijIdentificator` | `VARIABLES_PARTIJIDENTIFICATOR` |
| `Variables:OpenKlant:CodeRegister_Bijlage` | `VARIABLES_OPENKLANT_CODEREGISTER_BIJLAGE` |

> In Docker/Kubernetes-uitrollingen zijn omgevingsvariabelen de aanbevolen manier om configuratie in te stellen. Sla nooit geheimen op in `appsettings.json` in versiebeheer.

Zie [Omgevingsvariabelen](omgevingsvariabelen.md) voor de volledige referentie van alle beschikbare instellingen.
