# PostGuard

PostGuard biedt versleutelde PDF-aflevering via de Yivi-wallet. Het is een alternatief afleverkanaal voor burgers die geen e-mailadres of telefoonnummer hebben geregistreerd, of voor documenten die extra beveiliging vereisen.

---

## Hoe het werkt

1. Het OMC bepaalt dat PostGuard het juiste afleverkanaal is (geen e-mail/sms beschikbaar, of document vereist versleuteling)
2. Het OMC laadt het te versturen document up naar de PostGuard API
3. PostGuard versleutelt het document en koppelt het aan de Yivi-identiteit van de burger
4. NotifyNL verstuurt een notificatie naar de burger met instructies om het document op te halen via de Yivi-app
5. De burger opent de Yivi-app en ontvangt het versleutelde document

---

## Vereisten

- De burger moet een actieve Yivi-wallet hebben
- PostGuard moet zijn geconfigureerd in de OMC-omgeving
- Een NotifyNL-template moet zijn aangemaakt voor de PostGuard-aflevering

---

## Configuratie

Stel alle PostGuard-variabelen in op `-` als de integratie niet wordt gebruikt.

| Variabele | Beschrijving |
|---|---|
| `POSTGUARD_BASE_URL` | Basis-URL van de PostGuard-dienst |
| `POSTGUARD_ACCESS_TOKEN` | Toegangstoken voor de PostGuard API |
| `POSTGUARD_SENDER_EMAIL` | E-mailadres van de afzender voor PostGuard-berichten |
| `POSTGUARD_SENDER_NAME` | Naam van de afzender voor PostGuard-berichten |

---

## Template-richtlijnen

PostGuard-templates in NotifyNL moeten de Yivi-ophaalinstructies bevatten. Houd de templateinhoud beknopt en verwijs de burger naar de Yivi-app:

```
Geachte ((klant.voornaam)) ((klant.achternaam)),

Er is een beveiligd document voor u beschikbaar: ((document.onderwerp))

Open de Yivi-app op uw telefoon om het document te ontvangen.

Heeft u nog geen Yivi-app? Download deze via yivi.app

((afzender.naam))
```

---

## Beschikbaar sinds

PostGuard-integratie is beschikbaar vanaf **OMC versie 2.0.0**.
