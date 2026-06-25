# Step 1 — Configure your Notify environment

Before deploying OMC you need a working NotifyNL service with an API key and a set of notification templates.

> For full NotifyNL documentation see [admin.notifynl.nl](https://admin.notifynl.nl/using-notify/api-documentation).

---

## 1.1 Create an account and generate an API key

1. Create an account at [admin.notifynl.nl](https://admin.notifynl.nl)
2. Create a service (one per municipality or environment)
3. Go to **API Integration** and generate an API key

This gives you two environment variables for OMC:

| Variable | Value |
|---|---|
| `NOTIFY_API_KEY` | The generated API key (format: `name-UUID-UUID`) |
| `NOTIFY_API_BASEURL` | `https://api.notifynl.nl` |

> With a test account you can only send to yourself or team members. A production API key is needed to send to external recipients.

---

## 1.2 Create notification templates

OMC uses one template per scenario per channel. Go to **Templates** in the NotifyNL admin portal and create the templates below. Each template generates a UUID — copy these into your OMC environment variables.

### Required templates

| Scenario | Channel | Environment variable |
|---|---|---|
| Case Created | Email | `NOTIFY_TEMPLATEID_EMAIL_ZAAKCREATE` |
| Case Created | SMS | `NOTIFY_TEMPLATEID_SMS_ZAAKCREATE` |
| Case Updated | Email | `NOTIFY_TEMPLATEID_EMAIL_ZAAKUPDATE` |
| Case Updated | SMS | `NOTIFY_TEMPLATEID_SMS_ZAAKUPDATE` |
| Case Closed | Email | `NOTIFY_TEMPLATEID_EMAIL_ZAAKCLOSE` |
| Case Closed | SMS | `NOTIFY_TEMPLATEID_SMS_ZAAKCLOSE` |
| Task Assigned | Email | `NOTIFY_TEMPLATEID_EMAIL_TASKASSIGNED` |
| Task Assigned | SMS | `NOTIFY_TEMPLATEID_SMS_TASKASSIGNED` |
| Decision Made | Email | `NOTIFY_TEMPLATEID_EMAIL_DECISIONMADE` |
| Decision Made | SMS | `NOTIFY_TEMPLATEID_SMS_DECISIONMADE` |
| Message Received | Email | `NOTIFY_TEMPLATEID_EMAIL_MESSAGERECEIVED` |
| Message Received | SMS | `NOTIFY_TEMPLATEID_SMS_MESSAGERECEIVED` |

> Letter templates (`NOTIFY_TEMPLATEID_LETTER_*`) follow the same pattern. See [Environment variables](../configuration/environment-variables.md) for the full list.

Each scenario page documents the exact `((placeholder))` variables your templates must include. See [Scenarios](../workflows/scenarios/overview.md).

---

## 1.3 Example templates

The following are example templates in Dutch that you can adapt per municipality. They use the safety-conscious messaging convention of not including links in notifications.

### Case Created — Email (`NOTIFY_TEMPLATEID_EMAIL_ZAAKCREATE`)

**Subject:** Uw aanvraag ((zaak.identificatie)) is ontvangen

**Body:**
```
Beste ((klant.voornaam)) ((klant.voorvoegselAchternaam)) ((klant.achternaam)),

Wij hebben uw aanvraag ontvangen met betrekking tot: ((zaak.omschrijving))

Uw zaaknummer is: ((zaak.identificatie))

U hoeft op dit moment niets te doen. Wij houden u via e-mail op de hoogte van de voortgang.

Heeft u vragen? Bel ons via 14000 of bezoek onze website.

Gemeente X stuurt voor uw veiligheid geen e-mails met linkjes.
```

### Case Created — SMS (`NOTIFY_TEMPLATEID_SMS_ZAAKCREATE`)

```
MijnGemeenteX: Uw aanvraag ((zaak.identificatie)) is ontvangen. Wij nemen zo snel mogelijk contact met u op. Vragen? Bel 14000.
```

### Case Updated — Email (`NOTIFY_TEMPLATEID_EMAIL_ZAAKUPDATE`)

**Subject:** Update over uw aanvraag ((zaak.identificatie))

**Body:**
```
Beste ((klant.voornaam)) ((klant.voorvoegselAchternaam)) ((klant.achternaam)),

Er is een update over uw aanvraag: ((zaak.omschrijving))

Zaaknummer: ((zaak.identificatie))
Nieuwe status: ((status.omschrijving))

Heeft u vragen? Bel ons via 14000 of bezoek onze website.

Gemeente X stuurt voor uw veiligheid geen e-mails met linkjes.
```

### Case Closed — Email (`NOTIFY_TEMPLATEID_EMAIL_ZAAKCLOSE`)

**Subject:** Uw aanvraag ((zaak.identificatie)) is afgerond

**Body:**
```
Beste ((klant.voornaam)) ((klant.voorvoegselAchternaam)) ((klant.achternaam)),

Uw aanvraag is afgerond: ((zaak.omschrijving))

Zaaknummer: ((zaak.identificatie))
Status: ((status.omschrijving))

Heeft u vragen? Bel ons via 14000 of bezoek onze website.

Gemeente X stuurt voor uw veiligheid geen e-mails met linkjes.
```

> SMS variants follow the same concise pattern as the Case Created SMS example above, substituting the relevant placeholders.

See each [scenario page](../workflows/scenarios/overview.md) for the complete list of available `((placeholders))`.
