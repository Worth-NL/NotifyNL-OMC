// © 2024, Worth Systems.

using EventsHandler.Services.DataQuerying.Composition.Strategy.ObjectTypen.Interfaces;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Versioning.Interfaces;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.ObjectTypen.v1
{
    /// <inheritdoc cref="IQueryObjectTypen"/>
    /// <remarks>
    ///   Version: "ObjectTypen" (v2+) Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    internal sealed class QueryObjectTypen : IQueryObjectTypen
    {
        /// <inheritdoc cref="IQueryObjectTypen.Configuration"/>
        WebApiConfiguration IQueryObjectTypen.Configuration { get; set; } = null!;
        
        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Version => "2.2.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryObjectTypen"/> class.
        /// </summary>
        public QueryObjectTypen(WebApiConfiguration configuration)
        {
            ((IQueryObjectTypen)this).Configuration = configuration;
        }
    }
}