// © 2024, Worth Systems.

using EventsHandler.Behaviors.Versioning;
using EventsHandler.Configuration;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.ObjectTypen.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "ObjectTypen" Web API service.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    internal interface IQueryObjectTypen : IVersionDetails, IDomain
    {
        /// <inheritdoc cref="WebApiConfiguration"/>
        protected internal WebApiConfiguration Configuration { get; set; }

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "ObjectTypen";

        #region Polymorphic (Domain)
        /// <inheritdoc cref="IDomain.GetDomain"/>
        string IDomain.GetDomain() => this.Configuration.User.Domain.ObjectTypen();
        #endregion
    }
}