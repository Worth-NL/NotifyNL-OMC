// © 2023, Worth Systems.

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
using WebQueries.Register.Interfaces;
using WebQueries.Versioning;
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
    /// <summary>
    /// The entry point to the Web API application, responsible for configuring and starting its instance.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "This is startup class with dozens of dependencies")]
    internal static class Program
    {
        /// <summary>
        /// Custom simplified version of application configuration.
        /// </summary>
        /// <param name="args">The <see cref="Program"/> startup arguments.</param>
        internal static void Main(string[] args)
        {
            WebApplication.CreateBuilder(args)
                .AddConfiguration()       // 1. Configuration (appsettings.json)
                .AddExternalServices()    // 2. Microsoft .NET services
                .AddInternalServices()    // 3. Internal OMC services
                .ConfigureHttpPipeline()  // 4. Configure pipeline (what should happen during HTTP request-response cycle)
                .Run();                   // 5. Start the application
        }

        #region Configuration
        /// <summary>
        /// Adding application configurations from JSON files.
        /// </summary>
        /// <param name="builder">The builder of the web application (used for configuration).</param>
        /// <returns>Partially-configured <see cref="WebApplicationBuilder"/> with .NET services.</returns>
        private static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
        {
            // Configuration appsettings.json files
            const string appSettingsRootName = "appsettings";

            builder.Configuration.AddJsonFile($"{appSettingsRootName}.json", optional: false)
                                 .AddJsonFile($"{appSettingsRootName}.{builder.Environment.EnvironmentName}.json", optional: true);
            
            return builder;
        }
        #endregion

        #region Services: External (.NET)
        /// <summary>
        /// Registration of .NET services, used by the application.
        /// </summary>
        /// <param name="builder">The builder of the web application (used for configuration).</param>
        /// <returns>Partially-configured <see cref="WebApplicationBuilder"/> with .NET services.</returns>
        private static WebApplicationBuilder AddExternalServices(this WebApplicationBuilder builder)
        {
            // API Controllers
            builder.Services.AddControllers();

            // Navigate directly to the endpoints from API Controllers (instead of using explicit .Map() routing in config)
            builder.Services.AddEndpointsApiExplorer();

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

                // Disable some default validations, preventing the JWT token to be recognized as valid
                setup.TokenValidationParameters = new TokenValidationParameters
                {
                    // Validation parameters
                    ValidIssuer = configuration.OMC.Auth.JWT.Issuer(),
                    ValidAudience = configuration.OMC.Auth.JWT.Audience(),
                    IssuerSigningKey = encryptionContext.GetSecurityKey(configuration.OMC.Auth.JWT.Secret()),

                    // Validation criteria
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };

                // Skip repacking user Claims into Microsoft specific objects
                setup.MapInboundClaims = false;
            });

            // Swagger UI: Configuration
            builder.Services.AddSwaggerGen(setup =>
            {
                // Enable API documentation in Swagger UI
                setup.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = ApiResources.Swagger_UI_Version,
                    Title = ApiResources.Swagger_UI_Title,
                    Description = ApiResources.Swagger_UI_Description
                });

                // Enable [SwaggerRequestExample] filter for parameters in Swagger UI
                setup.ExampleFilters();

                // Map XML documentation from API Controllers into Swagger UI
                string xmlDocumentationFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                string xmlDocumentationPath = Path.Combine(AppContext.BaseDirectory, xmlDocumentationFile);
                setup.IncludeXmlComments(xmlDocumentationPath);

                // Enable required JWT authentication tokens from Headers in Swagger UI
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

                setup.AddSecurityDefinition(jwtSecurityScheme.Scheme, jwtSecurityScheme);  // Enable authentication input (button) to provide JWT in Swagger UI
                setup.AddSecurityRequirement(new OpenApiSecurityRequirement                // Add authentications requirements for API methods in Swagger UI
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });
            });

            // Swagger UI: Examples (showing custom values of API parameters instead of the default ones)
            builder.Services.AddSwaggerExamplesFromAssemblyOf<EventsController>();  // NOTE: Any class which belongs to the solution

            // Add logging using Sentry SDK and external monitoring service
            builder.WebHost.UseSentry(options =>
            {
                options.ConfigureSentryOptions(isDebugEnabled: builder.Environment.IsDevelopment());
            });

            return builder;
        }

        #region Sentry configuration
        /// <summary>
        /// Configure logging options for Sentry.
        /// <para>
        ///   Source: https://docs.sentry.io/platforms/dotnet/configuration/options/
        /// </para>
        /// </summary>
        private static void ConfigureSentryOptions(this SentryOptions options, bool isDebugEnabled)
        {
            // Sentry Data Source Name (DSN) => where to log application events
            options.Dsn = Environment.GetEnvironmentVariable(ConfigExtensions.SentryDsn)
                          ?? string.Empty;  // NOTE: SentrySDK will automatically reach "SENTRY_DSN" environment variable so, it's not needed to
                                            // do this manually; however, if this variable is not existing Sentry will throw ArgumentNullException.
                                            // The current fallback scenario is just disabling Sentry logging in case of missing DSN (no exception)

            // Informational messages are the most detailed to log
            options.DiagnosticLevel = isDebugEnabled ? SentryLevel.Debug  // More detailed (more insightful but noisy) settings for logs
                                                     : SentryLevel.Info;  // Less detailed (not affecting performance) settings for logs

            // Detailed debugging logs in the console window
            options.Debug = isDebugEnabled;

            // Enables Sentry's "Release Health" feature
            options.AutoSessionTracking = true;

            // Disables the case that all threads use the same global scope ("true" for client apps, "false" for server apps)
            options.IsGlobalModeEnabled = false;

            // The identifier indicating to which or on which platform / system the application is meant to run
            options.Distribution = $"{Environment.OSVersion.Platform} ({Environment.OSVersion.VersionString})";

            // Fetch version from EventsHandler.Csproj and set OmcVersion.
            Version? version = Assembly.GetEntryAssembly()?.GetName().Version;

            if (version is not null)
                OmcVersion.SetVersion(version.Major, version.Minor, version.Build);

            // Version of the application ("OMC Web API" in this case)
            options.Release = OmcVersion.GetExpandedVersion();

            // The environment of the application (Prod, Test, Dev, Staging, etc.)
            options.Environment = Environment.GetEnvironmentVariable(ConfigExtensions.SentryEnvironment) ??
                                  Environment.GetEnvironmentVariable(ConfigExtensions.AspNetCoreEnvironment) ??
                                  CommonValues.Default.Models.DefaultStringValue;
        }
        #endregion
        #endregion

        #region Services: Internal (OMC)
        /// <summary>
        /// Registration of custom services, used for business logic and internal processes.
        /// </summary>
        /// <param name="builder">The builder of the web application (used for configuration).</param>
        /// <returns>Partially-configured <see cref="WebApplicationBuilder"/> with custom services.</returns>
        private static WebApplicationBuilder AddInternalServices(this WebApplicationBuilder builder)
        {
            // Configurations
            builder.Services.AddSingleton<OmcConfiguration>();
            builder.Services.RegisterLoadingStrategies();

            // JWT generation
            builder.Services.RegisterEncryptionStrategy(builder);

            // Business logic
            builder.Services.AddSingleton<IValidationService<NotificationEvent>, NotificationValidator>();
            builder.Services.AddSingleton<ISerializationService, SpecificSerializer>();
            builder.Services.AddSingleton<IProcessingService, NotifyProcessor>();
            builder.Services.AddSingleton<ITemplatesService<TemplateResponse, NotificationEvent>, NotifyTemplatesAnalyzer>();
            builder.Services.AddSingleton<INotifyService<NotifyData>, NotifyService>();
            builder.Services.RegisterNotifyStrategies();

            // Domain queries and resources
            builder.Services.AddSingleton<IDataQueryService<NotificationEvent>, DataQueryService>();
            builder.Services.AddSingleton<IQueryContext, QueryContext>();
            builder.Services.RegisterOpenServices(builder);

            // HTTP communication
            builder.Services.AddSingleton<IHttpNetworkService, HttpNetworkService>();
            builder.Services.RegisterClientFactories();

            // Versioning
            builder.Services.AddSingleton<OmcVersionRegister>();
            builder.Services.AddSingleton<ZhvVersionRegister>();

            // User Interaction
            builder.Services.RegisterResponders(builder);
            builder.Services.AddSingleton<IDetailsBuilder, DetailsBuilder>();

            return builder;
        }

        #region Aggregated registrations
        private static void RegisterEncryptionStrategy(this IServiceCollection services, WebApplicationBuilder builder)
        {
            // Strategies
            services.AddSingleton(typeof(IJwtEncryptionStrategy),
                builder.Configuration.IsEncryptionAsymmetric()
                    ? typeof(AsymmetricEncryptionStrategy)
                    : typeof(SymmetricEncryptionStrategy));

            // Context
            services.AddSingleton<EncryptionContext>();
        }

        private static void RegisterLoadingStrategies(this IServiceCollection services)
        {
            // Strategy Context (acting like loader strategy facade)
            services.AddSingleton<ILoadersContext, LoadersContext>();

            // Strategies
            services.AddSingleton<AppSettingsLoader>();
            services.AddSingleton<EnvironmentLoader>();
        }

        private static void RegisterNotifyStrategies(this IServiceCollection services)
        {
            // Strategy Resolver (returning dedicated scenarios' strategy)
            services.AddSingleton<IScenariosResolver<INotifyScenario, NotificationEvent>, NotifyScenariosResolver>();

            // Strategies
            services.AddSingleton<CaseCreatedScenario>();
            services.AddSingleton<CaseStatusUpdatedScenario>();
            services.AddSingleton<CaseClosedScenario>();
            services.AddSingleton<TaskAssignedScenario>();
            services.AddSingleton<DecisionMadeScenario>();
            services.AddSingleton<MessageReceivedScenario>();
            services.AddSingleton<NotImplementedScenario>();
        }

        private static void RegisterOpenServices(this IServiceCollection services, WebApplicationBuilder builder)
        {
            byte omcWorkflowVersion = builder.Services.GetRequiredService<OmcConfiguration>().OMC.Feature.Workflow_Version();
            
            // Common query methods
            services.AddSingleton<IQueryBase, QueryBase>();

            // Strategies
            services.AddSingleton(typeof(OpenZaak.Interfaces.IQueryZaak), DetermineOpenZaakVersion(omcWorkflowVersion));
            services.AddSingleton(typeof(OpenKlant.Interfaces.IQueryKlant), DetermineOpenKlantVersion(omcWorkflowVersion));
            services.AddSingleton(typeof(Besluiten.Interfaces.IQueryBesluiten), DetermineBesluitenVersion(omcWorkflowVersion));
            services.AddSingleton(typeof(Objecten.Interfaces.IQueryObjecten), DetermineObjectenVersion(omcWorkflowVersion));
            services.AddSingleton(typeof(ObjectTypen.Interfaces.IQueryObjectTypen), DetermineObjectTypenVersion(omcWorkflowVersion));

            // Feedback and telemetry
            services.AddSingleton(typeof(ITelemetryService), DetermineTelemetryVersion(omcWorkflowVersion));

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
            services.AddSingleton(typeof(GeneralResponder), DetermineResponderVersion(omcWorkflowVersion));

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
        /// <summary>
        /// Configures the HTTP pipeline with middlewares.
        /// </summary>
        /// <param name="builder">The pre-configured <see cref="WebApplicationBuilder"/>.</param>
        /// <returns>Configured <see cref="WebApplication"/>.</returns>
        private static WebApplication ConfigureHttpPipeline(this WebApplicationBuilder builder)
        {
            WebApplication app = builder.Build();

            // Displaying Swagger UI as the main page of the Web API
            if (app.Environment.IsProduction() || app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();  // Try to redirect from HTTP to HTTPS (after first HTTP call)

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();  // Mapping actions from API controllers

            app.UseSentryTracing();  // Enable Sentry to capture transactions

            return app;
        }
        #endregion
    }
}