# Stap 5 — Configureer de afleverstatus callback

Nadat het OMC een notificatie heeft verstuurd via NotifyNL, moet NotifyNL de afleverstatus (afgeleverd, mislukt, geopend) terug kunnen sturen naar het OMC. Het OMC schrijft deze status vervolgens als contactmoment naar OpenKlant.

---

## 5.1 Lang levend token aanmaken

Het callback-eindpunt van het OMC vereist authenticatie. Genereer een lang levend JWT-token via een van de volgende methoden.

### Methode 1 — Secrets Manager (aanbevolen)

```bash
./SecretsManager --datetime "2030-01-01T00:00:00"
```

### Methode 2 — jwt.io

Ga naar [jwt.io](https://jwt.io) en maak handmatig een token aan:

**Header** — selecteer algoritme `HS256`:

```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload** — gebruik de waarden uit je OMC-configuratie:

```json
{
  "iss": "<OMC_AUTH_JWT_ISSUER>",
  "aud": "<OMC_AUTH_JWT_AUDIENCE>",
  "sub": "<OMC_AUTH_JWT_USERID>",
  "iat": 1700000000,
  "exp": 1893456000
}
```

> - `iss`, `aud` en `sub` moeten exact overeenkomen met de waarden in je OMC-configuratie (`OMC_AUTH_JWT_ISSUER`, `OMC_AUTH_JWT_AUDIENCE`, `OMC_AUTH_JWT_USERID`).
> - `aud` mag worden weggelaten als `OMC_AUTH_JWT_AUDIENCE` niet is ingesteld.
> - `iat` is de huidige tijd in epoch-seconden. `exp` is de vervaldatum — voor een token geldig tot 2030-01-01 gebruik je `1893456000`.
> - Gebruik [epochconverter.com](https://www.epochconverter.com) om een datum naar epoch-seconden te converteren.

**Verify Signature** — vul je geheime sleutel in:

```
<OMC_AUTH_JWT_SECRET>
```

Het gegenereerde token verschijnt links op de pagina. Kopieer de volledige string.

Sla het token op — je hebt het nodig in de volgende stap.

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
