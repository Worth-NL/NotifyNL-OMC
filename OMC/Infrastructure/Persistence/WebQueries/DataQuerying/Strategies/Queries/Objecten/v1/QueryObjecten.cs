// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Versioning.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.Objecten.Interfaces;
using WebQueries.Versioning.Interfaces;

namespace WebQueries.DataQuerying.Strategies.Queries.Objecten.v1
{
    /// <inheritdoc cref="IQueryObjecten"/>
    /// <remarks>
    ///   Version: "Objecten" (v1) Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    public sealed class QueryObjecten : IQueryObjecten
    {
        /// <inheritdoc cref="IQueryObjecten.Configuration"/>
        WebApiConfiguration IQueryObjecten.Configuration { get; set; } = null!;

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Version => "2.3.1";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryObjecten"/> class.
        /// </summary>
        public QueryObjecten(WebApiConfiguration configuration)  // Dependency Injection (DI)
        {
            ((IQueryObjecten)this).Configuration = configuration;
        }
    }
}