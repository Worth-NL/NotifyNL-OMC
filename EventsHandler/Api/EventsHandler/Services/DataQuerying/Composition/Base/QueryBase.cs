// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Extensions;
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
        private readonly IHttpSupplierService _httpSupplier;

        /// <inheritdoc cref="IQueryBase.Notification"/>
        NotificationEvent IQueryBase.Notification { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBase"/> class.
        /// </summary>
        public QueryBase(ISerializationService serializer, IHttpSupplierService httpSupplier)
        {
            this._serializer = serializer;
            this._httpSupplier = httpSupplier;
        }

        /// <inheritdoc cref="IQueryBase.ProcessGetAsync{TModel}(HttpClientTypes, Uri, string)"/>
        async Task<TModel> IQueryBase.ProcessGetAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, string fallbackErrorMessage)
        {
            string organizationId = ((IQueryBase)this).Notification.GetOrganizationId();

            (bool isSuccess, string jsonResult) = await this._httpSupplier.GetAsync(httpsClientType, organizationId, uri);

            return GetApiResult<TModel>(isSuccess, jsonResult, uri, fallbackErrorMessage);
        }

        /// <inheritdoc cref="IQueryBase.ProcessPostAsync{TModel}(HttpClientTypes, Uri, HttpContent, string)"/>
        async Task<TModel> IQueryBase.ProcessPostAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, HttpContent body, string fallbackErrorMessage)
        {
            string organizationId = ((IQueryBase)this).Notification.GetOrganizationId();

            (bool isSuccess, string jsonResult) = await this._httpSupplier.PostAsync(httpsClientType, organizationId, uri, body);

            return GetApiResult<TModel>(isSuccess, jsonResult, uri, fallbackErrorMessage);
        }

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