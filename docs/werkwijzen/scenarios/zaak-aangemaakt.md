# Zaak aangemaakt

Dit scenario wordt geactiveerd wanneer een nieuwe zaak voor een burger of organisatie wordt geopend.

---

## Triggercondities

Het OMC activeert dit scenario wanneer een event binnenkomt met de volgende kenmerken:

| Veld | Vereiste waarde |
|---|---|
| `kanaal` | `zaken` |
| `resource` | `status` |
| `actie` | `create` |

---

## Sequence diagram

Het onderstaande diagram toont de volledige call flow voor alle drie zaakscenario's. Het **Zaak aangemaakt** pad wordt gevolgd wanneer `volgnummer == 1`.

```mermaid
sequenceDiagram
    participant NA as NotificatieAPI
    participant OMC as OMC
    participant OZ as OpenZaak
    participant OK as OpenKlant
    participant NL as NotifyNL

    NA->>OMC: POST /api/v2/events<br/>{kanaal=zaken, resource=status, actie=create,<br/>resourceUrl, mainObjectUri}

    rect rgb(240, 248, 255)
        Note over OMC,OZ: Scenario resolution
        OMC->>OZ: GET /statussen/{uuid}
        OZ-->>OMC: {typeUri, datumStatusGezet}
        OMC->>OZ: GET /statustypen/{uuid}
        OZ-->>OMC: {volgnummer, isEindstatus, informeren}
        Note over OMC: ✓ informeren == true (anders: abort)
    end

    alt volgnummer == 1 → Zaak aangemaakt
        rect rgb(240, 255, 248)
            Note over OMC,OZ: Prepare data
            OMC->>OZ: GET /zaken/{uuid}
            OZ-->>OMC: {identificatie, omschrijving, zaaktype, ...}
            Note over OMC: ✓ zaaktype.identificatie ∈ ZGW_WHITELIST_ZAAKCREATE_IDS
            OMC->>OZ: GET /rollen/?zaak={uri}
            OZ-->>OMC: {betrokkeneIdentificatie: {inpBsn}, involvedPartyUri?}
        end

    else volgnummer > 1 AND isEindstatus == false → Zaak gewijzigd
        rect rgb(240, 255, 248)
            Note over OMC,OZ: Prepare data
            OMC->>OZ: GET /zaken/{uuid}
            OZ-->>OMC: {identificatie, omschrijving, zaaktype, ...}
            Note over OMC: ✓ zaaktype.identificatie ∈ ZGW_WHITELIST_ZAAKUPDATE_IDS
            OMC->>OZ: GET /rollen/?zaak={uri}
            OZ-->>OMC: {betrokkeneIdentificatie: {inpBsn}, involvedPartyUri?}
        end

    else isEindstatus == true → Zaak afgesloten
        rect rgb(240, 255, 248)
            Note over OMC,OZ: Prepare data
            OMC->>OZ: GET /zaken/{uuid}
            OZ-->>OMC: {identificatie, omschrijving, zaaktype, resultaat?, ...}
            Note over OMC: ✓ zaaktype.identificatie ∈ ZGW_WHITELIST_ZAAKCLOSE_IDS
            OMC->>OZ: GET /resultaattypen/{uuid} (alleen als zaak resultaat heeft)
            OZ-->>OMC: {omschrijving, toelichting}
            OMC->>OZ: GET /rollen/?zaak={uri}
            OZ-->>OMC: {betrokkeneIdentificatie: {inpBsn}, involvedPartyUri?}
        end
    end

    alt geen involvedPartyUri — gebruik BSN
        OMC->>OK: GET /klanten/?inpBsn={bsn}
    else involvedPartyUri aanwezig
        OMC->>OK: GET /partijen/{uuid}
    end
    OK-->>OMC: {voornaam, achternaam, emailadres, telefoonnummer, kanaal}

    rect rgb(253, 242, 248)
        Note over OMC,NL: Notify
        Note over OMC: Selecteer kanaal op basis van distributionChannel
        OMC->>NL: POST /v2/notifications/email (of /sms / /letter)<br/>{template_id, personalisation, reference: {caseId, partyId}}
        NL-->>OMC: {id, status: "created"}
    end

    rect rgb(255, 249, 231)
        Note over OMC,OK: Contactmoment
        OMC->>OK: POST /contactmomenten {kanaal, tekst, zaak, klant}
        OK-->>OMC: {uuid, url}
        OMC->>OK: POST /zaakcontactmomenten {zaak, contactmoment}
        OK-->>OMC: 201 Created
        OMC->>OK: POST /klantcontactmomenten {klant, contactmoment}
        OK-->>OMC: 201 Created
    end

    OMC-->>NA: HTTP 200 OK
```

---

## Verwerkingslogica

Nadat het event is ontvangen, haalt het OMC de statusgegevens op en controleert de volgende condities **in volgorde**. Als één conditie niet klopt, wordt de verwerking afgebroken.

### Stap 1 — Statustype ophalen

Het OMC haalt het statustype op via de `typeUri` van de ontvangen status.

### Stap 2 — Volgnummer check

```
statustype.volgnummer == 1
```

Alleen de **eerste** status op een zaak activeert dit scenario. Als het volgnummer hoger is, wordt het event overgeslagen (en mogelijk opgepakt door [Zaak gewijzigd](zaak-gewijzigd.md) of [Zaak afgesloten](zaak-afgesloten.md)).

### Stap 3 — Informeren check

```
statustype.isEindstatus == false  (impliciet: volgnummer 1 is nooit eindstatus)
statustype.informeren == true
```

Het statustype in Open Zaak moet het veld `informeren` op `true` hebben staan. Als dit `false` is, verstuurt het OMC geen notificatie voor dit statustype.

### Stap 4 — Whitelist check

```
zaaktype.identificatie ∈ ZGW_WHITELIST_ZAAKCREATE_IDS
```

Het zaaktype van de zaak moet voorkomen in de geconfigureerde whitelist. Gebruik `*` om alle zaaktypen toe te staan.

### Stap 5 — Gegevens ophalen

Het OMC haalt aanvullende gegevens op:

1. `GET /zaken/{uuid}` — zaakgegevens
2. `GET /zaaktypen/{uuid}` — zaaktype (voor whitelist check)
3. Contactgegevens van de burger via OpenKlant (BSN of KVK)

### Stap 6 — Notificatie versturen

Het OMC verstuurt de notificatie via NotifyNL met het geconfigureerde template en schrijft daarna een contactmoment terug naar OpenKlant.

---

## Vereisten samengevat

| Conditie | Waarde |
|---|---|
| `statustype.volgnummer` | `1` |
| `statustype.informeren` | `true` |
| `zaaktype.identificatie` | Moet op whitelist staan |
| Burger heeft contactgegevens | E-mail of telefoonnummer in OpenKlant |

---

## Beschikbare templateplaceholders

| Placeholder | Bron | Beschrijving |
|---|---|---|
| `((zaak.identificatie))` | zaak | Zaaknummer |
| `((zaak.omschrijving))` | zaak | Omschrijving van de zaak |
| `((zaak.startdatum))` | zaak | Startdatum van de zaak |
| `((status.omschrijving))` | statustype | Omschrijving van de huidige status |
| `((klant.voornaam))` | klant | Voornaam van de klant |
| `((klant.voorvoegselAchternaam))` | klant | Tussenvoegsel van de klant |
| `((klant.achternaam))` | klant | Achternaam van de klant |
| `((klant.emailadres))` | klant | E-mailadres van de klant |

---

## Relevante omgevingsvariabelen

| Variabele | Beschrijving |
|---|---|
| `ZGW_WHITELIST_ZAAKCREATE_IDS` | Kommagescheiden lijst van toegestane zaaktype-identificaties, of `*` |
| `NOTIFY_TEMPLATEID_EMAIL_ZAAKCREATE` | NotifyNL template-UUID voor e-mail |
| `NOTIFY_TEMPLATEID_SMS_ZAAKCREATE` | NotifyNL template-UUID voor sms |
| `NOTIFY_TEMPLATEID_LETTER_ZAAKCREATE` | NotifyNL template-UUID voor brief |
