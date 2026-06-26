# Werkwijze versies

Het OMC ondersteunt twee werkwijze versies die bepalen hoe burgercontactgegevens worden opgehaald en welke ZGW API's worden gebruikt.

---

## Vergelijking v1 vs. v2

| Eigenschap | Versie 1 | Versie 2 |
|---|---|---|
| OpenKlant versie | OpenKlant v1 | OpenKlant v2 |
| Ondersteunde klanttypen | Alleen BSN (burger) | BSN (burger) + KVK (organisatie) |
| Contactmomenten API | Contactmomenten v1 | Klantinteracties v2 |
| OpenKlant authenticatie | Geen API-sleutel vereist | `ZGW_AUTH_KEY_OPENKLANT` vereist |
| Status | Stabiel, legacy | Huidig, aanbevolen |

---

## Configureren

Stel de gewenste versie in via:

```
OMC_FEATURE_WORKFLOW_VERSION=2
```

Geldige waarden: `1` of `2`. Standaard: `2`.

---

## Breaking changes van v1 naar v2

- **OpenKlant API-sleutel vereist** — voeg `ZGW_AUTH_KEY_OPENKLANT` toe
- **Contactmomenten eindpunt gewijzigd** — `ZGW_ENDPOINT_CONTACTMOMENTEN` wijst nu naar de Klantinteracties v2 API
- **KVK-ondersteuning** — burgers met een KVK-nummer in plaats van BSN worden nu verwerkt
- **Klantinteracties schrijfformaat** — het formaat van de teruggeschreven contactmomenten is gewijzigd

> Raadpleeg de [Changelog](../project/changelog.md) voor de volledige lijst van wijzigingen per release.

---

## Welke versie moet ik gebruiken?

Gebruik **versie 2** tenzij je bestaande OpenKlant v1-infrastructuur hebt die je nog niet kunt migreren. Alle nieuwe installaties dienen versie 2 te gebruiken.
