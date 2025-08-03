using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Proxy.Interfaces;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;

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

            string ktoObject = (await QueryContext.GetMessageAsync()).Record.Data.KtoObject;

            if (string.IsNullOrEmpty(ktoObject))
            {
                throw new ArgumentException(@"KTO object cannot be null or empty.", nameof(ktoObject)); 
            }

            HttpRequestResponse result = await SendKtoRequestAsync(ktoObject);

            if (result.IsFailure)
                throw new HttpRequestException("Failed to send KTO request.");

            return result;
        }

        private async Task<HttpRequestResponse> SendKtoRequestAsync(string ktoObject)
        {
            return await QueryContext.SendKtoAsync(ktoObject);
        }
    }
}
