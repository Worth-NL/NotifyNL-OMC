// © 2023, Worth Systems.

using Common.Settings.Configuration;
using Common.Settings.Extensions;
using EventsHandler.Exceptions;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations;          // added
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Kto;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.MessageBox;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Print;
using EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.Tracing;
using ZgwModels.Extensions;
using ZgwModels.Mapping.Enums.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.OpenZaak;

namespace EventsHandler.Services.DataProcessing.Strategy.Manager
{
    /// <inheritdoc cref="IScenariosResolver{INotifyScenario, NotificationEvent}"/>
    internal sealed class NotifyScenariosResolver : IScenariosResolver<INotifyScenario, NotificationEvent>
    {
        private readonly OmcConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDataQueryService<NotificationEvent> _dataQuery;

        public NotifyScenariosResolver(
            OmcConfiguration configuration,
            IServiceProvider serviceProvider,
            IDataQueryService<NotificationEvent> dataQuery)
        {
            this._configuration = configuration;
            this._serviceProvider = serviceProvider;
            this._dataQuery = dataQuery;
        }

        /// <inheritdoc cref="IScenariosResolver{INotifyScenario, NotificationEvent}.DetermineScenarioAsync(NotificationEvent)"/>
        async Task<INotifyScenario> IScenariosResolver<INotifyScenario, NotificationEvent>.DetermineScenarioAsync(NotificationEvent model)
        {
            // Case scenarios
            if (IsCaseScenario(model))
            {
                IQueryContext queryContext = this._dataQuery.From(model);

                Guid statusId = model.ResourceUri.GetGuid();
                TraceContext.Emit("openzaak", "start", $"Attempting to retrieve status with id {statusId}");
                CaseStatus caseStatus;
                CaseStatusType currentCaseStatusType;
                try
                {
                    caseStatus = await queryContext.GetCaseStatusAsync(model.ResourceUri);
                    currentCaseStatusType = await queryContext.GetCaseStatusTypeAsync(caseStatus.TypeUri);
                }
                catch (Exception exception)
                {
                    TraceContext.Emit("openzaak", "fail", exception.Message);
                    throw;
                }
                TraceContext.Emit("openzaak", "ok", $"status with id {statusId} retrieved");

                if (!currentCaseStatusType.IsNotificationExpected)
                {
                    TraceContext.Emit("informerencheck", "abort", $"status type {currentCaseStatusType.Identification} has \"informeren\" set to false");
                    throw new AbortedNotifyingException(ApiResources.Processing_ABORT_DoNotSendNotification_Informeren);
                }
                TraceContext.Emit("informerencheck", "ok", $"status type {currentCaseStatusType.Identification} has \"informeren\" set to true");

                if (currentCaseStatusType.SerialNumber == 1)
                {
                    TraceContext.SetScenario("case-created");
                    return (this._serviceProvider.GetRequiredService<CaseCreatedScenario>()).SetCaseStatusType(currentCaseStatusType);
                }

                if (!currentCaseStatusType.IsFinalStatus)
                {
                    // Scenario #2: "Case status updated"
                    TraceContext.SetScenario("case-updated");
                    return (this._serviceProvider.GetRequiredService<CaseStatusUpdatedScenario>()).SetCaseStatusType(currentCaseStatusType);
                }

                // Scenario #3: "Case finished"
                TraceContext.SetScenario("case-closed");
                return (this._serviceProvider.GetRequiredService<CaseClosedScenario>()).SetCaseStatusType(currentCaseStatusType);
            }

            // Object scenarios
            if (IsObjectScenario(model))
            {
                Guid objectTypeId = model.Attributes.ObjectTypeUri.GetGuid();

                if (objectTypeId.Equals(this._configuration.ZGW.Variable.ObjectType.TaskObjectType_Uuid()))
                {
                    // Scenario #4: "Task assigned"
                    TraceContext.SetScenario("task-assigned");
                    return this._serviceProvider.GetRequiredService<TaskAssignedScenario>();
                }

                if (objectTypeId.Equals(this._configuration.ZGW.Variable.ObjectType.MessageObjectType_Uuid()))
                {
                    // Scenario #6: "Message received"
                    TraceContext.SetScenario("message-received");
                    return this._serviceProvider.GetRequiredService<MessageReceivedScenario>();
                }

                if (objectTypeId.Equals(this._configuration.ZGW.Variable.ObjectType.KtoObjectType_Uuid()))
                {
                    // Scenario #7: "KTO received"
                    TraceContext.SetScenario("kto");
                    return this._serviceProvider.GetRequiredService<KtoScenario>();
                }

                if (objectTypeId.Equals(this._configuration.ZGW.Variable.ObjectType.PrintObjectType_Uuid()))
                {
                    // Scenario #8: "Print requested"
                    TraceContext.SetScenario("print-requested");
                    return this._serviceProvider.GetRequiredService<PrintScenario>();
                }

                throw new AbortedNotifyingException(
                    string.Format(ApiResources.Processing_ABORT_DoNotSendNotification_Whitelist_GenObjectTypeGuid,
                        /* {0} */ $"{objectTypeId}",
                        /* {1} */ ConfigExtensions.GetGenericVariableObjectTypeEnvVarName()));
            }

            // Decision scenarios
            if (IsDecisionScenario(model))
            {
                TraceContext.SetScenario("decision-made");
                return this._serviceProvider.GetRequiredService<DecisionMadeScenario>();
            }

            // Message scenario (MOBB forwarding)
            if (IsMessageScenario(model))
            {
                return this._serviceProvider.GetRequiredService<MessageBoxScenario>();
            }

            // No matching scenario
            return this._serviceProvider.GetRequiredService<NotImplementedScenario>();
        }

        #region Filters

        private static bool IsCaseScenario(NotificationEvent notification)
        {
            return notification is
            {
                Action: Actions.Create,
                Channel: Channels.Cases,
                Resource: Resources.Status
            };
        }

        private static bool IsObjectScenario(NotificationEvent notification)
        {
            return notification is
            {
                Action: Actions.Create,
                Channel: Channels.Objects,
                Resource: Resources.Object
            };
        }

        private static bool IsDecisionScenario(NotificationEvent notification)
        {
            return notification is
            {
                Action: Actions.Create,
                Channel: Channels.Decisions,
                Resource: Resources.Decision
            };
        }

        /// <summary>
        /// Determines whether the notification corresponds to the "Message" scenario
        /// (forwarding to MOBB / OpenVTB).
        /// </summary>
        private static bool IsMessageScenario(NotificationEvent notification)
        {
            return notification is
            {
                Action: Actions.Create,
                Channel: Channels.Messages,   // Adjust to actual enum value if different
                Resource: Resources.Message    // Adjust to actual enum value if different
            };
        }

        #endregion
    }
}