// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Extensions;
using EventsHandler.Services.DataQuerying.Composition.Strategy.Besluiten.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.Objecten.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.ObjectTypen.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces;
using EventsHandler.Services.Register.Interfaces;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Versioning.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.Versioning
{
    /// <inheritdoc cref="IVersionsRegister"/>
    internal sealed class VersionsRegister : IVersionsRegister
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly WebApiConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionsRegister"/> class.
        /// </summary>
        public VersionsRegister(IServiceProvider serviceProvider, WebApiConfiguration configuration)
        {
            this._serviceProvider = serviceProvider;
            this._configuration = configuration;
        }

        /// <inheritdoc cref="IVersionsRegister.GetApisVersions()"/>
        string IVersionsRegister.GetApisVersions()
        {
            IVersionDetails[] services;

            try
            {
                services =
                [
                    this._serviceProvider.GetRequiredService<IQueryZaak>(),
                    this._serviceProvider.GetRequiredService<IQueryKlant>(),
                    this._serviceProvider.GetRequiredService<IQueryBesluiten>(),
                    this._serviceProvider.GetRequiredService<IQueryObjecten>(),
                    this._serviceProvider.GetRequiredService<IQueryObjectTypen>(),
                    this._serviceProvider.GetRequiredService<ITelemetryService>()
                ];
            }
            catch (InvalidOperationException)
            {
                services = [];
            }

            return services.IsNullOrEmpty()
                ? string.Empty
                : services.Select(service => $"{service.Name} v{service.Version}").Join();
        }

        /// <inheritdoc cref="IVersionsRegister.GetOmcVersion(string)"/>
        string IVersionsRegister.GetOmcVersion(string componentsVersions)
        {
            return string.Format(Resources.Events_OmcVersionSummary,
            /* {0} */ Resources.Application_Name,
            /* {1} */ DefaultValues.ApiController.Version,
            /* {2} */ Environment.GetEnvironmentVariable(DefaultValues.EnvironmentVariables.AspNetCoreEnvironment),
            /* {3} */ this._configuration.OMC.Features.Workflow_Version(),
            /* {4} */ componentsVersions);
        }
    }
}