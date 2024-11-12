// © 2023, Worth Systems.

using EventsHandler.Exceptions;
using EventsHandler.Extensions;
using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases;
using EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.Settings.Configuration;
using TextResources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataProcessing.Strategy.Manager
{
    /// <inheritdoc cref="IScenariosResolver{INotifyScenario, NotificationEvent}"/>
    public sealed class NotifyScenariosResolver : IScenariosResolver<INotifyScenario, NotificationEvent>
    {
        private readonly WebApiConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDataQueryService<NotificationEvent> _dataQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyScenariosResolver"/> nested class.
        /// </summary>
        public NotifyScenariosResolver(
            WebApiConfiguration configuration,
            IServiceProvider serviceProvider,
            IDataQueryService<NotificationEvent> dataQuery)
        {
            this._configuration = configuration;
            this._serviceProvider = serviceProvider;
            this._dataQuery = dataQuery;
        }

        /// <inheritdoc cref="IScenariosResolver{INotifyScenario, NotificationEvent}.DetermineScenarioAsync(NotificationEvent)"/>
        async Task<INotifyScenario> IScenariosResolver<INotifyScenario, NotificationEvent>.DetermineScenarioAsync(NotificationEvent notification)
        {
            // Case scenarios
            if (IsCaseScenario(notification))
            {
                IQueryContext queryContext = this._dataQuery.From(notification);
                CaseStatuses caseStatuses = await queryContext.GetCaseStatusesAsync();

                // Scenario #1: "Case created"
                if (caseStatuses.WereNeverUpdated())
                {
                    return this._serviceProvider.GetRequiredService<CaseCreatedScenario>();
                }

                return !(await queryContext.GetLastCaseTypeAsync(caseStatuses)).IsFinalStatus
                    // Scenario #2: "Case status updated"
                    ? this._serviceProvider.GetRequiredService<CaseStatusUpdatedScenario>()
                    // Scenario #3: "Case finished"
                    : this._serviceProvider.GetRequiredService<CaseClosedScenario>();
            }

            // Object scenarios
            if (IsObjectScenario(notification))
            {
                Guid objectTypeId = notification.Attributes.ObjectTypeUri.GetGuid();

                if (objectTypeId.Equals(this._configuration.ZGW.Variable.ObjectType.TaskObjectType_Uuid()))
                {
                    // Scenario #4: "Task assigned"
                    return this._serviceProvider.GetRequiredService<TaskAssignedScenario>();
                }

                if (objectTypeId.Equals(this._configuration.ZGW.Variable.ObjectType.MessageObjectType_Uuid()))
                {
                    // Scenario #6: "Message received"
                    return this._serviceProvider.GetRequiredService<MessageReceivedScenario>();
                }

                throw new AbortedNotifyingException(
                    string.Format(TextResources.Processing_ABORT_DoNotSendNotification_Whitelist_GenObjectTypeGuid,
                        /* {0} */ $"{objectTypeId}",
                        /* {1} */ Settings.Extensions.ConfigurationExtensions.GetGenericVariableObjectTypeEnvVarName()));
            }

            // Scenario #5: "Decision made"
            if (IsDecisionScenario(notification))
            {
                return this._serviceProvider.GetRequiredService<DecisionMadeScenario>();
            }

            // No matching scenario. There is no clear instruction what to do with the received Notification
            return this._serviceProvider.GetRequiredService<NotImplementedScenario>();
        }

        #region Filters
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
        private static bool IsObjectScenario(NotificationEvent notification)
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
        #endregion
    }
}