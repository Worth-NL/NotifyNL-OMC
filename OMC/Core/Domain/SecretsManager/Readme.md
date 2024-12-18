# Secrets Manager

The purpose of this application is to generate JWT tokens with:
a) internal pre-defined parameters (claims): **NotifyNL.SecretsManager.exe**
b) externally passed parameters (claims): **NotifyNL.SecretsManager.dll**

### 1.1. JWT structure

As a reminder, the JWT Token consists of 3 main components:
- **Header** (with type of token, and type of used encryption algorithm)
> {
  "alg": "HS256",
  "typ": "JWT"
}

- **Payload** (with JWT claims, used for enhanced token validation)
> {
  "iat": 1697183664,
  "exp": 1697187264,
  "iss": "My API",
  "aud": "Your API"
}

- **Verify signature** (encoded* Header + Payload + Secret**)

\* Using the provided encryption algorithm
\** Password or public key (asymmetric verification)

### 1.2. Claims

The currently supported JWT claims are:
- **Issuer** ("iss")
- **Audience** ("aud")
- **IssuedAt** ("iat")
- **Expires** ("exp")
- **Subject** (Claims Identities)
    - "client_id"
    - "user_id"
    - "user_representation"

Remarks:
- **Not Before** ("nbf") claim will be always ignored. It's not used in any business case
- **Audience** ("aud") will be ignored if the passed value for audience is *null* or *empty*

### 1.3. Encryption methods

The supported options are:

- **Symmetric Encryption**

The application will use "secret" (password) which will be encrypted using specific encryption algorithm. The result will be symmetric **SecurityKey** stored in the *verify signature* of the JWT token. The same "secret" have to be present on the API side (e.g., in configuration files such as: private `secrets.json` or public server-side-only `appsettings.json`). Based on its own "secret" the API will generate another **SecurityKey** internally and then compare it with received JWT token to match if secrets (and expected JWT claims) are identical. The current implementation is supporting *long* (preferred) and *short* passwords (not recommended), shorter than *64 bytes*.

This strategy will produce: `token.json` file (with JWT token inside)

- **Asymmetric Encryption**

The authentication process will be quite similar except the way how the JWT "secret" is generated. Instead of accepting a specific "password" as an input argument, the Asymmetric strategy will generate a randomized RSA *private key*. In the JWT token only the *public key* will be saved as part of an asymmetric RSA **SecurityKey**. At the same time, on the API side the *private key* should be present (like you did in *GitHub* or *GitLab* repositories when configuring SSH protocol to communicate with your repository using secure session from the terminal of your choice). After receiving a JWT token, the target API will compare its *public key* with an existing *private key*. The authentication will of course consider the expected JWT claims.

This strategy will produce: `token.json` (with JWT token inside) and `private_key` file (with RSA key inside)

### 1.4. Architecture

To achieve this result the Strategy Design Pattern was implemented internally.

---
### 2. How to use the application:

#### 2.1. Internally

- Navigate to `cd [...] \NotifyNL-OMC\OMC\Core\Domain\SecretsManager\bin\Debug\net8.0`

---
###### A) Default mode

- Double click on **NotifyNL.SecretsManager.exe**

**Result:** The produced JWT token will be valid for 60 minutes from now.

---
###### B) "Valid for minutes" mode

- Run executable with a single numeric parameter (should ba castable to *System.Double* type)

> NotifyNL.SecretsManager.exe 75

**Result:** The produced JWT token will be valid for the specified amount of minutes from now.
In this example, token is going to be valid for 75 minutes.

---
###### C) "Valid until datetime" mode 

- Run executable with a single datetime parameter (should be castable to *System.DateTime* type)

> NotifyNL.SecretsManager.exe 2023-12-31T23:59:59

**Result:** The produced JWT token will be valid until the specified date and time.
In this example, token is going to be valid until 31st of December 2023, 23:59:59.

Remarks:

- You can use your local time. All times will be converted to UTC under the hood
- You can specify any valid datetime (e.g., without time, or without seconds, etc.)
Check the documentation here: [Date time string formats](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings), and [System.DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime?view=net-7.0).

---
### 3.1. Externally

1. Add a reference to the `SecretsManager.csproj` project or use a delivered `.dll`

2. Use benefits of Strategy Design Pattern:

    2.1. Straightforward solution:
    - Create new instance of your desired encryption strategy
    - Create new instance of **EncryptionContext**
    - Pass reference of your strategy into the context
    - Now, you can call any public method from **EncryptionContext**. Follow the internal documentation.
    
    Example:

    ```csharp
    var strategy = new SymmetricEncryptionStrategy();
    var context = new EncryptionContext(strategy);

    SecurityKey key = context.GetSecurityKey("I really like hamsters");  // Symmetric key

    string jwtToken = context.GetJwtToken(key, ...);  // JWT token with symmetric verify signature
    ```

    2.2. Following DI (Dependency Injection) pattern:
    - At first, register a specific strategy as an implementation of **IJwtEncryptionStrategy**:
        - **SymmetricEncryptionStrategy**, or
        - **AsymmetricEncryptionStrategy** (not both at the same time!)
        To determine which encryption strategy should be used you can use:
        a) simple *control statement* (if-else / ternary expression),
        b) *input arguments* in *Main(**string[] args**)* method from *Program.cs*
        c) or a *flag in the configuration file*.
    
    - Now, since the strategy is registered as a service, you have to register **EncryptionContext** class (which is context component of Strategy Design Pattern)
    
    - When the context will be resolved from `IServiceProvider`, the concrete encryption strategy will be resolved as well along with it.
    
    - Now, you can call any public method from **EncryptionContext**. Follow the internal documentation.

    Example:

    ```csharp
    // Strategies
    builder.Services.AddSingleton(typeof(IJwtEncryptionStrategy),
        builder.Configuration.GetValue<bool>("Encryption:IsAsymmetric")  // Configuration file
            ? typeof(AsymmetricEncryptionStrategy)
            : typeof(SymmetricEncryptionStrategy));

    // Context
    builder.Services.AddSingleton<EncryptionContext>();
    ```

    Or:

    ```csharp
    // Strategy
    builder.Services.AddSingleton<IJwtEncryptionStrategy, SymmetricEncryptionStrategy>();

    // Context
    builder.Services.AddSingleton<EncryptionContext>();
    ```