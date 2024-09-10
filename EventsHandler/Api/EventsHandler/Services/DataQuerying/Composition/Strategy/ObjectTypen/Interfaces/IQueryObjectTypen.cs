// © 2024, Worth Systems.

using EventsHandler.Services.DataSending.Clients.Enums;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.DataSending.Responses;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Versioning.Interfaces;

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

        #region Parent (Create message object)        
        /// <summary>
        /// Creates the message object in "Objecten" Web API service.
        /// </summary>
        /// <returns>
        ///   The answer whether the message object was created successfully.
        /// </returns>
        internal sealed async Task<RequestResponse> CreateMessageObjectAsync(IHttpNetworkService networkService, string objectDataJson)
        {
            // Predefined URL components
            string createObjectEndpoint = $"https://{GetDomain()}/api/v2/objects";

            // Request URL
            Uri createObjectUri = new(createObjectEndpoint);

            // Prepare HTTP Request Body
            string jsonBody = PrepareCreateObjectJson(objectDataJson);

            return await networkService.PostAsync(
                httpClientType: HttpClientTypes.ObjectTypen,
                uri: createObjectUri,
                jsonBody);
        }

        private string PrepareCreateObjectJson(string objectDataJson)
        {
            return $"{{" +
                   $"  \"type\": \"https://{GetDomain()}/api/v2/objecttypes/{this.Configuration.User.Whitelist.MessageObjectType_Uuids()}\", " +
                   $"  \"record\": {{" +
                   $"    \"typeVersion\": \"{this.Configuration.AppSettings.Variables.Objecten.MessageObjectType_Version()}\", " +
                   $"    \"data\": {objectDataJson}, " +  // { data } => curly brackets are already included
                   $"    \"geometry\": {{" +
                   $"    }}, " +
                   $"    \"startAt\": \"{DateTime.UtcNow}\", " +
                   $"    \"correctionFor\": \"string\"" +
                   $"  }}" +
                   $"}}";
        }
        #endregion

        #region Polymorphic (Domain)
        /// <inheritdoc cref="IDomain.GetDomain"/>
        string IDomain.GetDomain() => this.Configuration.User.Domain.ObjectTypen();
        #endregion
    }
}