// © 2024, Worth Systems.

using EventsHandler.Behaviors.Versioning;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Composition.Strategy.Objecten.Interfaces;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.Objecten.v1
{
    /// <inheritdoc cref="IQueryObjecten"/>
    /// <remarks>
    ///   Version: "Objecten" (v2+) Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    internal sealed class QueryObjecten : IQueryObjecten
    {
        /// <inheritdoc cref="IQueryObjecten.Configuration"/>
        WebApiConfiguration IQueryObjecten.Configuration { get; set; } = null!;
        
        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Version => "2.3.1";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryObjecten"/> class.
        /// </summary>
        public QueryObjecten(WebApiConfiguration configuration)
        {
            ((IQueryObjecten)this).Configuration = configuration;
        }
    }
}