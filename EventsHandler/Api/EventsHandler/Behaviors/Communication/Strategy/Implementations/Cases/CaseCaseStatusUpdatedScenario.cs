// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Base;
using EventsHandler.Behaviors.Communication.Strategy.Implementations.Cases.Base;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;

namespace EventsHandler.Behaviors.Communication.Strategy.Implementations.Cases
{
    /// <summary>
    /// <inheritdoc cref="INotifyScenario"/>
    /// The strategy for "Case status updated" scenario.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    /// <seealso cref="BaseCaseScenario"/>
    internal sealed class CaseCaseStatusUpdatedScenario : BaseCaseScenario
    {
        /// <inheritdoc cref="CaseStatuses"/>
        private CaseStatuses? CachedCaseStatuses { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseCaseStatusUpdatedScenario"/> class.
        /// </summary>
        public CaseCaseStatusUpdatedScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
            : base(configuration, dataQuery)
        {
        }
        
        #region Polymorphic (Email logic)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override string GetEmailTemplateId()
            => this.Configuration.User.TemplateIds.Email.ZaakUpdate();

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalizationAsync(NotificationEvent, CommonPartyData)"/>
        protected override async Task<Dictionary<string, object>> GetEmailPersonalizationAsync(
            NotificationEvent notification, CommonPartyData partyData)
        {
            IQueryContext queryContext = this.DataQuery.From(notification);
            this.CachedCase ??= await queryContext.GetCaseAsync();
            this.CachedCaseStatuses ??= await queryContext.GetCaseStatusesAsync();
            this.CachedLastCaseStatusType ??= await queryContext.GetLastCaseStatusTypeAsync(this.CachedCaseStatuses);

            return new Dictionary<string, object>
            {
                { "zaak.omschrijving", this.CachedCase.Value.Name },
                { "zaak.identificatie", this.CachedCase.Value.Identification },
                { "klant.voorvoegselAchternaam", partyData.SurnamePrefix },
                { "klant.voornaam", partyData.Name },
                { "klant.achternaam", partyData.Surname },
                { "status.omschrijving", this.CachedLastCaseStatusType.Value.Description }
            };
        }
        #endregion
        
        #region Polymorphic (SMS logic)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override string GetSmsTemplateId()
          => this.Configuration.User.TemplateIds.Sms.ZaakUpdate();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalizationAsync(NotificationEvent, CommonPartyData)"/>
        protected override async Task<Dictionary<string, object>> GetSmsPersonalizationAsync(
            NotificationEvent notification, CommonPartyData partyData)
        {
            IQueryContext queryContext = this.DataQuery.From(notification);
            this.CachedCase ??= await queryContext.GetCaseAsync();
            this.CachedCaseStatuses ??= await queryContext.GetCaseStatusesAsync();
            this.CachedLastCaseStatusType ??= await queryContext.GetLastCaseStatusTypeAsync(this.CachedCaseStatuses);

            return new Dictionary<string, object>
            {
                { "zaak.omschrijving", this.CachedCase.Value.Name },
                { "zaak.identificatie", this.CachedCase.Value.Identification },
                { "klant.voornaam", partyData.Name },
                { "klant.voorvoegselAchternaam", partyData.SurnamePrefix },
                { "klant.achternaam", partyData.Surname },
                { "status.omschrijving", this.CachedLastCaseStatusType.Value.Description }
            };
        }
        #endregion

        #region Polymorphic (DropCache)
        /// <inheritdoc cref="BaseCaseScenario.DropCache()"/>
        /// <remarks>
        /// <list type="bullet">
        ///   <item><see cref="CachedCaseStatuses"/></item>
        /// </list>
        /// </remarks>
        protected override void DropCache()
        {
            base.DropCache();

            this.CachedCaseStatuses = null;
            this.CachedLastCaseStatusType = null;
        }
        #endregion
    }
}