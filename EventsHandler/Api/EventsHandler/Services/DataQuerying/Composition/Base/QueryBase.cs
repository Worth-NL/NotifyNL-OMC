// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;
using EventsHandler.Services.DataReceiving.Interfaces;
using EventsHandler.Services.Serialization.Interfaces;

namespace EventsHandler.Services.DataQuerying.Composition.Base
{
    /// <inheritdoc cref="IQueryBase"/>
    internal sealed class QueryBase : IQueryBase
    {
        private readonly ISerializationService _serializer;
        private readonly IHttpNetworkService _networkService;

        /// <inheritdoc cref="IQueryBase.Notification"/>
        NotificationEvent IQueryBase.Notification { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBase"/> class.
        /// </summary>
        public QueryBase(ISerializationService serializer, IHttpNetworkService networkService)
        {
            this._serializer = serializer;
            this._networkService = networkService;
        }

        #region Internal methods
        /// <inheritdoc cref="IQueryBase.ProcessGetAsync{TModel}(HttpClientTypes, Uri, string)"/>
        async Task<TModel> IQueryBase.ProcessGetAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, string fallbackErrorMessage)
        {
            (bool isSuccess, string jsonResult) = await this._networkService.GetAsync(httpsClientType, uri);

            return GetApiResult<TModel>(isSuccess, jsonResult, uri, fallbackErrorMessage);
        }

        /// <inheritdoc cref="IQueryBase.ProcessPostAsync{TModel}(HttpClientTypes, Uri, HttpContent, string)"/>
        async Task<TModel> IQueryBase.ProcessPostAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, HttpContent body, string fallbackErrorMessage)
        {
            (bool isSuccess, string jsonResult) = await this._networkService.PostAsync(httpsClientType, uri, body);

            return GetApiResult<TModel>(isSuccess, jsonResult, uri, fallbackErrorMessage);
        }
        #endregion

        #region Helper methods
        private TModel GetApiResult<TModel>(bool isSuccess, string jsonResult, Uri uri, string fallbackErrorMessage)
            where TModel : struct, IJsonSerializable
        {
            return isSuccess ? this._serializer.Deserialize<TModel>(jsonResult)
                             : throw new HttpRequestException($"{fallbackErrorMessage} | URI: {uri} | JSON response: {jsonResult}");
        }
        #endregion
    }
}