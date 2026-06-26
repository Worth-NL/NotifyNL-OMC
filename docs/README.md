# Output Management & NotifyNL

[![Build](https://img.shields.io/github/actions/workflow/status/worth-nl/notifynl-omc/merge.yaml?style=for-the-badge\&logo=github)](https://github.com/Worth-NL/NotifyNL-OMC)
[![Versie](https://img.shields.io/github/v/tag/worth-nl/notifynl-omc?style=for-the-badge\&logo=github\&label=versie)](https://github.com/Worth-NL/NotifyNL-OMC/releases)
[![Docker](https://img.shields.io/docker/v/worthnl/notifynl-omc?sort=date\&arch=amd64\&style=for-the-badge\&logo=docker)](https://hub.docker.com/r/worthnl/notifynl-omc)

---

## Wat is NotifyNL?

NotifyNL stelt centrale en lokale overheden in staat om berichten te versturen aan burgers en bedrijven via verschillende kanalen, waaronder e-mail en sms.

Organisaties beheren uitgaande berichten vaak verspreid over meerdere afdelingen en systemen, wat verschillende uitdagingen met zich meebrengt:

- Beveiligingstoezicht is moeilijk in gedecentraliseerde systemen
- Berichtconsistentie lijdt eronder, waardoor het voor ontvangers onduidelijk is of communicatie van de overheid afkomstig is
- Bounceafhandeling kan niet centraal worden beheerd
- Naleving van de WMEBV-wetgeving is ingewikkeld bij een gedecentraliseerde opzet

NotifyNL lost dit op door berichtbeheer te centraliseren, uniforme burgercommunicatie te bieden, transactionele berichten vanuit processen te vereenvoudigen en vragen van burgers over de status van diensten te verminderen.

> **NotifyNL** is het afleverplatform — het verzorgt de daadwerkelijke verzending van e-mails, sms-berichten en brieven. Het Output Management Component (OMC) is de integratielaag die jouw ZGW-omgeving verbindt met NotifyNL, zonder dat NotifyNL directe toegang nodig heeft tot je burger- of zaakregistraties.
>
> Voor NotifyNL documentatie, zie [admin.notifynl.nl](https://admin.notifynl.nl/using-notify/api-documentation).

---

## Wat doet het OMC?

1. Ontvangt eventberichten van **Open Notificaties** (bijv. een zaak is aangemaakt, een taak is toegewezen)
2. Haalt de relevante zaak-, burger- en contactgegevens op uit de ZGW API's
3. Bepaalt welk notificatiescenario van toepassing is en welk kanaal gebruikt wordt (e-mail, sms of brief)
4. Verstuurt de notificatie via NotifyNL met het geconfigureerde template
5. Ontvangt de afleverstatusmelding van NotifyNL
6. Schrijft een **contactmoment** terug naar OpenKlant zodat de aflevergeschiedenis zichtbaar is in het burgerportaal

Het OMC is **stateless** — je kunt zoveel instanties draaien als nodig zonder onderlinge afstemming.

---

## Ondersteunde ZGW diensten

Het OMC integreert met de volgende ZGW / Open Services:

| Dienst | Repository | Doel |
|---|---|---|
| **Open Notificaties** | [open-zaak/open-notificaties](https://github.com/open-zaak/open-notificaties) | Eventabonnementen en -aflevering |
| **Open Zaak** | [open-zaak/open-zaak](https://github.com/open-zaak/open-zaak) | Zaken, statussen, besluiten |
| **Open Klant** | [maykinmedia/open-klant](https://github.com/maykinmedia/open-klant) | Contactgegevens en voorkeuren van burgers |
| **Besluiten** | Onderdeel van Open Zaak | Besluiten gekoppeld aan zaken |
| **Objecten** | [maykinmedia/objects-api](https://github.com/maykinmedia/objects-api) | Taken, berichten, aangepaste objecten |
| **ObjectTypen** | [maykinmedia/objecttypes-api](https://github.com/maykinmedia/objecttypes-api) | Objecttype-definities |
| **Klantinteracties** | [vng-realisatie/klantinteracties](https://vng-realisatie.github.io/klantinteracties/) | Contactmomenten (v2, gebruikt in werkwijze v2) |

---

## Ondersteunde notificatiescenario's

| Scenario | Trigger |
|---|---|
| **Zaak aangemaakt** | Een nieuwe zaak wordt geopend voor een burger of organisatie |
| **Zaak gewijzigd** | De status van een bestaande zaak wijzigt |
| **Zaak afgesloten** | Een zaak bereikt de eindstatus |
| **Taak toegewezen** | Een taak wordt toegewezen aan een burger of organisatie |
| **Besluit genomen** | Een besluit dat een burger raakt wordt genomen |
| **Bericht ontvangen** | Een bericht wordt geplaatst in de berichtenbox van de burger |

---

## Open source

Zowel het OMC als NotifyNL zijn volledig open source.

- **OMC broncode:** [github.com/Worth-NL/NotifyNL-OMC](https://github.com/Worth-NL/NotifyNL-OMC)
- **NotifyNL platform:** [notificatie.nl/open-source](https://www.notificatie.nl/open-source)
- **Ontwikkeld en onderhouden door:** [Worth Systems](https://worth.systems)
- **Licentie:** EUPL v1.2
