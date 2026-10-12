# URN-mapping en meerdere registraties

Status: voorstel, nog niet gebouwd. Namen van instellingen en klassen kunnen nog veranderen.

## Waarom

Registraties gaan steeds vaker naar elkaar verwijzen met een URN in plaats van een URL. Het formaat dat binnen het Platform voor Dienstverlening is afgesproken ziet er zo uit:

```
urn:nld:<organisatie>:<systeem>:<resource>:<attribuut>:<identificatie>
```

Bijvoorbeeld `urn:nld:denhaag:zaken-sociaal:zaak:uuid:3df59f4f-ffa0-4caf-8a15-eeb19afffa52`.

Zo'n URN zegt wel wát iets is, maar niet waar het staat. En de OMC moet die dingen wel ophalen: de zaak bij een taak, het document bij een besluit, het product bij een notificatie. Voor elke URN moeten we dus kunnen uitzoeken bij welke registratie hij hoort, wat de URL is en met welke credentials we daar binnenkomen.

Daar komt bij dat een gemeente van elke registratie meer dan één installatie kan hebben. Denk aan twee zaaksystemen die allebei zaken in hetzelfde veld van een taak zetten. De OMC kan nu per registratie maar één endpoint en één set credentials aan (`ZGW_ENDPOINT_*` en `ZGW_AUTH_*`), dus dat gaat niet meer werken.

En de URN's zelf zijn ook nog niet overal hetzelfde. Den Haag zet er bijvoorbeeld een extra stuk voor het component in (`urn:nld:denhaag:zaken-main:zrc:zaak:uuid:…`), en voor BSN's komen we zowel `urn:nld:brp:bsn:nummer:…` als `urn:nld:overheid:brp:np:bsn:…` tegen. Vervelend, maar het is niet anders. We moeten het daarom per gemeente kunnen instellen, in plaats van één vaste indeling af te dwingen.

## Uitgangspunten

- Het API-pad is per soort registratie altijd hetzelfde. Alleen waar een registratie staat verschilt. Dat is een eis aan onze klanten: een Open Zaak biedt de Zaken API altijd aan op `<basis-URL>/zaken/api/v1`.
- Van elke registratie kunnen meerdere installaties bestaan. Die leggen we vast in configuratie, niet in code.
- Elke URN die we tegenkomen proberen we om te zetten naar een URL. Dat gaat vaak niet lukken, en dat is prima. Het is een normale uitkomst en geen crash.
- Credentials horen bij een installatie en staan in dezelfde configuratie. Geheime waarden komen nooit in een ConfigMap.
- Wie niets verandert, merkt niets. Zonder de nieuwe configuratie werkt alles zoals nu.

Een paar woorden die hieronder steeds terugkomen. Een type is het soort registratie, zoals `openzaak` of `openklant`. Het type bepaalt welke API's er zijn en op welke paden. Een instantie is één concrete installatie van zo'n type, met een eigen basis-URL en eigen credentials. Een URN-prefix is het begin van een URN dat bij een instantie hoort, zoals `urn:nld:denhaag:zaken-main`. En een referentie is een waarde in een veld die naar iets anders verwijst, een URL of een URN.

## Configuratie

De OMC leest de instanties uit `OMC_INSTANCES_PATH`. Dat mag één YAML-bestand zijn of een map met YAML-bestanden, die dan op alfabetische volgorde worden ingelezen. Staat deze variabele niet, dan gebruikt de OMC gewoon de bestaande `ZGW_ENDPOINT_*` en `ZGW_AUTH_*`.

Daarnaast is er `OMC_SECRETS_PATH`. Dat is een map waarin elk secret als los bestand staat, precies zoals Kubernetes een Secret als volume mount. In de configuratie verwijs je naar een secret met `secret:<naam>`, en de OMC leest dan het bestand `<OMC_SECRETS_PATH>/<naam>`.

Alles wordt één keer bij het opstarten ingelezen en gecontroleerd.

### Voorbeeld

Zo zou het er voor Den Haag uit kunnen zien, met twee zaaksystemen en een losse Documenten API:

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
      issuer: "notifynl-omc-sociaal"
      secret: "secret:openzaak-sociaal-jwt"
    links:
      klantinteracties: openklant-main

  # Losse Documenten API op een andere host
  - id: dms
    type: documenten
    baseUrl: "https://dms.denhaag.nl"
    urnPrefixes:
      - "urn:nld:denhaag:dms"
    auth:
      type: jwt
      issuer: "notifynl-omc"
      secret: "secret:dms-jwt"

  - id: openklant-main
    type: openklant
    baseUrl: "https://openklant.denhaag.nl"
    auth:
      type: apiKey
      key: "secret:openklant-main-key"

  - id: openvtb-main
    type: openvtb
    baseUrl: "https://openvtb-zgw.denhaag.nl"
    urnPrefixes:
      - "urn:nld:denhaag:vtb"
    auth:
      type: apiKey
      key: "secret:openvtb-main-key"

  - id: opennotificaties-main
    type: opennotificaties
    baseUrl: "https://opennotificaties.denhaag.nl"
    auth:
      type: jwt
      issuer: "notifynl-omc"
      secret: "secret:openzaak-main-jwt"   # hetzelfde secret mag bij meer instanties

  # Objecten, Objecttypen en Open Producten gaan op dezelfde manier
```

### Wat je per instantie opgeeft

- `id` is een unieke naam. Die gebruiken we in `links`, in de logs en in het dashboard.
- `type` is het soort registratie, zie de vaste paden hieronder.
- `baseUrl` is waar de instantie staat, zonder het API-pad. Een pad-prefix mag wel, bijvoorbeeld als de instantie achter een gateway hangt (`https://api.gemeente.nl/openzaak`).
- `default` zegt of dit de standaard is voor de API's van dit type. Is er van een API maar één instantie, dan is die vanzelf de standaard.
- `urnPrefixes` zijn de URN's die bij deze instantie horen. Zonder prefixen koppelen we nooit een URN aan deze instantie, URL's wel. De eerste prefix gebruiken we als we zelf een URN moeten maken.
- `links` zegt welke andere instantie bij objecten uit deze instantie hoort, bijvoorbeeld welke Open Klant bij een zaaksysteem hoort.
- `auth` zegt hoe we inloggen.

Voor `auth` zijn er twee smaken. `jwt` werkt zoals we het nu bij Open Zaak doen: je geeft een `issuer` op (in Open Zaak is dat de client-ID) en eventueel `audience`, `userId`, `userName` en `expiresInMin`, plus het `secret`. `apiKey` werkt zoals nu bij Open Klant, Objecten, OpenVTB en Open Producten, met de header `Authorization: Token <key>`, en heeft alleen een `key`. Voor lokaal ontwikkelen is er ook nog `none`.

Alleen `secret` en `key` zijn echt geheim, en daar accepteren we alleen een verwijzing als `secret:<naam>`. Een letterlijke waarde weigeren we, zodat niemand per ongeluk een sleutel in een ConfigMap of in git zet. De rest is niet geheim en staat gewoon in de config.

We lezen secrets pas bij gebruik uit het bestand, met een korte cache. Kubernetes ververst een gemounte Secret vanzelf, dus je kunt een sleutel roteren zonder de OMC te herstarten. Mount dan wel de hele map en niet met `subPath`, want dan ververst hij niet.

### Controle bij het opstarten

Bij het opstarten controleren we de hele configuratie. Klopt er iets niet, dan start de OMC niet en krijg je alle fouten in één keer te zien, met het bestand en de instantie erbij. We kijken onder andere naar dubbele id's, onbekende types, een `baseUrl` die geen goede URL is of al op een API-pad eindigt, `secret:`-verwijzingen zonder bestand, letterlijke secrets, `links` naar instanties die niet bestaan of die API niet hebben, URN-prefixen die bij twee instanties staan, en API's met meerdere instanties waar niet precies één standaard tussen zit.

## Vaste paden

De paden staan in de code en zijn voor elke instantie van een type hetzelfde. In de configuratie staat alleen de basis-URL.

| Type | API | Pad | Resources in een URN |
|---|---|---|---|
| `openzaak` | zaken | `/zaken/api/v1` | zaak, status, rol, resultaat, zaakinformatieobject |
| | catalogi | `/catalogi/api/v1` | zaaktype, statustype, resultaattype, roltype, informatieobjecttype, besluittype |
| | documenten | `/documenten/api/v1` | enkelvoudiginformatieobject |
| | besluiten | `/besluiten/api/v1` | besluit, besluitinformatieobject |
| `openklant` | klantinteracties | `/klantinteracties/api/v1` | partij, betrokkene, klantcontact |
| `objecten` | objecten | `/api/v2` | object |
| `objecttypen` | objecttypen | `/api/v2` (nog bevestigen) | objecttype |
| `openvtb` | taken | `/taken/api/v1` | externetaak |
| | berichten | `/berichten/api/v1` | bericht |
| | verzoeken | `/verzoeken/api/v1` (nog bevestigen) | verzoek, verzoektype |
| `openproducten` | producten | `/producten/api/v1` | product, producttype |
| `opennotificaties` | notificaties | `/api/v1` | |

`openzaak` is een afkorting voor een instantie die alle vier de Open Zaak-API's op dezelfde basis-URL heeft. Staat bijvoorbeeld de Documenten API ergens anders, dan registreer je die apart met `type: documenten` (zoals `dms` in het voorbeeld). Dat kan ook met `zaken`, `catalogi` en `besluiten`.

Welke collectie bij een resource hoort (`zaak` hoort bij `/zaken`, `externetaak` bij `/externetaken`) staat ook in de code.

## Hoe we een referentie oplossen

Alles loopt via één onderdeel, de instance registry. De code die iets ophaalt zegt alleen wat ze verwacht, bijvoorbeeld een zaak. De registry zoekt uit waar dat staat en met welke credentials we het ophalen.

### Van URN naar URL

Eerst zoeken we de instantie. De langste URN-prefix die past wint. Een prefix moet wel bij een `:` ophouden, dus `urn:nld:denhaag:zaken` past niet op `urn:nld:denhaag:zaken-main:…`. Hoofdletters maken niet uit, behalve in de identificatie zelf.

Daarna zoeken we in de rest van de URN het eerste stuk dat een resource is die deze instantie kent. Wat daarvoor staat, zoals `zrc` of `ztc` bij Den Haag, slaan we over. Na de resource komt het attribuut en dan de identificatie. Staat er maar één stuk na de resource, dan gaan we uit van `uuid`.

Bij een `uuid` bouwen we de URL gewoon op: basis-URL, vast pad, collectie en uuid. Bij een ander attribuut doen we een vaste zoekvraag op die instantie. Voorlopig zijn dat er twee: een zaak op zaaknummer (`GET /zaken/api/v1/zaken?identificatie=…`) en een zaaktype op identificatie (`GET /catalogi/api/v1/zaaktypen?identificatie=…`). Let wel op dat een zaak-identificatie alleen uniek is binnen een bronorganisatie. Komt er meer dan één zaak terug, dan noemen we de URN meerduidig. Van een zaaktype kunnen meerdere versies dezelfde identificatie hebben, en dan pakken we de versie die nu geldt. De uitkomst van zo'n zoekvraag bewaren we even, zodat we niet bij elke stap opnieuw zoeken.

Met de config van hierboven gaat dat zo:

```
urn:nld:denhaag:zaken-main:zrc:zaak:uuid:3df5…
  -> https://openzaak-zgw.denhaag.nl/zaken/api/v1/zaken/3df5…

urn:nld:denhaag:zaken-sociaal:zaak:3df5…
  -> https://zaken-sociaal.denhaag.nl/zaken/api/v1/zaken/3df5…

urn:nld:denhaag:zaken-main:ztc:zaaktype:uuid:7c1e…
  -> https://openzaak-zgw.denhaag.nl/catalogi/api/v1/zaaktypen/7c1e…

urn:nld:denhaag:vtb:taken:externetaak:uuid:a12b…
  -> https://openvtb-zgw.denhaag.nl/taken/api/v1/externetaken/a12b…

urn:nld:denhaag:zaken-main:zrc:zaak:zaaknummer:ZAAK-2026-0001
  -> zoekvraag op openzaak-main

urn:nld:rotterdam:zaken:zaak:uuid:…
  -> lukt niet, geen instantie met deze prefix
```

### Van URL naar instantie

Ook gewone URL's gaan via de registry. Een URL hoort bij de instantie met de langste `baseUrl` die erop past. Dat geldt ook voor URL's die we uit antwoorden volgen, zoals van zaak naar zaaktype naar statustype, of van zaak naar documenten. Die komen zo vanzelf bij de goede instantie en credentials uit, ook als de Catalogi API op een andere host staat dan de Zaken API.

Een URL die bij geen enkele instantie hoort, halen we niet op. Onze credentials gaan dus nooit naar een host die we niet kennen. Dat vervangt de controle die we nu al in de [printstraat](../werkwijzen/scenarios/print-printstraat.md) hebben, waar de `pdfurl` naar `ZGW_ENDPOINT_DOCUMENTEN` moet wijzen, en trekt die door naar alles.

### Van URL naar URN

Als we zelf een verwijzing wegschrijven, bijvoorbeeld bij een klantcontact in Open Klant of in een CloudEvent dat we versturen, kunnen we een URL omzetten naar een URN. Dat wordt dan `<eerste urnPrefix>:<resource>:uuid:<uuid>`. Bijvoorbeeld `https://zaken-sociaal.denhaag.nl/zaken/api/v1/zaken/3df5…` wordt `urn:nld:denhaag:zaken-sociaal:zaak:uuid:3df5…`. Heeft een instantie geen URN-prefix, dan schrijven we gewoon de URL weg, zoals nu.

Wil een gemeente het componentstuk (`zrc`, `ztc`) in haar URN's houden, dan zet ze dat in de prefix. Dat werkt alleen als een instantie maar één component bedient.

### Calls zonder referentie

Soms hebben we niets om op te lossen. Bijvoorbeeld als we een partij op BSN zoeken in Open Klant, iets aanmaken, of een zaak-URL opnieuw moeten opbouwen uit alleen een UUID. Dan kiest de registry een instantie in deze volgorde. Eerst de huidige instantie, als die de API heeft. De huidige instantie is die van het `hoofdObject` van de notificatie waar we mee bezig zijn, dus een zaak uit `openzaak-sociaal` haalt zijn zaaktype en statussen ook daar op. Daarna kijken we naar de `links` van de huidige instantie. En anders nemen we de standaard voor die API. Omdat we bij het opstarten afdwingen dat er per API precies één standaard is, komt hier altijd precies één instantie uit.

### Als een URN niet lukt

Dat gaat regelmatig gebeuren, en dat is geen probleem. Gaat het om iets wat we er alleen bij willen hebben, zoals de naam van een document of de omschrijving van een zaaktype voor in een template, dan slaan we het over, loggen we het en gaan we verder met de notificatie.

Gaat het om iets wat we echt nodig hebben, zoals de zaak waar een taak bij hoort, dan is dat een blijvende fout. Opnieuw proberen heeft geen zin, want dat verandert niets aan de configuratie. De fout staat dan in het dashboard, met de URN en de reden. Die reden is een van deze: de URN klopt niet (`OngeldigeUrn`), er is geen instantie met die prefix (`GeenInstantie`), de instantie kent die resource niet (`ResourceNietOndersteund`), we hebben geen zoekvraag voor dat attribuut (`AttribuutNietOndersteund`), de zoekvraag vond niets (`NietGevonden`) of de zoekvraag vond meer dan één ding (`Meerduidig`).

De URN zelf houden we wel altijd bij de hand. In een zaaknummer-URN staat het zaaknummer al, dus dat kan in een template ook zonder dat we de zaak ophalen. En het dashboard telt per prefix hoe vaak een URN niet lukte, zodat een gemeente ziet welke instantie ze nog moet toevoegen.

BSN- en KvK-URN's zoals `urn:nld:bsn:nummer:…` en `urn:nld:hr:kvk:nummer:…` verwijzen niet naar een registratie en vallen hier buiten. Die handelt `BetrokkeneUrn` af, net als nu.

## Aanleveren via de Helm-chart

De chart [`notifynl-omc`](https://github.com/Worth-NL/helm-charts) gebruikt `dh-lib`, en die kan `configMaps`, `secrets`, `volumes` en `volumeMounts` al uit de values maken. Aan de chart hoeft dus niets te veranderen.

De instanties zet je in een ConfigMap en de sleutels in een Secret:

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
            urnPrefixes: ["urn:nld:denhaag:zaken-main"]
            auth:
              type: jwt
              issuer: "notifynl-omc"
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

In de Secret staat per `secret:`-verwijzing één sleutel:

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

Een zaaksysteem erbij betekent dus één blok in de config en één sleutel in de Secret. Geen nieuwe omgevingsvariabelen en niets aan de chart.

Waar die Secret vandaan komt maakt voor de OMC niet uit. Je kunt hem zelf beheren (met sealed-secrets of GitOps bijvoorbeeld), hem via de `secrets` van dh-lib in de values zetten, of per sleutel uit Azure Key Vault halen met `syncAKV`. In dat laatste geval krijg je per sleutel een eigen Kubernetes-Secret, en die voeg je met een `projected`-volume samen in één map:

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

Je kunt de config ook als los bestand bijhouden en meegeven bij de installatie:

```bash
helm upgrade --install omc worth-nl/notifynl-omc -f values.yaml --set-file "configMaps.instances.data.instances\.yaml=./denhaag-instances.yaml"
```

De config lezen we alleen bij het opstarten. Zet daarom een checksum als pod-annotatie, dan herstart de pod vanzelf als de config verandert. Voor de secrets hoeft dat niet, die lezen we bij gebruik opnieuw.

```yaml
podAnnotations:
  checksum/instances: '{{ .Values.configMaps.instances | toYaml | sha256sum }}'
```

## Bestaande installaties

Staat `OMC_INSTANCES_PATH` niet, dan maakt de OMC bij het opstarten zelf per API een instantie uit de bestaande variabelen. De basis-URL krijgen we door het vaste pad van `ZGW_ENDPOINT_*` af te halen, dus `https://openzaak.mijnstad.nl/zaken/api/v1` wordt `https://openzaak.mijnstad.nl`. Eindigt een endpoint niet op het pad dat we verwachten, dan krijg je een waarschuwing en gebruiken we de waarde zoals hij is. De credentials komen dan gewoon uit `ZGW_AUTH_JWT_*` en `ZGW_AUTH_KEY_*`.

Staat `OMC_INSTANCES_PATH` wel, dan negeren we `ZGW_ENDPOINT_*` en `ZGW_AUTH_*`. Staan ze er toch nog, dan zeggen we dat bij het opstarten, zodat er nooit twee bronnen door elkaar lopen.

`OMC_AUTH_JWT_*` (voor calls naar de OMC zelf) en de instellingen voor NotifyNL, KTO en PostGuard blijven zoals ze zijn. Dat zijn geen ZGW-registraties.

## Wat er in de code verandert

Voor de ontwikkelaars, met namen die nog kunnen veranderen.

- Er komt een `IInstanceRegistry` met `TryResolveAsync(reference, context)` voor URN's en URL's, `TryGetUrn(url, out urn)` voor de andere kant op, en `GetInstance(api, context)` voor calls zonder referentie. Die wordt bij het opstarten gebouwd uit de config of uit de oude variabelen.
- `HttpNetworkService` maakt en bewaart HTTP-clients per instantie in plaats van per `HttpClientTypes`. De auth-header komt van de instantie.
- `QueryBase` laat elke referentie eerst door de registry oplossen en gebruikt dan de client van die instantie. Het `HttpClientTypes`-argument kan daar weg, of blijft hooguit voor telemetry.
- `IDomain.GetDomain()` (32 aanroepen) wordt `GetInstance(api, context)`: basis-URL van de gekozen instantie plus het vaste pad.
- Een `ResolutionContext` houdt de huidige instantie bij, afgeleid van het `hoofdObject`. Die hoort bij de verwerking van één notificatie, net als `IQueryBase.Notification`.
- De checks in `UriExtensions` (`IsNotCase`, `IsNotCaseType` enzovoort) kijken naar het pad en werken niet op URN's. Die gaan werken op de opgeloste referentie of op de resource uit de URN. `RecreateCaseUri` gebruikt de registry in plaats van `ZGW_ENDPOINT_OPENZAAK`.
- .NET's `Uri` slikt `urn:` als absolute URI, dus de modellen kunnen waarschijnlijk `Uri` blijven. Dat moeten we nog checken met `UriJsonConverter`.
- Er komt een `UnresolvableReferenceException` voor een URN die we echt nodig hebben maar niet kunnen oplossen. Die proberen we niet opnieuw en die komt met reden in het dashboard.
- Open Klant (de onderwerpobject-identificator en `ContactMoment.ReferenceUri`) en de CloudEvents die we versturen gebruiken `TryGetUrn`.
- De SSRF-check op `pdfurl` in de printstraat wordt "de URL hoort bij een bekende instantie met de Documenten API".
- `ConfigurationCheckService` en het dashboard laten de instanties zien (zonder secrets) en tellen de URN's die niet lukten.

## Stappen

Elke stap is een eigen pull request, en na elke stap werkt alles nog zoals het werkte.

1. De registry en de config: inlezen, controleren, de oude variabelen omzetten naar instanties, en tests. Nog nergens aangesloten.
2. Clients per instantie: `HttpNetworkService` en `QueryBase` via de registry, en `IDomain.GetDomain()` vervangen. Met alleen de oude variabelen doet de OMC precies hetzelfde als nu.
3. URN's: herkennen en omzetten, `UriExtensions` aanpassen, de nieuwe exception en de tellingen in het dashboard.
4. URL naar URN op de plekken waar we iets wegschrijven.
5. Documentatie en chart-voorbeelden bijwerken, ook `omgevingsvariabelen.md`.

## Open punten

- De vaste paden moeten vastliggen voordat stap 2 af is. Bij Objecttypen zien we nu zowel `/api/v1` als `/api/v2` in de voorbeelden, en de paden van OpenVTB (vooral voor verzoeken) moeten nog bevestigd worden.
- Moet een vraag als "alle zaken van deze persoon" naar alle zaaksystemen gaan, of alleen naar de standaard?
- Als er meerdere Open Notificaties zijn: inkomend komt het goed via het `hoofdObject`, maar wie regelt de abonnementen per bron, en welke callback-auth hoort bij welke bron?
- Hoe lang mogen we de uitkomst van een zoekvraag op zaaknummer of zaaktype bewaren?
- De verschillen in URN-notatie tussen gemeenten vangen we op met de prefixen per instantie. Een gedeeld overzicht van welke gemeente welke prefixen gebruikt zou verrassingen schelen.
- Als gemeenten hun systemen ooit op een `.well-known/systems` publiceren, kan dat later de bron van de instanties worden. De config is alleen data, dus dat kan zonder grote verbouwing.

## TL;DR: overstappen

Heeft een gemeente van elke registratie maar één installatie en gebruikt ze geen URN's? Dan hoeft ze niets te doen. Zonder `OMC_INSTANCES_PATH` werkt de OMC precies zoals nu.

Wil een gemeente wel over, dan moet ze dit doen:

1. Een lijstje maken van al haar registraties: elke Open Zaak, Open Klant, Objecten, Objecttypen, OpenVTB, Open Producten en Open Notificaties. Per stuk de basis-URL, dus zonder `/zaken/api/v1` en dergelijke.
2. Checken dat elke installatie haar API's op de standaardpaden aanbiedt. Is dat niet zo, dan moet dat eerst gefixt worden.
3. Per installatie uitzoeken welke URN-prefix er in haar registraties voor wordt gebruikt.
4. Waar ze van een API meer dan één installatie heeft, kiezen welke de standaard is, en per zaaksysteem aangeven welke Open Klant erbij hoort.
5. Zorgen dat de OMC in elke installatie als applicatie bekend is met de goede rechten, en per installatie een JWT-secret of API-key regelen.
6. De sleutels in één Kubernetes Secret zetten, of per sleutel in Key Vault.
7. De instanties in een ConfigMap zetten en beide koppelen in de values, met `OMC_INSTANCES_PATH` en `OMC_SECRETS_PATH`.
8. `ZGW_ENDPOINT_*` en `ZGW_AUTH_*` uit de values halen. Ze doen niets meer en de OMC waarschuwt erover.
9. Zorgen dat de Open Notificaties van elk zaaksysteem notificaties naar de OMC stuurt.
10. Uitrollen en het opstartlog en het dashboard checken. Start de OMC niet, dan staat in het log precies wat er mis is.

Wat een gemeente aanlevert, nu en straks:

| | Nu | Straks |
|---|---|---|
| Waar registraties staan | Eén `ZGW_ENDPOINT_*` per API, met het API-pad erin | Per instantie een `baseUrl` zonder API-pad, zoveel instanties als nodig |
| Credentials | Eén gedeelde JWT (`ZGW_AUTH_JWT_*`) en per soort één API-key (`ZGW_AUTH_KEY_*`), als losse omgevingsvariabelen | Per instantie een `auth`-blok in de config, met de geheime delen in één Secret |
| URN's | Niet mogelijk | URN-prefixen per instantie |
| Meer dan één zaaksysteem | Niet mogelijk | Een standaard per API en `links` tussen instanties |
| Omgevingsvariabelen | Zo'n twintig `ZGW_*`-variabelen | Twee: `OMC_INSTANCES_PATH` en `OMC_SECRETS_PATH` |

Nieuw voor gemeenten is dus vooral: de URN-prefixen, kiezen wat de standaard is en wat bij elkaar hoort, en de OMC als applicatie toevoegen in elke extra installatie. Wat wegvalt zijn de losse omgevingsvariabelen per endpoint en per sleutel.
