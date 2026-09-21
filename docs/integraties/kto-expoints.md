# KTO / Expoints

Het OMC ondersteunt integratie met de KTO (Klanttevredenheidsonderzoek) Expoints-dienst: het OMC kan automatisch een uitnodiging voor een klanttevredenheidsonderzoek versturen.

---

## Hoe het werkt

1. Er wordt een KTO-object aangemaakt in de "Objecten" Web API-dienst
2. Het OMC ontvangt de notificatie daarvan en vergelijkt het objecttype met `ZGW_VARIABLE_OBJECTTYPE_KTOOBJECTTYPE_UUID`; bij een match kiest het scenario **KTO received**
3. Het OMC haalt het object op en bouwt uit `record.data` de aanvraag voor de Expoints API
4. Het OMC haalt een OAuth2-token op (`client_credentials`) en post de aanvraag naar `KTO_URL`
5. Na een geslaagde verzending verwijdert het OMC het KTO-object

> De KTO-uitnodiging wordt dus niet door het scenario "Zaak afgesloten" aangestuurd, en er is geen omgevingsvariabele die zaaktypen op enquêtes koppelt: het bestaan van het KTO-object bepaalt of er een uitnodiging uitgaat.

---

## Configuratie

Stel alle KTO-variabelen in op `-` als de integratie niet wordt gebruikt.

| Variabele | Beschrijving |
|---|---|
| `KTO_URL` | Endpoint van de KTO Expoints-dienst waarnaar de aanvraag wordt gepost |
| `KTO_AUTH_JWT_CLIENTID` | OAuth2 client ID voor de KTO-dienst |
| `KTO_AUTH_JWT_SECRET` | OAuth2 clientgeheim voor de KTO-dienst |
| `KTO_AUTH_JWT_SCOPE` | OAuth2 scope voor de KTO-dienst |
| `KTO_AUTH_JWT_ISSUER` | Het token-endpoint waar het OMC het OAuth2-token ophaalt (ondanks de naam een volledige URL, geen issuer-identifier) |

Daarnaast bepaalt `ZGW_VARIABLE_OBJECTTYPE_KTOOBJECTTYPE_UUID` welk objecttype het KTO-scenario activeert — zie [Omgevingsvariabelen](../configuratie/omgevingsvariabelen.md).

---

## Indeling van het KTO-object

Het OMC leest de volgende velden uit `record.data` van het KTO-object:

| Veld | Verplicht | Beschrijving |
|---|---|---|
| `email` | Ja | E-mailadres waarnaar Expoints de uitnodiging stuurt |
| `transactiedatum` | Ja | Datum van de transactie waarop het onderzoek betrekking heeft |
| `istest` | Nee | Markeert de aanvraag als test (standaard `false`) |
| `extradata` | Ja | Array met aanvullende velden die ongewijzigd aan Expoints worden doorgegeven |

```json
{
  "email": "burger@example.nl",
  "istest": false,
  "transactiedatum": "2026-01-15",
  "extradata": [
    { "name": "zaaktype", "value": "Aanvraag parkeervergunning" }
  ]
}
```

Ontbreekt `email`, `transactiedatum` of `extradata`, dan breekt het OMC de verwerking van dit object af.

---

## Voorbeeld KTO-instellingen

![KTO Expoints instellingen voorbeeld](../images/example_kto_settings.png)
