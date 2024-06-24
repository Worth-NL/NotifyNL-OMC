// © 2024, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Base;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;

namespace EventsHandler.Behaviors.Communication.Strategy.Implementations
{
    /// <summary>
    /// <inheritdoc cref="INotifyScenario"/>
    /// The strategy for "Decision made" scenario.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    internal sealed class DecisionMadeScenario : BaseScenario
    {
        /// <inheritdoc cref="Decision"/>
        private Decision? CachedDecision { get; set; }

        /// <inheritdoc cref="Case"/>
        private Case? CachedCase { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecisionMadeScenario"/> class.
        /// </summary>
        public DecisionMadeScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
            : base(configuration, dataQuery)
        {
        }

        #region Polymorphic (GetAllNotifyDataAsync)
        /// <inheritdoc cref="BaseScenario.GetAllNotifyDataAsync(NotificationEvent)"/>
        internal override async Task<NotifyData[]> GetAllNotifyDataAsync(NotificationEvent notification)
        {
            IQueryContext queryContext = this.DataQuery.From(notification);
            this.CachedDecision ??= await queryContext.GetDecisionAsync();
            this.CachedCommonPartyData ??=
                await queryContext.GetPartyDataAsync(
                await queryContext.GetBsnNumberAsync(
                      this.CachedDecision.Value.CaseTypeUrl));
            
            return await base.GetAllNotifyDataAsync(notification);
        }
        #endregion
        
        #region Polymorphic (Email logic)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override string GetEmailTemplateId()
            => this.Configuration.User.TemplateIds.Email.DecisionMade();

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalizationAsync(NotificationEvent, CommonPartyData)"/>
        protected override async Task<Dictionary<string, object>> GetEmailPersonalizationAsync(
            NotificationEvent notification, CommonPartyData partyData)
        {
            this.CachedCase ??= await this.DataQuery.From(notification)
                                                    .GetCaseAsync(this.CachedDecision!.Value.CaseTypeUrl);
            return new Dictionary<string, object>
            {
                { "zaak.omschrijving", this.CachedCase.Value.Name },
                { "zaak.identificatie", this.CachedCase.Value.Identification }
            };
        }
        #endregion

        #region Polymorphic (SMS logic)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override string GetSmsTemplateId()
          => this.Configuration.User.TemplateIds.Sms.DecisionMade();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalizationAsync(NotificationEvent, CommonPartyData)"/>
        protected override async Task<Dictionary<string, object>> GetSmsPersonalizationAsync(
            NotificationEvent notification, CommonPartyData partyData)
        {
            this.CachedCase ??= await this.DataQuery.From(notification)
                                                    .GetCaseAsync(this.CachedDecision!.Value.CaseTypeUrl);
            return new Dictionary<string, object>
            {
                { "zaak.omschrijving", this.CachedCase.Value.Name },
                { "zaak.identificatie", this.CachedCase.Value.Identification }
            };
        }
        #endregion
        
        #region Polymorphic (DropCache)
        /// <inheritdoc cref="BaseScenario.DropCache()"/>
        /// <remarks>
        /// <list type="bullet">
        ///   <item><see cref="CachedDecision"/></item>
        ///   <item><see cref="CachedCase"/></item>
        /// </list>
        /// </remarks>
        protected override void DropCache()
        {
            base.DropCache();

            this.CachedDecision = null;
            this.CachedCase = null;
        }
        #endregion
    }
}