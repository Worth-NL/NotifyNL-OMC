# Stap 5 — Configureer de afleverstatus callback

Nadat het OMC een notificatie heeft verstuurd via NotifyNL, moet NotifyNL de afleverstatus (afgeleverd, mislukt, geopend) terug kunnen sturen naar het OMC. Het OMC schrijft deze status vervolgens als contactmoment naar OpenKlant.

---

## 5.1 Lang levend token aanmaken

Het callback-eindpunt van het OMC vereist authenticatie. Genereer een lang levend JWT-token met de [Secrets Manager](../authenticatie/secrets-manager.md) in datetime-modus:

```bash
./SecretsManager --datetime "2030-01-01T00:00:00"
```

Sla het gegenereerde token op — je hebt het nodig in de volgende stap.

---

## 5.2 Callback configureren in NotifyNL

1. Ga naar [admin.notifynl.nl](https://admin.notifynl.nl) → **Instellingen** → **API Integratie**
2. Vul het callback-eindpunt in:

```
https://<jouw-omc-domein>/Notify/Confirm
```

3. Voeg het Bearer-token toe dat je in stap 5.1 hebt gegenereerd

NotifyNL stuurt nu na elke notificatie een POST-verzoek naar dit eindpunt met de afleverstatusdetails.

---

## 5.3 Verificatie

Stuur een testnotificatie vanuit NotifyNL en controleer:

1. Dat een contactmoment zichtbaar is in de OpenKlant-beheerinterface voor de betreffende burger
2. Dat de OMC-logs een geslaagde `POST /Notify/Confirm`-aanroep tonen (geen 401 of 403)

> Als het OMC draait op meerdere instanties, zijn alle instanties stateless en kunnen ze de callback afhandelen — je hoeft de callback slechts naar één eindpunt te sturen.
