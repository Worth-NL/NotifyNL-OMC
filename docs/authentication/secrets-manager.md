# Secrets Manager

The Secrets Manager is a small utility included in the OMC repository that generates JWT tokens. It reads credentials from environment variables (or a launch profile) and outputs a ready-to-use token.

It is available as both a standalone executable (`OMC.SecretsManager.exe`) and a .NET library (`SecretsManager.dll`) that can be referenced from other projects.

---

## Usage — standalone executable

Navigate to the build output directory:

```
cd [...]\NotifyNL-OMC\OMC\Core\Domain\SecretsManager\bin\Debug\net10.0
```

> **Note:** Use CMD, not PowerShell.

### Default mode (60-minute token)

```cmd
OMC.SecretsManager.exe
```

Generates a token valid for 60 minutes from now.

### Valid for N minutes

```cmd
OMC.SecretsManager.exe 75
```

Generates a token valid for 75 minutes.

### Valid until a specific datetime

```cmd
OMC.SecretsManager.exe 2025-12-31T23:59:59
```

Generates a token valid until 31 December 2025 at 23:59:59 local time (converted to UTC internally).

Any valid .NET `DateTime` string format is accepted. See [standard date and time format strings](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings).

### Output

The tool writes a `token.json` file containing the generated JWT token. For asymmetric encryption, it also writes a `private_key` file containing the RSA private key.

---

## Required environment variables

The Secrets Manager reads `OMC_AUTH_JWT_*` variables from the environment (or from a `launchSettings.json` profile if running from Visual Studio):

```json
{
  "profiles": {
    "SecretsManager": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Production",
        "OMC_AUTH_JWT_SECRET": "",
        "OMC_AUTH_JWT_ISSUER": "",
        "OMC_AUTH_JWT_AUDIENCE": "",
        "OMC_AUTH_JWT_EXPIRESINMIN": "",
        "OMC_AUTH_JWT_USERID": "OMC",
        "OMC_AUTH_JWT_USERNAME": "OMC"
      }
    }
  }
}
```

---

## Usage — as a .NET library

Add a reference to `SecretsManager.csproj` or include the `.dll`, then use the Strategy Design Pattern:

### Direct usage

```csharp
var strategy = new SymmetricEncryptionStrategy();
var context = new EncryptionContext(strategy);

SecurityKey key = context.GetSecurityKey("your-secret");
string jwtToken = context.GetJwtToken(key, ...);
```

### With dependency injection

```csharp
// Register the encryption strategy (symmetric or asymmetric — not both)
builder.Services.AddSingleton(typeof(IJwtEncryptionStrategy),
    builder.Configuration.GetValue<bool>("Encryption:IsAsymmetric")
        ? typeof(AsymmetricEncryptionStrategy)
        : typeof(SymmetricEncryptionStrategy));

// Register the context
builder.Services.AddSingleton<EncryptionContext>();
```

---

## Encryption methods

### Symmetric (default)

Uses a shared secret (`OMC_AUTH_JWT_SECRET`) to sign and verify tokens. The same secret must be present on both the issuing side (Secrets Manager) and the receiving side (OMC). Output: `token.json`.

### Asymmetric (RSA)

Generates a randomised RSA private key. The public key is embedded in the JWT; the private key must be configured on the API side. Output: `token.json` + `private_key`. Enable via `"Encryption:IsAsymmetric": true` in `appsettings.json`.
