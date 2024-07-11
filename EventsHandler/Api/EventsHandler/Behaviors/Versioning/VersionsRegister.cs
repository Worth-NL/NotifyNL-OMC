// © 2024, Worth Systems.

using EventsHandler.Services.DataQuerying.Composition.Strategy.Objecten.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.ObjectTypen.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces;
using EventsHandler.Services.Telemetry.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace EventsHandler.Behaviors.Versioning
{
    /// <inheritdoc cref="IVersionsRegister"/>
    internal sealed class VersionsRegister : IVersionsRegister
    {
        private readonly IServiceProvider _serviceProvider;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionsRegister"/> class.
        /// </summary>
        public VersionsRegister(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        /// <inheritdoc cref="IVersionsRegister.GetVersions()"/>
        string IVersionsRegister.GetVersions()
        {
            IVersionDetails[] services;

            try
            {
                services = new IVersionDetails[]
                {
                    this._serviceProvider.GetRequiredService<IQueryZaak>(),
                    this._serviceProvider.GetRequiredService<IQueryKlant>(),
                    this._serviceProvider.GetRequiredService<IQueryObjecten>(),
                    this._serviceProvider.GetRequiredService<IQueryObjectTypen>(),
                    this._serviceProvider.GetRequiredService<ITelemetryService>()
                };
            }
            catch (InvalidOperationException)
            {
                services = Array.Empty<IVersionDetails>();
            }

            return services.IsNullOrEmpty()
                ? string.Empty
                : $"({string.Join(", ", services.Select(service => $"{service.Name} v{service.Version}"))})";
        }
    }
}