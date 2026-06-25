# Besluit genomen

Dit scenario wordt geactiveerd wanneer een besluit wordt genomen dat een burger raakt.

---

## Triggercondities

| Veld | Vereiste waarde |
|---|---|
| `kanaal` | `besluiten` |
| `resource` | `besluit` |
| `actie` | `create` |
| `besluittype.besluitcategorie` | `definitief` |
| `besluittype.publicatietekst` aanwezig | Ja (openbaar besluit) |

---

## Voorbeeldpayload

```json
{
  "kanaal": "besluiten",
  "resource": "besluit",
  "actie": "create",
  "kenmerken": {
    "bronorganisatie": "123456789",
    "besluittype": "https://openzaak.mijnstad.nl/catalogi/api/v1/besluittypen/...",
    "verantwoordelijkeOrganisatie": "123456789"
  },
  "resourceUrl": "https://openzaak.mijnstad.nl/besluiten/api/v1/besluiten/..."
}
```

---

## Vereisten

- Het besluittype moet op de whitelist staan (`ZGW_WHITELIST_DECISIONMADE_IDS`)
- `besluittype.besluitcategorie` moet `definitief` zijn
- Het besluit moet openbaar zijn (`publicatietekst` aanwezig)
- De burger moet contactgegevens hebben in OpenKlant via de gekoppelde zaak
- Het template-ID moet zijn ingesteld

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
