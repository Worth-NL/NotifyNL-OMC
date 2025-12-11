// © 2023, Worth Systems.

using Common.Settings.Configuration;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases.Base;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;

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
        private IQueryContext _queryContext = null!;
        private Case _case;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseStatusUpdatedScenario"/> class.
        /// </summary>
        public CaseStatusUpdatedScenario(
            OmcConfiguration configuration,
            IDataQueryService<NotificationEvent> dataQuery,
            INotifyService<NotifyData> notifyService)  // Dependency Injection (DI)
            : base(configuration, dataQuery, notifyService)
        {
        }

        #region Polymorphic (PrepareDataAsync)
        /// <inheritdoc cref="BaseScenario.PrepareDataAsync(NotificationEvent)"/>
        protected override async Task<PreparedData> PrepareDataAsync(NotificationEvent notification)
        {
            // Setup
            this._queryContext = this.DataQuery.From(notification);

            // Validation #1: The case type identifier must be whitelisted
            ValidateCaseId(
                this.Configuration.ZGW.Whitelist.ZaakUpdate_IDs().IsAllowed,
                this._caseStatusType.Identification, GetWhitelistEnvVarName());

            this._case = await this._queryContext.GetCaseAsync(notification.MainObjectUri);

            // Preparing party details
            return new PreparedData(
                party: await this._queryContext.GetPartyDataAsync(this._case.Uri, caseIdentifier: this._case.Identification),
                caseUri: this._case.Uri);
        }
        #endregion

        #region Polymorphic (Email logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override Guid GetEmailTemplateId()
            => this.Configuration.Notify.TemplateId.Email.ZaakUpdate();

        private static readonly object s_padlock = new();
        private static readonly Dictionary<string, object> s_emailPersonalization = [];  // Cached dictionary no need to be initialized every time
        private static readonly Dictionary<string, object> s_letterPersonalization = [];  // Cached dictionary no need to be initialized every time

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalization(ZhvModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData)"/>
        protected override Dictionary<string, object> GetEmailPersonalization(CommonPartyData partyData)
        {
            lock (s_padlock)
            {
                s_emailPersonalization["klant.voornaam"] = partyData.Name;
                s_emailPersonalization["klant.voorvoegselAchternaam"] = partyData.SurnamePrefix;
                s_emailPersonalization["klant.achternaam"] = partyData.Surname;

                s_emailPersonalization["zaak.identificatie"] = this._case.Identification;
                s_emailPersonalization["zaak.omschrijving"] = this._case.Name;

                s_emailPersonalization["status.omschrijving"] = this._caseStatusType.Name;

                return s_emailPersonalization;
            }
        }
        #endregion

        #region Polymorphic (SMS logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override Guid GetSmsTemplateId()
          => this.Configuration.Notify.TemplateId.Sms.ZaakUpdate();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalization(ZhvModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData)"/>
        protected override Dictionary<string, object> GetSmsPersonalization(CommonPartyData partyData)
        {
            return GetEmailPersonalization(partyData);  // NOTE: Both implementations are identical
        }
        #endregion

        #region Polymorphic (Letter logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetLetterTemplateId"/>
        protected override Guid GetLetterTemplateId()
            => this.Configuration.Notify.TemplateId.Letter.ZaakUpdate();

        /// <inheritdoc cref="BaseScenario.GetLetterPersonalization(ZhvModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData)"/>
        protected override Dictionary<string, object> GetLetterPersonalization(CommonPartyData partyData)
        {
            lock (s_padlock)
            {
                s_letterPersonalization["klant.voornaam"] = partyData.Name;
                s_letterPersonalization["klant.voorvoegselAchternaam"] = partyData.SurnamePrefix;
                s_letterPersonalization["klant.achternaam"] = partyData.Surname;
                s_letterPersonalization["klant.street"] = partyData.LetterAddress.Street;
                s_letterPersonalization["klant.number"] = partyData.LetterAddress.Number;
                s_letterPersonalization["klant.zip"] = partyData.LetterAddress.Zip;
                s_letterPersonalization["klant.city"] = partyData.LetterAddress.City;
                s_letterPersonalization["klant.country"] = partyData.LetterAddress.Country;

                s_letterPersonalization["zaak.identificatie"] = this._case.Identification;
                s_letterPersonalization["zaak.omschrijving"] = this._case.Name;

                s_letterPersonalization["status.omschrijving"] = this._caseStatusType.Name;

                return s_letterPersonalization;
            }
        }
        #endregion

        #region Polymorphic (GetWhitelistEnvVarName)
        /// <inheritdoc cref="BaseScenario.GetWhitelistEnvVarName()"/>
        protected override string GetWhitelistEnvVarName() => this.Configuration.ZGW.Whitelist.ZaakUpdate_IDs().ToString();
        #endregion
    }
}