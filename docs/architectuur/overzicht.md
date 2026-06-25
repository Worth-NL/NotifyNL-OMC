# Architectuuroverzicht

Het OMC volgt een **Clean Architecture** (ui-laag architectuur) waarbij de domeinlogica volledig is geïsoleerd van infrastructuurdetails.

---

## Lagen

| Laag | Project | Verantwoordelijkheid |
|---|---|---|
| **Domein** | `ZgwModels`, `SecretsManager` | Entiteiten, waardeobjecten, domeinlogica |
| **Applicatie** | `Common` | Gebruikscases, interfaces, scenario-orchestratie |
| **Infrastructuur** | `WebQueries`, `EventsHandler` | ZGW API-clients, NotifyNL-client, JWT-validatie, controllers |

Afhankelijkheden wijzen altijd naar binnen toe — infrastructuur hangt af van applicatie, applicatie hangt af van domein, nooit andersom.

---

## Ontwerppatronen

| Patroon | Toepassing |
|---|---|
| **Strategy Pattern** | Scenario's, versleutelingsstrategieën, configuratieladers |
| **Adapter Pattern** | `IQueryContext` / `QueryContext` voor ZGW API-aanroepen |
| **Loader/Fallback Pattern** | Configuratie-overschrijvingen via omgevingsvariabelen |

---

## Technologiestack

| Component | Technologie |
|---|---|
| Runtime | .NET 10 (ASP.NET Core) |
| API-protocol | REST / HTTP |
| Authenticatie | JWT Bearer (HS256 / RS256) |
| Containerisatie | Docker |
| Orkestratie | Kubernetes via Helm |
| Monitoring | Application Insights (optioneel) |

---

## Architectuurdiagram

![OMC ui-laag architectuur](../images/omc_architecture.png)

---

## Stateless ontwerp

Het OMC slaat geen toestand op tussen aanroepen. Elke aanroep op `/Events/Listen` wordt volledig zelfstandig afgehandeld:

1. JWT-token valideren
2. Event inspecteren en scenario bepalen
3. Gegevens ophalen uit ZGW API's
4. Notificatie versturen via NotifyNL
5. Contactmoment terugschrijven naar OpenKlant

Dit maakt horizontale schaling mogelijk zonder shared state of sessieaffiniteit. Zie [Schaalbaarheid](schaalbaarheid.md) voor details.
