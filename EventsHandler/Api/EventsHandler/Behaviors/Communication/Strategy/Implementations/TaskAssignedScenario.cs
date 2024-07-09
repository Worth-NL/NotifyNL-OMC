// © 2024, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Base;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Enums.Objecten;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.Objecten;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Configuration;
using EventsHandler.Constants;
using EventsHandler.Exceptions;
using EventsHandler.Properties;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using System.Globalization;

namespace EventsHandler.Behaviors.Communication.Strategy.Implementations
{
    /// <summary>
    /// <inheritdoc cref="INotifyScenario"/>
    /// The strategy for "Task assigned" scenario.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    internal sealed class TaskAssignedScenario : BaseScenario
    {
        /// <inheritdoc cref="Data"/>
        private Data? CachedTaskData { get; set; }

        /// <inheritdoc cref="Case"/>
        private Case? CachedCase { get; set; }

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

            // Validation #1: The task needs to be of a specific type
            if (!queryContext.IsValidType())
            {
                throw new AbortedNotifyingException(Resources.Processing_ABORT_DoNotSendNotification_TaskType);
            }

            this.CachedTaskData ??= (await queryContext.GetTaskAsync()).Record.Data;

            // Validation #2: The task needs to have an open status
            if (this.CachedTaskData.Value.Status != TaskStatuses.Open)
            {
                throw new AbortedNotifyingException(Resources.Processing_ABORT_DoNotSendNotification_TaskClosed);
            }

            // Validation #3: The task needs to be assigned to a person
            if (this.CachedTaskData.Value.Identification.Type != IdTypes.Bsn)
            {
                throw new AbortedNotifyingException(Resources.Processing_ABORT_DoNotSendNotification_TaskNotPerson);
            }

            this.CachedCommonPartyData ??=
                await queryContext.GetPartyDataAsync(this.CachedTaskData.Value.Identification.Value);

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
            IQueryContext queryContext = this.DataQuery.From(notification);

            this.CachedCase ??= await queryContext.GetCaseAsync(this.CachedTaskData!.Value.CaseUrl);

            bool hasExpirationDate = this.CachedTaskData!.Value.ExpirationDate == default;
            string expirationDate = hasExpirationDate
                ? this.CachedTaskData!.Value.ExpirationDate.ToString(new CultureInfo("nl-NL"))
                : DefaultValues.Models.DefaultEnumValueName;

            return new Dictionary<string, object>
            {
                { "taak.verloopdatum", expirationDate },
                { "taak.heeft_verloopdatum", hasExpirationDate ? "yes" : "no" },
                { "taak.record.data.title", this.CachedTaskData!.Value.Title },
                { "zaak.omschrijving", this.CachedCase.Value.Name },
                { "zaak.identificatie", this.CachedCase.Value.Identification }
            };
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
            return await GetEmailPersonalizationAsync(notification, partyData);  // NOTE: Both implementations are identical
        }
        #endregion

        #region Polymorphic (DropCache)
        /// <inheritdoc cref="BaseScenario.DropCache()"/>
        /// <remarks>
        /// <list type="bullet">
        ///   <item><see cref="CachedTaskData"/></item>
        ///   <item><see cref="CachedCase"/></item>
        /// </list>
        /// </remarks>
        protected override void DropCache()
        {
            this.CachedTaskData = null;
            this.CachedCase = null;

            base.DropCache();
        }
        #endregion
    }
}