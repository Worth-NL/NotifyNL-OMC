// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.Settings.Configuration;

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