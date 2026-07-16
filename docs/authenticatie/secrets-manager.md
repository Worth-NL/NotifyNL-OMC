# Secrets Manager

De Secrets Manager is een apart uitvoerbaar hulpprogramma dat JWT-tokens genereert die compatibel zijn met het OMC. Het kan ook worden gebruikt als een .NET-bibliotheek in je eigen code.

---

## Gebruik als zelfstandig uitvoerbaar bestand

Het uitvoerbare bestand is beschikbaar in de OMC-release-artefacten als `SecretsManager` (Linux/macOS) of `SecretsManager.exe` (Windows).

### Standaardmodus — verloopt na geconfigureerde minuten

```bash
./SecretsManager
```

Genereert een token dat verloopt na het aantal minuten ingesteld in `OMC_AUTH_JWT_EXPIRESINMIN`.

### Minutenmodus — aangepaste levensduur

```bash
./SecretsManager --minutes 1440
```

Genereert een token dat verloopt na het opgegeven aantal minuten.

### Datetime-modus — vaste vervaldatum

```bash
./SecretsManager --datetime "2030-01-01T00:00:00"
```

Genereert een token dat verloopt op de opgegeven datum en tijd. Gebruik dit voor lang levende tokens die worden gebruikt in NotifyNL callback-configuratie (Stap 5).

---

## Gebruik als .NET-bibliotheek

De Secrets Manager is beschikbaar als NuGet-pakket voor gebruik in je eigen .NET-toepassingen.

### Installatie

```bash
dotnet add package Worth.SecretsManager
```

### Symmetrische JWT genereren (HS256)

```csharp
using Worth.SecretsManager;

var generator = new JwtGenerator(new JwtOptions
{
    Secret = "jouw-geheime-sleutel",
    Issuer = "jouw-uitgever",
    Audience = "jouw-doelgroep",
    ExpiresInMin = 60,
    UserId = "gebruiker@voorbeeld.nl",
    UserName = "Gemeente Rotterdam"
});

string token = generator.GenerateToken();
```

### Asymmetrische JWT genereren (RS256)

```csharp
using Worth.SecretsManager;

var generator = new JwtGenerator(new JwtOptions
{
    PrivateKey = File.ReadAllText("private.pem"),
    Issuer = "jouw-uitgever",
    Audience = "jouw-doelgroep",
    ExpiresInMin = 60,
    UserId = "gebruiker@voorbeeld.nl",
    UserName = "Gemeente Rotterdam"
});

string token = generator.GenerateToken();
```

---

## Symmetrisch vs. asymmetrisch

| Eigenschap | Symmetrisch (HS256) | Asymmetrisch (RS256) |
|---|---|---|
| Geheim type | Gedeeld geheim | RSA privésleutel + publieke sleutel |
| Configuratie | `OMC_AUTH_JWT_SECRET` | `OMC_AUTH_JWT_PRIVATEKEYPATH` |
| Aanbevolen voor | Intern/enkel-omgeving gebruik | Productie, multi-tenant |
| Sleutelrotatie | Één geheim bijwerken | Privésleutel bijwerken, publieke sleutel verspreiden |
