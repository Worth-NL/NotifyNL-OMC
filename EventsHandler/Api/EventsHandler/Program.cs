// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Communication.Strategy.Manager;
using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Enums;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Responding.Results.Builder;
using EventsHandler.Behaviors.Responding.Results.Builder.Interface;
using EventsHandler.Configuration;
using EventsHandler.Constants;
using EventsHandler.Extensions;
using EventsHandler.Properties;
using EventsHandler.Services.DataLoading;
using EventsHandler.Services.DataLoading.Strategy.Interfaces;
using EventsHandler.Services.DataLoading.Strategy.Manager;
using EventsHandler.Services.DataProcessing;
using EventsHandler.Services.DataProcessing.Interfaces;
using EventsHandler.Services.DataQuerying;
using EventsHandler.Services.DataQuerying.Adapter;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Base;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.DataReceiving;
using EventsHandler.Services.DataReceiving.Factories;
using EventsHandler.Services.DataReceiving.Factories.Interfaces;
using EventsHandler.Services.DataReceiving.Interfaces;
using EventsHandler.Services.DataSending;
using EventsHandler.Services.DataSending.Clients.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.Serialization;
using EventsHandler.Services.Serialization.Interfaces;
using EventsHandler.Services.Telemetry;
using EventsHandler.Services.Telemetry.Interfaces;
using EventsHandler.Services.Templates;
using EventsHandler.Services.Templates.Interfaces;
using EventsHandler.Services.UserCommunication;
using EventsHandler.Services.UserCommunication.Interfaces;
using EventsHandler.Services.Validation;
using EventsHandler.Services.Validation.Interfaces;
using EventsHandler.Utilities.Swagger.Examples;
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
using OpenKlant = EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant;
using OpenZaak = EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak;

namespace EventsHandler
{
    /// <summary>
    /// The entry point to the Web API application, responsible for configuring and starting the application.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class Program
    {
        /// <summary>
        /// Custom simplified version of application configuration.
        /// </summary>
        /// <param name="args">The <see cref="Program"/> startup arguments.</param>
        internal static void Main(string[] args)
        {
            WebApplication.CreateBuilder(args)
                .ConfigureServices()      // 1. Add and configure different types of services used by this application
                .ConfigureHttpPipeline()  // 2. Configure pipeline what should happen during HTTP request-response cycle
                .Run();                   // 3. Start the application
        }

        #region Services: External (.NET)
        /// <summary>
        /// Adds services to the <see cref="IServiceCollection"/> container.
        /// </summary>
        /// <returns>Configured <see cref="WebApplicationBuilder"/>.</returns>
        private static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
        {
            return builder
                .AddNetServices()      // Microsoft .NET services
                .AddCustomServices();  // Our custom Web API services
        }

        /// <summary>
        /// Registration of .NET services, used by the application.
        /// </summary>
        /// <param name="builder">The builder of the web application (used for configuration).</param>
        /// <returns>Partially-configured <see cref="WebApplicationBuilder"/> with .NET services.</returns>
        private static WebApplicationBuilder AddNetServices(this WebApplicationBuilder builder)
        {
            // Configuration appsettings.json files
            const string settingsFileName = "appsettings";
            builder.Configuration.AddJsonFile($"{settingsFileName}.json", optional: false)
                                 .AddJsonFile($"{settingsFileName}.{builder.Environment.EnvironmentName}.json", optional: true);
            
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
                WebApiConfiguration configuration = builder.Services.GetRequiredService<WebApiConfiguration>();

                // Disable some default validations, preventing the JWT token to be recognized as valid
                setup.TokenValidationParameters = new TokenValidationParameters
                {
                    // Validation parameters
                    ValidIssuer = configuration.OMC.Authorization.JWT.Issuer(),
                    ValidAudience = configuration.OMC.Authorization.JWT.Audience(),
                    IssuerSigningKey = encryptionContext.GetSecurityKey(configuration.OMC.Authorization.JWT.Secret()),

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
                    Version = Resources.Swagger_Version,
                    Title = Resources.Swagger_Title,
                    Description = Resources.Swagger_Description
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
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = DefaultValues.Authorization.OpenApiSecurityScheme.BearerFormat,
                    Name = DefaultValues.Authorization.Name,
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = Resources.Swagger_Authentication_Description,
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
            builder.Services.AddSwaggerExamplesFromAssemblyOf<NotificationEventExample>();

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
            options.Dsn = Environment.GetEnvironmentVariable("SENTRY_DSN") ?? string.Empty;  // NOTE: SentrySDK will automatically reach "SENTRY_DSN" environment variable so, it's not needed to
                                                                                             // do this manually; however, if this variable is not existing Sentry will throw ArgumentNullException.
                                                                                             // The current fallback scenario is just disabling Sentry logging in case of missing DSN (no exception).
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
                
            // Version of the application ("OMC Web API" in this case)
            options.Release = DefaultValues.ApiController.Version;
            
            // The environment of the application (Prod, Test, Dev, Staging, etc.)
            options.Environment = Environment.GetEnvironmentVariable("SENTRY_ENVIRONMENT") ??
                                  Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        }
        #endregion
        #endregion

        #region Services: Internal (OMC)
        /// <summary>
        /// Registration of custom services, used for business logic and internal processes.
        /// </summary>
        /// <param name="builder">The builder of the web application (used for configuration).</param>
        /// <returns>Partially-configured <see cref="WebApplicationBuilder"/> with custom services.</returns>
        private static WebApplicationBuilder AddCustomServices(this WebApplicationBuilder builder)
        {
            // Configurations
            builder.Services.AddSingleton<WebApiConfiguration>();
            builder.RegisterEncryptionStrategy();
            builder.Services.RegisterLoadingStrategies();

            // Business logic
            builder.Services.AddSingleton<IValidationService<NotificationEvent>, NotificationValidator>();
            builder.Services.AddSingleton<ISerializationService, SpecificSerializer>();
            builder.Services.AddSingleton<IProcessingService<NotificationEvent>, NotifyProcessor>();
            builder.Services.AddSingleton<ITemplatesService<TemplateResponse, NotificationEvent>, NotifyTemplatesAnalyzer>();
            builder.Services.AddSingleton<ISendingService<NotificationEvent, NotifyData>, NotifySender>();
            builder.Services.RegisterNotifyStrategies();

            // Queries and HTTP resources
            builder.Services.AddSingleton<IDataQueryService<NotificationEvent>, DataQueryService>();
            builder.Services.AddSingleton<IQueryContext, QueryContext>();
            builder.RegisterOpenServices();

            // HTTP communication + authorization
            builder.Services.AddSingleton<IHttpNetworkService, HttpNetworkService>();
            builder.Services.RegisterClientFactories();

            // Feedback and telemetry
            builder.Services.AddSingleton<ITelemetryService, ContactRegistration>();

            // User Interaction
            builder.Services.RegisterResponders();
            builder.Services.AddSingleton<IDetailsBuilder, DetailsBuilder>();

            return builder;
        }

        #region Aggregated registrations
        private static void RegisterEncryptionStrategy(this WebApplicationBuilder builder)
        {
            // Strategies
            builder.Services.AddSingleton(typeof(IJwtEncryptionStrategy),
                builder.Configuration.IsEncryptionAsymmetric()
                    ? typeof(AsymmetricEncryptionStrategy)
                    : typeof(SymmetricEncryptionStrategy));

            // Context
            builder.Services.AddSingleton<EncryptionContext>();
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
            services.AddSingleton<IScenariosResolver, ScenariosResolver>();

            // Strategies
            services.AddSingleton<CaseCreatedScenario>();
            services.AddSingleton<CaseStatusUpdatedScenario>();
            services.AddSingleton<CaseFinishedScenario>();
            services.AddSingleton<NotImplementedScenario>();
        }

        private static void RegisterOpenServices(this WebApplicationBuilder builder)
        {
            // Common query methods
            builder.Services.AddSingleton<IQueryBase, QueryBase>();

            // Strategies
            // TODO: To be moved into strategies
            // TODO: Implement "GetOrAddService" method
            builder.Services.AddSingleton(typeof(OpenZaak.Interfaces.IQueryZaak), DetermineOpenZaakVersion(builder));
            builder.Services.AddSingleton(typeof(OpenKlant.Interfaces.IQueryKlant), DetermineOpenKlantVersion(builder));
            return;

            // NOTE: Versions
            // 
            // "OpenServices" v1 workflow:
            // - "OpenZaak" v1.0.0
            // - "OpenKlant" v1.0.0
            // - "ContactMomenten"
            //
            // "OpenServices" v2 workflow:
            // - "OpenZaak" v1.0.0 (enhanced models)
            // - "OpenKlant" v2.0.0
            // - "KlantContacten"

            static Type DetermineOpenZaakVersion(WebApplicationBuilder builder)
            {
                return builder.Configuration.OpenServicesVersion() switch
                {
                    1 => typeof(OpenZaak.v1.QueryZaak),
                    2 => typeof(OpenZaak.v2.QueryZaak),
                    _ => throw new NotImplementedException(Resources.Configuration_ERROR_OpenZaakVersionUnknown)
                };
            }

            static Type DetermineOpenKlantVersion(WebApplicationBuilder builder)
            {
                return builder.Configuration.OpenServicesVersion() switch
                {
                    1 => typeof(OpenKlant.v1.QueryKlant),
                    2 => typeof(OpenKlant.v2.QueryKlant),
                    _ => throw new NotImplementedException(Resources.Configuration_ERROR_OpenKlantVersionUnknown)
                };
            }
        }

        private static void RegisterClientFactories(this IServiceCollection services)
        {
            services.AddSingleton<IHttpClientFactory<HttpClient, (string, string)[]>, RegularHttpClientFactory>();
            services.AddSingleton<IHttpClientFactory<INotifyClient, string>, NotificationClientFactory>();
        }

        private static void RegisterResponders(this IServiceCollection services)
        {
            // Implicit interface (Adapter) for the main EventsController (is used most often, and it looks cleaner with single generic)
            services.AddSingleton<IRespondingService<NotificationEvent>, NotificationResponder>();
            
            // Explicit interfaces
            services.AddSingleton<IRespondingService<ProcessingResult, string>, NotifyResponder>();
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
            
            // Development settings
            if (app.Environment.IsDevelopment())
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