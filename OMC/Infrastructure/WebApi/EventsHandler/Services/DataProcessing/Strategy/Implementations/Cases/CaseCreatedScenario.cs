// © 2023, Worth Systems.

using Common.Settings.Configuration;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Cases.Base;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.Tracing;
using ZgwModels.Extensions;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.OpenZaak;

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
        private IQueryContext _queryContext = null!;
        private Case _case;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseCreatedScenario"/> class.
        /// </summary>
        public CaseCreatedScenario(
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
            // Setup set on Notification but base not used suspected thread issue
            this._queryContext = this.DataQuery.From(notification);

            // Validation #1: The case type identifier must be whitelisted
            ValidateCaseId(
                this.Configuration.ZGW.Whitelist.ZaakCreate_IDs().IsAllowed,
                this._caseStatusType.Identification, GetWhitelistEnvVarName());

            Guid zaakId = notification.MainObjectUri.GetGuid();
            TraceContext.Emit("openzaak", "start", $"Attempting to retrieve zaak with id {zaakId}");
            this._case = await this._queryContext.GetCaseAsync(notification.MainObjectUri);
            TraceContext.Emit("openzaak", "ok", $"zaak with id {zaakId} retrieved");

            // Preparing party details
            TraceContext.Emit("openklant", "start", $"Attempting to retrieve klant for zaak with id {zaakId}");
            CommonPartyData party = await this._queryContext.GetPartyDataAsync(this._case.Uri, caseIdentifier: this._case.Identification);
            TraceContext.Emit("openklant", "ok", $"klant for zaak with id {zaakId} retrieved");

            return new PreparedData(party: party, caseUri: this._case.Uri, subjectTypeUri: this._case.CaseTypeUri);
        }
        #endregion

        #region Polymorphic (Email logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override Guid GetEmailTemplateId()
            => this.Configuration.Notify.TemplateId.Email.ZaakCreate();

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalization(ZgwModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData)"/>
        protected override Dictionary<string, object> GetEmailPersonalization(CommonPartyData partyData)
        {
            Dictionary<string, object> personalization = [];

            personalization["klant.voornaam"] = partyData.Name;
            personalization["klant.voorvoegselAchternaam"] = partyData.SurnamePrefix;
            personalization["klant.achternaam"] = partyData.Surname;

            personalization["zaak.identificatie"] = this._case.Identification;
            personalization["zaak.omschrijving"] = this._case.Name;

            return personalization;
        }
        #endregion

        #region Polymorphic (SMS logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override Guid GetSmsTemplateId()
          => this.Configuration.Notify.TemplateId.Sms.ZaakCreate();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalization(ZgwModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData)"/>
        protected override Dictionary<string, object> GetSmsPersonalization(CommonPartyData partyData)
        {
            return GetEmailPersonalization(partyData);  // NOTE: Both implementations are identical
        }
        #endregion

        #region Polymorphic (Letter logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetLetterTemplateId"/>
        protected override Guid GetLetterTemplateId()
            => this.Configuration.Notify.TemplateId.Letter.ZaakCreate();

        /// <inheritdoc cref="BaseScenario.GetLetterPersonalization(ZgwModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData)"/>
        protected override Dictionary<string, object> GetLetterPersonalization(CommonPartyData partyData)
        {
            Dictionary<string, object> personalization = [];

            personalization["klant.voornaam"] = partyData.Name;
            personalization["klant.voorvoegselAchternaam"] = partyData.SurnamePrefix;
            personalization["klant.achternaam"] = partyData.Surname;
            personalization["klant.street"] = partyData.LetterAddress.Street;
            personalization["klant.number"] = partyData.LetterAddress.Number;
            personalization["klant.zip"] = partyData.LetterAddress.Zip;
            personalization["klant.city"] = partyData.LetterAddress.City;
            personalization["klant.country"] = partyData.LetterAddress.Country;

            personalization["zaak.identificatie"] = this._case.Identification;
            personalization["zaak.omschrijving"] = this._case.Name;

            return personalization;
        }
        #endregion

        #region Polymorphic (GetWhitelistEnvVarName)
        /// <inheritdoc cref="BaseScenario.GetWhitelistEnvVarName()"/>
        protected override string GetWhitelistEnvVarName() => this.Configuration.ZGW.Whitelist.ZaakCreate_IDs().ToString();
        #endregion
    }
}