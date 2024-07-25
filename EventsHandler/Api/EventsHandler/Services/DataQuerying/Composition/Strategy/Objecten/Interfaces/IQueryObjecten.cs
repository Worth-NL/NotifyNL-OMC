// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.Objecten;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataSending.Clients.Enums;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Versioning.Interfaces;
using System.Text.Json;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.Objecten.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    internal interface IQueryObjecten : IVersionDetails, IDomain
    {
        /// <inheritdoc cref="WebApiConfiguration"/>
        protected internal WebApiConfiguration Configuration { get; set; }

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "Objecten";

        #region Parent
#pragma warning disable CA1822  // The method can be marked as static but that would be inconsistent for interfaces
        /// <summary>
        /// Gets the <see cref="TaskObject"/> from "Objecten" Web API service.
        /// </summary>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal sealed async Task<TaskObject> GetTaskAsync(IQueryBase queryBase)
        {
            return await queryBase.ProcessGetAsync<TaskObject>(
                httpClientType: HttpClientTypes.Objecten,
                uri: queryBase.Notification.MainObject,  // Request URL
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoTask);
        }
#pragma warning restore CA1822
        #endregion

        #region Polymorphic (Domain)
        /// <inheritdoc cref="IDomain.GetDomain"/>
        string IDomain.GetDomain() => this.Configuration.User.Domain.Objecten();
        #endregion
    }
}