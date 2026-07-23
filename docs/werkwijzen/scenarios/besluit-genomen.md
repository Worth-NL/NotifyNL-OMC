# Besluit genomen

Dit scenario wordt geactiveerd wanneer een besluit wordt genomen dat een burger raakt.

---

## Triggercondities

Het OMC activeert dit scenario wanneer een event binnenkomt met de volgende kenmerken:

| Veld | Vereiste waarde |
|---|---|
| `kanaal` | `besluiten` |
| `resource` | `besluit` |
| `actie` | `create` |

### Voorbeeld payload

```json
{
  "actie": "create",
  "kanaal": "besluiten",
  "resource": "besluit",
  "kenmerken": {
    "besluittype": "https://openzaak.mijnstad.nl/catalogi/api/v1/besluittypen/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "verantwoordelijkeOrganisatie": "123456789"
  },
  "hoofdObject": "https://openzaak.mijnstad.nl/besluiten/api/v1/besluiten/bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
  "resourceUrl": "https://openzaak.mijnstad.nl/besluiten/api/v1/besluiten/bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
  "aanmaakdatum": "2026-01-15T10:30:00Z"
}
```

---

## Verwerkingslogica

Dit is het meest uitgebreide scenario — het OMC voert meerdere validaties uit en haalt documenten op voordat een notificatie wordt verstuurd.

### Stap 1 — Besluitresource ophalen

Het OMC haalt de besluitresource op via de `resourceUrl` uit het event.

### Stap 2 — Informatieobject UUID check

```
informatieobject.objectType UUID ∈ ZGW_VARIABLE_OBJECTTYPE_DECISIONINFOBJECTTYPE_UUIDS
```

Het informatieobject gekoppeld aan het besluit moet een objecttype-UUID hebben dat voorkomt in de geconfigureerde lijst. Meerdere UUID's zijn mogelijk.

### Stap 3 — Informatieobject status check

```
informatieobject.status == "definitief"
```

Alleen informatieobjecten met status `definitief` leiden tot een notificatie.

### Stap 4 — Vertrouwelijkheid check

```
informatieobject.vertrouwelijkheidaanduiding == "openbaar"
```

Het informatieobject moet als `openbaar` zijn aangemerkt. Vertrouwelijke documenten worden niet genotificeerd.

### Stap 5 — Informeren check

```
zaaktype.informeren == true
```

Het zaaktype van de gekoppelde zaak moet `informeren` op `true` hebben staan in Open Zaak.

### Stap 6 — Whitelist check

```
zaaktype.identificatie ∈ ZGW_WHITELIST_DECISIONMADE_IDS
```

Het zaaktype moet voorkomen in de geconfigureerde whitelist.

### Stap 7 — Gegevens ophalen

Het OMC haalt aanvullende gegevens op:

1. `GET /besluiten/{uuid}` — besluitgegevens
2. `GET /enkelvoudiginformatieobjecten/{uuid}` — informatieobject
3. `GET /zaken/{uuid}/statussen` — statussen van de gekoppelde zaak
4. `GET /zaaktypen/{uuid}` — zaaktype
5. `GET /besluittypen/{uuid}` — besluittype
6. `GET /zaken/{uuid}` — zaakgegevens
7. Contactgegevens van de burger via OpenKlant (BSN)
8. `GET /besluitinformatieobjecten` — documenten gekoppeld aan het besluit

### Stap 8 — Documenten filteren

Per document wordt aanvullend gecontroleerd:

```
document.vertrouwelijkheidaanduiding == "openbaar"
AND
document.status == "definitief"
```

Alleen documenten die aan beide condities voldoen worden meegenomen.

### Stap 9 — Notificatie versturen

Het OMC verstuurt de notificatie via NotifyNL en slaat daarna een object op in de Objecten API met de templatepreview en documentverwijzingen.

---

## Vereisten samengevat

| Conditie | Waarde |
|---|---|
| `informatieobject.objectType` UUID | Moet in geconfigureerde UUID-lijst staan |
| `informatieobject.status` | `definitief` |
| `informatieobject.vertrouwelijkheidaanduiding` | `openbaar` |
| `zaaktype.informeren` | `true` |
| `zaaktype.identificatie` | Moet op whitelist staan |
| Burger heeft contactgegevens | E-mail of telefoonnummer in OpenKlant |

---

## Beschikbare templateplaceholders

### Besluit

| Placeholder | Bron | Beschrijving |
|---|---|---|
| `((besluit.identificatie))` | besluit | Identificatienummer van het besluit |
| `((besluit.datum))` | besluit | Datum van het besluit |
| `((besluit.ingangsdatum))` | besluit | Ingangsdatum van het besluit |
| `((besluit.vervaldatum))` | besluit | Vervaldatum van het besluit |
| `((besluit.toelichting))` | besluit | Toelichting op het besluit |

### Besluittype

| Placeholder | Bron | Beschrijving |
|---|---|---|
| `((besluittype.omschrijving))` | besluittype | Omschrijving van het besluittype |
| `((besluittype.besluitcategorie))` | besluittype | Categorie van het besluit |
| `((besluittype.publicatietekst))` | besluittype | Publieke toelichting |
| `((besluittype.toelichting))` | besluittype | Interne toelichting |

### Zaak (gekoppeld)

| Placeholder | Bron | Beschrijving |
|---|---|---|
| `((zaak.identificatie))` | zaak | Zaaknummer gekoppeld aan het besluit |
| `((zaak.omschrijving))` | zaak | Omschrijving van de zaak |

### Klant

| Placeholder | Bron | Beschrijving |
|---|---|---|
| `((klant.voornaam))` | klant | Voornaam van de klant |
| `((klant.voorvoegselAchternaam))` | klant | Tussenvoegsel van de klant |
| `((klant.achternaam))` | klant | Achternaam van de klant |

---

## Relevante omgevingsvariabelen

| Variabele | Beschrijving |
|---|---|
| `ZGW_WHITELIST_DECISIONMADE_IDS` | Kommagescheiden lijst van toegestane besluittype-identificaties, of `*` |
| `NOTIFY_TEMPLATEID_EMAIL_DECISIONMADE` | NotifyNL template-UUID voor e-mail |
| `NOTIFY_TEMPLATEID_SMS_DECISIONMADE` | NotifyNL template-UUID voor sms |
| `NOTIFY_TEMPLATEID_LETTER_DECISIONMADE` | NotifyNL template-UUID voor brief |
