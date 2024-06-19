// © 2024, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Base;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Interfaces;

namespace EventsHandler.Behaviors.Communication.Strategy
{
    /// <summary>
    /// <inheritdoc cref="INotifyScenario"/>
    /// The strategy for "Decision made" scenario.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    internal sealed class DecisionMadeScenario : BaseScenario
    {
        private Decision? CachedDecision { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecisionMadeScenario"/> class.
        /// </summary>
        public DecisionMadeScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
            : base(configuration, dataQuery)
        {
        }

        #region Polymorphic
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override string GetSmsTemplateId()
          => this.Configuration.User.TemplateIds.Sms.DecisionMade();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalizationAsync(NotificationEvent, CommonPartyData)"/>
        protected override async Task<Dictionary<string, object>> GetSmsPersonalizationAsync(
            NotificationEvent notification, CommonPartyData partyData)
        {
            this.CachedDecision ??= await this.DataQuery.From(notification).GetDecisionAsync();
            this.CachedCase ??= await this.DataQuery.From(notification).GetCaseAsync(this.CachedDecision.Value.CaseTypeUrl);

            return new Dictionary<string, object>
            {
                { "zaak.omschrijving", this.CachedCase.Value.Name },
                { "zaak.identificatie", this.CachedCase.Value.Identification }
            };
        }

        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override string GetEmailTemplateId()
          => this.Configuration.User.TemplateIds.Email.DecisionMade();

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalizationAsync(NotificationEvent, CommonPartyData)"/>
        protected override async Task<Dictionary<string, object>> GetEmailPersonalizationAsync(
            NotificationEvent notification, CommonPartyData partyData)
        {
            this.CachedDecision ??= await this.DataQuery.From(notification).GetDecisionAsync();
            this.CachedCase ??= await this.DataQuery.From(notification).GetCaseAsync(this.CachedDecision.Value.CaseTypeUrl);

            return new Dictionary<string, object>
            {
                { "zaak.omschrijving", this.CachedCase.Value.Name },
                { "zaak.identificatie", this.CachedCase.Value.Identification }
            };
        }

        /// <inheritdoc cref="BaseScenario.DropCache()"/>
        protected override void DropCache()
        {
            this.CachedDecision = null;
            base.DropCache();
        }
        #endregion
    }
}