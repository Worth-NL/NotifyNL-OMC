using System.Text.Json.Nodes;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Proxy.Interfaces;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.Objecten.Message;

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

            MessageObject messageObject = await QueryContext.GetMessageAsync();
            HttpRequestResponse response = await QueryContext.GetObjectJsonAsync();
            //string ktoObject = messageObject.Record.Data.KtoObject;
            string ktoObject = messageObject.Record.Data.ActionsPerspective;
            HttpRequestResponse listObjectTypen = await QueryContext.GetObjectTypenHealthCheckAsync();
            HttpRequestResponse messageObjectResponse = await RemoveKtoObjectAsync(response.JsonResponse);
            if (string.IsNullOrEmpty(ktoObject))
            {
#pragma warning disable CA2208
                throw new ArgumentException(@"KTO object cannot be null or empty.", nameof(ktoObject)); 
#pragma warning restore CA2208
            }

            HttpRequestResponse ktoResult = await SendKtoRequestAsync(ktoObject);

            if (ktoResult.IsFailure)
            {
                throw new HttpRequestException("Failed to send KTO request. JSonMessage: " + ktoResult.JsonResponse);
            }

            //HttpRequestResponse messageObjectResponse = await RemoveKtoObjectAsync();

            if (messageObjectResponse.IsFailure)
            {
                throw new HttpRequestException("Failed to set kto object to null. Kto has been sent successfully." +
                                               " JSonMessage: " + messageObjectResponse.JsonResponse);
            }

            return ktoResult;
        }

        #region Helpers
        private async Task<HttpRequestResponse> SendKtoRequestAsync(string ktoObject)
        {
            return await QueryContext.SendKtoAsync(ktoObject);
        }

        private async Task<HttpRequestResponse> RemoveKtoObjectAsync(string originalJson)
        {
            JsonNode jsonNode = JsonNode.Parse(originalJson)!;

            // Replace "TODO" → "TADA"
            JsonNode? node = jsonNode["record"]?["data"];
            if (node != null)
            {
                node["handelingsperspectief"] = "TADA";
            }

            // Replace null correctionFor → "kto"
            JsonNode? node1 = jsonNode["record"];
            if (node1 != null)
            {
                node1["correctionFor"] = "kto";
            }

            // Serialize back to string (pretty or compact)
            string updatedJson = jsonNode.ToJsonString(new() { WriteIndented = true }); ;

            return await QueryContext.PatchObjectAsync(updatedJson);
        }
        #endregion
    }
}
