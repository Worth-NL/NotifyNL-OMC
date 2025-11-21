using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Proxy.Interfaces;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.Objecten.KTO;

namespace WebQueries.KTO.Models
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class KtoScenario
    {
        /// <inheritdoc cref="IQueryContext"/>
        private IQueryContext QueryContext { get; set; }

        /// <inheritdoc cref="IDataQueryService{TModel}"/>
        private IDataQueryService<NotificationEvent> DataQuery { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KtoScenario"/> class.
        /// </summary>
        public KtoScenario(
            IDataQueryService<NotificationEvent> dataQuery,
            IQueryContext queryContext)
        {
            DataQuery = dataQuery;
            QueryContext = queryContext;
        }

        /// <summary>
        /// Checks if there are KTO settings, if so tries to send a request to KTO Service.
        /// </summary>
        /// <param name="notification"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        ///   The JSON content for HTTP Request Body.
        /// </returns>
        public async Task<HttpRequestResponse> SendKtoAsync(NotificationEvent notification)
        {
            QueryContext = DataQuery.From(notification);

            KtoObject response = await QueryContext.GetKtoObjectAsync(notification.MainObjectUri.GetGuid());
            string ktoMessage = response.Record.Data.KtoMessage;

            if (string.IsNullOrEmpty(ktoMessage))
            {
#pragma warning disable CA2208
                throw new ArgumentException(@"KTO object cannot be null or empty.", nameof(ktoMessage));
#pragma warning restore CA2208
            }

            HttpRequestResponse ktoResult = await SendKtoRequestAsync(ktoMessage);

            if (ktoResult.IsFailure)
            {
                throw new HttpRequestException("Failed to send KTO request. JSonMessage: " + ktoResult.JsonResponse);
            }

            HttpRequestResponse resultRemoveObjectResponse = await RemoveKtoObjectAsync(notification.MainObjectUri.GetGuid());

            if (resultRemoveObjectResponse.IsFailure)
            {
                throw new HttpRequestException("Failed to delete kto object. Kto has been sent successfully." +
                                               " JSonMessage: " + resultRemoveObjectResponse.JsonResponse);
            }

            return ktoResult;
        }

        #region Helpers
        private async Task<HttpRequestResponse> SendKtoRequestAsync(string ktoPayload)
        {
            return await QueryContext.SendKtoAsync(ktoPayload);
        }

        private async Task<HttpRequestResponse> RemoveKtoObjectAsync(Guid ktoObject)
        {
            return await QueryContext.DeleteObjectAsync(ktoObject);
        }
        #endregion
    }
}