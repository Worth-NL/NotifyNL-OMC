# Security by design

Het OMC past meerdere beveiligingslagen toe om ongeautoriseerde toegang en gegevenslekken te voorkomen.

---

## Authenticatielagen

| Laag | Mechanisme | Van toepassing op |
|---|---|---|
| Inkomende aanroepen naar OMC | JWT Bearer (HS256/RS256) | Alle `/Events/*` en `/Notify/*` eindpunten |
| OMC → Open Zaak / Besluiten | JWT Bearer (gegenereerd door OMC) | Alle uitgaande ZGW-aanroepen |
| OMC → Open Klant | API-sleutel | Alle Open Klant-aanroepen (v2) |
| OMC → Objecten / ObjectTypen | API-sleutel | Alle Objecten-aanroepen |
| OMC → BRP / Haal Centraal | OAuth2 (Keycloak) + mTLS | BRP-gegevensopvragen |
| OMC → NotifyNL | API-sleutel | Alle NotifyNL-aanroepen |

---

## JWT-beveiligingsregels

- Tokens worden gevalideerd op handtekening, verloopdatum en uitgeverbinding
- Verlopen tokens worden geweigerd met `401 Unauthorized`
- De geheime sleutel (`OMC_AUTH_JWT_SECRET`) wordt nooit opgenomen in logs
- Gebruik minimale tokenlevensduur voor interactief gebruik (30–60 minuten)

---

## mTLS voor BRP

De verbinding met BRP/Haal Centraal vereist wederzijdse TLS-authenticatie (mTLS):

- Het OMC presenteert een clientcertificaat bij elke BRP-aanroep
- De BRP-server valideert dit certificaat
- Certificaat en privésleutel worden via omgevingsvariabelen (`BRP_MTLS_CERTIFICATE`, `BRP_MTLS_KEY`) aangeboden als base64-gecodeerde PEM

---

## Geen persistentieprincipe

Het OMC slaat geen gegevens op:

- Geen database
- Geen lokale bestanden
- Geen in-memorycaches die tussen aanroepen worden gedeeld

Elke aanroep is volledig zelfstandig. Dit elimineert een complete klasse van beveiligingsrisico's (SQL-injectie, opslagdatalekken, IDOR).

---

## Aanbevelingen voor productie

- Gebruik HTTPS met een geldig certificaat voor het OMC-eindpunt
- Beperk toegang tot het OMC via netwerksegmentatie (alleen Open Notificaties mag `/Events/Listen` aanroepen)
- Roteer JWT-geheimen en API-sleutels periodiek
- Schakel Application Insights in voor auditlogging van alle aanroepen
- Gebruik Kubernetes network policies om uitgaande aanroepen te beperken tot bekende ZGW-eindpunten
