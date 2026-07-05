using System.Text;
using System.Text.Json;
using Common.Settings.Configuration;
using Notify.Models;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.MOBB.Models;
using ZgwModels.Extensions;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;

namespace WebQueries.MOBB
{
    /// <summary>
    /// Handles the "Message" scenario: fetches a message from OpenVTB,
    /// transforms it, and sends it to the MOBB / Notify API.
    /// </summary>
    internal sealed class MessageBoxScenario
    {
        private readonly IDataQueryService<NotificationEvent> _dataQuery;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OmcConfiguration _configuration;

        public MessageBoxScenario(
            IDataQueryService<NotificationEvent> dataQuery,
            IHttpClientFactory httpClientFactory,
            OmcConfiguration configuration)
        {
            _dataQuery = dataQuery;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<HttpRequestResponse> ProcessMessageAsync(NotificationEvent notification)
        {
            // Step 1: Whitelist check (placeholder – replace with actual identifier from notification)
            string messageIdentifier = "PLACEHOLDER_MESSAGE_TYPE";
            if (!IsMessageTypeWhitelisted(messageIdentifier))
            {
                return HttpRequestResponse.Failure(
                    $"Message processing not allowed. Identifier '{messageIdentifier}' not in whitelist.");
            }

            // Step 2: Fetch message data from OpenVTB
            IQueryContext queryContext = _dataQuery.From(notification);
            Guid messageUuid = notification.MainObjectUri.GetGuid();
            MessageData messageData = await queryContext.GetMessageDataAsync(messageUuid);

            if (string.IsNullOrWhiteSpace(messageData.MessageText))
            {
                return HttpRequestResponse.Failure("Message text is empty or missing.");
            }

            // Step 3: Build the request payload for MOBB API
            MessageBoxRequest request = BuildMobbRequest(notification, messageData);

            // Step 4: Serialize to JSON
            string jsonPayload = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            // Step 5: Send to external MOBB API
            string apiUrl = _configuration.MOBB.ApiUrl();
            using HttpClient client = _httpClientFactory.CreateClient();
            using StringContent content = new(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(apiUrl, content);
            string responseBody = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode
                ? HttpRequestResponse.Success(responseBody)
                : HttpRequestResponse.Failure(responseBody);
        }

        #region Private helpers

        private bool IsMessageTypeWhitelisted(string messageTypeIdentifier)
        {
            return _configuration.ZGW.Whitelist.Message_IDs().IsAllowed(messageTypeIdentifier);
        }

        private MessageBoxRequest BuildMobbRequest(NotificationEvent notification, MessageData messageData)
        {
            // TODO: Replace with actual BSN retrieval
            string bsn = "000000000";

            return new MessageBoxRequest
            {
                Bsn = bsn,
                Message = messageData.MessageText ?? string.Empty,
                Reference = messageData.Reference ?? "NO_REFERENCE",
                Personalisation = new Personalisation
                {
                    Subject = messageData.Subject ?? "Berichtenboxbericht"
                },
                BatchId = Guid.NewGuid().ToString(),
                MessageType = messageData.MessageType ?? "bericht",
                Attachments = MapAttachments(messageData.Attachments)
            };
        }

        private List<MobbAttachment>? MapAttachments(List<Attachment>? attachments)
        {
            if (attachments == null || attachments.Count == 0)
                return null;

            List<MobbAttachment> result = new();

            foreach (Attachment attachment in attachments)
            {
                result.Add(new MobbAttachment
                {
                    Filename = attachment.filename ?? "attachment",
                    Content = "PLACEHOLDER_BASE64_CONTENT" // TODO: Fetch actual content
                });
            }

            return result;
        }

        #endregion
    }
}