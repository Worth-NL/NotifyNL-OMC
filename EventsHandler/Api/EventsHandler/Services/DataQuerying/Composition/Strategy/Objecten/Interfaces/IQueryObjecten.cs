// © 2024, Worth Systems.

using EventsHandler.Behaviors.Versioning;
using EventsHandler.Configuration;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.Objecten.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    internal interface IQueryObjecten : IVersionDetails
    {
        /// <inheritdoc cref="WebApiConfiguration"/>
        protected internal WebApiConfiguration Configuration { get; set; }

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "Objecten";

        #region Domain
        /// <summary>
        /// Gets the domain part of the organization-specific (e.g., municipality) "Objecten" Web API service URI:
        /// <code>
        ///   http(s)://[DOMAIN]/ApiEndpoint
        /// </code>
        /// </summary>
        /// <exception cref="KeyNotFoundException"/>
        internal sealed string GetSpecificObjectenDomain() => this.Configuration.User.Domain.Objecten();
        #endregion
    }
}