# JWT-tokens

Het OMC gebruikt JWT Bearer-token authenticatie voor alle inkomende API-aanroepen. Tokens worden gegenereerd buiten het OMC en meegestuurd als `Authorization: Bearer <token>` header.

---

## Tokenvereisten

Tokens moeten voldoen aan de volgende vereisten:

| Claim | Beschrijving |
|---|---|
| `iss` (issuer) | Moet overeenkomen met `OMC_AUTH_JWT_ISSUER` |
| `aud` (audience) | Moet overeenkomen met `OMC_AUTH_JWT_AUDIENCE` indien ingesteld |
| `exp` (expiry) | Token mag niet verlopen zijn |
| `sub` of `client_id` | Gebruikers-ID-claim |

Tokens worden ondertekend met HMAC SHA-256 (HS256) symmetrische encryptie met behulp van `OMC_AUTH_JWT_SECRET`.

---

## Claimsstructuur

```json
{
  "iss": "<OMC_AUTH_JWT_ISSUER>",
  "aud": "<OMC_AUTH_JWT_AUDIENCE>",
  "sub": "<OMC_AUTH_JWT_USERID>",
  "exp": <unix-tijdstempel>,
  "iat": <unix-tijdstempel>
}
```

---

## Tokens genereren

### Methode 1 — Secrets Manager (aanbevolen)

De meegeleverde [Secrets Manager](secrets-manager.md) genereert tokens die precies overeenkomen met de OMC-configuratie:

```bash
./SecretsManager
# Genereert een token dat verloopt na OMC_AUTH_JWT_EXPIRESINMIN minuten

./SecretsManager --minutes 1440
# Genereert een token dat verloopt na 1440 minuten (1 dag)

./SecretsManager --datetime "2030-01-01T00:00:00"
# Genereert een token dat verloopt op een specifieke datum/tijd
```

### Methode 2 — jwt.io

Ga naar [jwt.io](https://jwt.io), selecteer algoritme **HS256** en vul de claims handmatig in. Gebruik `OMC_AUTH_JWT_SECRET` als geheime sleutel.

### Methode 3 — Postman

De NotifyNL Postman-werkruimte bevat een pre-request script dat automatisch een token genereert op basis van de geconfigureerde omgevingsvariabelen. Neem contact op met Worth Systems voor toegang.

---

## Token gebruiken in Swagger UI

1. Ga naar `https://<jouw-omc-domein>/swagger/index.html`
2. Klik op **Authorize** (hangslotpictogram rechtsboven)
3. Voer in: `Bearer <jouw-token>`
4. Klik op **Authorize**

![JWT invoeren in Swagger UI](../images/swagger_authorize_bearer.png)

Alle eindpunten zullen nu het token gebruiken in de aanroepen.

---

## Token gebruiken in Postman

Voeg in de **Headers**-tab van je aanroep het volgende toe:

| Sleutel | Waarde |
|---|---|
| `Authorization` | `Bearer <jouw-token>` |

![Bearer-token in Postman](../images/postman_bearer_token.png)

---

## Asymmetrische encryptie (RSA)

Voor productie-uitrollingen waarbij hoge beveiliging vereist is, ondersteunt het OMC RSA asymmetrische tokenondertekening via de Secrets Manager. Zie [Secrets Manager](secrets-manager.md) voor details.

---

## Beveiligingsaanbevelingen

- Gebruik **korte levensduur** (30–60 minuten) voor interactief gebruik
- Gebruik **lange levensduur** tokens (maanden/jaren) alleen voor geautomatiseerde callbacks (bijv. NotifyNL callback in Stap 5)
- Deel tokens nooit in logs, issue-trackers of documentatie
- Roteer het JWT-geheim periodiek en werk tokens dienovereenkomstig bij
