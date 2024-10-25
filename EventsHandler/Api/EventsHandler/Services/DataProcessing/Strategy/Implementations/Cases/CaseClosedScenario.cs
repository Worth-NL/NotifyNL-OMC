// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases.Base;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
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
        private IQueryContext _queryContext = null!;
        private CaseType _caseType;
        private Case _case;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseClosedScenario"/> class.
        /// </summary>
        public CaseClosedScenario(
            WebApiConfiguration configuration,
            IDataQueryService<NotificationEvent> dataQuery,
            INotifyService<NotifyData> notifyService)
            : base(configuration, dataQuery, notifyService)
        {
        }

        #region Polymorphic (PrepareDataAsync)
        /// <inheritdoc cref="BaseScenario.PrepareDataAsync(NotificationEvent)"/>
        protected override async Task<PreparedData> PrepareDataAsync(NotificationEvent notification)
        {
            // Setup
            this._queryContext = this.DataQuery.From(notification);
            
            this._caseType = await this._queryContext.GetLastCaseTypeAsync(     // 2. Case type
                             await this._queryContext.GetCaseStatusesAsync());  // 1. Case statuses
            
            // Validation #1: The case type identifier must be whitelisted
            ValidateCaseId(
                this.Configuration.ZGW.Whitelist.ZaakClose_IDs().IsAllowed,
                this._caseType.Identification, GetWhitelistEnvVarName());

            // Validation #2: The notifications must be enabled
            ValidateNotifyPermit(this._caseType.IsNotificationExpected);

            this._case = await this._queryContext.GetCaseAsync();

            // Preparing party details
            return new PreparedData(
                party: await this._queryContext.GetPartyDataAsync(this._case.Uri),
                caseUri: this._case.Uri);
        }
        #endregion

        #region Polymorphic (Email logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override Guid GetEmailTemplateId()
            => this.Configuration.ZGW.TemplateIds.Email.ZaakClose();

        private static readonly object s_padlock = new();
        private static readonly Dictionary<string, object> s_emailPersonalization = new();  // Cached dictionary no need to be initialized every time

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalization(CommonPartyData)"/>
        protected override Dictionary<string, object> GetEmailPersonalization(CommonPartyData partyData)
        {
            lock (s_padlock)
            {
                s_emailPersonalization["klant.voornaam"] = partyData.Name;
                s_emailPersonalization["klant.voorvoegselAchternaam"] = partyData.SurnamePrefix;
                s_emailPersonalization["klant.achternaam"] = partyData.Surname;

                s_emailPersonalization["zaak.identificatie"] = this._case.Identification;
                s_emailPersonalization["zaak.omschrijving"] = this._case.Name;

                s_emailPersonalization["status.omschrijving"] = this._caseType.Name;

                return s_emailPersonalization;
            }
        }
        #endregion

        #region Polymorphic (SMS logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId"/>
        protected override Guid GetSmsTemplateId()
            => this.Configuration.ZGW.TemplateIds.Sms.ZaakClose();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalization(CommonPartyData)"/>
        protected override Dictionary<string, object> GetSmsPersonalization(CommonPartyData partyData)
        {
            return GetEmailPersonalization(partyData);  // NOTE: Both implementations are identical
        }
        #endregion

        #region Polymorphic (GetWhitelistEnvVarName)
        /// <inheritdoc cref="BaseScenario.GetWhitelistEnvVarName()"/>
        protected override string GetWhitelistEnvVarName() => this.Configuration.ZGW.Whitelist.ZaakClose_IDs().ToString();
        #endregion
    }
}