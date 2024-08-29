// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.Objecten.Task;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataSending.Clients.Enums;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Versioning.Interfaces;
using System.Text.Json;
using EventsHandler.Mapping.Models.POCOs.Objecten.Message;
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

        #region Parent (Task)
        #pragma warning disable CA1822  // The method can be marked as static but that would be inconsistent for interfaces
        /// <summary>
        /// Gets the <see cref="TaskObject"/> from "Objecten" Web API service.
        /// </summary>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal sealed async Task<TaskObject> GetTaskAsync(IQueryBase queryBase)
        {
            // TODO: Main Object validation

            return await queryBase.ProcessGetAsync<TaskObject>(
                httpClientType: HttpClientTypes.Objecten,
                uri: queryBase.Notification.MainObjectUri,  // Request URL
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoTask);
        }
        #pragma warning restore CA1822
        #endregion

        #region Parent (Message)
        #pragma warning disable CA1822  // The method can be marked as static but that would be inconsistent for interfaces
        /// <summary>
        /// Gets the <see cref="MessageObject"/> from "Objecten" Web API service.
        /// </summary>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal sealed async Task<MessageObject> GetMessageAsync(IQueryBase queryBase)
        {
            // TODO: Main Object validation

            return await queryBase.ProcessGetAsync<MessageObject>(
                httpClientType: HttpClientTypes.Objecten,
                uri: queryBase.Notification.MainObjectUri,  // Request URL
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoMessage);
        }
        #pragma warning restore CA1822
        #endregion

        #region Polymorphic (Domain)
        /// <inheritdoc cref="IDomain.GetDomain"/>
        string IDomain.GetDomain() => this.Configuration.User.Domain.Objecten();
        #endregion
    }
}