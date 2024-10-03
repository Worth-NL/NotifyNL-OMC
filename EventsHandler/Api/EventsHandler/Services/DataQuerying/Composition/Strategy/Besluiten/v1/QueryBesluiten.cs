// © 2024, Worth Systems.

using EventsHandler.Services.DataQuerying.Composition.Strategy.Besluiten.Interfaces;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Versioning.Interfaces;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.Besluiten.v1
{
    /// <inheritdoc cref="IQueryBesluiten"/>
    /// <remarks>
    ///   Version: "Besluiten" (v1+) Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    internal sealed class QueryBesluiten : IQueryBesluiten
    {
        /// <inheritdoc cref="IQueryBesluiten.Configuration"/>
        WebApiConfiguration IQueryBesluiten.Configuration { get; set; } = null!;

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Version => "1.1.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBesluiten"/> class.
        /// </summary>
        public QueryBesluiten(WebApiConfiguration configuration)
        {
            ((IQueryBesluiten)this).Configuration = configuration;
        }
    }
}