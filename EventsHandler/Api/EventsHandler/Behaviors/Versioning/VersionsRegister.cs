// © 2024, Worth Systems.

using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces;
using EventsHandler.Services.Telemetry.Interfaces;

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
            var services = new IVersionDetails[]
            {
                this._serviceProvider.GetRequiredService<IQueryZaak>(),
                this._serviceProvider.GetRequiredService<IQueryKlant>(),
                this._serviceProvider.GetRequiredService<ITelemetryService>()
            };

            return $"({string.Join(", ", services.Select(service => $"{service.Name} v{service.Version}"))})";
        }
    }
}