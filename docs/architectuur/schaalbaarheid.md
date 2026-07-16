# Schaalbaarheid

Het OMC is ontworpen voor horizontale schaalbaarheid zonder shared state of sessieaffiniteit.

---

## Stateless ontwerp

Elke aanroep op `/Events/Listen` wordt volledig onafhankelijk afgehandeld. Het OMC:

- Slaat geen toestand op tussen aanroepen
- Heeft geen database of cache die tussen instanties wordt gedeeld
- Vereist geen sessieaffiniteit (sticky sessions)

Dit betekent dat je zoveel OMC-instanties kunt draaien als nodig, achter een standaard load balancer.

---

## HTTP-verbindingspoolinstellingen

Het OMC hergebruikt HTTP-verbindingen naar ZGW-diensten via `IHttpClientFactory`. De volgende instellingen zijn beschikbaar via omgevingsvariabelen:

| Variabele | Standaard | Beschrijving |
|---|---|---|
| `OMC_HTTP_CONNECTIONLIFETIMEMINUTES` | `15` | Levensduur van HTTP-verbindingen in minuten |
| `OMC_HTTP_MAXCONNECTIONSPERSERVER` | `100` | Maximum aantal verbindingen per host |

---

## Schalen met Helm

Het OMC Helm chart ondersteunt het instellen van het aantal replica's:

```yaml
replicaCount: 3

resources:
  requests:
    memory: "128Mi"
    cpu: "100m"
  limits:
    memory: "256Mi"
    cpu: "500m"
```

Zie de Worth-NL Helm chart repository voor de volledige configuratieopties.

---

## Aanbevelingen voor productie

- Stel minimaal 2 replica's in voor hoge beschikbaarheid
- Gebruik een Kubernetes HorizontalPodAutoscaler (HPA) gebaseerd op CPU of aanvraagduur
- Open Notificaties levert events opnieuw af als het OMC niet reageert — het OMC hoeft geen wachtrij te beheren
- Controleer de Application Insights metrieken om knelpunten per ZGW-dienst te identificeren
