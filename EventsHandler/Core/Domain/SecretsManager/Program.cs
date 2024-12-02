// © 2023, Worth Systems.

using Common.Settings;
using Common.Settings.Configuration;
using Common.Settings.Extensions;
using Common.Settings.Strategy.Interfaces;
using Common.Settings.Strategy.Manager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using SecretsManager.Services.Authentication.Encryptions.Strategy;
using SecretsManager.Services.Authentication.Encryptions.Strategy.Context;
using SecretsManager.Services.Authentication.Encryptions.Strategy.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace SecretsManager
{
    /// <summary>
    /// The entry point to the console application, responsible for configuring and starting its instance.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "This is startup class with dozens of dependencies")]
    internal static class Program
    {
        private static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .AddConfiguration()     // 1. Configuration (appsettings.json)
                .AddInternalServices()  // 2. Internal SecretsManager services
                .Build();
            
            // 3. Business logic
            ProduceJwtToken(host, args);
        }

        #region Configuration
        /// <summary>
        /// Adding application configurations from JSON files.
        /// </summary>
        /// <param name="builder">The builder of the web application (used for configuration).</param>
        /// <returns>Partially-configured <see cref="IHostBuilder"/> with .NET services.</returns>
        private static IHostBuilder AddConfiguration(this IHostBuilder builder)
        {
            // Configuration appsettings.json files
            return builder.ConfigureAppConfiguration((builderContext, configuration) =>
            {
                const string settingsFileName = "manager.appsettings";

                configuration.AddJsonFile($"{settingsFileName}.json", optional: false)
                             .AddJsonFile($"{settingsFileName}.{builderContext.HostingEnvironment.EnvironmentName}.json", optional: true);
            });
        }
        #endregion

        #region Services: Internal (Secrets Manager)
        /// <summary>
        /// Registration of custom services, used for business logic and internal processes.
        /// </summary>
        /// <param name="builder">The builder of the web application (used for configuration).</param>
        /// <returns>Partially-configured <see cref="IHostBuilder"/> with custom services.</returns>
        private static IHostBuilder AddInternalServices(this IHostBuilder builder)
        {
            return builder.ConfigureServices((builderContext, services) =>
            {
                // Configurations
                services.AddSingleton<WebApiConfiguration>();
                services.RegisterLoadingStrategies();

                // JWT generation
                services.RegisterEncryptionStrategy(builderContext);
            });
        }

        #region Aggregated registrations
        private static void RegisterLoadingStrategies(this IServiceCollection services)
        {
            // Strategy Context (acting like loader strategy facade)
            services.AddSingleton<ILoadersContext, LoadersContext>();

            // Strategies
            services.AddSingleton<AppSettingsLoader>();
            services.AddSingleton<EnvironmentLoader>();
        }
        
        private static void RegisterEncryptionStrategy(this IServiceCollection services, HostBuilderContext builderContext)
        {
            // Strategies
            services.AddSingleton(typeof(IJwtEncryptionStrategy),
                builderContext.Configuration.IsEncryptionAsymmetric()
                    ? typeof(AsymmetricEncryptionStrategy)
                    : typeof(SymmetricEncryptionStrategy));

            // Context
            services.AddSingleton<EncryptionContext>();
        }
        #endregion
        #endregion

        #region Business logic        
        /// <summary>
        /// Produces a new JWT token and saves it to the file.
        /// </summary>
        private static void ProduceJwtToken(IHost host, string[] args)
        {
            // Determine for how long the JWT token should be valid
            DateTime validDateTime = GetJwtTokenValidity(args);

            // Generate a new JWT token and save it to the file
            GenerateJwtToken(host, validDateTime);
        }

        private static DateTime GetJwtTokenValidity(string[] args)
        {
            double validForNextMinutes = 60;  // NOTE: Standard value, unless specified otherwise
            DateTime currentDateTime = DateTime.UtcNow;
            DateTime futureDateTime;

            // Using custom validity for JWT token
            if (args.Length != 0)
            {
                // Variant #1: Use specific validity date (e.g., valid until 31st of December 2023 at 23:59:59)
                if (DateTime.TryParse(args[0], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out futureDateTime))
                {
                    if (futureDateTime <= currentDateTime)
                    {
                        throw new ArgumentException("The expected date time should be from the future!");
                    }

                    Console.WriteLine($"The token will be valid until: {futureDateTime:F}");

                    return futureDateTime;
                }

                // Variant #2: Use specific range in minutes (e.g., valid for 120 minutes from now)
                if (double.TryParse(args[0], out double minutes))
                {
                    validForNextMinutes = minutes;
                }
                else
                {
                    throw new FormatException("The expected program argument should be date time (e.g., 2023-12-31T23:59:59) or minutes!");
                }
            }

            futureDateTime = currentDateTime.AddMinutes(validForNextMinutes);

            Console.WriteLine($"The token will be valid for the next {validForNextMinutes} minutes");

            return futureDateTime;
        }

        private static void GenerateJwtToken(IHost host, DateTime validDateTime)
        {
            // Resolve dependencies
            WebApiConfiguration configuration = host.Services.GetRequiredService<WebApiConfiguration>();
            EncryptionContext context = host.Services.GetRequiredService<EncryptionContext>();

            // Get security key
            SecurityKey securityKey = context.GetSecurityKey(configuration.OMC.Auth.JWT.Secret());

            // Generate JSON Web Token
            string jwtToken = context.GetJwtToken(
                securityKey,
                issuer: configuration.OMC.Auth.JWT.Issuer(),
                audience: configuration.OMC.Auth.JWT.Audience(),
                expiresAt: validDateTime,
                userId: configuration.OMC.Auth.JWT.UserId(),
                userName: configuration.OMC.Auth.JWT.UserName());

            // Write JWT tokens
            context.SaveJwtToken(jwtToken);
        }
        #endregion
    }
}