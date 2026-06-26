# Stap 1 — Configureer je Notify omgeving

Voordat je het OMC uitrolt, heb je een werkende NotifyNL-dienst nodig met een API-sleutel en een set notificatietemplates.

> Voor volledige NotifyNL documentatie, zie [admin.notifynl.nl](https://admin.notifynl.nl/using-notify/api-documentation).

---

## 1.1 Account aanmaken en API-sleutel genereren

1. Maak een account aan op [admin.notifynl.nl](https://admin.notifynl.nl)
2. Maak een dienst aan (één per gemeente of omgeving)
3. Ga naar **API Integratie** en genereer een API-sleutel

Dit geeft je twee omgevingsvariabelen voor het OMC:

| Variabele | Waarde |
|---|---|
| `NOTIFY_API_KEY` | De gegenereerde API-sleutel (formaat: `naam-UUID-UUID`) |
| `NOTIFY_API_BASEURL` | `https://api.notifynl.nl` |

> Met een testaccount kun je alleen berichten sturen aan jezelf of aan teamleden. Een productie API-sleutel is nodig om berichten te sturen aan externe ontvangers.

---

## 1.2 Notificatietemplates aanmaken

Het OMC gebruikt één template per scenario per kanaal. Ga naar **Templates** in de NotifyNL-beheerportal en maak de onderstaande templates aan. Elk template genereert een UUID — kopieer deze naar je OMC-omgevingsvariabelen.

### Vereiste templates

| Scenario | Kanaal | Omgevingsvariabele |
|---|---|---|
| Zaak aangemaakt | E-mail | `NOTIFY_TEMPLATEID_EMAIL_ZAAKCREATE` |
| Zaak aangemaakt | Sms | `NOTIFY_TEMPLATEID_SMS_ZAAKCREATE` |
| Zaak gewijzigd | E-mail | `NOTIFY_TEMPLATEID_EMAIL_ZAAKUPDATE` |
| Zaak gewijzigd | Sms | `NOTIFY_TEMPLATEID_SMS_ZAAKUPDATE` |
| Zaak afgesloten | E-mail | `NOTIFY_TEMPLATEID_EMAIL_ZAAKCLOSE` |
| Zaak afgesloten | Sms | `NOTIFY_TEMPLATEID_SMS_ZAAKCLOSE` |
| Taak toegewezen | E-mail | `NOTIFY_TEMPLATEID_EMAIL_TASKASSIGNED` |
| Taak toegewezen | Sms | `NOTIFY_TEMPLATEID_SMS_TASKASSIGNED` |
| Besluit genomen | E-mail | `NOTIFY_TEMPLATEID_EMAIL_DECISIONMADE` |
| Besluit genomen | Sms | `NOTIFY_TEMPLATEID_SMS_DECISIONMADE` |
| Bericht ontvangen | E-mail | `NOTIFY_TEMPLATEID_EMAIL_MESSAGERECEIVED` |
| Bericht ontvangen | Sms | `NOTIFY_TEMPLATEID_SMS_MESSAGERECEIVED` |

> Brieftemplates (`NOTIFY_TEMPLATEID_LETTER_*`) volgen hetzelfde patroon. Zie [Omgevingsvariabelen](../configuratie/omgevingsvariabelen.md) voor de volledige lijst.

Elke scenariopagina beschrijft de exacte `((placeholder))`-variabelen die je templates moeten bevatten. Zie [Scenario's](../werkwijzen/scenarios/overzicht.md).

---

## 1.3 Voorbeeldtemplates

De volgende voorbeeldtemplates kun je aanpassen per gemeente. Ze volgen de veiligheidsconventie van geen links in notificaties.

### Zaak aangemaakt — E-mail (`NOTIFY_TEMPLATEID_EMAIL_ZAAKCREATE`)

**Onderwerp:** Uw aanvraag ((zaak.identificatie)) is ontvangen

**Inhoud:**
```
Beste ((klant.voornaam)) ((klant.voorvoegselAchternaam)) ((klant.achternaam)),

Wij hebben uw aanvraag ontvangen met betrekking tot: ((zaak.omschrijving))

Uw zaaknummer is: ((zaak.identificatie))

U hoeft op dit moment niets te doen. Wij houden u via e-mail op de hoogte van de voortgang.

Heeft u vragen? Bel ons via 14000 of bezoek onze website.

Gemeente X stuurt voor uw veiligheid geen e-mails met linkjes.
```

### Zaak aangemaakt — Sms (`NOTIFY_TEMPLATEID_SMS_ZAAKCREATE`)

```
MijnGemeenteX: Uw aanvraag ((zaak.identificatie)) is ontvangen. Wij nemen zo snel mogelijk contact met u op. Vragen? Bel 14000.
```

### Zaak gewijzigd — E-mail (`NOTIFY_TEMPLATEID_EMAIL_ZAAKUPDATE`)

**Onderwerp:** Update over uw aanvraag ((zaak.identificatie))

**Inhoud:**
```
Beste ((klant.voornaam)) ((klant.voorvoegselAchternaam)) ((klant.achternaam)),

Er is een update over uw aanvraag: ((zaak.omschrijving))

Zaaknummer: ((zaak.identificatie))
Nieuwe status: ((status.omschrijving))

Heeft u vragen? Bel ons via 14000 of bezoek onze website.

Gemeente X stuurt voor uw veiligheid geen e-mails met linkjes.
```

### Zaak afgesloten — E-mail (`NOTIFY_TEMPLATEID_EMAIL_ZAAKCLOSE`)

**Onderwerp:** Uw aanvraag ((zaak.identificatie)) is afgerond

**Inhoud:**
```
Beste ((klant.voornaam)) ((klant.voorvoegselAchternaam)) ((klant.achternaam)),

Uw aanvraag is afgerond: ((zaak.omschrijving))

Zaaknummer: ((zaak.identificatie))
Status: ((status.omschrijving))

Heeft u vragen? Bel ons via 14000 of bezoek onze website.

Gemeente X stuurt voor uw veiligheid geen e-mails met linkjes.
```

> Sms-varianten volgen hetzelfde beknopte patroon als het voorbeeld voor Zaak aangemaakt hierboven, waarbij de relevante placeholders worden ingevuld.

Zie elke [scenariopagina](../werkwijzen/scenarios/overzicht.md) voor de volledige lijst met beschikbare `((placeholders))`.
