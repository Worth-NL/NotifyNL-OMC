// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.Objecten;
using EventsHandler.Behaviors.Versioning;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;
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
        #endregion

        #region Polymorphic (Domain)
        /// <inheritdoc cref="IDomain.GetDomain"/>
        string IDomain.GetDomain() => this.Configuration.User.Domain.Objecten();
        #endregion
    }
}