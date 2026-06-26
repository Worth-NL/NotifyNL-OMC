# Stap 3 — Uitrollen en testen

---

## 3.1 Uitrollen met Helm

Het OMC wordt gedistribueerd als een Docker-image en uitgerold via een Helm chart. Het chart is beschikbaar voor geautoriseerde gebruikers in de Worth-NL Helm chart repository.

Stel alle vereiste omgevingsvariabelen in je Helm `values.yaml` of als Kubernetes secrets in. Je hebt minimaal het volgende nodig:

```yaml
env:
  ASPNETCORE_ENVIRONMENT: "Production"

  OMC_AUTH_JWT_SECRET: ""
  OMC_AUTH_JWT_ISSUER: ""
  OMC_AUTH_JWT_AUDIENCE: ""
  OMC_AUTH_JWT_EXPIRESINMIN: "60"
  OMC_AUTH_JWT_USERID: ""
  OMC_AUTH_JWT_USERNAME: ""

  OMC_FEATURE_WORKFLOW_VERSION: "2"

  ZGW_AUTH_JWT_SECRET: ""
  ZGW_AUTH_JWT_ISSUER: ""
  ZGW_AUTH_JWT_EXPIRESINMIN: "60"
  ZGW_AUTH_JWT_USERID: ""
  ZGW_AUTH_JWT_USERNAME: ""

  ZGW_AUTH_KEY_OPENKLANT: ""
  ZGW_AUTH_KEY_OBJECTEN: ""
  ZGW_AUTH_KEY_OBJECTTYPEN: ""

  ZGW_ENDPOINT_OPENNOTIFICATIES: ""
  ZGW_ENDPOINT_OPENZAAK: ""
  ZGW_ENDPOINT_OPENKLANT: ""
  ZGW_ENDPOINT_BESLUITEN: ""
  ZGW_ENDPOINT_OBJECTEN: ""
  ZGW_ENDPOINT_OBJECTTYPEN: ""
  ZGW_ENDPOINT_CONTACTMOMENTEN: ""

  NOTIFY_API_BASEURL: "https://api.notifynl.nl"
  NOTIFY_API_KEY: ""

  # Voor initieel testen: alle zaaktypen toestaan met wildcard
  ZGW_WHITELIST_ZAAKCREATE_IDS: "*"
  ZGW_WHITELIST_ZAAKUPDATE_IDS: "*"
  ZGW_WHITELIST_ZAAKCLOSE_IDS: "*"
  ZGW_WHITELIST_TASKASSIGNED_IDS: "*"
  ZGW_WHITELIST_DECISIONMADE_IDS: "*"
  ZGW_WHITELIST_MESSAGE_ALLOWED: "false"
```

> `"*"` als whitelist-waarde accepteert alle zaaktypen. Vervang dit door specifieke zaaktype-identificaties (`zaaktypeIdentificatie`) zodra je hebt bevestigd dat de integratie werkt.

Voor de volledige lijst van omgevingsvariabelen zie [Omgevingsvariabelen](../configuratie/omgevingsvariabelen.md).

---

## 3.2 Een JWT-token genereren voor testen

Om de health check-eindpunten van het OMC aan te roepen heb je een JWT Bearer-token nodig. Genereer er één met de [Secrets Manager](../authenticatie/secrets-manager.md) of handmatig via [jwt.io](https://jwt.io) met de `OMC_AUTH_JWT_*`-referenties.

Zie [JWT-tokens](../authenticatie/jwt-tokens.md) voor de volledige claimsstructuur.

---

## 3.3 Health checks uitvoeren

Zodra het OMC draait, verifieer je de verbinding met alle ZGW-diensten via de test-eindpunten. Voeg het JWT-token toe als Bearer-header (`Authorization: Bearer <token>`).

```
POST /Test/OMC/Configuration
POST /Test/ZGW/Endpoints
GET  /Test/Notify/HealthCheck
POST /Test/Notify/SendEmail
POST /Test/Notify/SendSms
```

Een `200 OK`-respons van elk eindpunt bevestigt dat het OMC alle geconfigureerde diensten kan bereiken. Een Postman-collectie (NotifyNL-workspace) is beschikbaar via Worth Systems voor het uitvoeren van deze checks.

Zie [Eindpunten](../api-referentie/endpoints.md) voor volledige eindpuntdocumentatie.

---

## 3.4 Verifiëren met een testzaak

Maak een nieuwe zaak aan in je ZGW-omgeving gekoppeld aan een burger met een e-mailadres dat geregistreerd staat in je NotifyNL-team of gastlijst. Als het OMC is geabonneerd op events (Stap 4) en de callback is geconfigureerd (Stap 5), zou je het volgende moeten zien:

1. Een notificatie-event verschijnt in Open Notificaties gericht aan het OMC
2. Het OMC geeft een `200 OK` (of `202 Accepted`) terug aan Open Notificaties
3. De notificatie verschijnt in de NotifyNL-beheerportal onder API-activiteit

---

## 3.5 Swagger UI

Het OMC biedt een Swagger UI voor interactieve API-verkenning:

```
https://<jouw-omc-domein>/swagger/index.html
```

Beschikbaar in zowel `Development`- als `Production`-omgevingen. Vereist een geldig JWT Bearer-token — zie [JWT-tokens](../authenticatie/jwt-tokens.md).

![Swagger UI](../images/swagger_ui_example.png)
