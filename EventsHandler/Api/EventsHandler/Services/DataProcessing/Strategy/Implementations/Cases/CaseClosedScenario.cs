// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases.Base;
using EventsHandler.Services.DataProcessing.Strategy.Interfaces;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="CaseClosedScenario"/> class.
        /// </summary>
        public CaseClosedScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
            : base(configuration, dataQuery)
        {
        }

        #region Polymorphic (PrepareDataAsync)
        /// <inheritdoc cref="BaseScenario.PrepareDataAsync(NotificationEvent)"/>
        protected override async Task<CommonPartyData> PrepareDataAsync(NotificationEvent notification)
        {
            // Setup
            IQueryContext queryContext = this.DataQuery.From(notification);

            this.CachedCase = await queryContext.GetCaseAsync();

            // Validation #1: The case identifier must be whitelisted
            ValidateCaseId(
                this.Configuration.User.Whitelist.ZaakClose_IDs().IsAllowed,
                this.CachedCase.Identification, GetWhitelistName());
            
            this.CachedCaseType ??= await queryContext.GetLastCaseTypeAsync(     // 2. Case type
                                    await queryContext.GetCaseStatusesAsync());  // 1. Case statuses

            // Validation #2: The notifications must be enabled
            ValidateNotifyPermit(this.CachedCaseType.Value.IsNotificationExpected);

            // Preparing citizen details
            return await queryContext.GetPartyDataAsync();
        }
        #endregion

        #region Polymorphic (Email logic)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override Guid GetEmailTemplateId()
            => this.Configuration.User.TemplateIds.Email.ZaakClose();

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalization(CommonPartyData)"/>
        protected override Dictionary<string, object> GetEmailPersonalization(CommonPartyData partyData)
        {
            return new Dictionary<string, object>
            {
                { "zaak.omschrijving", this.CachedCase.Name },
                { "zaak.identificatie", this.CachedCase.Identification },

                { "klant.voornaam", partyData.Name },
                { "klant.voorvoegselAchternaam", partyData.SurnamePrefix },
                { "klant.achternaam", partyData.Surname },

                { "status.omschrijving", this.CachedCaseType!.Value.Name }
            };
        }
        #endregion

        #region Polymorphic (SMS logic)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId"/>
        protected override Guid GetSmsTemplateId()
            => this.Configuration.User.TemplateIds.Sms.ZaakClose();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalization(CommonPartyData)"/>
        protected override Dictionary<string, object> GetSmsPersonalization(CommonPartyData partyData)
        {
            return GetEmailPersonalization(partyData);  // NOTE: Both implementations are identical
        }
        #endregion

        #region Polymorphic (GetWhitelistName)
        /// <inheritdoc cref="BaseScenario.GetWhitelistName"/>
        protected override string GetWhitelistName() => this.Configuration.User.Whitelist.ZaakClose_IDs().ToString();
        #endregion
    }
}