// © 2024, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Base;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Enums.Objecten;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Configuration;
using EventsHandler.Exceptions;
using EventsHandler.Properties;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;

namespace EventsHandler.Behaviors.Communication.Strategy.Implementations
{
    /// <summary>
    /// <inheritdoc cref="INotifyScenario"/>
    /// The strategy for "Task assigned" scenario.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    internal sealed class TaskAssignedScenario : BaseScenario
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskAssignedScenario"/> class.
        /// </summary>
        public TaskAssignedScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
            : base(configuration, dataQuery)
        {
        }

        #region Polymorphic (GetAllNotifyDataAsync)
        /// <inheritdoc cref="BaseScenario.GetAllNotifyDataAsync(NotificationEvent)"/>
        /// <exception cref="AbortedNotifyingException"/>
        internal override async Task<NotifyData[]> GetAllNotifyDataAsync(NotificationEvent notification)
        {
            IQueryContext queryContext = this.DataQuery.From(notification);

            // Validation #1: Only the matching types (configuration) of the tasks should be processed
            if (!queryContext.IsValidType())
            {
                throw new AbortedNotifyingException(Resources.Processing_ABORT_DoNotSendNotification_TaskType);
            }

            if ((await queryContext.GetTaskAsync()).Record.Data.Status != TaskStatuses.Open)
            {
                throw new AbortedNotifyingException(Resources.Processing_ABORT_DoNotSendNotification_TaskClosed);
            }

            return await base.GetAllNotifyDataAsync(notification);
        }
        #endregion
        
        #region Polymorphic (Email logic)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override string GetEmailTemplateId()
            => throw new NotImplementedException();

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalizationAsync(NotificationEvent, CommonPartyData)"/>
        protected override async Task<Dictionary<string, object>> GetEmailPersonalizationAsync(
            NotificationEvent notification, CommonPartyData partyData)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Polymorphic (SMS logic)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override string GetSmsTemplateId()
          => throw new NotImplementedException();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalizationAsync(NotificationEvent, CommonPartyData)"/>
        protected override async Task<Dictionary<string, object>> GetSmsPersonalizationAsync(
            NotificationEvent notification, CommonPartyData partyData)
        {
            throw new NotImplementedException();
        }
        #endregion
        
        #region Polymorphic (DropCache)
        /// <inheritdoc cref="BaseScenario.DropCache()"/>
        /// <remarks>
        /// <list type="bullet">
        /// </list>
        /// </remarks>
        protected override void DropCache()
        {
            base.DropCache();
        }
        #endregion
    }
}