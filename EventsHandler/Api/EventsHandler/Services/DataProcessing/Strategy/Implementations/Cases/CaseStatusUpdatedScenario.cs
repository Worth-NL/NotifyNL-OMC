﻿// © 2023, Worth Systems.

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
    /// The strategy for "Case status updated" scenario.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    /// <seealso cref="BaseCaseScenario"/>
    internal sealed class CaseStatusUpdatedScenario : BaseCaseScenario
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaseStatusUpdatedScenario"/> class.
        /// </summary>
        public CaseStatusUpdatedScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
            : base(configuration, dataQuery)
        {
        }

        #region Polymorphic (PrepareDataAsync)
        /// <inheritdoc cref="BaseScenario.PrepareDataAsync(NotificationEvent)"/>
        protected override async Task<CommonPartyData> PrepareDataAsync(NotificationEvent notification)
        {
            // Setup
            this.QueryContext ??= this.DataQuery.From(notification);
            this.CachedCase ??= await this.QueryContext.GetCaseAsync();

            // Validation #1: The case identifier must be whitelisted
            ValidateCaseId(
                this.Configuration.User.Whitelist.ZaakUpdate_IDs().IsAllowed,
                this.CachedCase.Value.Identification, GetWhitelistName());
            
            this.CachedCaseType ??= await this.QueryContext.GetLastCaseTypeAsync(     // 2. Case type
                                    await this.QueryContext.GetCaseStatusesAsync());  // 1. Case statuses

            // Validation #2: The notifications must be enabled
            ValidateNotifyPermit(this.CachedCaseType.Value.IsNotificationExpected);
            
            // Preparing citizen details
            return await this.QueryContext.GetPartyDataAsync();
        }
        #endregion

        #region Polymorphic (Email logic)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override Guid GetEmailTemplateId()
            => this.Configuration.User.TemplateIds.Email.ZaakUpdate();

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalization(CommonPartyData)"/>
        protected override Dictionary<string, object> GetEmailPersonalization(CommonPartyData partyData)
        {
            return new Dictionary<string, object>
            {
                { "zaak.omschrijving", this.CachedCase!.Value.Name },
                { "zaak.identificatie", this.CachedCase!.Value.Identification },
                { "klant.voornaam", partyData.Name },
                { "klant.voorvoegselAchternaam", partyData.SurnamePrefix },
                { "klant.achternaam", partyData.Surname },
                { "status.omschrijving", this.CachedCaseType!.Value.Name }
            };
        }
        #endregion

        #region Polymorphic (SMS logic)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override Guid GetSmsTemplateId()
          => this.Configuration.User.TemplateIds.Sms.ZaakUpdate();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalization(CommonPartyData)"/>
        protected override Dictionary<string, object> GetSmsPersonalization(CommonPartyData partyData)
        {
            return GetEmailPersonalization(partyData);  // NOTE: Both implementations are identical
        }
        #endregion

        #region Polymorphic (GetWhitelistName)
        /// <inheritdoc cref="BaseScenario.GetWhitelistName"/>
        protected override string GetWhitelistName() => this.Configuration.User.Whitelist.ZaakUpdate_IDs().ToString();
        #endregion
    }
}