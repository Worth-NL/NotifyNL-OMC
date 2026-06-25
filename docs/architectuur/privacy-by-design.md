# Privacy by design

Het OMC is ontworpen met privacy als kernprincipe, in lijn met de AVG en de principes van dataminimalisatie.

---

## Dataminimalisatie

Het OMC haalt alleen die gegevens op die strikt noodzakelijk zijn voor het versturen van de notificatie:

- Alleen het contactadres (e-mail of telefoonnummer) van de burger
- Alleen de zaakgegevens die als templateplaceholder worden gebruikt
- Geen kopieën of opslag van persoonsgegevens

Het OMC slaat **geen persoonsgegevens op**. Alle gegevens worden enkel in het geheugen verwerkt tijdens de verwerking van een enkel event en daarna weggegooid.

---

## Retentietabel

| Systeem | Retentie van persoonsgegevens |
|---|---|
| **OMC** | Geen — stateless, geen opslag |
| **NotifyNL** | 5 dagen (afleverrecords) |
| **OMC logs** | Geen PII — alleen technische metadata (event-ID, status) |
| **Application Insights** | Geen PII — alleen duur, statuscodes, foutmeldingen |

---

## BRP-gegevens

Als de BRP/Haal Centraal-integratie is ingeschakeld, haalt het OMC aanvullende persoonsgegevens op om templates te verrijken (bijv. voornaam, achternaam). Deze gegevens worden:

- Uitsluitend in het geheugen verwerkt
- Direct doorgegeven aan de NotifyNL API-aanroep
- Nooit opgeslagen, gelogd of gecached

De BRP-verbinding is beveiligd via Keycloak OAuth2-tokenuitwisseling en mutuele TLS (mTLS). Zie [BRP / Haal Centraal](../integraties/brp-haalcentraal.md).

---

## Contactmomenten

Na elke verstuurd notificatie schrijft het OMC een **contactmoment** terug naar de Contactmomenten/Klantinteracties API. Dit contactmoment bevat:

- De datum en tijd van de notificatie
- Het kanaal (e-mail, sms, brief)
- De status (verstuurd)
- Een referentie naar de zaak

Het BSN of KVK-nummer wordt **niet** opgeslagen in het contactmoment zelf — alleen de relatie naar de klantregistratie wordt vastgelegd.

---

## Informatiebeperkte notificaties

Notificaties bevatten bewust minimale informatie om het risico van datalekken via e-mail of sms te beperken. De aanbevolen templateconventie is:

- Verwijs naar het zaaknummer, niet naar gevoelige zaakinhoud
- Geef geen links mee (phishingpreventie)
- Verwijs burgers naar het officiële portaal of telefoonnummer voor verdere informatie
