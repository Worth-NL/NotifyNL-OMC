# appsettings.json

Het OMC laadt zijn configuratie vanuit `appsettings.json`, dat kan worden overschreven via omgevingsvariabelen. In Kubernetes/Docker-uitrollingen zijn omgevingsvariabelen de aanbevolen methode.

---

## Configuratiestructuur

```json
{
  "OMC": {
    "Authorization": {
      "JWT": {
        "Secret": "",
        "Issuer": "",
        "Audience": "",
        "ExpiresInMin": 60,
        "UserId": "",
        "UserName": ""
      }
    },
    "Feature": {
      "Workflow_Version": 2
    },
    "Actor": {
      "Name": ""
    }
  },
  "ZGW": {
    "Authorization": {
      "JWT": {
        "Secret": "",
        "Issuer": "",
        "Audience": "",
        "ExpiresInMin": 60,
        "UserId": "",
        "UserName": ""
      },
      "Key": {
        "OpenKlant": "",
        "Objecten": "",
        "ObjectTypen": ""
      }
    },
    "Endpoint": {
      "OpenNotificaties": "",
      "OpenZaak": "",
      "OpenKlant": "",
      "Besluiten": "",
      "Objecten": "",
      "ObjectTypen": "",
      "ContactMomenten": ""
    },
    "Whitelist": {
      "ZaakCreate_IDs": "",
      "ZaakUpdate_IDs": "",
      "ZaakClose_IDs": "",
      "TaskAssigned_IDs": "",
      "DecisionMade_IDs": "",
      "Message_Allowed": false
    },
    "Variable": {
      "ObjectType": {
        "Taak_UUID": "",
        "Message_UUID": "",
        "ProductRequest_UUID": "",
        "KtoObjectType_UUID": ""
      },
      "Objecten": {
        "MessageObjectType_Version": "",
        "TaakObjectType_Version": ""
      }
    }
  },
  "Notify": {
    "API": {
      "BaseURL": "https://api.notifynl.nl",
      "Key": ""
    },
    "TemplateId": {
      "Email": {
        "ZaakCreate": "",
        "ZaakUpdate": "",
        "ZaakClose": "",
        "TaskAssigned": "",
        "DecisionMade": "",
        "MessageReceived": ""
      },
      "Sms": {
        "ZaakCreate": "",
        "ZaakUpdate": "",
        "ZaakClose": "",
        "TaskAssigned": "",
        "DecisionMade": "",
        "MessageReceived": ""
      },
      "Letter": {
        "ZaakCreate": "",
        "ZaakUpdate": "",
        "ZaakClose": "",
        "TaskAssigned": "",
        "DecisionMade": "",
        "MessageReceived": ""
      }
    }
  },
  "User": {
    "Authorization": {
      "JWT": {
        "Secret": "",
        "Issuer": "",
        "Audience": "",
        "ExpiresInMin": 60,
        "UserId": "",
        "UserName": ""
      }
    }
  }
}
```

---

## Omgevingsvariabelen als overschrijving

Alle instellingen in `appsettings.json` kunnen worden overschreven via omgevingsvariabelen. De omgevingsvariabelenaam wordt afgeleid van het configuratiepad door dubbele punten te vervangen door underscores en de naam te kapitaliseren.

Bijvoorbeeld:

| JSON pad | Omgevingsvariabele |
|---|---|
| `OMC:Authorization:JWT:Secret` | `OMC_AUTH_JWT_SECRET` |
| `ZGW:Authorization:JWT:Secret` | `ZGW_AUTH_JWT_SECRET` |
| `ZGW:Endpoint:OpenZaak` | `ZGW_ENDPOINT_OPENZAAK` |
| `Notify:API:Key` | `NOTIFY_API_KEY` |

> In Docker/Kubernetes-uitrollingen zijn omgevingsvariabelen de aanbevolen manier om configuratie in te stellen. Sla nooit geheimen op in `appsettings.json` in versiebeheer.

Zie [Omgevingsvariabelen](omgevingsvariabelen.md) voor de volledige referentie van alle beschikbare instellingen.
