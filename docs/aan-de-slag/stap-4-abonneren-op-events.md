# Stap 4 — Abonneren op events

Het OMC heeft een actief abonnement op de **NotificatiesAPI** nodig zodat het events ontvangt wanneer zaken, taken, besluiten of berichten worden aangemaakt of gewijzigd.

---

## 4.1 Waarop abonneren

Configureer abonnementen voor de volgende kanalen (`kanaal`) in Open Notificaties:

| Kanaal (`kanaal`) | Relevante scenario's |
|---|---|
| `zaken` | Zaak aangemaakt, Zaak gewijzigd, Zaak afgesloten |
| `objecten` | Taak toegewezen, Bericht ontvangen |
| `besluiten` | Besluit genomen |

Abonneer voor elk kanaal op de `create`-actie (`actie`).

> **Let op:** Vanwege een race condition in de ZGW-stack luistert het OMC naar `status`-resource-events op het `zaken`-kanaal in plaats van de `zaak`-resource direct. Dit is by design — abonneer niet op `resource: zaak` voor zaakscenario's.

> **MijnOverheid-forwarding:** Als je ook [MijnOverheid](../integraties/mijnoverheid.md)-forwarding gebruikt, is een apart abonnement nodig met een callback naar `/Events/MijnZaken`, inclusief `resource: zaak` met de acties `read` en `destroy`. Dit is een uitzondering op de regel hierboven — die geldt alleen voor het `/Events/Listen`-abonnement.

---

## 4.2 Abonnements-eindpunt

Wijs de callback-URL van het abonnement naar het luistereindpunt van het OMC:

```
POST https://<jouw-omc-domein>/Events/Listen
```

Het OMC moet extern bereikbaar zijn vanuit Open Notificaties. Als het OMC achter een reverse proxy zit, zorg dan dat de proxy de juiste headers doorstuurt.

![Abonnement instellen in Open Notificaties](../images/step%204%20image%201.png)

---

## 4.3 Het abonnement verifiëren

Na het aanmaken van het abonnement stuurt Open Notificaties een testping naar het OMC-eindpunt om te controleren of het bereikbaar is. Het OMC reageert op testpings met `206 Partial Content` — dit is verwacht gedrag en duidt niet op een fout.

Je kunt dit bevestigen in de Open Notificaties-beheerinterface waar de abonnementsstatus als actief moet worden weergegeven.

Maak een testzaak aan in je ZGW-omgeving en verifieer dat er een notificatie-event verschijnt in Open Notificaties gericht aan het OMC.

![Abonnement actief in Open Notificaties](../images/step%204%20image%202.png)
