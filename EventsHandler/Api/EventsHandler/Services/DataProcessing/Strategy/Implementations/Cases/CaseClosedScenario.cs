﻿// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases.Base;
using EventsHandler.Services.DataProcessing.Strategy.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.Settings.Configuration;

namespace EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases
{
    /// <summary>
    /// <inheritdoc cref="INotifyScenario"/>
    /// The strategy for "Case finished" scenario.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    /// <seealso cref="BaseCaseScenario"/>
    internal sealed class CaseClosedScenario : BaseCaseScenario
    {
        /// <inheritdoc cref="CaseStatuses"/>
        private CaseStatuses? CachedCaseStatuses { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseClosedScenario"/> class.
        /// </summary>
        public CaseClosedScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
            : base(configuration, dataQuery)
        {
        }

        #region Polymorphic (Email logic)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override string GetEmailTemplateId()
            => this.Configuration.User.TemplateIds.Email.ZaakClose();

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalizationAsync(CommonPartyData)"/>
        protected override async Task<Dictionary<string, object>> GetEmailPersonalizationAsync(CommonPartyData partyData)
        {
            this.CachedCase ??= await this.QueryContext!.GetCaseAsync();

            ValidateCaseId(
                this.Configuration.User.Whitelist.ZaakClose_IDs().IsAllowed,
                this.CachedCase.Value.Identification, GetWhitelistName());

            this.CachedCaseStatuses ??= await this.QueryContext!.GetCaseStatusesAsync();
            this.CachedCaseType ??= await this.QueryContext!.GetLastCaseTypeAsync(this.CachedCaseStatuses);

            ValidateNotifyPermit(this.CachedCaseType.Value.IsNotificationExpected);

            return new Dictionary<string, object>
            {
                { "zaak.omschrijving", this.CachedCase.Value.Name },
                { "zaak.identificatie", this.CachedCase.Value.Identification },
                { "klant.voornaam", partyData.Name },
                { "klant.voorvoegselAchternaam", partyData.SurnamePrefix },
                { "klant.achternaam", partyData.Surname },
                { "status.omschrijving", this.CachedCaseType.Value.Description }
            };
        }
        #endregion

        #region Polymorphic (SMS logic)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId"/>
        protected override string GetSmsTemplateId()
            => this.Configuration.User.TemplateIds.Sms.ZaakClose();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalizationAsync(CommonPartyData)"/>
        protected override async Task<Dictionary<string, object>> GetSmsPersonalizationAsync(CommonPartyData partyData)
        {
            return await GetEmailPersonalizationAsync(partyData);  // NOTE: Both implementations are identical
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
        }
        #endregion

        #region Polymorphic (GetWhitelistName)
        /// <inheritdoc cref="BaseScenario.GetWhitelistName"/>
        protected override string GetWhitelistName() => this.Configuration.User.Whitelist.ZaakClose_IDs().ToString();
        #endregion
    }
}