// © 2026, Worth Systems.

using Common.Settings.Configuration;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.Producten.Interfaces;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;

namespace WebQueries.Producten
{
    /// <inheritdoc cref="IProductScenario"/>
    public sealed class ProductScenarioImplementation : IProductScenario
    {
        private readonly OmcConfiguration _configuration;
        private readonly IQueryContext _queryContext;
        private readonly INotifyService<NotifyData> _notifyService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductScenarioImplementation"/> class.
        /// </summary>
        public ProductScenarioImplementation(
            OmcConfiguration configuration,
            IQueryContext queryContext,
            INotifyService<NotifyData> notifyService)  // Dependency Injection (DI)
        {
            this._configuration = configuration;
            this._queryContext = queryContext;
            this._notifyService = notifyService;
        }

        /// <inheritdoc cref="IProductScenario.ProcessProductAsync(NotificationEvent)"/>
        public Task<HttpRequestResponse> ProcessProductAsync(NotificationEvent notification)
        {
            // TODO: Fetching the product, the whitelist and gepubliceerd gates, the per-owner party
            //       lookup, the e-mail send and the contactmoment registration land in the follow-up
            //       commits for Worth-NL/notifynl#112 - #116. Routing is wired up first so the rest can
            //       be built against a notification that actually reaches this point.
            throw new NotImplementedException();
        }
    }
}
