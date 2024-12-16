// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Versioning.Interfaces;
using System.Text.Json;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataSending.Clients.Enums;
using WebQueries.DataSending.Interfaces;
using WebQueries.Properties;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Models.POCOs.Objecten.Message;
using ZhvModels.Mapping.Models.POCOs.Objecten.Task;
using ZhvModels.Properties;

namespace WebQueries.DataQuerying.Strategies.Queries.Objecten.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    /// <seealso cref="IDomain"/>
    public interface IQueryObjecten : IVersionDetails, IDomain
    {
        /// <inheritdoc cref="WebApiConfiguration"/>
        protected internal WebApiConfiguration Configuration { get; set; }

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "Objecten";

        #region Health Check
        /// <inheritdoc cref="IDomain.GetHealthCheckAsync(IHttpNetworkService)"/>
        async Task<HttpRequestResponse> IDomain.GetHealthCheckAsync(IHttpNetworkService networkService)
        {
            Uri healthCheckEndpointUri = new($"https://{GetDomain()}/objects");  // NOTE: There is no dedicated health check endpoint, calling anything should be fine

            return await networkService.GetAsync(HttpClientTypes.Objecten, healthCheckEndpointUri);
        }
        #endregion

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
                throw new ArgumentException(QueryResources.Querying_ERROR_Internal_NotObjectUri);
            }

            return await queryBase.ProcessGetAsync<CommonTaskData>(
                httpClientType: HttpClientTypes.Objecten,
                uri: taskObjectUri,
                fallbackErrorMessage: ZhvResources.HttpRequest_ERROR_NoTask);
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
                throw new ArgumentException(QueryResources.Querying_ERROR_Internal_NotObjectUri);
            }

            return await queryBase.ProcessGetAsync<MessageObject>(
                httpClientType: HttpClientTypes.Objecten,
                uri: taskObjectUri,
                fallbackErrorMessage: ZhvResources.HttpRequest_ERROR_NoMessage);
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
        internal sealed async Task<HttpRequestResponse> CreateObjectAsync(IHttpNetworkService networkService, string objectJsonBody)
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
        string IDomain.GetDomain() => Configuration.ZGW.Endpoint.Objecten();
        #endregion
    }
}