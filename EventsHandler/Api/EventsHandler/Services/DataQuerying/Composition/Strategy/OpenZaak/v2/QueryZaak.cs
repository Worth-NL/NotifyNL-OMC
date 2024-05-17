// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak.v2;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.v2
{
    /// <inheritdoc cref="IQueryZaak"/>
    /// <remarks>
    ///   Version: "OpenZaak" (2.0) Web service.
    /// </remarks>
    internal class QueryZaak : IQueryZaak
    {
        /// <inheritdoc cref="IQueryZaak.Configuration"/>
        WebApiConfiguration IQueryZaak.Configuration { get; set; } = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryZaak"/> class.
        /// </summary>
        public QueryZaak(WebApiConfiguration configuration)
        {
            ((IQueryZaak)this).Configuration = configuration;
        }

        #region Polymorphism
        /// <inheritdoc cref="IQueryZaak.GetCaseTypeUriFromDetailsAsync(IQueryBase)"/>
        async Task<Uri> IQueryZaak.GetCaseTypeUriFromDetailsAsync(IQueryBase queryBase)
        {
            return (await GetCaseDetailsV2Async(queryBase)).CaseType;
        }
        
        /// <summary>
        /// Gets the <see cref="Case"/> details from "OpenZaak" Web service.
        /// </summary>
        private static async Task<CaseDetails> GetCaseDetailsV2Async(IQueryBase queryBase)
        {
            return await queryBase.ProcessGetAsync<CaseDetails>(
                httpsClientType: HttpClientTypes.Data,
                uri: queryBase.Notification.MainObject,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseDetails);
        }
        #endregion
    }
}