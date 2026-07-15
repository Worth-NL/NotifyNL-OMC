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
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing;
using EventsHandler.Services.DataProcessing.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Kto;
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
using WebQueries.Register.Interfaces;
using WebQueries.Versioning;
using ZhvModels.Mapping.Events;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Serialization;
using ZhvModels.Serialization.Interfaces;
using Besluiten = WebQueries.DataQuerying.Strategies.Queries.Besluiten;
using Objecten = WebQueries.DataQuerying.Strategies.Queries.Objecten;
using ObjectTypen = WebQueries.DataQuerying.Strategies.Queries.ObjectTypen;
using OpenKlant = WebQueries.DataQuerying.Strategies.Queries.OpenKlant;
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

        #region Services: External (.NET)
        private static WebApplicationBuilder AddExternalServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

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
            builder.Services.RegisterNotifyStrategies();

            builder.Services.AddScoped<CloudEventNormalizer>();
            builder.Services.AddScoped<IDataQueryService<NotificationEvent>, DataQueryService>();
            builder.Services.AddScoped<IQueryContext, QueryContext>();
            builder.Services.RegisterOpenServices(builder);

            builder.Services.AddSingleton<IHttpNetworkService, HttpNetworkService>();
            builder.Services.AddSingleton<IHttpNetworkServiceKto, KtoHttpNetworkService>();
            builder.Services.AddHttpClient<KtoHttpNetworkService>();
            builder.Services.AddHttpClient<KeycloakTokenService>();
            builder.Services.AddScoped<IMijnOverheidClient, MijnOverheidClient>();
            builder.Services.AddScoped<IMijnOverheidForwarder, MijnOverheidForwarder>();
            builder.Services.RegisterClientFactories();
            builder.Services.AddHttpClient<BrpClient>()
                .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
                {
                    ILogger<BrpClient> logger = serviceProvider.GetRequiredService<ILogger<BrpClient>>();

                    string certPath = Environment.GetEnvironmentVariable("BRP_CLIENTCERT_PEM_PATH")!;
                    string keyPath = Environment.GetEnvironmentVariable("BRP_CLIENTKEY_PEM_PATH")!;

                    var handler = new HttpClientHandler();

                    if (!string.IsNullOrEmpty(certPath) && !string.IsNullOrEmpty(keyPath))
                    {
                        try
                        {
                            if (File.Exists(certPath) && File.Exists(keyPath))
                            {
                                logger.LogInformation("Loading certificate from PEM files: {CertPath}, {KeyPath}",
                                    certPath, keyPath);

                                string certPem = File.ReadAllText(certPath);
                                string keyPem = File.ReadAllText(keyPath);

                                var cert = X509Certificate2.CreateFromPem(certPem, keyPem);

                                byte[] pfxBytes = cert.Export(X509ContentType.Pfx);
                                var fullCert = new X509Certificate2(
                                    pfxBytes,
                                    (string?)null,
                                    X509KeyStorageFlags.MachineKeySet |
                                    X509KeyStorageFlags.EphemeralKeySet |
                                    X509KeyStorageFlags.Exportable
                                );

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

                                IEnumerable<string> subjectParts = fullCert.Subject
                                    .Split(',')
                                    .Select(p => p.Trim())
                                    .Where(p => p.StartsWith("CN=", StringComparison.OrdinalIgnoreCase) ||
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
                                    File.Exists(certPath), File.Exists(keyPath)
                                );
                                logger.LogWarning(
                                    "The application will start without BRP functionality. " +
                                    "BRP API calls will fail with certificate errors.");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to load BRP client certificate");
                            logger.LogWarning(
                                "The application will start without BRP functionality. " +
                                "BRP API calls will fail with certificate errors.");
                        }
                    }
                    else
                    {
                        logger.LogWarning(
                            "⚠️ BRP certificate paths not configured. BRP_CLIENTCERT_PEM_PATH: {CertPathSet}, BRP_CLIENTKEY_PEM_PATH: {KeyPathSet}",
                            !string.IsNullOrEmpty(certPath), !string.IsNullOrEmpty(keyPath)
                        );
                        logger.LogInformation(
                            "The application will start without BRP functionality. " +
                            "Configure certificates to enable BRP API access.");
                    }

#if DEBUG
                    handler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                    logger.LogWarning("SSL certificate validation is DISABLED (DEBUG mode only!)");
#endif

                    return handler;
                });

            builder.Services.RegisterClientFactories();

            builder.Services.AddSingleton<OmcVersionRegister>();
            builder.Services.AddSingleton<ZhvVersionRegister>();

            builder.Services.RegisterResponders(builder);
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
        }

        private static void RegisterOpenServices(this IServiceCollection services, WebApplicationBuilder builder)
        {
            byte omcWorkflowVersion = builder.Services.GetRequiredService<OmcConfiguration>().OMC.Feature.Workflow_Version();

            services.AddSingleton<IQueryBase, QueryBase>();

            services.AddSingleton(typeof(OpenZaak.Interfaces.IQueryZaak), DetermineOpenZaakVersion(omcWorkflowVersion));
            services.AddSingleton(typeof(OpenKlant.Interfaces.IQueryKlant), DetermineOpenKlantVersion(omcWorkflowVersion));
            services.AddSingleton(typeof(Besluiten.Interfaces.IQueryBesluiten), DetermineBesluitenVersion(omcWorkflowVersion));
            services.AddSingleton(typeof(Objecten.Interfaces.IQueryObjecten), DetermineObjectenVersion(omcWorkflowVersion));
            services.AddSingleton(typeof(ObjectTypen.Interfaces.IQueryObjectTypen), DetermineObjectTypenVersion(omcWorkflowVersion));

            services.AddScoped(typeof(ITelemetryService), DetermineTelemetryVersion(omcWorkflowVersion));

            return;

            static Type DetermineOpenZaakVersion(byte omvWorkflowVersion)
            {
                return omvWorkflowVersion switch
                {
                    1 => typeof(OpenZaak.v1.QueryZaak),
                    2 => typeof(OpenZaak.v2.QueryZaak),
                    _ => throw new NotImplementedException(ApiResources.ServiceResolving_ERROR_VersionOpenZaakUnknown)
                };
            }

            static Type DetermineOpenKlantVersion(byte omvWorkflowVersion)
            {
                return omvWorkflowVersion switch
                {
                    1 => typeof(OpenKlant.v1.QueryKlant),
                    2 => typeof(OpenKlant.v2.QueryKlant),
                    _ => throw new NotImplementedException(ApiResources.ServiceResolving_ERROR_VersionOpenKlantUnknown)
                };
            }

            static Type DetermineBesluitenVersion(byte omvWorkflowVersion)
            {
                return omvWorkflowVersion switch
                {
                    1 or 2 => typeof(Besluiten.v1.QueryBesluiten),
                    _ => throw new NotImplementedException(ApiResources.ServiceResolving_ERROR_VersionBesluitenUnknown)
                };
            }

            static Type DetermineObjectenVersion(byte omvWorkflowVersion)
            {
                return omvWorkflowVersion switch
                {
                    1 or 2 => typeof(Objecten.v1.QueryObjecten),
                    _ => throw new NotImplementedException(ApiResources.ServiceResolving_ERROR_VersionObjectenUnknown)
                };
            }

            static Type DetermineObjectTypenVersion(byte omvWorkflowVersion)
            {
                return omvWorkflowVersion switch
                {
                    1 or 2 => typeof(ObjectTypen.v1.QueryObjectTypen),
                    _ => throw new NotImplementedException(ApiResources.ServiceResolving_ERROR_VersionObjectTypenUnknown)
                };
            }

            static Type DetermineTelemetryVersion(byte omvWorkflowVersion)
            {
                return omvWorkflowVersion switch
                {
                    1 => typeof(Register.v1.ContactRegistration),
                    2 => typeof(Register.v2.ContactRegistration),
                    _ => throw new NotImplementedException(ApiResources.ServiceResolving_ERROR_VersionTelemetryUnknown)
                };
            }
        }

        private static void RegisterClientFactories(this IServiceCollection services)
        {
            services.AddSingleton<IHttpClientFactory<HttpClient, (string, string)[]>, RegularHttpClientFactory>();
            services.AddSingleton<IHttpClientFactory<INotifyClient, string>, NotificationClientFactory>();
        }

        private static void RegisterResponders(this IServiceCollection services, WebApplicationBuilder builder)
        {
            byte omcWorkflowVersion = builder.Services.GetRequiredService<OmcConfiguration>().OMC.Feature.Workflow_Version();

            services.AddSingleton<NotificationEventResponder>();
            services.AddScoped(typeof(GeneralResponder), DetermineResponderVersion(omcWorkflowVersion));

            return;

            static Type DetermineResponderVersion(byte omvWorkflowVersion)
            {
                return omvWorkflowVersion switch
                {
                    1 => typeof(Responder.v1.NotifyCallbackResponder),
                    2 => typeof(Responder.v2.NotifyCallbackResponder),
                    _ => throw new NotImplementedException(ApiResources.ServiceResolving_ERROR_VersionNotifyResponderUnknown)
                };
            }
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

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.UseSentryTracing();

            return app;
        }
        #endregion
    }
}