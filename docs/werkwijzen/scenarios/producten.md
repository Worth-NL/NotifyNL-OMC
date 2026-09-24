# Product aangemaakt

Dit scenario wordt geactiveerd wanneer in **Open Product** een product wordt aangemaakt. Het OMC haalt het product op en stuurt elke *eigenaar* van dat product een e-mail.

Het wijkt op twee punten af van de overige scenario's. Er is geen zaak bij betrokken — het onderwerp van de notificatie is het product zelf — en er is niet één ontvanger maar een onbekend aantal: een product kan meerdere eigenaren hebben, en die krijgen allemaal een eigen e-mail en een eigen contactmoment. Er is geen sms- of briefvariant; het kanaal is altijd e-mail.

---

## Triggercondities

Het OMC activeert dit scenario wanneer een event binnenkomt met de volgende kenmerken:

| Veld | Vereiste waarde |
|---|---|
| `kanaal` | `producten` |
| `resource` | `product` |
| `actie` | `create` |
| `producttype.code` van het opgehaalde product | Moet voorkomen in `ZGW_WHITELIST_PRODUCTCREATE_IDS` |

### Voorbeeld payload

```json
{
  "actie": "create",
  "kanaal": "producten",
  "resource": "product",
  "kenmerken": {
    "producttype.uuid": "11111111-1111-1111-1111-111111111111",
    "producttype.code": "PARKEERVERGUNNING",
    "producttype.uniforme_product_naam": "parkeervergunning"
  },
  "hoofdObject": "https://openproduct.mijnstad.nl/producten/api/v1/producten/22222222-2222-2222-2222-222222222222",
  "resourceUrl": "https://openproduct.mijnstad.nl/producten/api/v1/producten/22222222-2222-2222-2222-222222222222",
  "aanmaakdatum": "2026-01-15T10:30:00Z"
}
```

> **Let op:** het OMC gebruikt de `kenmerken` **niet** om te bepalen of het product op de whitelist staat. De `producttype.code` wordt gelezen uit het opgehaalde product zelf. Open Product publiceert deze kenmerken in snake\_case terwijl de API-documentatie ze in camelCase weergeeft; door de whitelist op het opgehaalde product te toetsen is de routering ongevoelig voor die inconsistentie.

---

## Verwerkingslogica

De verwerking valt uiteen in twee fasen met verschillende foutafhandeling.

**Fase 1 (stap 1 t/m 4) bepaalt de HTTP-statuscode.** Alles wat bepaalt óf de notificatie überhaupt verstuurd kan worden, wordt hier gecontroleerd. Mislukt een van deze stappen, dan wordt niemand genotificeerd.

**Fase 2 (stap 5 en 6) kan die statuscode niet meer wijzigen.** Zodra fase 1 geslaagd is, staat het antwoord vast. Wat daarna misgaat, wordt vastgelegd als een mislukt contactmoment — niet als een mislukte notificatie. De reden daarvoor is dat een niet-2xx ertoe leidt dat Open Notificaties het hele event opnieuw aanbiedt, en bij zo'n herlevering krijgen ook alle eigenaren die de e-mail wél ontvingen hem een tweede keer.

### Stap 1 — Product ophalen

Het OMC haalt het product op bij `resourceUrl` uit de Open Producten API.

Het `producttype` zit **volledig genest** in het antwoord (`uuid`, `code`, `naam`, `uniforme_product_naam`, `gepubliceerd`); er is dus geen tweede aanroep nodig om het producttype op te halen.

Antwoordt Open Product met **404**, dan is het product tussen het publiceren van het event en de verwerking ervan verwijderd. Opnieuw aanbieden kan dan nooit meer slagen, dus dit wordt afgebroken (206) in plaats van als mislukking gemeld. Elk ander probleem — niet bereikbaar, niet geautoriseerd, serverfout — blijft een mislukking (412), zodat een tijdelijke storing het product niet definitief laat vallen.

### Stap 2 — Producttype-whitelist

```
product.producttype.code ∈ ZGW_WHITELIST_PRODUCTCREATE_IDS
```

Anders dan bij de zaakscenario's wordt hier getoetst op de **`code`** van het producttype. Open Product kent geen `identificatie`-veld; de `code` is per producttype uniek en leesbaar in configuratie. Gebruik `*` om alle producttypen toe te staan.

### Stap 3 — Publicatiecontrole

```
product.gepubliceerd == true
```

`gepubliceerd` wordt gelezen van het **product**, niet van het producttype: onder een gepubliceerd producttype kunnen producten hangen die nog niet getoond mogen worden. Een niet-gepubliceerd product wordt niet aan zijn eigenaren getoond en dus ook niet aan hen aangekondigd.

### Stap 4 — Partij per eigenaar opzoeken

Voor elke eigenaar in `product.eigenaren` zoekt het OMC de bijbehorende partij op in OpenKlant, via `partijIdentificator`:

| Veld op de eigenaar | `codeSoortObjectId` |
|---|---|
| `bsn` | `bsn` |
| `kvk_nummer` | `kvk` |

Open Product bewaakt zelf dat een eigenaar óf een BSN (eventueel met klantnummer) óf een KVK-nummer heeft, nooit beide.

- `klantnummer` wordt **niet** ondersteund: OpenKlant kan daar alleen op filteren via een filter dat in het eigen schema als `deprecated` is gemarkeerd.
- `vestigingsnummer` wordt genegeerd — er wordt opgezocht op het KVK-nummer alleen.
- De `uuid` van een eigenaar is de primaire sleutel van Open Product zelf en heeft **geen** relatie met OpenKlant; die kan dus niet gebruikt worden om de partij te vinden.

Deze stap is **alles-of-niets**: resolveert één van de drie eigenaren niet, dan wordt er aan niemand iets verstuurd. Dat is geen striktheid om de striktheid — een mislukking wordt vastgelegd als klantcontact met een `betrokkene.wasPartij`, en juist de partij die daar ingevuld moet worden ontbreekt in dit geval. Het OMC maakt hier bewust géén ontbrekende partij aan (anders dan de printstraat-flow). Door hier af te breken heeft elke mislukking die later nog gemeld wordt, per definitie een bestaande partij om aan te hangen.

**Adresselectie.** Bij dezelfde opzoeking bepaalt het OMC ook het e-mailadres, met deze voorrang:

1. een digitaal adres met `referentie` = `portaalvoorkeur`;
2. anders het voorkeursadres van de partij (`voorkeursDigitaalAdres`);
3. anders het eerste e-mailadres dat de partij heeft.

De zoektocht wordt daarbij beperkt tot het kanaal e-mail. Zonder die beperking zou een partij met een telefoonnummer als voorkeursadres gelezen worden als "geen e-mailadres bekend": de selectie stopt dan bij dat voorkeursadres en bereikt het e-mailadres daarachter nooit.

Een **ontbrekend adres is geen reden om te stoppen.** De partij is gevonden, dus de mislukking kan geregistreerd worden — dat gebeurt in stap 6.

### Stap 5 — Versturen per eigenaar

Elke eigenaar krijgt een eigen e-mail via het template uit `NOTIFY_TEMPLATEID_EMAIL_PRODUCTCREATED`, met een eigen personalisatie. Dat één eigenaar mislukt, stopt de overige niet.

Een geslaagde verzending (HTTP 201 van NotifyNL) betekent alleen dat NotifyNL het verzoek **geaccepteerd** heeft; de daadwerkelijke aflevering volgt later en wordt via de callback teruggemeld. Er wordt op dit moment dan ook nog géén contactmoment geregistreerd.

### Stap 6 — Contactmoment registreren

Er wordt één klantcontact per eigenaar geregistreerd, elk met een eigen `betrokkene.wasPartij`, en alle met hetzelfde product als `onderwerpobject`. Er komt geen zaak aan te pas.

| Uitkomst | Wanneer geregistreerd | Inhoud |
|---|---|---|
| Geen e-mailadres bekend | Direct | Mislukt contactmoment, met de `VARIABLES_UXMESSAGES_EMAIL_FAILURE_*`-teksten |
| NotifyNL weigerde de verzending of gaf een fout | Direct | Mislukt contactmoment, idem |
| Verzending geaccepteerd | Later, vanuit de afleverstatus-callback | Onderwerp en inhoud worden met het notificatie-ID teruggehaald bij NotifyNL, zodat wordt vastgelegd wat de eigenaar daadwerkelijk ontvangen heeft |

De twee directe mislukkingen bereiken NotifyNL nooit, dus daarvoor komt geen callback en is er niets om terug te halen — vandaar de terugval op de geconfigureerde UxMessages-teksten.

---

## Statuscodes

| Situatie | Statuscode | Herlevering door Open Notificaties |
|---|---|---|
| Alle controles geslaagd | `202 Accepted` | Nee |
| Product bestaat niet meer (404 bij Open Product) | `206 Partial Content` | Nee |
| Producttype niet op de whitelist | `206 Partial Content` | Nee |
| Product niet gepubliceerd | `206 Partial Content` | Nee |
| Product heeft geen eigenaren | `206 Partial Content` | Nee |
| Een eigenaar heeft geen BSN of KVK-nummer | `206 Partial Content` | Nee |
| Een eigenaar heeft geen partij in OpenKlant | `206 Partial Content` | Nee |
| Open Product of OpenKlant onbereikbaar of foutief tijdens fase 1 | `412 Precondition Failed` | Ja |
| Verzending of registratie mislukt in fase 2 | `202 Accepted` | Nee — vastgelegd als mislukt contactmoment |

De laatste regel is de belangrijkste: **een eigenaar zonder e-mailadres, of een verzending die NotifyNL weigert, levert nog steeds een `202` op.** Heeft een product drie eigenaren die alle drie een partij hebben maar geen van drieën een e-mailadres, dan is dat een `202` met drie mislukte contactmomenten.

---

## Vereisten samengevat

| Conditie | Waarde |
|---|---|
| `ZGW_ENDPOINT_OPENPRODUCTEN` / `ZGW_AUTH_KEY_OPENPRODUCTEN` | Ingesteld — het OMC bouwt bij het opstarten voor elke bekende dienst een HTTP-client, dus deze zijn ook vereist als het scenario niet gebruikt wordt |
| `ZGW_WHITELIST_PRODUCTCREATE_IDS` | `*` of een kommagescheiden lijst van producttype-codes |
| `NOTIFY_TEMPLATEID_EMAIL_PRODUCTCREATED` | UUID van een bestaand e-mailtemplate in NotifyNL |
| `product.gepubliceerd` | `true` |
| Eigenaar heeft BSN of KVK-nummer | Vereist — klantnummer alleen is niet genoeg |
| Eigenaar heeft een partij in OpenKlant | Vereist voor álle eigenaren; het OMC maakt er geen aan |
| Eigenaar heeft een e-mailadres | Niet vereist — levert een mislukt contactmoment op, geen mislukte notificatie |
| Abonnement op het kanaal `producten` in Open Notificaties | Vereist, en Open Product moet erop publiceren |

---

## Productstructuur (Open Producten API)

Open Product gebruikt snake\_case veldnamen:

```json
{
  "uuid": "22222222-2222-2222-2222-222222222222",
  "url": "https://openproduct.mijnstad.nl/producten/api/v1/producten/22222222-2222-2222-2222-222222222222",
  "naam": "verhuurvergunning: straatweg 14",
  "status": "gereed",
  "gepubliceerd": true,
  "producttype": {
    "uuid": "11111111-1111-1111-1111-111111111111",
    "code": "PARKEERVERGUNNING",
    "naam": "Parkeervergunning",
    "uniforme_product_naam": "parkeervergunning",
    "gepubliceerd": true
  },
  "eigenaren": [
    {
      "uuid": "33333333-3333-3333-3333-333333333333",
      "bsn": "999990019"
    },
    {
      "uuid": "44444444-4444-4444-4444-444444444444",
      "kvk_nummer": "12345678",
      "vestigingsnummer": "000012345678"
    }
  ]
}
```

---

## Templateplaceholders

| Placeholder | Bron |
|---|---|
| `((klant.voornaam))` | Voornaam van de partij uit OpenKlant |
| `((klant.voorvoegselAchternaam))` | Tussenvoegsel van de partij |
| `((klant.achternaam))` | Achternaam van de partij |
| `((producttype.naam))` | `producttype.naam` van het product |
| `((product.naam))` | `naam` van het product |
| `((product.status))` | `status` van het product |

Elke eigenaar krijgt zijn eigen set waarden; er wordt geen personalisatie tussen eigenaren gedeeld.

---

## Relevante omgevingsvariabelen

| Variabele | Beschrijving |
|---|---|
| `ZGW_ENDPOINT_OPENPRODUCTEN` | Basis-URL van de Open Producten API, inclusief pad — bijv. `https://openproduct.mijnstad.nl/producten/api/v1` |
| `ZGW_AUTH_KEY_OPENPRODUCTEN` | API-sleutel voor Open Product (`Authorization: Token <sleutel>`) |
| `ZGW_WHITELIST_PRODUCTCREATE_IDS` | Toegestane producttype-codes (`*` = alle) |
| `NOTIFY_TEMPLATEID_EMAIL_PRODUCTCREATED` | Template-UUID voor de e-mail aan de eigenaar |
| `VARIABLES_OPENKLANT_CODEOBJECTTYPE_PRODUCT` | `codeObjecttype` van het `onderwerpobject` op het klantcontact (standaard: `product`) |
| `VARIABLES_OPENKLANT_CODEREGISTER_PRODUCT` | `codeRegister` van het `onderwerpobject` (standaard: `open-product`) |
| `VARIABLES_UXMESSAGES_EMAIL_FAILURE_SUBJECT` | Terugvalonderwerp bij een mislukt contactmoment |
| `VARIABLES_UXMESSAGES_EMAIL_FAILURE_BODY` | Terugvalinhoud bij een mislukt contactmoment |

Zie [Omgevingsvariabelen](../../configuratie/omgevingsvariabelen.md) voor de volledige lijst.

> ⚠️ **Nog te bevestigen:** `VARIABLES_OPENKLANT_CODEOBJECTTYPE_PRODUCT` en `VARIABLES_OPENKLANT_CODEREGISTER_PRODUCT` hebben voorlopige standaardwaarden. Het objecttype "product" is nog niet bevestigd geregistreerd in OpenKlant — stem de waarden af met de beheerder van het OpenKlant-objecttyperegister voordat dit scenario in productie gaat. Hetzelfde geldt voor het `_BERICHT`-paar van de MOBB-flow.
