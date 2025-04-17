// © 2023, Worth Systems.

using System.Xml;
using Common.Settings.Configuration;
using Common.Settings.Extensions;
using EventsHandler.Exceptions;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases;
using EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces;
using net.adamec.lib.common.dmn.engine.engine.decisions;
using net.adamec.lib.common.dmn.engine.engine.definition;
using net.adamec.lib.common.dmn.engine.engine.execution.context;
using net.adamec.lib.common.dmn.engine.engine.execution.result;
using net.adamec.lib.common.dmn.engine.parser;
using net.adamec.lib.common.dmn.engine.parser.dto;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Proxy.Interfaces;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Enums.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;
using static net.adamec.lib.common.dmn.engine.parser.DmnParser;

namespace EventsHandler.Services.DataProcessing.Strategy.Manager
{
    /// <inheritdoc cref="IScenariosResolver{INotifyScenario, NotificationEvent}"/>
    internal sealed class NotifyScenariosResolver : IScenariosResolver<INotifyScenario, NotificationEvent>
    {
        private readonly OmcConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDataQueryService<NotificationEvent> _dataQuery;

        private const string DmnPath = @".\Dmn\dmn_test.xml";

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyScenariosResolver"/> nested class.
        /// </summary>
        public NotifyScenariosResolver(
            OmcConfiguration configuration,
            IServiceProvider serviceProvider,
            IDataQueryService<NotificationEvent> dataQuery)  // Dependency Injection (DI)
        {
            this._configuration = configuration;
            this._serviceProvider = serviceProvider;
            this._dataQuery = dataQuery;
        }

        /// <inheritdoc cref="IScenariosResolver{INotifyScenario, NotificationEvent}.DetermineScenarioAsync(NotificationEvent)"/>
        async Task<INotifyScenario> IScenariosResolver<INotifyScenario, NotificationEvent>.DetermineScenarioAsync(NotificationEvent model)
        {
            DmnExecutionContext? ctx = null;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(DmnPath);

                // ✅ Explicitly specify DMN version
                DmnModel? dmnModel = DmnParser.Parse(DmnPath, DmnVersionEnum.V1_1);

                // ✅ Validate DMN model (from README)
                if (dmnModel == null)
                {
                    throw new Exception("Failed to parse DMN model.");
                }

                DmnDefinition? definition = DmnDefinitionFactory.CreateDmnDefinition(dmnModel);
                ctx = DmnExecutionContextFactory.CreateExecutionContext(definition);

                Console.WriteLine("DMN successfully loaded and parsed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Parsing DMN: {ex.Message}\n{ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                }

                throw;
            }

            if (ctx == null)
            {
                throw new InvalidOperationException("DMN execution context could not be created.");
            }

            // ✅ Ensure input variables match DMN definitions
            var variables = new Dictionary<string, object>
            {
                { "Action", "Create" },
                { "Channel", "Cases" },
                { "Resource", "Status" }
            };

            // ✅ Validate if decision exists before executing
            if (!ctx.Definition.Decisions.ContainsKey("scenarioPicker"))
            {
                throw new Exception("Decision 'scenarioPicker' not found in the DMN definition.");
            }

            // Case scenarios
            if (IsCaseScenario(model))
            {
                IQueryContext queryContext = this._dataQuery.From(model);
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
            if (IsObjectScenario(model))
            {
                Guid objectTypeId = model.Attributes.ObjectTypeUri.GetGuid();

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
                    string.Format(ApiResources.Processing_ABORT_DoNotSendNotification_Whitelist_GenObjectTypeGuid,
                        /* {0} */ $"{objectTypeId}",
                        /* {1} */ ConfigExtensions.GetGenericVariableObjectTypeEnvVarName()));
            }

            // Scenario #5: "Decision made"
            if (IsDecisionScenario(model))
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