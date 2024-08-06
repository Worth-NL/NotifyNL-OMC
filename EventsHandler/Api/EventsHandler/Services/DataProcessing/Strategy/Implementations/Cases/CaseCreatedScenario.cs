// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases.Base;
using EventsHandler.Services.DataProcessing.Strategy.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.Settings.Configuration;

namespace EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases
{
    /// <summary>
    /// <inheritdoc cref="INotifyScenario"/>
    /// The strategy for "Case created" scenario.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    /// <seealso cref="BaseCaseScenario"/>
    internal sealed class CaseCreatedScenario : BaseCaseScenario
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaseCreatedScenario"/> class.
        /// </summary>
        public CaseCreatedScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
            : base(configuration, dataQuery)
        {
        }

        #region Polymorphic (Email logic)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override Guid GetEmailTemplateId()
            => this.Configuration.User.TemplateIds.Email.ZaakCreate();

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalizationAsync(CommonPartyData)"/>
        protected override async Task<Dictionary<string, object>> GetEmailPersonalizationAsync(CommonPartyData partyData)
        {
            this.CachedCase ??= await this.QueryContext!.GetCaseAsync();

            ValidateCaseId(
                this.Configuration.User.Whitelist.ZaakCreate_IDs().IsAllowed,
                this.CachedCase.Value.Identification, GetWhitelistName());

            ValidateNotifyPermit((
                await this.QueryContext!.GetLastCaseTypeAsync(     // Case type
                await this.QueryContext!.GetCaseStatusesAsync()))  // Case status
                .IsNotificationExpected);

            return new Dictionary<string, object>
            {
                { "zaak.omschrijving", this.CachedCase.Value.Name },
                { "zaak.identificatie", this.CachedCase.Value.Identification },
                { "klant.voornaam", partyData.Name },
                { "klant.voorvoegselAchternaam", partyData.SurnamePrefix },
                { "klant.achternaam", partyData.Surname }
            };
        }
        #endregion

        #region Polymorphic (SMS logic)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override Guid GetSmsTemplateId()
          => this.Configuration.User.TemplateIds.Sms.ZaakCreate();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalizationAsync(CommonPartyData)"/>
        protected override async Task<Dictionary<string, object>> GetSmsPersonalizationAsync(CommonPartyData partyData)
        {
            return await GetEmailPersonalizationAsync(partyData);  // NOTE: Both implementations are identical
        }
        #endregion

        #region Polymorphic (GetWhitelistName)
        /// <inheritdoc cref="BaseScenario.GetWhitelistName"/>
        protected override string GetWhitelistName() => this.Configuration.User.Whitelist.ZaakCreate_IDs().ToString();
        #endregion
    }
}