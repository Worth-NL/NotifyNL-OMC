// © 2023. Worth Systems.

using Common.Constants;
using Common.Extensions;
using Common.Settings;
using Common.Settings.Configuration;
using Common.Settings.Extensions;
using Common.Settings.Strategy.Interfaces;
using Common.Settings.Strategy.Manager;
using Common.Versioning.Models;
using EventsHandler.Controllers;
using EventsHandler.Services.Configuration;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing;
using EventsHandler.Services.DataProcessing.Interfaces;
using Sentry;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Kto;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Print;
using EventsHandler.Services.DataProcessing.Strategy.Manager;
using EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces;
using EventsHandler.Services.Responding;
using EventsHandler.Services.Responding.Results.Builder;
using EventsHandler.Services.Responding.Results.Builder.Interface;
using EventsHandler.Services.Templates;
using EventsHandler.Services.Templates.Interfaces;
using EventsHandler.Services.Validation;
using EventsHandler.Services.Validation.Interfaces;
using EventsHandler.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Notify.Models.Responses;
using SecretsManager.Services.Authentication.Encryptions.Strategy;
using SecretsManager.Services.Authentication.Encryptions.Strategy.Context;
using SecretsManager.Services.Authentication.Encryptions.Strategy.Interfaces;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using WebQueries.Tracing;
using WebQueries.BRP;
using WebQueries.DataQuerying.Adapter;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Proxy;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataQuerying.Strategies.Base;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataSending;
using WebQueries.DataSending.Clients.Factories;
using WebQueries.DataSending.Clients.Factories.Interfaces;
using WebQueries.DataSending.Clients.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.KTO;
using WebQueries.KTO.Interfaces;
using WebQueries.MijnOverheid;
using WebQueries.MijnOverheid.Clients;
using WebQueries.MijnOverheid.Interfaces;
using WebQueries.MOBB;
using WebQueries.MOBB.Interfaces;
using WebQueries.Print;
using WebQueries.Print.Interfaces;
using WebQueries.Register.Interfaces;
using WebQueries.Versioning;
using ZgwModels.Mapping.Events;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Serialization;
using ZgwModels.Serialization.Interfaces;
using Besluiten = WebQueries.DataQuerying.Strategies.Queries.Besluiten;
using Documenten = WebQueries.DataQuerying.Strategies.Queries.Documenten;
using Objecten = WebQueries.DataQuerying.Strategies.Queries.Objecten;
using ObjectTypen = WebQueries.DataQuerying.Strategies.Queries.ObjectTypen;
using OpenKlant = WebQueries.DataQuerying.Strategies.Queries.OpenKlant;
using OpenVtb = WebQueries.DataQuerying.Strategies.Queries.OpenVtb;
using OpenZaak = WebQueries.DataQuerying.Strategies.Queries.OpenZaak;
using Register = WebQueries.Register;
using Responder = EventsHandler.Services.Responding;

namespace EventsHandler
{
    [ExcludeFromCodeCoverage(Justification = "This is startup class with dozens of dependencies")]
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            WebApplication.CreateBuilder(args)
                .AddConfiguration()
                .AddExternalServices()
                .AddInternalServices()
                .ConfigureHttpPipeline()
                .Run();
        }

        #region Configuration
        private static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
        {
            const string appSettingsRootName = "appsettings";

            builder.Configuration.AddJsonFile($"{appSettingsRootName}.json", optional: false)
                                 .AddJsonFile($"{appSettingsRootName}.{builder.Environment.EnvironmentName}.json", optional: true);

            return builder;
        }
        #endregion

        private const string DashboardCorsPolicy = "Dashboard";

        #region Services: External (.NET)
        private static WebApplicationBuilder AddExternalServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddCors(setup =>
            {
                setup.AddPolicy(DashboardCorsPolicy, policy =>
                {
                    string[] origins = (Environment.GetEnvironmentVariable(ConfigExtensions.DashboardOrigins) ?? "http://localhost:3000")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    policy.WithOrigins(origins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // Authentication using JWT (JSON Web Tokens) Bearer
            builder.Services.AddAuthentication(setup =>
            {
                setup.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                setup.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                setup.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(setup =>
            {
                EncryptionContext encryptionContext = builder.Services.GetRequiredService<EncryptionContext>();
                OmcConfiguration configuration = builder.Services.GetRequiredService<OmcConfiguration>();

                setup.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = configuration.OMC.Auth.JWT.Issuer(),
                    ValidAudience = configuration.OMC.Auth.JWT.Audience(),
                    IssuerSigningKey = encryptionContext.GetSecurityKey(configuration.OMC.Auth.JWT.Secret()),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };

                setup.MapInboundClaims = false;
            });

            builder.Services.AddSwaggerGen(setup =>
            {
                setup.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = ApiResources.Swagger_UI_Version,
                    Title = ApiResources.Swagger_UI_Title,
                    Description = ApiResources.Swagger_UI_Description
                });

                setup.ExampleFilters();

                string xmlDocumentationFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                string xmlDocumentationPath = Path.Combine(AppContext.BaseDirectory, xmlDocumentationFile);
                setup.IncludeXmlComments(xmlDocumentationPath);

                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Description = ApiResources.Swagger_UI_Authentication_Description,
                    Name = CommonValues.Default.Authorization.OpenApi.SecurityScheme.Name,
                    In = ParameterLocation.Header,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = CommonValues.Default.Authorization.OpenApi.SecurityScheme.BearerFormat,
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                setup.AddSecurityDefinition(jwtSecurityScheme.Scheme, jwtSecurityScheme);
                setup.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });
            });

            builder.Services.AddSwaggerExamplesFromAssemblyOf<EventsController>();

            builder.WebHost.UseSentry(options =>
            {
                options.ConfigureSentryOptions(isDebugEnabled: builder.Environment.IsDevelopment());
            });

            return builder;
        }

        private static void ConfigureSentryOptions(this SentryOptions options, bool isDebugEnabled)
        {
            options.Dsn = Environment.GetEnvironmentVariable(ConfigExtensions.SentryDsn)
                          ?? string.Empty;

            options.DiagnosticLevel = isDebugEnabled ? SentryLevel.Debug : SentryLevel.Info;
            options.Debug = isDebugEnabled;
            options.AutoSessionTracking = true;
            options.IsGlobalModeEnabled = false;
            options.Distribution = $"{Environment.OSVersion.Platform} ({Environment.OSVersion.VersionString})";

            Version? version = Assembly.GetEntryAssembly()?.GetName().Version;

            if (version is not null)
                OmcVersion.SetVersion(version.Major, version.Minor, version.Build);

            options.Release = OmcVersion.GetExpandedVersion();
            options.Environment = Environment.GetEnvironmentVariable(ConfigExtensions.SentryEnvironment) ??
                                  Environment.GetEnvironmentVariable(ConfigExtensions.AspNetCoreEnvironment) ??
                                  CommonValues.Default.Models.DefaultStringValue;
        }
        #endregion

        #region Services: Internal (OMC)
        private static WebApplicationBuilder AddInternalServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<OmcConfiguration>();
            builder.Services.RegisterLoadingStrategies();

            builder.Services.RegisterEncryptionStrategy(builder);

            builder.Services.AddSingleton<IValidationService<NotificationEvent>, NotificationValidator>();
            builder.Services.AddSingleton<ISerializationService, SpecificSerializer>();
            builder.Services.AddScoped<IProcessingService, NotifyProcessor>();
            builder.Services.AddSingleton<ITemplatesService<TemplateResponse, NotificationEvent>, NotifyTemplatesAnalyzer>();
            builder.Services.AddSingleton<INotifyService<NotifyData>, NotifyService>();
            builder.Services.AddScoped<IKtoScenarioFactory, KtoScenarioFactory>();
            builder.Services.AddScoped<IMessageBoxScenario, MessageBoxScenarioImplementation>();
            builder.Services.AddScoped<IPrintScenario, PrintScenarioImplementation>();
            builder.Services.RegisterNotifyStrategies();

            // Domain queries and resources
            builder.Services.AddScoped<CloudEventNormalizer>();
            builder.Services.AddScoped<IDataQueryService<NotificationEvent>, DataQueryService>();
            builder.Services.AddScoped<IQueryContext, QueryContext>();
            builder.Services.AddScoped<ConfigurationCheckService>();
            builder.Services.AddSingleton<ScenarioFlowService>();
            builder.Services.AddSingleton<TraceEmitter>();
            builder.Services.RegisterOpenServices();

            builder.Services.AddSingleton<IHttpNetworkService, HttpNetworkService>();
            builder.Services.AddSingleton<IHttpNetworkServiceKto, KtoHttpNetworkService>();
            builder.Services.AddHttpClient<KtoHttpNetworkService>();
            builder.Services.AddHttpClient<KeycloakTokenService>();
            // MijnOverheidClient resolves its HttpClient by name (IHttpClientFactory.CreateClient(nameof(MijnOverheidClient)))
            // rather than the typed-client pattern used by its siblings above — the name must match exactly.
            builder.Services.AddHttpClient(nameof(MijnOverheidClient));
            builder.Services.AddScoped<IMijnOverheidClient, MijnOverheidClient>();
            builder.Services.AddScoped<IMijnOverheidForwarder, MijnOverheidForwarder>();
            builder.Services.AddHttpClient<BrpClient>()
                .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
                {
                    ILogger<BrpClient> logger = serviceProvider.GetRequiredService<ILogger<BrpClient>>();

                    string? certPath = Environment.GetEnvironmentVariable("BRP_CLIENTCERT_PEM_PATH");
                    string? keyPath = Environment.GetEnvironmentVariable("BRP_CLIENTKEY_PEM_PATH");

                    var handler = new HttpClientHandler();

                    if (!string.IsNullOrWhiteSpace(certPath) && !string.IsNullOrWhiteSpace(keyPath))
                    {
                        try
                        {
                            if (File.Exists(certPath) && File.Exists(keyPath))
                            {
                                logger.LogInformation("Loading certificate from PEM files: {CertPath}, {KeyPath}",
                                    certPath, keyPath);

                                string certPem = File.ReadAllText(certPath);
                                string keyPem = File.ReadAllText(keyPath);

                                // ✅ Modern API (no obsolete constructors, no PFX roundtrip)
                                using var cert = X509Certificate2.CreateFromPem(certPem, keyPem);

                                // ⚠️ IMPORTANT FIX:
                                // Ensure private key is actually usable across platforms
                                // CreateFromPem sometimes yields a cert that is not fully exportable depending on runtime
                                var fullCert = cert.CopyWithPrivateKey(cert.GetRSAPrivateKey()!);

                                logger.LogInformation(
                                    "Certificate loaded successfully:\n" +
                                    "  Subject: {Subject}\n" +
                                    "  Issuer: {Issuer}\n" +
                                    "  Thumbprint: {Thumbprint}\n" +
                                    "  NotBefore: {NotBefore}\n" +
                                    "  NotAfter: {NotAfter}\n" +
                                    "  HasPrivateKey: {HasPrivateKey}\n" +
                                    "  KeyAlgorithm: {KeyAlgorithm}",
                                    fullCert.Subject,
                                    fullCert.Issuer,
                                    fullCert.Thumbprint,
                                    fullCert.NotBefore,
                                    fullCert.NotAfter,
                                    fullCert.HasPrivateKey,
                                    fullCert.GetKeyAlgorithm()
                                );

                                // Log CN/OU for debugging WS Gateway identity checks
                                IEnumerable<string> subjectParts = fullCert.Subject
                                    .Split(',')
                                    .Select(p => p.Trim())
                                    .Where(p =>
                                        p.StartsWith("CN=", StringComparison.OrdinalIgnoreCase) ||
                                        p.StartsWith("OU=", StringComparison.OrdinalIgnoreCase));

                                foreach (string part in subjectParts)
                                {
                                    logger.LogInformation("  Subject part: {Part}", part);
                                }

                                handler.ClientCertificates.Add(fullCert);

                                logger.LogInformation("Certificate added to HttpClientHandler");
                            }
                            else
                            {
                                logger.LogWarning(
                                    "⚠️ Certificate files not found. Cert exists: {CertExists}, Key exists: {KeyExists}",
                                    File.Exists(certPath), File.Exists(keyPath));

                                logger.LogWarning(
                                    "BRP functionality disabled due to missing certificate files.");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to load BRP client certificate");

                            logger.LogWarning(
                                "Application will continue without BRP functionality.");
                        }
                    }
                    else
                    {
                        logger.LogWarning(
                            "⚠️ BRP certificate paths not configured. CertPathSet: {CertSet}, KeyPathSet: {KeySet}",
                            !string.IsNullOrWhiteSpace(certPath),
                            !string.IsNullOrWhiteSpace(keyPath));

                        logger.LogInformation(
                            "BRP functionality disabled (no certificate configuration).");
                    }

                #if DEBUG
                    handler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                    logger.LogWarning("SSL certificate validation is DISABLED (DEBUG mode only)");
                #endif

                    return handler;
                });

            builder.Services.RegisterClientFactories();

            builder.Services.AddSingleton<OmcVersionRegister>();
            builder.Services.AddSingleton<ZgwVersionRegister>();

            // User Interaction
            builder.Services.RegisterResponders();
            builder.Services.AddSingleton<IDetailsBuilder, DetailsBuilder>();

            return builder;
        }

        #region Aggregated registrations
        private static void RegisterEncryptionStrategy(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddSingleton(typeof(IJwtEncryptionStrategy),
                builder.Configuration.IsEncryptionAsymmetric()
                    ? typeof(AsymmetricEncryptionStrategy)
                    : typeof(SymmetricEncryptionStrategy));

            services.AddSingleton<EncryptionContext>();
        }

        private static void RegisterLoadingStrategies(this IServiceCollection services)
        {
            services.AddSingleton<ILoadersContext, LoadersContext>();

            services.AddSingleton<AppSettingsLoader>();
            services.AddSingleton<EnvironmentLoader>();
        }

        private static void RegisterNotifyStrategies(this IServiceCollection services)
        {
            services.AddScoped<IScenariosResolver<INotifyScenario, NotificationEvent>, NotifyScenariosResolver>();

            services.AddScoped<CaseCreatedScenario>();
            services.AddScoped<CaseStatusUpdatedScenario>();
            services.AddScoped<CaseClosedScenario>();
            services.AddScoped<TaskAssignedScenario>();
            services.AddScoped<DecisionMadeScenario>();
            services.AddScoped<MessageReceivedScenario>();
            services.AddScoped<NotImplementedScenario>();
            services.AddScoped<KtoScenario>();
            services.AddScoped<PrintScenario>();
        }

        // NOTE: v1 workflow versioning (OpenKlant v1, OpenZaak v1/v2 distinction, and the
        // OMC_FEATURE_WORKFLOW_VERSION switch) was removed in 2.0.2 — this always wires up the
        // v2 implementations directly instead of resolving a version at startup.
        private static void RegisterOpenServices(this IServiceCollection services)
        {
            // Common query methods
            services.AddSingleton<IQueryBase, QueryBase>();

            // Strategies
            services.AddSingleton<OpenZaak.Interfaces.IQueryZaak, OpenZaak.QueryZaak>();
            services.AddSingleton<OpenKlant.Interfaces.IQueryKlant, OpenKlant.v2.QueryKlant>();
            services.AddSingleton<Besluiten.Interfaces.IQueryBesluiten, Besluiten.QueryBesluiten>();
            services.AddSingleton<Objecten.Interfaces.IQueryObjecten, Objecten.QueryObjecten>();
            services.AddSingleton<ObjectTypen.Interfaces.IQueryObjectTypen, ObjectTypen.QueryObjectTypen>();
            services.AddSingleton<OpenVtb.Interfaces.IQueryVtb, OpenVtb.QueryVtb>();
            services.AddSingleton<Documenten.Interfaces.IQueryDocumenten, Documenten.QueryDocumenten>();

            // Feedback and telemetry
            services.AddScoped<ITelemetryService, Register.v2.ContactRegistration>();
        }

        private static void RegisterClientFactories(this IServiceCollection services)
        {
            services.AddSingleton<IHttpClientFactory<HttpClient, (string, string)[]>, RegularHttpClientFactory>();
            services.AddSingleton<IHttpClientFactory<INotifyClient, string>, NotificationClientFactory>();
        }

        // NOTE: v1 workflow versioning was removed in 2.0.2 — see RegisterOpenServices above.
        private static void RegisterResponders(this IServiceCollection services)
        {
            services.AddSingleton<NotificationEventResponder>();
            services.AddScoped<GeneralResponder, Responder.v2.NotifyCallbackResponder>();
        }
        #endregion
        #endregion

        #region HTTP Pipeline
        private static WebApplication ConfigureHttpPipeline(this WebApplicationBuilder builder)
        {
            WebApplication app = builder.Build();

            OmcConfiguration configuration = app.Services.GetRequiredService<OmcConfiguration>();
            string pathBase = configuration.OMC.Context.Path();

            configuration.OMC.Actor.Id();

            if (!string.IsNullOrEmpty(pathBase))
            {
                app.Use((context, next) =>
                {
                    if (context.Request.Path.StartsWithSegments(pathBase))
                    {
                        return next();
                    }

                    // CodeQL cs/web/unvalidated-url-redirection: reject a request path starting
                    // with "//" before it reaches Redirect() — some clients/proxies pass that
                    // through as a literal PathString, and left unchecked it would make the
                    // built target protocol-relative (browsers treat "//host/..." as a redirect
                    // to a different host, same as "https://host/..."). pathBase is trusted
                    // configuration and always prepended first, so this is otherwise always a
                    // same-origin relative path — this closes the one way that stops being true.
                    if (context.Request.Path.Value?.StartsWith("//") == true)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        return Task.CompletedTask;
                    }

                    string newPath = pathBase + context.Request.Path + context.Request.QueryString;
                    context.Response.Redirect(newPath);
                    return Task.CompletedTask;
                });

                app.UsePathBase(pathBase);
            }

            if (app.Environment.IsProduction() || app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Serves the statically-exported dashboard (wwwroot/status, wwwroot/status/flow,
            // wwwroot/_next/*) baked into this image at build time — see the explicit
            // /status and /status/flow routes below for the corresponding index.html files.
            app.UseStaticFiles();

            app.UseCors(DashboardCorsPolicy);

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGet("/", () =>
            {
                // DASHBOARD_URL lets a future deployment point this at a separately hosted
                // dashboard; today it's unset and the dashboard is co-hosted at /status.
                string? dashboardUrl = Environment.GetEnvironmentVariable(ConfigExtensions.DashboardUrl);

                return Results.Redirect(string.IsNullOrWhiteSpace(dashboardUrl) ? "/status" : dashboardUrl);
            });

            // Falls back to /swagger when the dashboard hasn't been built locally (e.g. plain
            // `dotnet run`/Visual Studio F5 without ever running `npm run build` in dashboard/)
            // so development against the API is never blocked by a missing frontend build.
            app.MapGet("/status", () => ServeDashboardPage(app, "status", "index.html"));
            app.MapGet("/status/flow", () => ServeDashboardPage(app, "status", "flow", "index.html"));

            app.MapControllers();  // Mapping actions from API controllers

            app.UseSentryTracing();

            return app;
        }

        private static IResult ServeDashboardPage(WebApplication app, params string[] relativePathSegments)
        {
            // WebRootPath is null (not just missing on disk) when wwwroot doesn't exist at all —
            // e.g. a plain `dotnet run`/Visual Studio F5 without ever building the dashboard.
            string? webRootPath = app.Environment.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                return Results.Redirect("/swagger");
            }

            string path = Path.Combine([webRootPath, .. relativePathSegments]);

            return File.Exists(path) ? Results.File(path, "text/html") : Results.Redirect("/swagger");
        }
        #endregion
    }
}
