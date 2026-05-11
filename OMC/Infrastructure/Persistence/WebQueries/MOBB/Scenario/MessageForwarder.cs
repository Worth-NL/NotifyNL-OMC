using Common.Settings.Configuration;
using System.Text;
using System.Text.Json;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Proxy.Interfaces;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Models.POCOs.Berichten;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;

namespace WebQueries.MOBB.Scenario
{
    internal sealed class MessageForwarder
    {
        private readonly IDataQueryService<NotificationEvent> _dataQuery;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OmcConfiguration _configuration;

        public MessageForwarder(
            IDataQueryService<NotificationEvent> dataQuery,
            IHttpClientFactory httpClientFactory,
            OmcConfiguration configuration)
        {
            _dataQuery = dataQuery;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<HttpRequestResponse> ForwardMessageAsync(NotificationEvent notification)
        {
            // Step 1: Placeholder identifier (replace later)
            string messageIdentifier = "PLACEHOLDER_MESSAGE_TYPE";

            if (!IsMessageTypeWhitelisted(messageIdentifier))
            {
                return HttpRequestResponse.Failure(
                    $"Message forwarding not allowed. Identifier '{messageIdentifier}' not in whitelist.");
            }

            IQueryContext queryContext = _dataQuery.From(notification);
            Guid messageUuid = notification.MainObjectUri.GetGuid(); // or wherever the UUID comes from
            MessageData messageData = await queryContext.GetMessageDataAsync(messageUuid);

            return HttpRequestResponse.Success("Whitelist passed – fetching message data pending."); //TODO: Implement actual forwarding logic after fetching message data
        }

        private bool IsMessageTypeWhitelisted(string messageTypeIdentifier)
        {
            return _configuration.ZGW.Whitelist.Message_IDs().IsAllowed(messageTypeIdentifier);
        }
    }
}