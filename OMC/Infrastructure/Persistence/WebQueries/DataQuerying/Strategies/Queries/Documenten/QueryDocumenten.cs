using Common.Settings.Configuration;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.Documenten.Interfaces;
using WebQueries.DataSending.Clients.Enums;
using WebQueries.DataSending.Interfaces;
using WebQueries.Versioning.Interfaces;
using ZgwModels.Mapping.Models.POCOs.OpenZaak.Documents;

namespace WebQueries.DataQuerying.Strategies.Queries.Documenten
{
    /// <summary>
    /// Implementation of <see cref="IQueryDocumenten"/> for the OpenZaak Documenten API.
    /// </summary>
    /// <remarks>
    ///   This class provides methods to retrieve document metadata (EnkelvoudigInformatieObject)
    ///   from the Documenten API. The API endpoint is configured via <see cref="OmcConfiguration.ZGW.Endpoint.Documenten"/>.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    public sealed class QueryDocumenten : IQueryDocumenten
    {
        /// <inheritdoc cref="IQueryDocumenten.Configuration"/>
        OmcConfiguration IQueryDocumenten.Configuration { get; set; } = null!;

        /// <inheritdoc cref="IVersionDetails.Version"/>
        string IVersionDetails.Version => "1.0.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryDocumenten"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration containing the Documenten API endpoint.</param>
        public QueryDocumenten(OmcConfiguration configuration)
        {
            ((IQueryDocumenten)this).Configuration = configuration;
        }

        /// <inheritdoc cref="IQueryDocumenten.TryGetDocumentAsync(IQueryBase, Guid)"/>
        async Task<SingularInformationObject> IQueryDocumenten.TryGetDocumentAsync(IQueryBase queryBase, Guid documentUuid)
        {
            string domain = ((IQueryDocumenten)this).GetDomain();
            Uri requestUri = new($"{domain}/enkelvoudiginformatieobjecten/{documentUuid:D}");

            return await queryBase.ProcessGetAsync<SingularInformationObject>(
                httpClientType: HttpClientTypes.OpenZaak_v1, // Uses JWT authentication (same as OpenZaak)
                uri: requestUri,
                fallbackErrorMessage: $"Failed to retrieve document with UUID {documentUuid} from Documenten API."
            );
        }

        /// <inheritdoc cref="IQueryDocumenten.TryGetDocumentContentAsync(IQueryBase, Uri)"/>
        async Task<string> IQueryDocumenten.TryGetDocumentContentAsync(IQueryBase queryBase, Uri contentUri)
        {
            return await queryBase.ProcessGetBinaryAsBase64Async(
                httpClientType: HttpClientTypes.OpenZaak_v1, // Same JWT authentication - the download link lives on the same Documenten API domain
                uri: contentUri,
                fallbackErrorMessage: $"Failed to download document content from {contentUri}."
            );
        }

        /// <summary>
        /// Checks the health of the Documenten API.
        /// </summary>
        /// <param name="networkService">The HTTP network service to use for the request.</param>
        /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
        /// <exception cref="NotImplementedException">This method is not yet implemented.</exception>
        /// <remarks>
        ///   TODO: Implement health check for the Documenten API once a suitable endpoint is identified.
        /// </remarks>
        public Task<HttpRequestResponse> GetHealthCheckAsync(IHttpNetworkService networkService)
        {
            throw new NotImplementedException();
        }
    }
}