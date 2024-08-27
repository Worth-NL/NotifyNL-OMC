// © 2023, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Extensions;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing;
using EventsHandler.Services.DataProcessing.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases;
using EventsHandler.Services.DataProcessing.Strategy.Manager;
using EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataQuerying;
using EventsHandler.Services.DataQuerying.Adapter;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Base;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.Objecten.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.Objecten.v1;
using EventsHandler.Services.DataQuerying.Composition.Strategy.ObjectTypen.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.ObjectTypen.v1;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.DataSending;
using EventsHandler.Services.DataSending.Clients.Factories;
using EventsHandler.Services.DataSending.Clients.Factories.Interfaces;
using EventsHandler.Services.DataSending.Clients.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.Register.Interfaces;
using EventsHandler.Services.Responding.Interfaces;
using EventsHandler.Services.Responding.Results.Builder;
using EventsHandler.Services.Responding.Results.Builder.Interface;
using EventsHandler.Services.Serialization;
using EventsHandler.Services.Serialization.Interfaces;
using EventsHandler.Services.Settings;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Settings.Strategy.Interfaces;
using EventsHandler.Services.Settings.Strategy.Manager;
using EventsHandler.Services.Templates;
using EventsHandler.Services.Templates.Interfaces;
using EventsHandler.Services.Validation;
using EventsHandler.Services.Validation.Interfaces;
using EventsHandler.Services.Versioning;
using EventsHandler.Services.Versioning.Interfaces;
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
using Register = EventsHandler.Services.Register;
using Responder = EventsHandler.Services.Responding;

namespace EventsHandler
{
    /// <summary>
    /// The entry point to the Web API application, responsible for configuring and starting the application.
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
                    Version = Resources.Swagger_UI_Version,
                    Title = Resources.Swagger_UI_Title,
                    Description = Resources.Swagger_UI_Description
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
                    Description = Resources.Swagger_UI_Authentication_Description,
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
            options.Dsn = Environment.GetEnvironmentVariable(DefaultValues.EnvironmentVariables.SentryDsn)
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

            // Version of the application ("OMC Web API" in this case)
            options.Release = DefaultValues.ApiController.Version;

            // The environment of the application (Prod, Test, Dev, Staging, etc.)
            options.Environment = Environment.GetEnvironmentVariable(DefaultValues.EnvironmentVariables.SentryEnvironment) ??
                                  Environment.GetEnvironmentVariable(DefaultValues.EnvironmentVariables.AspNetCoreEnvironment) ??
                                  DefaultValues.EnvironmentVariables.Missing;
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
            builder.Services.AddSingleton<INotifyService<NotificationEvent, NotifyData>, NotifyService>();
            builder.Services.RegisterNotifyStrategies();

            // Domain queries and resources
            builder.Services.AddSingleton<IDataQueryService<NotificationEvent>, DataQueryService>();
            builder.Services.AddSingleton<IQueryContext, QueryContext>();
            builder.RegisterOpenServices();

            // HTTP communication
            builder.Services.AddSingleton<IHttpNetworkService, HttpNetworkService>();
            builder.Services.RegisterClientFactories();

            // Versioning
            builder.Services.AddSingleton<IVersionsRegister, VersionsRegister>();

            // User Interaction
            builder.RegisterResponders();
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
            services.AddSingleton<CaseClosedScenario>();
            services.AddSingleton<TaskAssignedScenario>();
            services.AddSingleton<DecisionMadeScenario>();
            services.AddSingleton<MessageReceivedScenario>();
            services.AddSingleton<NotImplementedScenario>();
        }

        private static void RegisterOpenServices(this WebApplicationBuilder builder)
        {
            // Common query methods
            builder.Services.AddSingleton<IQueryBase, QueryBase>();

            byte omcWorkflowVersion = builder.Configuration.OmcWorkflowVersion();

            // Strategies
            builder.Services.AddSingleton(typeof(OpenZaak.Interfaces.IQueryZaak), DetermineOpenZaakVersion(omcWorkflowVersion));
            builder.Services.AddSingleton(typeof(OpenKlant.Interfaces.IQueryKlant), DetermineOpenKlantVersion(omcWorkflowVersion));
            builder.Services.AddSingleton(typeof(IQueryObjecten), DetermineObjectenVersion(omcWorkflowVersion));
            builder.Services.AddSingleton(typeof(IQueryObjectTypen), DetermineObjectTypenVersion(omcWorkflowVersion));

            // Feedback and telemetry
            builder.Services.AddSingleton(typeof(ITelemetryService), DetermineTelemetryVersion(omcWorkflowVersion));

            return;

            static Type DetermineOpenZaakVersion(byte omcWorkflowVersion)
            {
                return omcWorkflowVersion switch
                {
                    1 => typeof(OpenZaak.v1.QueryZaak),
                    2 => typeof(OpenZaak.v2.QueryZaak),
                    _ => throw new NotImplementedException(Resources.Configuration_ERROR_VersionOpenZaakUnknown)
                };
            }

            static Type DetermineOpenKlantVersion(byte omcWorkflowVersion)
            {
                return omcWorkflowVersion switch
                {
                    1 => typeof(OpenKlant.v1.QueryKlant),
                    2 => typeof(OpenKlant.v2.QueryKlant),
                    _ => throw new NotImplementedException(Resources.Configuration_ERROR_VersionOpenKlantUnknown)
                };
            }

            static Type DetermineObjectenVersion(byte omcWorkflowVersion)
            {
                return omcWorkflowVersion switch
                {
                    1 or 2 => typeof(QueryObjecten),
                    _ => throw new NotImplementedException(Resources.Configuration_ERROR_VersionObjectenUnknown)
                };
            }

            static Type DetermineObjectTypenVersion(byte omcWorkflowVersion)
            {
                return omcWorkflowVersion switch
                {
                    1 or 2 => typeof(QueryObjectTypen),
                    _ => throw new NotImplementedException(Resources.Configuration_ERROR_VersionObjectTypenUnknown)
                };
            }

            static Type DetermineTelemetryVersion(byte omcWorkflowVersion)
            {
                return omcWorkflowVersion switch
                {
                    1 => typeof(Register.v1.ContactRegistration),
                    2 => typeof(Register.v2.ContactRegistration),
                    _ => throw new NotImplementedException(Resources.Configuration_ERROR_VersionTelemetryUnknown)
                };
            }
        }

        private static void RegisterClientFactories(this IServiceCollection services)
        {
            services.AddSingleton<IHttpClientFactory<HttpClient, (string, string)[]>, RegularHttpClientFactory>();
            services.AddSingleton<IHttpClientFactory<INotifyClient, string>, NotificationClientFactory>();
        }

        private static void RegisterResponders(this WebApplicationBuilder builder)
        {
            // Implicit interface (Adapter) used by EventsController => check "IRespondingService<TModel>"
            builder.Services.AddSingleton<IRespondingService<NotificationEvent>, Responder.OmcResponder>();

            // Explicit interfaces (generic) used by other controllers => check "IRespondingService<TResult, TDetails>"
            builder.Services.AddSingleton(typeof(Responder.NotifyResponder), DetermineResponderVersion(builder));

            return;

            static Type DetermineResponderVersion(WebApplicationBuilder builder)
            {
                return builder.Configuration.OmcWorkflowVersion() switch
                {
                    1 => typeof(Responder.v1.NotifyCallbackResponder),
                    2 => typeof(Responder.v2.NotifyCallbackResponder),
                    _ => throw new NotImplementedException(Resources.Configuration_ERROR_VersionNotifyResponderUnknown)
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