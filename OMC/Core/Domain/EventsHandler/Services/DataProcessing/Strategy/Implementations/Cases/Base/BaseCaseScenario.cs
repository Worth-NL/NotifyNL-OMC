// © 2024, Worth Systems.

using Common.Settings.Configuration;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;

namespace EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases.Base
{
    /// <summary>
    /// Common methods and properties used only by case-related scenarios.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    internal abstract class BaseCaseScenario : BaseScenario
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCaseScenario"/> class.
        /// </summary>
        protected BaseCaseScenario(
            WebApiConfiguration configuration,
            IDataQueryService<NotificationEvent> dataQuery,
            INotifyService<NotifyData> notifyService)
            : base(configuration, dataQuery, notifyService)
        {
        }
    }
}