// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Interfaces;

namespace EventsHandler.Behaviors.Communication.Strategy.Base
{
    /// <summary>
    /// <inheritdoc cref="INotifyScenario"/>
    /// Common logic for <see cref="CaseStatusUpdatedScenario"/> and <see cref="CaseFinishedScenario"/> strategies.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    internal abstract class BaseStatusScenario : BaseScenario
    {
        /// <inheritdoc cref="CaseStatusType"/>
        protected CaseStatusType? LastCaseStatusType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseStatusScenario"/> class.
        /// </summary>
        protected BaseStatusScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
            : base(configuration, dataQuery)
        {
        }

        #region Regular
        /// <summary>
        /// Tells the strategy about already queried <see cref="CaseStatusType"/>
        /// result so, she could spare some processing and network effort later.
        /// </summary>
        protected internal void PassAlreadyQueriedResult(CaseStatusType queriedResult)
        {
            this.LastCaseStatusType = queriedResult;
        }

        /// <summary>
        /// Ensures that required data will be retried to be loaded (re-queried)
        /// even if they were not passed to the strategy from the outside world.
        /// </summary>
        protected async Task<CaseStatusType> ReQueryCaseStatusTypeAsync(NotificationEvent notification)
        {
            CaseStatuses caseStatuses = await this.DataQuery.From(notification).GetCaseStatusesAsync();
            CaseStatusType lastCaseStatusType = await this.DataQuery.From(notification).GetLastCaseStatusTypeAsync(caseStatuses);

            return lastCaseStatusType;
        }
        #endregion
    }
}