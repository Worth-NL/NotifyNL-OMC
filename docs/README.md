# NotifyNL Output Management Component (OMC)

[![Build](https://img.shields.io/github/actions/workflow/status/worth-nl/notifynl-omc/merge.yaml?style=for-the-badge\&logo=github)](https://github.com/Worth-NL/NotifyNL-OMC)
[![Version](https://img.shields.io/github/v/tag/worth-nl/notifynl-omc?style=for-the-badge\&logo=github\&label=version)](https://github.com/Worth-NL/NotifyNL-OMC/releases)
[![Docker](https://img.shields.io/docker/v/worthnl/notifynl-omc?sort=date\&arch=amd64\&style=for-the-badge\&logo=docker)](https://hub.docker.com/r/worthnl/notifynl-omc)

---

## What is OMC?

The **Output Management Component (OMC)** is an open-source, stateless middleware service that bridges Dutch municipal case management systems (ZGW APIs) with [NotifyNL](https://admin.notifynl.nl) — the Dutch government notification platform for sending emails, SMS messages, and letters to citizens and organisations.

When deployed in a ZGW environment, OMC listens for events on the **NotificatiesAPI** and automatically handles the full notification workflow: fetching case and citizen data, selecting the right template, sending the notification via NotifyNL, and writing a delivery receipt back to **OpenKlant** as a contact moment.

> **NotifyNL** is the delivery engine — it handles the actual sending of emails, SMS, and letters. OMC is the integration layer that connects your ZGW environment to NotifyNL without requiring NotifyNL to have direct access to your citizen or case registrations.
>
> For NotifyNL documentation, see [admin.notifynl.nl](https://admin.notifynl.nl/using-notify/api-documentation).

---

## What does OMC do?

1. Receives event notifications from **Open Notificaties** (e.g. a case was created, a task was assigned)
2. Fetches the relevant case, citizen, and contact details from the ZGW APIs
3. Determines which notification scenario applies and which channel to use (email, SMS, or letter)
4. Sends the notification via NotifyNL using the configured template
5. Receives the delivery status callback from NotifyNL
6. Writes a **contact moment** back to OpenKlant so delivery history is visible in the citizen portal

OMC is **stateless** — you can run as many instances as needed without coordination between them.

---

## Supported ZGW services

OMC integrates with the following ZGW / Open Services:

| Service | Repository | Purpose |
|---|---|---|
| **Open Notificaties** | [open-zaak/open-notificaties](https://github.com/open-zaak/open-notificaties) | Event subscription and delivery |
| **Open Zaak** | [open-zaak/open-zaak](https://github.com/open-zaak/open-zaak) | Cases, statuses, decisions |
| **Open Klant** | [maykinmedia/open-klant](https://github.com/maykinmedia/open-klant) | Citizen contact details and preferences |
| **Besluiten** | Part of Open Zaak | Decisions linked to cases |
| **Objecten** | [maykinmedia/objects-api](https://github.com/maykinmedia/objects-api) | Tasks, messages, custom objects |
| **ObjectTypen** | [maykinmedia/objecttypes-api](https://github.com/maykinmedia/objecttypes-api) | Object type definitions |
| **Klantinteracties** | [vng-realisatie/klantinteracties](https://vng-realisatie.github.io/klantinteracties/) | Contact moments (v2, used in workflow v2) |

---

## Supported notification scenarios

| Scenario | Trigger |
|---|---|
| **Case Created** | A new case (`zaak`) is opened for a citizen or organisation |
| **Case Updated** | The status of an existing case changes |
| **Case Closed** | A case reaches its final status |
| **Task Assigned** | A task (`taak`) is assigned to a citizen or organisation |
| **Decision Made** | A decision (`besluit`) is made that affects a citizen |
| **Message Received** | A message is placed in the citizen's message box |

---

## Open source

Both OMC and NotifyNL are fully open source.

- **OMC source code:** [github.com/Worth-NL/NotifyNL-OMC](https://github.com/Worth-NL/NotifyNL-OMC)
- **NotifyNL platform:** [notificatie.nl/open-source](https://www.notificatie.nl/open-source)
- **Developed and maintained by:** [Worth Systems](https://worth.systems)
- **License:** EUPL v1.2
