// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Base;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Mapping.Enums.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataQuerying.Interfaces;

namespace EventsHandler.Behaviors.Communication.Strategy.Manager
{
    /// <inheritdoc cref="IScenariosResolver"/>
    public sealed class ScenariosResolver : IScenariosResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDataQueryService<NotificationEvent> _dataQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScenariosResolver"/> nested class.
        /// </summary>
        public ScenariosResolver(IServiceProvider serviceProvider, IDataQueryService<NotificationEvent> dataQuery)
        {
            this._serviceProvider = serviceProvider;
            this._dataQuery = dataQuery;
        }

        /// <inheritdoc cref="IScenariosResolver.DetermineScenarioAsync(NotificationEvent)"/>
        async Task<INotifyScenario> IScenariosResolver.DetermineScenarioAsync(NotificationEvent notification)
        {
            // Scenarios for Cases
            if (notification.Action == Actions.Create &&
                notification.Channel == Channels.Cases &&
                notification.Resource == Resources.Status)
            {
                CaseStatuses caseStatuses = await this._dataQuery.From(notification).GetCaseStatusesAsync();

                // Scenario #1: "Case created"
                if (caseStatuses.WereNeverUpdated())
                {
                    return this._serviceProvider.GetRequiredService<CaseCreatedScenario>();
                }

                CaseStatusType lastCaseStatusType =
                    await this._dataQuery.From(notification).GetLastCaseStatusTypeAsync(caseStatuses);

                BaseStatusScenario strategy = !lastCaseStatusType.IsFinalStatus
                    // Scenario #2: "Case status updated"
                    ? this._serviceProvider.GetRequiredService<CaseStatusUpdatedScenario>()
                    // Scenario #3: "Case finished"
                    : this._serviceProvider.GetRequiredService<CaseFinishedScenario>();

                strategy.PassAlreadyQueriedResult(lastCaseStatusType);

                return strategy;
            }

            // There is no matching scenario to be applied
            return this._serviceProvider.GetRequiredService<NotImplementedScenario>();
        }
    }
}