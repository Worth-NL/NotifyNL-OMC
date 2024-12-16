﻿// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Versioning.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.ObjectTypen.Interfaces;

namespace WebQueries.DataQuerying.Strategies.Queries.ObjectTypen.v1
{
    /// <inheritdoc cref="IQueryObjectTypen"/>
    /// <remarks>
    ///   Version: "ObjectTypen" (v1) Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    public sealed class QueryObjectTypen : IQueryObjectTypen
    {
        /// <inheritdoc cref="IQueryObjectTypen.Configuration"/>
        WebApiConfiguration IQueryObjectTypen.Configuration { get; set; } = null!;

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Version => "2.2.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryObjectTypen"/> class.
        /// </summary>
        public QueryObjectTypen(WebApiConfiguration configuration)  // Dependency Injection (DI)
        {
            ((IQueryObjectTypen)this).Configuration = configuration;
        }
    }
}