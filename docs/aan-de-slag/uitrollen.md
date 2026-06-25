# OMC uitrollen

Het OMC vereist dat de volgende ZGW API's actief en bereikbaar zijn:

- Open Notificaties
- Open Zaak
- Open Klant (v1 of v2 afhankelijk van de [werkwijze versie](../werkwijzen/versies.md))
- Objecten + ObjectTypen
- Contactmomenten / Klantinteracties

Je kunt het OMC uitrollen door de broncode te bouwen of door gebruik te maken van het Helm chart dat beschikbaar is voor geautoriseerde gebruikers in de Worth-NL Helm chart repository.

---

## Uitrollen in 5 stappen

Volg deze stappen op volgorde. Elke stap wordt in detail beschreven op een aparte pagina.

| Stap | Wat je doet |
|---|---|
| [Stap 1](stap-1-notify-omgeving.md) | Maak een NotifyNL-account aan, genereer een API-sleutel en maak templates voor elk scenario |
| [Stap 2](stap-2-zgw-api-sleutels.md) | Genereer API-sleutels en JWT-referenties zodat het OMC toegang heeft tot je ZGW-diensten |
| [Stap 3](stap-3-uitrollen-en-testen.md) | Rol het OMC uit met het Helm chart, stel alle omgevingsvariabelen in en voer health checks uit |
| [Stap 4](stap-4-abonneren-op-events.md) | Abonneer het OMC op de relevante events in de NotificatiesAPI |
| [Stap 5](stap-5-callback-configuratie.md) | Configureer NotifyNL om de afleverstatus terug te sturen naar het OMC |

---

## Bouwen vanuit broncode

```bash
git clone git@github.com:Worth-NL/NotifyNL-OMC.git
cd NotifyNL-OMC
docker build -f OMC/Infrastructure/WebApi/EventsHandler/Dockerfile --force-rm -t omc .
```

> De `--force-rm` vlag en het opgeven van het Dockerfile-pad vanuit de repository-root zijn beide vereist om een Docker cache key-fout te vermijden.

Na het bouwen van de image, ga verder met [Stap 3](stap-3-uitrollen-en-testen.md) om de container te configureren en te starten.
