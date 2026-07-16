# Eindpunten

Alle eindpunten vereisen een geldig JWT Bearer-token tenzij anders vermeld. Voeg de token toe als `Authorization: Bearer <token>` header.

---

## Events

### POST /Events/Listen

Het primaire eindpunt dat events ontvangt van Open Notificaties.

**Authenticatie:** JWT Bearer  
**Verzoekinhoud:** CloudEvents JSON-payload (geleverd door Open Notificaties)

**Responscodes:**

| Code | Betekenis |
|---|---|
| `200 OK` | Event verwerkt (inclusief overgeslagen events) |
| `202 Accepted` | Event ontvangen, wordt asynchroon verwerkt |
| `206 Partial Content` | Testping van Open Notificaties — verwacht gedrag |
| `400 Bad Request` | Ongeldig event-formaat |
| `401 Unauthorized` | Ontbrekend of ongeldig JWT-token |

---

### GET /Events/Version

Geeft de huidige OMC-versie terug.

**Authenticatie:** Geen  
**Respons:**

```json
{
  "version": "2.0.0"
}
```

---

## Notify

### POST /Notify/Confirm

Ontvangt afleverstatusberichten van NotifyNL.

**Authenticatie:** JWT Bearer (lang levend token — zie [Stap 5](../aan-de-slag/stap-5-callback-configuratie.md))  
**Verzoekinhoud:** NotifyNL callback-payload

**Responscodes:**

| Code | Betekenis |
|---|---|
| `200 OK` | Afleverstatus verwerkt, contactmoment geschreven |
| `400 Bad Request` | Ongeldige callback-payload |
| `401 Unauthorized` | Ontbrekend of ongeldig JWT-token |

---

## Test-eindpunten

Test-eindpunten zijn beschikbaar in zowel `Development` als `Production` omgevingen.

### POST /Test/OMC/Configuration

Valideert alle geconfigureerde OMC-omgevingsvariabelen.

**Respons:** Lijst van configuratiefouten of lege lijst bij succes.

---

### POST /Test/ZGW/Endpoints

Test de bereikbaarheid van alle geconfigureerde ZGW-service-eindpunten.

**Respons:** Status per eindpunt.

---

### GET /Test/Notify/HealthCheck

Controleert of de NotifyNL API bereikbaar is met de geconfigureerde API-sleutel.

**Respons:**

```json
{
  "status": "healthy"
}
```

---

### POST /Test/Notify/SendEmail

Verstuurt een teste-mail via NotifyNL naar een opgegeven adres.

**Verzoekinhoud:**

```json
{
  "emailAddress": "test@voorbeeld.nl",
  "templateId": "uuid-van-template"
}
```

---

### POST /Test/Notify/SendSms

Verstuurt een test-sms via NotifyNL naar een opgegeven telefoonnummer.

**Verzoekinhoud:**

```json
{
  "mobileNumber": "+31612345678",
  "templateId": "uuid-van-template"
}
```

---

## Foutresponsformaat

Alle foutresponsen volgen dit formaat:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Omschrijving van de fout",
  "traceId": "00-abc123-def456-00"
}
```

---

## Swagger UI

Het OMC biedt een interactieve Swagger UI:

```
https://<jouw-omc-domein>/swagger/index.html
```

Klik op **Authorize** en voer `Bearer <token>` in om de eindpunten interactief te verkennen.

![Swagger UI](../images/swagger_ui_example.png)
