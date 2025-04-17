using Common.Settings.Configuration;
using EventsHandler.Dmn;
using EventsHandler.Exceptions;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases;
using EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces;

using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Proxy.Interfaces;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;

namespace EventsHandler.Services.DataProcessing.Strategy.Manager
{
    /// <summary>
    /// Resolves the appropriate <see cref="INotifyScenario"/> for a given <see cref="NotificationEvent"/>
    /// by evaluating input parameters using a DMN decision table.
    /// </summary>
    /// <inheritdoc cref="IScenariosResolver{INotifyScenario, NotificationEvent}"/>
    internal sealed class NotifyScenariosResolver : IScenariosResolver<INotifyScenario, NotificationEvent>
    {
        private readonly OmcConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDataQueryService<NotificationEvent> _dataQuery;
        private readonly IDmnDecisionService _dmnService;
        private readonly string _scenarioDecisionName = "Scenario Picker";

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyScenariosResolver"/> class.
        /// </summary>
        public NotifyScenariosResolver(
            OmcConfiguration configuration,
            IServiceProvider serviceProvider,
            IDataQueryService<NotificationEvent> dataQuery,
            IDmnDecisionService dmnService)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _dataQuery = dataQuery;
            _dmnService = dmnService;
        }

        public NotifyScenariosResolver(OmcConfiguration omcConfiguration, ServiceProvider serviceProvider, IDataQueryService<NotificationEvent> dataQuery)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<INotifyScenario> DetermineScenarioAsync(NotificationEvent model)
        {
            var input = new Dictionary<string, object>
            {
                { "Action", model.Action.ToString() },
                { "Channel", model.Channel.ToString() },
                { "Resource", model.Resource.ToString() }
            };

            string? scenarioKey = _dmnService.EvaluateDecision(_scenarioDecisionName, input);

            return scenarioKey switch
            {
                "CaseScenario" => await ResolveCaseScenarioAsync(model),
                "ObjectScenario" => ResolveObjectScenario(model),
                "DecisionScenario" => _serviceProvider.GetRequiredService<DecisionMadeScenario>(),
                _ => _serviceProvider.GetRequiredService<NotImplementedScenario>()
            };
        }

        /// <summary>
        /// Handles case-related scenarios based on case status history and finality.
        /// </summary>
        private async Task<INotifyScenario> ResolveCaseScenarioAsync(NotificationEvent model)
        {
            IQueryContext queryContext = _dataQuery.From(model);
            CaseStatuses caseStatuses = await queryContext.GetCaseStatusesAsync();

            if (caseStatuses.WereNeverUpdated())
            {
                return _serviceProvider.GetRequiredService<CaseCreatedScenario>();
            }

            CaseType lastCaseType = await queryContext.GetLastCaseTypeAsync(caseStatuses);

            return !lastCaseType.IsFinalStatus
                ? _serviceProvider.GetRequiredService<CaseStatusUpdatedScenario>()
                : _serviceProvider.GetRequiredService<CaseClosedScenario>();
        }

        /// <summary>
        /// Handles object-based scenarios by inspecting the object type.
        /// </summary>
        private INotifyScenario ResolveObjectScenario(NotificationEvent model)
        {
            Guid objectTypeId = model.Attributes.ObjectTypeUri.GetGuid();

            if (objectTypeId == _configuration.ZGW.Variable.ObjectType.TaskObjectType_Uuid())
            {
                return _serviceProvider.GetRequiredService<TaskAssignedScenario>();
            }

            if (objectTypeId == _configuration.ZGW.Variable.ObjectType.MessageObjectType_Uuid())
            {
                return _serviceProvider.GetRequiredService<MessageReceivedScenario>();
            }

            throw new AbortedNotifyingException(string.Format(ApiResources.Processing_ABORT_DoNotSendNotification_Whitelist_GenObjectTypeGuid,
                objectTypeId,
                Common.Settings.Extensions.ConfigExtensions.GetGenericVariableObjectTypeEnvVarName()));
        }
    }
}
