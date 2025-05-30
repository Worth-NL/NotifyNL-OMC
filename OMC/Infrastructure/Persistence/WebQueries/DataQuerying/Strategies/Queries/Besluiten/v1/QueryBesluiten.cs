﻿// © 2024, Worth Systems.

using Common.Settings.Configuration;
using WebQueries.DataQuerying.Strategies.Queries.Besluiten.Interfaces;
using WebQueries.Versioning.Interfaces;

namespace WebQueries.DataQuerying.Strategies.Queries.Besluiten.v1
{
    /// <inheritdoc cref="IQueryBesluiten"/>
    /// <remarks>
    ///   Version: "Besluiten" (v1) Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    public sealed class QueryBesluiten : IQueryBesluiten
    {
        /// <inheritdoc cref="IQueryBesluiten.Configuration"/>
        OmcConfiguration IQueryBesluiten.Configuration { get; set; } = null!;

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Version => "1.1.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBesluiten"/> class.
        /// </summary>
        public QueryBesluiten(OmcConfiguration configuration)  // Dependency Injection (DI)
        {
            ((IQueryBesluiten)this).Configuration = configuration;
        }
    }
}