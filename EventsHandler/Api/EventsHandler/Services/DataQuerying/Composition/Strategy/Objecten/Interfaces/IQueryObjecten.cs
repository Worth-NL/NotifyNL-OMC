// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Mapping.Models.POCOs.Objecten.Message;
using EventsHandler.Mapping.Models.POCOs.Objecten.Task;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataSending.Clients.Enums;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.DataSending.Responses;
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
    /// <seealso cref="IDomain"/>
    internal interface IQueryObjecten : IVersionDetails, IDomain
    {
        /// <inheritdoc cref="WebApiConfiguration"/>
        protected internal WebApiConfiguration Configuration { get; set; }

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "Objecten";

        #pragma warning disable CA1822  // These methods can be marked as static but that would be inconsistent for interfaces
        #region Parent (Task)
        /// <summary>
        /// Gets the <see cref="CommonTaskData"/> from "Objecten" Web API service.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException">
        ///   This method might fail when deserializing generic JSON response from Objects endpoint to <see cref="CommonTaskData"/> model.
        /// </exception>
        internal sealed async Task<CommonTaskData> GetTaskAsync(IQueryBase queryBase)
        {
            // Request URL
            Uri taskObjectUri = queryBase.Notification.MainObjectUri;

            if (taskObjectUri.IsNotObject())
            {
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotObjectUri);
            }

            return await queryBase.ProcessGetAsync<CommonTaskData>(
                httpClientType: HttpClientTypes.Objecten,
                uri: taskObjectUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoTask);
        }
        #endregion

        #region Parent (Message)
        /// <summary>
        /// Gets the <see cref="MessageObject"/> from "Objecten" Web API service.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException">
        ///   This method might fail when deserializing generic JSON response from Objects endpoint to <see cref="MessageObject"/> model.
        /// </exception>
        internal sealed async Task<MessageObject> GetMessageAsync(IQueryBase queryBase)
        {
            // Request URL
            Uri taskObjectUri = queryBase.Notification.MainObjectUri;

            if (taskObjectUri.IsNotObject())
            {
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotObjectUri);
            }

            return await queryBase.ProcessGetAsync<MessageObject>(
                httpClientType: HttpClientTypes.Objecten,
                uri: taskObjectUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoMessage);
        }
        #endregion
        #pragma warning restore CA1822

        #region Parent (Create object)        
        /// <summary>
        /// Creates an object in "Objecten" Web API service.
        /// </summary>
        /// <returns>
        ///   The answer whether the object was created successfully.
        /// </returns>
        /// <exception cref="KeyNotFoundException"/>
        internal sealed async Task<RequestResponse> CreateObjectAsync(IHttpNetworkService networkService, string objectJsonBody)
        {
            // Predefined URL components
            string createObjectEndpoint = $"https://{GetDomain()}/objects";

            // Request URL
            Uri createObjectUri = new(createObjectEndpoint);

            return await networkService.PostAsync(
                httpClientType: HttpClientTypes.Objecten,
                uri: createObjectUri,
                objectJsonBody);
        }
        #endregion

        #region Polymorphic (Domain)
        /// <inheritdoc cref="IDomain.GetDomain"/>
        string IDomain.GetDomain() => this.Configuration.ZGW.Endpoint.Objecten();
        #endregion
    }
}