// © 2024, Worth Systems.

using Common.Extensions;
using Common.Settings.Configuration;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataSending.Clients.Enums;
using WebQueries.DataSending.Interfaces;
using WebQueries.Versioning.Interfaces;

namespace WebQueries.DataQuerying.Strategies.Queries.ObjectTypen.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "ObjectTypen" Web API service.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    /// <seealso cref="IDomain"/>
    public interface IQueryObjectTypen : IVersionDetails, IDomain
    {
        /// <inheritdoc cref="OmcConfiguration"/>
        protected internal OmcConfiguration Configuration { get; set; }

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "ObjectTypen";

        #region Parent (Create message object)
        /// <summary>
        /// Prepares an object type JSON representation following a schema from "ObjectTypen" Web API service.
        /// </summary>
        /// <param name="dataJson">The data JSON (without outer curly brackets).</param>
        /// <returns>
        ///   The JSON representation of object type.
        /// </returns>
        /// <exception cref="KeyNotFoundException"/>
        internal string PrepareObjectJsonBody(string dataJson)
        {
            return $"{{" +
                     $"\"type\":\"https://{GetDomain()}/objecttypes/{Configuration.ZGW.Variable.ObjectType.MessageObjectType_Uuid()}\"," +
                     $"\"record\":{{" +
                       $"\"typeVersion\":\"{Configuration.ZGW.Variable.ObjectType.MessageObjectType_Version()}\"," +
                       $"\"data\":{{" +
                         $"{dataJson}" +
                       $"}}," +
                       $"\"startAt\":\"{DateTime.UtcNow.ConvertToDutchDateString()}\"" +
                     $"}}" +
                   $"}}";
        }
        #endregion

        #region Polymorphic (Domain)
        /// <inheritdoc cref="IDomain.GetDomain"/>
        string IDomain.GetDomain() => this.Configuration.ZGW.Endpoint.ObjectTypen();
        #endregion

        #region Polymorphic (Health Check)
        /// <inheritdoc cref="IDomain.GetHealthCheckAsync(IHttpNetworkService)"/>
        async Task<HttpRequestResponse> IDomain.GetHealthCheckAsync(IHttpNetworkService networkService)
        {
            Uri healthCheckEndpointUri = new($"https://{GetDomain()}/objecttypes");  // NOTE: There is no dedicated health check endpoint, calling anything should be fine

            return await networkService.GetAsync(HttpClientTypes.ObjectTypen, healthCheckEndpointUri);
        }
        #endregion
    }
}