# MijnOverheid

Het OMC ondersteunt het doorsturen van relevante zaakgebeurtenissen naar MijnOverheid, zodat burgers hun zaken kunnen inzien in de "MijnZaken"-lijst van MijnOverheid.

---

## Hoe het werkt

1. Open Zaak stuurt een notificatie (via Open Notificaties) wanneer een zaak wordt aangemaakt, gewijzigd, geopend of verwijderd
2. Het eindpunt `POST /Events/MijnZaken` ontvangt deze notificatie en normaliseert deze naar een CloudEvent (volgens het NL-GOV CloudEvents-profiel)
3. Het OMC past filters toe **die per gebeurtenistype verschillen** (zie hieronder)
4. Als de filters slagen, verstuurt het OMC een CloudEvent naar de MijnOverheid-webhook

### Filters per gebeurtenis

| Filter | Zaak gemuteerd | Zaak geopend | Zaak verwijderd |
|---|:---:|:---:|:---:|
| Initiator is natuurlijk persoon | ✅ | ✅ (alleen als de zaak al eerder geopend is geweest) | — |
| Statustype heeft `informeren = true` | ✅ | — | — |
| Zaaktype staat op de whitelist | ✅ | — | — |
| Gebeurtenis niet verouderd (t.o.v. `laatstGemuteerd`/`laatstGeopend`) | ✅ | ✅ | — |

**Zaak verwijderd** wordt altijd direct doorgestuurd, zonder enige filtering — een zaak die verwijderd is, hoeft niet eerst opgehaald te worden.

**Zaak geopend** slaat de initiator- en verouderd-check over als de zaak nog geen eerdere `laatstGeopend`-datum heeft (de eerste keer openen wordt altijd doorgestuurd).

---

## Ondersteunde gebeurtenissen

MijnOverheid onderscheidt geen native "geopend"/"verwijderd"-acties in de ZGW Notificaties API-standaard. Open Zaak/Open Notificaties leveren daarom de volgende (afgesproken, niet-standaard) combinaties van `kanaal`/`resource`/`actie` aan om deze gebeurtenissen te signaleren:

| Gebeurtenis | `kanaal` | `resource` | `actie` | CloudEvent `type` |
|---|---|---|---|---|
| Zaak gemuteerd (statuswijziging) | `zaken` | `status` | `create` | `nl.overheid.zaken.zaak-gemuteerd` |
| Zaak geopend | `zaken` | `zaak` | `read` | `nl.overheid.zaken.zaak-geopend` |
| Zaak verwijderd | `zaken` | `zaak` | `destroy` | `nl.overheid.zaken.zaak-verwijderd` |

> **Let op:** `create`/`update`/`destroy` zijn de standaard VNG Notificaties API-acties. `read` is geen standaard VNG-actie — de standaard kent geen gebeurtenis voor het openen/inzien van een zaak. Dit is een projectafspraak tussen het OMC en het zaaksysteem, geen VNG-standaard.

Het eindpunt accepteert daarnaast ook rechtstreekse CloudEvent-payloads (met een `specversion`-veld), voor het geval een bronsysteem al in dit formaat aanlevert.

### Voorbeeld payloads (NotificationEvent-formaat)

**Zaak gemuteerd:**

```json
{
  "actie": "create",
  "kanaal": "zaken",
  "resource": "status",
  "kenmerken": {
    "zaaktype": "https://openzaak.mijnstad.nl/catalogi/api/v1/zaaktypen/11111111-1111-1111-1111-111111111111",
    "bronorganisatie": "123456789",
    "vertrouwelijkheidaanduiding": "openbaar"
  },
  "hoofdObject": "https://openzaak.mijnstad.nl/zaken/api/v1/zaken/22222222-2222-2222-2222-222222222222",
  "resourceUrl": "https://openzaak.mijnstad.nl/zaken/api/v1/statussen/33333333-3333-3333-3333-333333333333",
  "aanmaakdatum": "2026-01-15T10:30:00Z"
}
```

**Zaak geopend:**

```json
{
  "actie": "read",
  "kanaal": "zaken",
  "resource": "zaak",
  "kenmerken": {
    "zaaktype": "https://openzaak.mijnstad.nl/catalogi/api/v1/zaaktypen/11111111-1111-1111-1111-111111111111",
    "bronorganisatie": "123456789",
    "vertrouwelijkheidaanduiding": "openbaar"
  },
  "hoofdObject": "https://openzaak.mijnstad.nl/zaken/api/v1/zaken/22222222-2222-2222-2222-222222222222",
  "resourceUrl": "https://openzaak.mijnstad.nl/zaken/api/v1/zaken/22222222-2222-2222-2222-222222222222",
  "aanmaakdatum": "2026-01-15T10:30:00Z"
}
```

**Zaak verwijderd:**

```json
{
  "actie": "destroy",
  "kanaal": "zaken",
  "resource": "zaak",
  "kenmerken": {
    "zaaktype": "https://openzaak.mijnstad.nl/catalogi/api/v1/zaaktypen/11111111-1111-1111-1111-111111111111",
    "bronorganisatie": "123456789",
    "vertrouwelijkheidaanduiding": "openbaar"
  },
  "hoofdObject": "https://openzaak.mijnstad.nl/zaken/api/v1/zaken/22222222-2222-2222-2222-222222222222",
  "resourceUrl": "https://openzaak.mijnstad.nl/zaken/api/v1/zaken/22222222-2222-2222-2222-222222222222",
  "aanmaakdatum": "2026-01-15T10:30:00Z"
}
```

> Bij `zaak geopend` en `zaak verwijderd` zijn `hoofdObject` en `resourceUrl` gelijk — er is geen apart substatusobject zoals bij `zaak gemuteerd`.

### Voorbeeld payload (rechtstreeks CloudEvent-formaat)

```json
{
  "specversion": "1.0",
  "type": "nl.overheid.zaken.zaak-gemuteerd",
  "source": "urn:nld:oin:123456789000:zakensysteem",
  "subject": "22222222-2222-2222-2222-222222222222",
  "id": "44444444-4444-4444-4444-444444444444",
  "time": "2026-01-15T10:30:00Z",
  "dataref": "/api/v1/zaken/22222222-2222-2222-2222-222222222222",
  "datacontenttype": "application/json"
}
```

---

## Responsgedrag

De HTTP-statuscode en -body die `/Events/MijnZaken` teruggeeft, zijn een direct doorgestuurde kopie van de respons van MijnOverheid zelf — het OMC vertaalt deze niet. Een paar specifieke gevallen:

| Respons | Betekenis |
|---|---|
| Statuscode van MijnOverheid (bijv. `204`) | De gebeurtenis is doorgestuurd en geaccepteerd door MijnOverheid |
| `200 OK` met body `"Event was not forwarded (skipped)."` | Het OMC heeft zelf besloten niet door te sturen (filter hierboven niet geslaagd) — dit is geen foutsituatie |
| `400 Bad Request` | De payload kon niet worden herkend, óf MijnOverheid heeft de doorgestuurde CloudEvent afgewezen |
| `500 Internal Server Error` | Er ging iets mis buiten een payloadprobleem om (bijv. token ophalen bij MijnOverheid mislukt) |

---

## Configuratie

| Variabele | Vereist | Beschrijving |
|---|---|---|
| `MIJNOVERHEID_WEBHOOK_URL` | Ja | URL van de MijnOverheid "zaak-muteren"-webhook waarnaar CloudEvents worden verstuurd |
| `MIJNOVERHEID_AUTH_CLIENTID` | Ja | OAuth2 client-ID voor de MijnOverheid-tokenuitwisseling |
| `MIJNOVERHEID_AUTH_SECRET` | Ja | OAuth2 clientgeheim voor MijnOverheid |
| `MIJNOVERHEID_AUTH_TOKEN_ENDPOINT` | Ja | OAuth2 token-eindpunt van MijnOverheid |

De whitelist- en zaaktype-filtering hergebruikt de bestaande `ZGW_WHITELIST_ZAAKCREATE_IDS` / `ZGW_WHITELIST_ZAAKUPDATE_IDS` / `ZGW_WHITELIST_ZAAKCLOSE_IDS` variabelen — zie [Omgevingsvariabelen](../configuratie/omgevingsvariabelen.md).

---

## Bekende beperking: `laatstGemuteerd` / `laatstGeopend`

De "gebeurtenis niet verouderd"-filter is gebaseerd op de velden `laatstGemuteerd` en `laatstGeopend` op de zaak. Dit zijn **geen standaard VNG Zaken API-velden** — een zaaksysteem dat strikt de standaard volgt, levert deze velden niet. Als een zaaksysteem ze niet levert, wordt dit filter automatisch overgeslagen (de gebeurtenis wordt dan altijd doorgestuurd, ongeacht volgorde of veroudering) — dit leidt niet tot een fout.

Bij zaaksystemen die deze velden niet leveren, kan ook MijnOverheid's eigen "ongelezen"-indicator (gebaseerd op `laatstGewijzigd` versus `laatstGelezen`, na een eigen zaak-ophaling door MijnOverheid) niet correct werken zolang deze velden ontbreken — dit is dus breder dan alleen het OMC.
