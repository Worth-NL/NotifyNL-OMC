// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Implementations;
using EventsHandler.Behaviors.Communication.Strategy.Implementations.Cases;
using EventsHandler.Behaviors.Communication.Strategy.Implementations.Cases.Base;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Mapping.Enums.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
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
            // Supported scenarios for business cases
            if (IsCaseScenario(notification))
            {
                IQueryContext queryContext = this._dataQuery.From(notification);
                CaseStatuses caseStatuses = await queryContext.GetCaseStatusesAsync();

                // Scenario #1: "Case created"
                if (caseStatuses.WereNeverUpdated())
                {
                    return this._serviceProvider.GetRequiredService<CaseCreatedScenario>();
                }

                CaseStatusType lastCaseStatusType =
                    await queryContext.GetLastCaseStatusTypeAsync(caseStatuses);

                BaseCaseScenario strategy = !lastCaseStatusType.IsFinalStatus
                    // Scenario #2: "Case status updated"
                    ? this._serviceProvider.GetRequiredService<CaseCaseStatusUpdatedScenario>()
                    // Scenario #3: "Case finished"
                    : this._serviceProvider.GetRequiredService<CaseCaseFinishedScenario>();

                strategy.PassAlreadyQueriedResult(lastCaseStatusType);

                return strategy;
            }

            // Scenario #4: "Task ..."
            if (IsTaskScenario(notification))
            {

            }
            
            // Scenario #5: "Decision made"
            if (IsDecisionScenario(notification))
            {
                return this._serviceProvider.GetRequiredService<DecisionMadeScenario>();
            }

            // There is no matching scenario to be applied. There is no clear instruction what to do with received Notification
            return this._serviceProvider.GetRequiredService<NotImplementedScenario>();
        }

        /// <summary>
        /// OMC is meant to process <see cref="NotificationEvent"/>s with certain characteristics (determining the workflow).
        /// </summary>
        /// <remarks>
        ///   This check is verifying whether case scenarios would be processed.
        /// </remarks>
        private static bool IsCaseScenario(NotificationEvent notification)
        {
            return notification is
            {
                Action:   Actions.Create,
                Channel:  Channels.Cases,
                Resource: Resources.Status
            };
        }

        /// <summary>
        ///   <inheritdoc cref="IsCaseScenario(NotificationEvent)"/>
        /// </summary>
        /// <remarks>
        ///   This check is verifying whether task scenarios would be processed.
        /// </remarks>
        private static bool IsTaskScenario(NotificationEvent notification)
        {
            return notification is
            {
                Action:   Actions.Create,
                Channel:  Channels.Objects,
                Resource: Resources.Object
            };
        }

        /// <summary>
        ///   <inheritdoc cref="IsCaseScenario(NotificationEvent)"/>
        /// </summary>
        /// <remarks>
        ///   This check is verifying whether decision scenarios would be processed.
        /// </remarks>
        private static bool IsDecisionScenario(NotificationEvent notification)
        {
            return notification is
            {
                Action:   Actions.Create,
                Channel:  Channels.Decisions,
                Resource: Resources.Decision
            };
        }
    }
}