using System.Text.Json;
using Common.Settings.Configuration;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenVtb.Interfaces;
using WebQueries.DataSending.Clients.Enums;
using ZhvModels.Mapping.Models.POCOs.Berichten;

namespace WebQueries.DataQuerying.Strategies.Queries.OpenVtb
{
    /// <summary>
    /// Implementation of IQueryVtb for OpenVTB API.
    /// </summary>
    internal sealed class QueryVtb(OmcConfiguration configuration) : IQueryVtb
    {
        private string GetDomain()
        {
            return configuration.ZGW.Endpoint.Berichten(); // To be added to OmcConfiguration
        }

        async Task<MessageData> IQueryVtb.GetMessageDataAsync(IQueryBase queryBase, Guid messageUuid)
        {
            // Build the request URL: /berichten/{uuid}
            string domain = GetDomain();
            Uri requestUri = new($"{domain}/berichten/{messageUuid:D}");

            // Make the HTTP request
            MessageData messageData = await queryBase.ProcessGetAsync<MessageData>(
                httpClientType: HttpClientTypes.Berichten, // Add to HttpClientTypes enum
                uri: requestUri,
                fallbackErrorMessage: $"Failed to retrieve message with UUID {messageUuid} from OpenVTB"
            );

            return messageData;
        }
    }
}
