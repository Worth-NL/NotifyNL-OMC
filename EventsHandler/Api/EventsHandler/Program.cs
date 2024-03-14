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
using EventsHandler.Services.DataLoading.Interfaces;
using EventsHandler.Services.DataLoading.Strategy.Interfaces;
using EventsHandler.Services.DataLoading.Strategy.Manager;
using EventsHandler.Services.DataProcessing;
using EventsHandler.Services.DataProcessing.Interfaces;
using EventsHandler.Services.DataQuerying;
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
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Notify.Models.Responses;
using SecretsManager.Services.Authentication.Encryptions.Strategy;
using SecretsManager.Services.Authentication.Encryptions.Strategy.Context;
using SecretsManager.Services.Authentication.Encryptions.Strategy.Interfaces;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using ConfigurationExtensions = EventsHandler.Extensions.ConfigurationExtensions;

namespace EventsHandler
{
    /// <summary>
    /// The entry point to the Web API application, responsible for configuring and starting the application.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Custom simplified version of application configuration.
        /// </summary>
        private static void Main(string[] args)
        {
            ConfigureServices(args)       // 1. Add and configure different types of services used by this application
                .ConfigureHttpPipeline()  // 2. Configure pipeline what should happen during HTTP request-response cycle
                .Run();                   // 3. Start the application
        }

        /// <summary>
        /// Adds services to the <see cref="IServiceCollection"/> container.
        /// </summary>
        /// <param name="args">The <see cref="Program"/> startup arguments.</param>
        /// <returns>Configured <see cref="WebApplicationBuilder"/>.</returns>
        private static WebApplicationBuilder ConfigureServices(string[] args)
        {
            return WebApplication.CreateBuilder(args)
                .AddCustomServices()  // Our custom Web API services
                .AddNetServices();    // Microsoft .NET services
        }

        /// <summary>
        /// Registration of .NET services, used by the application.
        /// </summary>
        /// <param name="builder">The builder of the web application (used for configuration).</param>
        /// <returns>Partially-configured <see cref="WebApplicationBuilder"/> with .NET services.</returns>
        private static WebApplicationBuilder AddNetServices(this WebApplicationBuilder builder)
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
                                
                                // Disable some default validations, preventing the JWT token to be recognized as valid
                                setup.TokenValidationParameters = new TokenValidationParameters
                                {
                                    // Validation parameters
                                    ValidIssuer = ConfigurationExtensions.GetNotifyJwtIssuer(),
                                    ValidAudience = ConfigurationExtensions.GetNotifyJwtAudience(),
                                    IssuerSigningKey = encryptionContext.GetSecurityKey(ConfigurationExtensions.GetNotifyJwtSecret()),

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

            // Swagger UI: Examples
            builder.Services.AddSwaggerExamplesFromAssemblyOf<NotificationEventExample>();

            // Application insights: Monitoring
            builder.Services.AddApplicationInsightsTelemetry();

            // Application insights: Logging
            builder.Logging.AddApplicationInsights()
                           .AddFilter<ApplicationInsightsLoggerProvider>(
                               DefaultValues.Logging.Category,
                               builder.Configuration.GetApplicationInsightsLogLevel());

            return builder;
        }

        /// <summary>
        /// Registration of custom services, used for business logic and internal processes.
        /// </summary>
        /// <param name="builder">The builder of the web application (used for configuration).</param>
        /// <returns>Partially-configured <see cref="WebApplicationBuilder"/> with custom services.</returns>
        private static WebApplicationBuilder AddCustomServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<WebApiConfiguration>();

            builder.RegisterEncryptionStrategy();
            builder.Services.RegisterLoadingStrategies();
            builder.Services.AddSingleton<ITelemetryInitializer, AzureTelemetryService>();
            builder.Services.AddSingleton<IValidationService<NotificationEvent>, NotificationValidator>();
            builder.Services.AddSingleton<IDetailsBuilder, DetailsBuilder>();
            builder.Services.AddSingleton<ISerializationService, SpecificSerializer>();
            builder.Services.AddSingleton<IProcessingService<NotificationEvent>, NotifyProcessor>();
            builder.Services.RegisterNotifyStrategies();
            builder.Services.AddSingleton<IDataQueryService<NotificationEvent>, ApiDataQuery>();
            builder.Services.AddSingleton<IHttpSupplierService, JwtHttpSupplier>();
            builder.Services.RegisterClientFactories();
            builder.Services.AddSingleton<ITemplatesService<TemplateResponse, NotificationEvent>, NotifyTemplatesAnalyzer>();
            builder.Services.AddSingleton<ISendingService<NotificationEvent, NotifyData>, NotifySender>();
            builder.Services.AddSingleton<IFeedbackTelemetryService, NotifyTelemetryService>();
            builder.Services.RegisterResponders();

            return builder;
        }

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
            // Default configuration loader strategy
            services.AddSingleton<ILoadingService, EnvironmentLoader>();

            // Strategy Context (acting like loader strategy facade)
            services.AddSingleton<ILoadersContext, LoadersContext>();

            // Strategies
            services.AddSingleton<ConfigurationLoader>();
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

        private static void RegisterClientFactories(this IServiceCollection services)
        {
            services.AddSingleton<IHttpClientFactory<HttpClient, (string, string)[]>, HeadersHttpClientFactory>();
            services.AddSingleton<IHttpClientFactory<INotifyClient, string>, NotificationClientFactory>();
        }

        private static void RegisterResponders(this IServiceCollection services)
        {
            // Implicit interface (Adapter) for the main EventsController (is used most often, and it looks cleaner with single generic)
            services.AddSingleton<IRespondingService<NotificationEvent>, NotificationResponder>();
            
            // Explicit interfaces
            services.AddSingleton<IRespondingService<ProcessingResult, string>, NotifyResponder>();
        }

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

            // Production settings
            app.UseHttpsRedirection();  // Try to redirect from HTTP to HTTPS (after first HTTP call)

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.MapControllers();  // Mapping actions from API controllers

            return app;
        }
    }
}