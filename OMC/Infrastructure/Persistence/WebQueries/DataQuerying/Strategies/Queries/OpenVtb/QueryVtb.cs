using Common.Settings.Configuration;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenKlant.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenVtb.Interfaces;
using WebQueries.DataSending.Clients.Enums;
using WebQueries.DataSending.Interfaces;
using WebQueries.Versioning.Interfaces;
using ZgwModels.Mapping.Models.POCOs.OpenVtb;
using ZgwModels.Properties;

namespace WebQueries.DataQuerying.Strategies.Queries.OpenVtb
{
    /// <inheritdoc cref="IQueryVtb"/>
    /// <remarks>
    ///   Version: "OpenKlant" (v2) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    public sealed class QueryVtb : IQueryVtb
    {
        /// <inheritdoc cref="IQueryKlant.Configuration"/>
        OmcConfiguration IQueryVtb.Configuration { get; set; } = null!;
        /// <inheritdoc cref="IVersionDetails.Version"/>
        string IVersionDetails.Version => "2.0.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryVtb"/> class.
        /// </summary>
        public QueryVtb(OmcConfiguration configuration)  // Dependency Injection (DI)
        {
            ((IQueryVtb)this).Configuration = configuration;
        }

        #region Abstract (Messages data)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryBase"></param>
        /// <param name="vtbMessageUri"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<VtbMessage> TryGetVtbMessageAsync(IQueryBase queryBase, Uri vtbMessageUri)
        {
            return await queryBase.ProcessGetAsync<VtbMessage>(
                httpClientType: HttpClientTypes.OpenVtb,
                uri: vtbMessageUri,  // Request URL
                fallbackErrorMessage: ZgwResources.HttpRequest_ERROR_NoPartyResults);//TODO Create new error message for OpenVtb
        }
        #endregion

        #region Polymorphic (Health Check)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="networkService"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<HttpRequestResponse> GetHealthCheckAsync(IHttpNetworkService networkService)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
