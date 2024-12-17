// © 2024, Worth Systems.

using Common.Extensions;
using Common.Versioning.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using WebQueries.DataQuerying.Strategies.Queries.Besluiten.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.Objecten.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.ObjectTypen.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenKlant.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenZaak.Interfaces;
using WebQueries.Register.Interfaces;
using WebQueries.Versioning.Interfaces;

namespace WebQueries.Versioning
{
    /// <inheritdoc cref="IVersionRegister"/>
    public sealed class ZhvVersionRegister : IVersionRegister
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZhvVersionRegister"/> class.
        /// </summary>
        public ZhvVersionRegister(IServiceProvider serviceProvider)  // Dependency Injection (DI)
        {
            this._serviceProvider = serviceProvider;
        }

        /// <inheritdoc cref="IVersionRegister.GetVersion(string)"/>
        /// <remarks>
        /// NOTE: This implementation is handling versions of registered external API Query services.
        /// </remarks>
        string IVersionRegister.GetVersion(string _)
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
    }
}