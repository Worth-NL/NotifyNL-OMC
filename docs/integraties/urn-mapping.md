# URN-mapping en registratie-instanties

> **Status: ontwerpvoorstel.** Dit document beschrijft hoe het OMC URN's gaat herkennen, omzetten naar URL's en daarbij per bron de juiste credentials kiest. Het is nog niet geïmplementeerd; namen van instellingen en klassen kunnen nog wijzigen.

---

## Achtergrond

Registraties (Open Zaak, OpenVTB, Open Producten, Open Klant, …) gaan steeds vaker naar elkaar verwijzen met een **URN** in plaats van een URL. Het algemene formaat volgens de afspraken binnen het Platform voor Dienstverlening is:

```
urn:nld:<organisatie>:<systeem>:<resource>:<attribuut>:<identificatie>
```

Bijvoorbeeld:

```
urn:nld:denhaag:zaken-sociaal:zaak:uuid:3df59f4f-ffa0-4caf-8a15-eeb19afffa52
```

Een URN zegt *wat* een object is, maar niet *waar* het staat. Het OMC moet die objecten wel ophalen (een zaak bij een taak, een document bij een besluit, een product bij een notificatie). Daarvoor moet het OMC weten:

1. bij welke instantie van welke registratie een URN hoort;
2. op welke URL het object daar te vinden is;
3. met welke credentials het OMC die instantie mag bevragen.

Daarnaast geldt: **elke registratie kan meerdere instanties hebben.** Een gemeente kan bijvoorbeeld twee zaaksystemen hebben die allebei zaken aanleveren in hetzelfde veld van een taak of bericht. Het OMC kan nu per registratie maar één endpoint en één set credentials gebruiken (`ZGW_ENDPOINT_*` / `ZGW_AUTH_*`); dat volstaat dan niet meer.

In de praktijk is de URN-notatie bovendien nog niet uniform. Den Haag gebruikt bijvoorbeeld een extra componentsegment (`urn:nld:denhaag:zaken-main:zrc:zaak:uuid:…`), en voor BSN's zijn zowel `urn:nld:brp:bsn:nummer:…` als `urn:nld:overheid:brp:np:bsn:…` in omloop. Het OMC moet daarom niet één vaste URN-indeling afdwingen, maar per gemeente configureerbaar zijn.

---

## Uitgangspunten

| # | Uitgangspunt |
|---|---|
| 1 | **Het API-pad per registratietype is vast.** Alleen de locatie (schema, host en eventueel een pad-prefix) verschilt per instantie. Dit is een eis aan klanten: een Open Zaak-instantie biedt de Zaken API altijd aan op `<basis>/zaken/api/v1`. |
| 2 | **Elke registratie kan meerdere instanties hebben.** Instanties worden dynamisch geregistreerd in configuratie, niet in code. |
| 3 | **Elke URN die het OMC tegenkomt, probeert het te mappen naar een URL.** Dat zal maar af en toe lukken; een URN die niet te mappen is, is een normale uitkomst en geen crash. |
| 4 | **Credentials horen bij een instantie** en worden in dezelfde configuratie geregeld. Geheime waarden staan nooit in een ConfigMap. |
| 5 | **Achterwaarts compatibel.** Installaties zonder de nieuwe configuratie werken ongewijzigd met de bestaande `ZGW_ENDPOINT_*`- en `ZGW_AUTH_*`-variabelen. |

---

## Begrippen

| Begrip | Betekenis |
|---|---|
| **Registratietype** | Het soort registratie, bijvoorbeeld `openzaak` of `openklant`. Het type bepaalt welke API's een instantie aanbiedt en op welke vaste paden. |
| **API** | Eén ZGW-API binnen een registratie, bijvoorbeeld `zaken`, `catalogi` of `klantinteracties`. |
| **Instantie** | Eén concrete installatie van een registratietype, met een eigen basis-URL en eigen credentials. |
| **URN-prefix** | Het begin van een URN dat aangeeft dat het object bij een bepaalde instantie hoort, bijvoorbeeld `urn:nld:denhaag:zaken-main`. |
| **Referentie** | Een waarde in een veld die naar een object verwijst: een URL óf een URN. |

---

## Configuratie

### Instellingen

| Variabele | Verplicht | Beschrijving |
|---|---|---|
| `OMC_INSTANCES_PATH` | Nee | Pad naar een YAML-bestand of een map met YAML-bestanden (`*.yaml`, `*.yml`, alfabetisch ingelezen). Niet gezet: het OMC gebruikt de bestaande `ZGW_ENDPOINT_*`/`ZGW_AUTH_*` (zie [Achterwaartse compatibiliteit](#achterwaartse-compatibiliteit)). |
| `OMC_SECRETS_PATH` | Ja, als `OMC_INSTANCES_PATH` gezet is | Map waarin elk secret als los bestand staat, zoals Kubernetes een Secret als volume mount. `secret:<naam>` in de configuratie verwijst naar het bestand `<OMC_SECRETS_PATH>/<naam>`. |

Het bestand of de map wordt één keer bij het opstarten ingelezen en gevalideerd.

### Volledig voorbeeld

```yaml
version: 1

instances:
  # Hoofd-zaaksysteem: Zaken, Catalogi, Documenten en Besluiten op één host
  - id: openzaak-main
    type: openzaak
    baseUrl: "https://openzaak-zgw.denhaag.nl"
    default: true
    urnPrefixes:
      - "urn:nld:denhaag:zaken-main"
    auth:
      type: jwt
      issuer: "notifynl-omc"
      clientId: "notifynl-omc"
      userId: "omc"
      userName: "NotifyNL OMC"
      secret: "secret:openzaak-main-jwt"
    links:
      klantinteracties: openklant-main

  # Tweede zaaksysteem, eigen host en eigen credentials
  - id: openzaak-sociaal
    type: openzaak
    baseUrl: "https://zaken-sociaal.denhaag.nl"
    urnPrefixes:
      - "urn:nld:denhaag:zaken-sociaal"
    auth:
      type: jwt
      issuer: "notifynl-omc"
      clientId: "notifynl-omc-sociaal"
      secret: "secret:openzaak-sociaal-jwt"
    links:
      klantinteracties: openklant-main

  # Losse Documenten API (DRC) op een andere host
  - id: dms
    type: documenten
    baseUrl: "https://dms.denhaag.nl"
    urnPrefixes:
      - "urn:nld:denhaag:dms"
    auth:
      type: jwt
      issuer: "notifynl-omc"
      clientId: "notifynl-omc"
      secret: "secret:dms-jwt"

  - id: openklant-main
    type: openklant
    baseUrl: "https://openklant.denhaag.nl"
    default: true
    auth:
      type: apiKey
      key: "secret:openklant-main-key"

  - id: openvtb-main
    type: openvtb
    baseUrl: "https://openvtb-zgw.denhaag.nl"
    default: true
    urnPrefixes:
      - "urn:nld:denhaag:vtb"
    auth:
      type: apiKey
      key: "secret:openvtb-main-key"

  - id: openproducten-main
    type: openproducten
    baseUrl: "https://openproduct-zgw.denhaag.nl"
    default: true
    urnPrefixes:
      - "urn:nld:denhaag:producten-main"
    auth:
      type: apiKey
      key: "secret:openproducten-main-key"

  - id: objecten-main
    type: objecten
    baseUrl: "https://objecten.denhaag.nl"
    auth:
      type: apiKey
      key: "secret:objecten-main-key"

  - id: objecttypen-main
    type: objecttypen
    baseUrl: "https://objecttypen.denhaag.nl"
    auth:
      type: apiKey
      key: "secret:objecttypen-main-key"

  - id: opennotificaties-main
    type: opennotificaties
    baseUrl: "https://opennotificaties.denhaag.nl"
    auth:
      type: jwt
      issuer: "notifynl-omc"
      clientId: "notifynl-omc"
      secret: "secret:openzaak-main-jwt"   # zelfde secret mag door meerdere instanties gebruikt worden
```

### Velden

| Veld | Verplicht | Beschrijving |
|---|---|---|
| `id` | Ja | Unieke naam van de instantie. Wordt gebruikt in `links`, logging en het dashboard. |
| `type` | Ja | Registratietype, zie [Registratietypen en vaste paden](#registratietypen-en-vaste-paden). |
| `baseUrl` | Ja | Basis-URL van de instantie, **zonder** het API-pad. Mag een pad-prefix bevatten als de instantie achter een gateway staat (bijvoorbeeld `https://api.gemeente.nl/openzaak`). |
| `default` | Nee | Of dit de standaardinstantie is voor de API's van dit type. Zie [Calls zonder referentie](#calls-zonder-referentie). |
| `urnPrefixes` | Nee | URN-prefixen die bij deze instantie horen. Zonder prefixen worden URN's nooit naar deze instantie gemapt; URL's wel. De eerste prefix wordt gebruikt bij het omzetten van een URL naar een URN. |
| `auth` | Ja | Hoe het OMC zich bij deze instantie aanmeldt, zie hieronder. |
| `links` | Nee | Per API welke andere instantie hoort bij objecten uit deze instantie, bijvoorbeeld welke Open Klant hoort bij een zaaksysteem. Sleutel = API-naam, waarde = instantie-`id`. |

### Authenticatie

| `type` | Velden | Header |
|---|---|---|
| `jwt` | `secret` (geheim), `issuer`, `clientId`, optioneel `userId`, `userName`, `audience`, `expiresInMin` | `Authorization: Bearer <JWT>`, zoals nu voor Open Zaak |
| `apiKey` | `key` (geheim) | `Authorization: Token <key>`, zoals nu voor Open Klant, Objecten, OpenVTB en Open Producten |
| `none` | — | Geen; alleen bedoeld voor lokale ontwikkeling |

Alleen `secret` en `key` zijn geheim. Deze velden accepteren **uitsluitend** een verwijzing in de vorm `secret:<naam>`; een letterlijke waarde wordt geweigerd. Zo kan een sleutel nooit per ongeluk in een ConfigMap of in git terechtkomen.

Secrets worden bij gebruik uit het bestand gelezen (met een korte cache). Kubernetes ververst een gemounte Secret automatisch, dus een credential kan geroteerd worden zonder herstart, mits de map gemount wordt en niet met `subPath`.

### Validatie bij het opstarten

De configuratie wordt bij het opstarten volledig ingelezen en gecontroleerd. Bij een fout start het OMC niet en meldt het alle gevonden fouten in één keer, met het bestand en de instantie erbij:

* dubbele `id`'s;
* onbekend `type` of onbekend `auth.type`;
* `baseUrl` ontbreekt, is geen absolute `http(s)`-URL, of eindigt al op een vast API-pad;
* een `secret:`-verwijzing waarvoor geen niet-leeg bestand bestaat in `OMC_SECRETS_PATH`;
* een letterlijke waarde in `secret` of `key`;
* `links` die naar een onbekende instantie verwijzen, of naar een instantie die de genoemde API niet aanbiedt;
* dezelfde URN-prefix bij meer dan één instantie;
* twee instanties met dezelfde `baseUrl` en een overlappende API;
* een API met meerdere instanties waarvan er niet precies één `default: true` heeft.

---

## Registratietypen en vaste paden

De paden staan in code en zijn per type gelijk voor iedere instantie. De configuratie bevat alleen de basis-URL.

| Type | API | Vast pad | Resources (in URN) |
|---|---|---|---|
| `openzaak` | `zaken` | `/zaken/api/v1` | `zaak`, `status`, `rol`, `resultaat`, `zaakinformatieobject` |
| | `catalogi` | `/catalogi/api/v1` | `zaaktype`, `statustype`, `resultaattype`, `roltype`, `informatieobjecttype`, `besluittype` |
| | `documenten` | `/documenten/api/v1` | `enkelvoudiginformatieobject` |
| | `besluiten` | `/besluiten/api/v1` | `besluit`, `besluitinformatieobject` |
| `zaken`, `catalogi`, `documenten`, `besluiten` | één van de vier hierboven | idem | idem |
| `openklant` | `klantinteracties` | `/klantinteracties/api/v1` | `partij`, `betrokkene`, `klantcontact` |
| `objecten` | `objecten` | `/api/v2` | `object` |
| `objecttypen` | `objecttypen` | `/api/v2` *(te bevestigen, zie open punten)* | `objecttype` |
| `openvtb` | `taken` | `/taken/api/v1` | `externetaak` |
| | `berichten` | `/berichten/api/v1` | `bericht` |
| | `verzoeken` | `/verzoeken/api/v1` *(te bevestigen)* | `verzoek`, `verzoektype` |
| `openproducten` | `producten` | `/producten/api/v1` | `product`, `producttype` |
| `opennotificaties` | `notificaties` | `/api/v1` | — |

`openzaak` is een verkorte notatie voor een instantie die alle vier de Open Zaak-API's op dezelfde basis-URL aanbiedt. Draait bijvoorbeeld de Documenten API elders, dan wordt die als eigen instantie met `type: documenten` geregistreerd (zie `dms` in het voorbeeld).

De koppeling van resource naar collectie (`zaak` → `/zaken`, `enkelvoudiginformatieobject` → `/enkelvoudiginformatieobjecten`, `externetaak` → `/externetaken`, …) staat ook in code.

---

## Resolutie

Alle referenties lopen via één component, de **instance registry**. De aanroepende code geeft alleen aan *wat* ze verwacht (bijvoorbeeld een zaak); de registry bepaalt *waar* het object staat en *met welke credentials* het opgehaald wordt.

### URN → URL

1. **Instantie bepalen.** De langste URN-prefix uit `urnPrefixes` die overeenkomt, wint. Een prefix moet op een segmentgrens eindigen (`urn:nld:denhaag:zaken` matcht dus niet op `urn:nld:denhaag:zaken-main:…`). De vergelijking is niet hoofdlettergevoelig; de identificatie zelf wordt ongewijzigd overgenomen.
2. **Resource vinden.** In de rest van de URN wordt het eerste segment gezocht dat een bekende resource is van een API die deze instantie aanbiedt. Segmenten daarvoor, zoals het componentsegment `zrc` of `ztc` bij Den Haag, worden overgeslagen.
3. **Attribuut en identificatie.** Na de resource volgt `<attribuut>:<identificatie>`. Staat er maar één segment, dan wordt `uuid` als attribuut aangenomen.
4. **URL opbouwen.**
   * Bij attribuut `uuid` wordt de URL direct opgebouwd: `<baseUrl><vast pad>/<collectie>/<uuid>`.
   * Bij een ander attribuut voert het OMC een vaste zoekvraag uit op die instantie (zie de tabel hieronder). Ook deze zoekvragen staan in code, niet in configuratie.

Voorbeelden met de configuratie hierboven:

| URN | Instantie | URL |
|---|---|---|
| `urn:nld:denhaag:zaken-main:zrc:zaak:uuid:3df59f4f-…` | `openzaak-main` | `https://openzaak-zgw.denhaag.nl/zaken/api/v1/zaken/3df59f4f-…` |
| `urn:nld:denhaag:zaken-sociaal:zaak:3df59f4f-…` | `openzaak-sociaal` | `https://zaken-sociaal.denhaag.nl/zaken/api/v1/zaken/3df59f4f-…` |
| `urn:nld:denhaag:zaken-main:ztc:zaaktype:uuid:7c1e…` | `openzaak-main` | `https://openzaak-zgw.denhaag.nl/catalogi/api/v1/zaaktypen/7c1e…` |
| `urn:nld:denhaag:vtb:taken:externetaak:uuid:a12b…` | `openvtb-main` | `https://openvtb-zgw.denhaag.nl/taken/api/v1/externetaken/a12b…` |
| `urn:nld:denhaag:zaken-main:zrc:zaak:zaaknummer:ZAAK-2026-0001` | `openzaak-main` | via zoekvraag, zie hieronder |
| `urn:nld:rotterdam:zaken:zaak:uuid:…` | — | niet te mappen: geen instantie met deze prefix |
| `urn:nld:denhaag:vtb:verzoeken:verzoek:uuid:…` | `openvtb-main` | alleen als `verzoeken` als API van OpenVTB wordt ondersteund |

Ondersteunde attributen anders dan `uuid`:

| Resource | Attribuut | Zoekvraag | Bijzonderheden |
|---|---|---|---|
| `zaak` | `zaaknummer`, `identificatie` | `GET /zaken/api/v1/zaken?identificatie=<id>` | `identificatie` is alleen uniek binnen een bronorganisatie. Meer dan één resultaat geeft *meerduidig*. |
| `zaaktype` | `identificatie` | `GET /catalogi/api/v1/zaaktypen?identificatie=<id>` | Meerdere versies kunnen dezelfde identificatie hebben; de op dit moment geldige versie wordt gekozen. |

Uitkomsten van zoekvragen worden kort gecachet, zodat dezelfde URN binnen één verwerking niet steeds opnieuw opgezocht wordt.

### URL → instantie

Ook gewone URL's lopen via de registry. De instantie is die met de langste `baseUrl` die een prefix van de URL is. Dit geldt ook voor URL's die het OMC uit responses volgt (zaak → zaaktype → statustype, zaak → documenten). Die komen zo automatisch bij de juiste instantie en credentials uit, ook als bijvoorbeeld de Catalogi API op een andere host staat dan de Zaken API.

Een URL die bij geen enkele instantie hoort, wordt **niet** opgehaald. Credentials gaan dus nooit naar een host die niet geregistreerd is. Dit vervangt en veralgemeniseert de bestaande SSRF-controle in de [printstraat](../werkwijzen/scenarios/print-printstraat.md), waar de `pdfurl` op de origin van `ZGW_ENDPOINT_DOCUMENTEN` moet wijzen.

### URL → URN

Waar het OMC zelf een verwijzing wegschrijft (bijvoorbeeld de onderwerpobject-identificator van een klantcontact in Open Klant, of kenmerken in een uitgaand CloudEvent), kan het een URL omzetten naar een URN:

```
<eerste urnPrefix van de instantie>:<resource>:uuid:<uuid>
```

Bijvoorbeeld `https://zaken-sociaal.denhaag.nl/zaken/api/v1/zaken/3df59f4f-…` → `urn:nld:denhaag:zaken-sociaal:zaak:uuid:3df59f4f-…`.

Dat lukt alleen voor instanties met minstens één URN-prefix. Zonder prefix schrijft het OMC de URL weg, zoals nu.

> **Let op:** de URN die het OMC schrijft volgt altijd `<prefix>:<resource>:uuid:<id>`. Wil een gemeente het componentsegment (`zrc`, `ztc`) behouden, dan neemt ze dat op in de prefix per instantie. Dat werkt alleen als één instantie één component bedient.

### Calls zonder referentie

Een deel van de calls heeft geen URN of URL om op te resolven: zoeken (bijvoorbeeld een partij op BSN in Open Klant), objecten aanmaken, en het opnieuw opbouwen van een zaak-URL uit alleen een UUID. Voor die calls kiest de registry een instantie in deze volgorde:

1. **De huidige instantie**, als die de benodigde API aanbiedt. De huidige instantie is die van het `hoofdObject` van de notificatie die verwerkt wordt. Een zaak uit `openzaak-sociaal` haalt zijn zaaktype en statussen dus ook daar op.
2. **Een link** vanuit de huidige instantie: `links.klantinteracties` van `openzaak-sociaal` wijst naar `openklant-main`.
3. **De standaardinstantie** voor die API. Heeft een API maar één instantie, dan is die automatisch de standaard.

Omdat de configuratie bij het opstarten afdwingt dat er per API precies één standaard is, levert deze volgorde altijd precies één instantie op.

### Als een URN niet te mappen is

Dat zal regelmatig gebeuren, en het OMC behandelt het als een gewone uitkomst:

* **Optionele gegevens** (bijvoorbeeld de naam van een document of de omschrijving van een zaaktype voor in een template) worden overgeslagen. De notificatie wordt verder verwerkt en het OMC logt een informatieve melding.
* **Verplichte gegevens** (bijvoorbeeld de zaak waar een taak bij hoort, als het scenario die nodig heeft) leveren een **permanente** fout op. Het OMC probeert de notificatie niet opnieuw, want opnieuw proberen verandert niets aan de configuratie. De fout is zichtbaar in het dashboard met de URN en de reden.
* **De geparste URN blijft beschikbaar.** Een `zaaknummer`-URN bevat het zaaknummer al, dus dat kan in een template gebruikt worden zonder de zaak op te halen.
* **Telling per prefix.** Het dashboard toont hoe vaak URN's met een bepaalde prefix niet te mappen waren. Zo ziet een gemeente welke instantie ze nog moet registreren.

Mogelijke redenen:

| Reden | Betekenis |
|---|---|
| `OngeldigeUrn` | De waarde begint met `urn:` maar heeft geen bruikbare opbouw. |
| `GeenInstantie` | Geen enkele instantie heeft een passende URN-prefix. |
| `ResourceNietOndersteund` | De instantie biedt geen API aan voor deze resource. |
| `AttribuutNietOndersteund` | Voor dit attribuut bestaat geen zoekvraag (bijvoorbeeld `zaak:kenmerk:…`). |
| `NietGevonden` | De zoekvraag leverde geen resultaat op. |
| `Meerduidig` | De zoekvraag leverde meer dan één resultaat op. |

BSN- en KvK-URN's (`urn:nld:bsn:nummer:…`, `urn:nld:hr:kvk:nummer:…`) zijn geen verwijzingen naar een registratie-instantie en vallen buiten deze mapping. Die worden zoals nu door `BetrokkeneUrn` afgehandeld.

---

## Aanleveren via de Helm-chart

De chart [`notifynl-omc`](https://github.com/Worth-NL/helm-charts) gebruikt `dh-lib`, dat `configMaps`, `secrets`, `volumes` en `volumeMounts` al uit de values rendert. Er is dus geen wijziging in de chart nodig.

### Configuratie als ConfigMap, secrets als Secret

```yaml
env:
  - name: OMC_INSTANCES_PATH
    value: /config/instances
  - name: OMC_SECRETS_PATH
    value: /config/secrets

configMaps:
  instances:
    data:
      10-zaken.yaml: |
        version: 1
        instances:
          - id: openzaak-main
            type: openzaak
            baseUrl: "https://openzaak-zgw.denhaag.nl"
            default: true
            urnPrefixes: ["urn:nld:denhaag:zaken-main"]
            auth:
              type: jwt
              issuer: "notifynl-omc"
              clientId: "notifynl-omc"
              secret: "secret:openzaak-main-jwt"
      20-klant.yaml: |
        version: 1
        instances:
          - id: openklant-main
            type: openklant
            baseUrl: "https://openklant.denhaag.nl"
            auth:
              type: apiKey
              key: "secret:openklant-main-key"

volumes:
  - name: instances
    configMap:
      name: '{{ include "common.names.fullname" . }}-instances'
  - name: instance-secrets
    secret:
      secretName: omc-instance-secrets
volumeMounts:
  - name: instances
    mountPath: /config/instances
    readOnly: true
  - name: instance-secrets
    mountPath: /config/secrets
    readOnly: true
```

De Secret `omc-instance-secrets` bevat één sleutel per `secret:`-verwijzing:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: omc-instance-secrets
type: Opaque
stringData:
  openzaak-main-jwt: "<jwt-secret>"
  openklant-main-key: "<api-key>"
```

Een extra zaaksysteem toevoegen betekent: één blok in de configuratie en één sleutel in de Secret. Er komen geen nieuwe omgevingsvariabelen bij en de chart blijft ongewijzigd.

### Waar de Secret vandaan kan komen

Voor het OMC maakt het niet uit hoe de bestanden in `OMC_SECRETS_PATH` terechtkomen:

* **Een eigen Secret**, beheerd door de gemeente (bijvoorbeeld via sealed-secrets of GitOps), zoals hierboven.
* **dh-lib `secrets`** in de values.
* **Azure Key Vault via `syncAKV`**, met één Key Vault-object per sleutel. Dat levert één Kubernetes-Secret per sleutel op; die worden met een `projected`-volume in één map samengevoegd:

```yaml
volumes:
  - name: instance-secrets
    projected:
      sources:
        - secret:
            name: '{{ include "app.fullname" . }}-openzaak-main-jwt'
            items: [{ key: secret, path: openzaak-main-jwt }]
        - secret:
            name: '{{ include "app.fullname" . }}-openklant-main-key'
            items: [{ key: secret, path: openklant-main-key }]
```

### Configuratie als los bestand aanleveren

Een gemeente kan de configuratie ook als los bestand bijhouden en meegeven bij de installatie:

```bash
helm upgrade --install omc worth-nl/notifynl-omc -f values.yaml --set-file "configMaps.instances.data.instances\.yaml=./denhaag-instances.yaml"
```

### Herstart bij wijzigingen

De configuratie wordt alleen bij het opstarten ingelezen. Zet een checksum van de configuratie als pod-annotatie, zodat pods herstarten zodra de configuratie verandert:

```yaml
podAnnotations:
  checksum/instances: '{{ .Values.configMaps.instances | toYaml | sha256sum }}'
```

Secrets hoeven niet in de checksum: die worden bij gebruik opnieuw gelezen.

---

## Achterwaartse compatibiliteit

* **`OMC_INSTANCES_PATH` niet gezet:** het OMC bouwt bij het opstarten zelf één instantie per API uit de bestaande variabelen. De basis-URL ontstaat door het vaste pad van `ZGW_ENDPOINT_*` af te halen (`https://openzaak.mijnstad.nl/zaken/api/v1` → `https://openzaak.mijnstad.nl`). Eindigt een endpoint niet op het verwachte pad, dan geeft het OMC een waarschuwing en gebruikt het de waarde zoals die is. Credentials komen uit `ZGW_AUTH_JWT_*` en `ZGW_AUTH_KEY_*`. Voor bestaande installaties verandert er niets.
* **`OMC_INSTANCES_PATH` wel gezet:** `ZGW_ENDPOINT_*` en `ZGW_AUTH_*` worden genegeerd. Zijn ze toch gezet, dan meldt het OMC dat bij het opstarten, zodat er nooit twee bronnen naast elkaar gelden.
* **Buiten deze wijziging:** `OMC_AUTH_JWT_*` (authenticatie van inkomende calls naar het OMC zelf) en de instellingen van NotifyNL, KTO en PostGuard. Dat zijn geen ZGW-registraties.

---

## Impact op de code

Bedoeld voor ontwikkelaars; de genoemde namen zijn een voorstel.

| Onderdeel | Wijziging |
|---|---|
| **`IInstanceRegistry`** (nieuw) | `TryResolveAsync(string reference, ResolutionContext context)` voor URN's en URL's; `TryGetUrn(Uri url, out string urn)`; `GetInstance(ApiType api, ResolutionContext context)` voor calls zonder referentie. Wordt bij het opstarten gebouwd uit de configuratie of uit de legacy-variabelen. |
| **`HttpNetworkService`** | HTTP-clients worden per instantie-`id` aangemaakt en gecachet in plaats van per `HttpClientTypes`. De auth-header komt uit de instantie. |
| **`QueryBase`** (`ProcessGetAsync`, `ProcessPostAsync`, `ProcessGetBinaryAsBase64Async`) | Accepteert een referentie, laat die resolven door de registry en gebruikt de client van de gevonden instantie. Het `HttpClientTypes`-argument verdwijnt of blijft hooguit als hint voor telemetry. |
| **`IDomain.GetDomain()`** (32 aanroepen) | Wordt `GetInstance(api, context)`: de basis-URL komt van de gekozen instantie plus het vaste pad. |
| **`ResolutionContext`** | Houdt de huidige instantie bij (afgeleid van het `hoofdObject`). Hoort bij de verwerking van één notificatie, net als `IQueryBase.Notification`. |
| **`UriExtensions`** | `IsNotCase`, `IsNotCaseType` en verwante controles kijken naar het pad en werken niet op URN's. Ze werken voortaan op de geresolvede referentie of op de resource uit de URN. `RecreateCaseUri` gebruikt de registry in plaats van `ZGW_ENDPOINT_OPENZAAK`. |
| **Modellen** | .NET's `Uri` accepteert `urn:` als absolute URI, dus velden kunnen waarschijnlijk `Uri` blijven. Te verifiëren met `UriJsonConverter`. |
| **`UnresolvableReferenceException`** (nieuw) | Permanente fout voor een verplichte referentie die niet te mappen is. Wordt niet opnieuw geprobeerd en komt met reden in het dashboard. |
| **Schrijfplekken** | Open Klant (onderwerpobject-identificator, `ContactMoment.ReferenceUri`) en uitgaande CloudEvents gebruiken `TryGetUrn`. |
| **Printstraat** | De SSRF-controle op `pdfurl` wordt vervangen door de controle "URL hoort bij een geregistreerde instantie die de Documenten API aanbiedt". |
| **Configuratiecontrole** | `ConfigurationCheckService` en het dashboard tonen de geregistreerde instanties (zonder secrets) en de tellingen van niet-gemapte URN-prefixen. |

---

## Implementatiestappen

Elke stap is een aparte pull request en laat het bestaande gedrag intact.

1. **Registry en configuratie.** `IInstanceRegistry`, het inlezen en valideren van de configuratie, de legacy-instanties uit `ZGW_*` en unit tests. Nog niet aangesloten, dus geen gedragswijziging.
2. **Clients per instantie.** `HttpNetworkService` en `QueryBase` laten resolven via de registry; `IDomain.GetDomain()` vervangen. Met alleen legacy-instanties gedraagt het OMC zich exact als nu.
3. **URN-resolutie.** URN's herkennen en mappen, `UriExtensions` aanpassen, `UnresolvableReferenceException` en de dashboardtellingen.
4. **URL → URN** op de schrijfplekken.
5. **Documentatie en chart-voorbeelden** bijwerken, inclusief `omgevingsvariabelen.md`.

---

## Open punten

* **Vaste paden bevestigen.** Voor Objecttypen komen in de huidige voorbeelden zowel `/api/v1` als `/api/v2` voor, en de paden van OpenVTB (met name voor verzoeken) moeten nog bevestigd worden. Omdat de paden een eis aan klanten worden, moeten ze vastliggen voordat stap 2 klaar is.
* **Zoeken over meerdere instanties.** Moet een vraag als "alle zaken van deze persoon" naar alle instanties van een API gaan, of alleen naar de standaardinstantie?
* **Meerdere Open Notificaties-instanties.** Inkomende notificaties worden via het `hoofdObject` aan de juiste instantie gekoppeld. Nog te bepalen is wie de abonnementen per bron beheert en welke callback-authenticatie per bron geldt.
* **Cacheduur van zoekvragen.** Hoe lang mag de uitkomst van een zaaknummer- of zaaktype-zoekvraag bewaard worden?
* **Uniforme URN-notatie.** De afwijkingen tussen gemeenten (componentsegment, BSN-notatie) moeten per gemeente in de URN-prefixen worden opgevangen. Een gedeeld overzicht van gebruikte prefixen per gemeente voorkomt verrassingen.
* **`.well-known/systems`.** Als gemeenten hun systemen op die manier gaan publiceren, kan dat later een bron voor de instanties worden. De configuratie hierboven bevat alleen gegevens en geen code, zodat dat zonder grote wijzigingen kan.
