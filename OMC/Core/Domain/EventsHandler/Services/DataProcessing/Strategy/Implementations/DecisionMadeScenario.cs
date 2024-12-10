// © 2024, Worth Systems.

using Common.Extensions;
using Common.Settings.Configuration;
using Common.Settings.Extensions;
using EventsHandler.Exceptions;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Models.Responses;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.DataSending.Models.Reponses;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Enums.NotificatieApi;
using ZhvModels.Mapping.Enums.Objecten;
using ZhvModels.Mapping.Enums.OpenZaak;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;
using ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision;

namespace EventsHandler.Services.DataProcessing.Strategy.Implementations
{
    /// <summary>
    /// <inheritdoc cref="INotifyScenario"/>
    /// The strategy for "Decision made" scenario.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    internal sealed partial class DecisionMadeScenario : BaseScenario  // The keyword "partial" added to allow using [GeneratedRegex] attribute
    {
        private IQueryContext _queryContext = null!;
        private DecisionResource _decisionResource;
        private Decision _decision;
        private CaseType _caseType;
        private string _bsnNumber = string.Empty;
        private DecisionType _decisionType;
        private Case _case;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecisionMadeScenario"/> class.
        /// </summary>
        public DecisionMadeScenario(
            WebApiConfiguration configuration,
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
            
            this._decisionResource = await this._queryContext.GetDecisionResourceAsync();
            InfoObject infoObject = await this._queryContext.GetInfoObjectAsync(this._decisionResource);

            // Validation #1: The info object needs to be of a specific type
            Guid infoObjectTypeId = infoObject.TypeUri.GetGuid();
            if (!this.Configuration.ZGW.Variable.ObjectType.DecisionInfoObjectType_Uuids().Contains(infoObjectTypeId))
            {
                throw new AbortedNotifyingException(
                    string.Format(ApiResources.Processing_ABORT_DoNotSendNotification_Whitelist_InfoObjectType,
                        /* {0} */ $"{infoObjectTypeId}",
                        /* {1} */ ConfigExtensions.GetVariableInfoObjectsEnvVarName()));
            }

            // Validation #2: Status needs to be definitive
            if (infoObject.Status != MessageStatus.Definitive)
            {
                throw new AbortedNotifyingException(ApiResources.Processing_ABORT_DoNotSendNotification_DecisionStatus);
            }

            // Validation #3: Confidentiality needs to be acceptable
            if (infoObject.Confidentiality != PrivacyNotices.NonConfidential)
            {
                throw new AbortedNotifyingException(
                    string.Format(ApiResources.Processing_ABORT_DoNotSendNotification_DecisionConfidentiality,
                        infoObject.Confidentiality));
            }

            this._decision = await this._queryContext.GetDecisionAsync(this._decisionResource);
            this._caseType = await this._queryContext.GetLastCaseTypeAsync(  // 3. Case type
                             await this._queryContext.GetCaseStatusesAsync(  // 2. Case statuses
                                   this._decision.CaseUri));                 // 1. Case URI

            // Validation #4: The case type identifier must be whitelisted
            ValidateCaseId(
                this.Configuration.ZGW.Whitelist.DecisionMade_IDs().IsAllowed,
                this._caseType.Identification, GetWhitelistEnvVarName());

            // Validation #5: The notifications must be enabled
            ValidateNotifyPermit(this._caseType.IsNotificationExpected);

            // Preparing party details
            this._bsnNumber = await GetBsnNumberAsync();
            this._decisionType = await this._queryContext.GetDecisionTypeAsync(this._decision);
            this._case = await this._queryContext.GetCaseAsync(this._decision.CaseUri);

            return new PreparedData(
                party: await this._queryContext.GetPartyDataAsync(
                    this._case.Uri,
                    this._bsnNumber),
                caseUri: this._case.Uri);
        }

        private async Task<string> GetBsnNumberAsync()
        {
            try
            {
                return await this._queryContext.GetBsnNumberAsync(  // 2. BSN number
                             this._decision.CaseUri);               // 1. Case URI
            }
            catch (Exception)
            {
                return string.Empty;  // NOTE: Organization will not have BSN number
            }
        }
        #endregion

        #region Polymorphic (Email logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override Guid GetEmailTemplateId()
            => this.Configuration.Notify.TemplateId.DecisionMade();  // NOTE: Decision has only one template

        private static readonly object s_padlock = new();
        private static readonly Dictionary<string, object> s_emailPersonalization = [];  // Cached dictionary no need to be initialized every time

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalization(ZhvModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData)"/>
        protected override Dictionary<string, object> GetEmailPersonalization(CommonPartyData partyData)
        {
            lock (s_padlock)
            {
                s_emailPersonalization["klant.voornaam"] = partyData.Name;
                s_emailPersonalization["klant.voorvoegselAchternaam"] = partyData.SurnamePrefix;
                s_emailPersonalization["klant.achternaam"] = partyData.Surname;

                s_emailPersonalization["besluit.identificatie"] = this._decision.Identification;
                s_emailPersonalization["besluit.datum"] = $"{this._decision.Date.ConvertToDutchDateString()}";
                s_emailPersonalization["besluit.toelichting"] = this._decision.Explanation;
                s_emailPersonalization["besluit.bestuursorgaan"] = this._decision.GoverningBody;
                s_emailPersonalization["besluit.ingangsdatum"] = $"{this._decision.EffectiveDate.ConvertToDutchDateString()}";
                s_emailPersonalization["besluit.vervaldatum"] = $"{this._decision.ExpirationDate.ConvertToDutchDateString()}";
                s_emailPersonalization["besluit.vervalreden"] = this._decision.ExpirationReason;
                s_emailPersonalization["besluit.publicatiedatum"] = $"{this._decision.PublicationDate.ConvertToDutchDateString()}";
                s_emailPersonalization["besluit.verzenddatum"] = $"{this._decision.ShippingDate.ConvertToDutchDateString()}";
                s_emailPersonalization["besluit.uiterlijkereactiedatum"] = $"{this._decision.ResponseDate.ConvertToDutchDateString()}";

                s_emailPersonalization["besluittype.omschrijving"] = this._decisionType.Name;
                s_emailPersonalization["besluittype.omschrijvingGeneriek"] = this._decisionType.Description;
                s_emailPersonalization["besluittype.besluitcategorie"] = this._decisionType.Category;
                s_emailPersonalization["besluittype.publicatieindicatie"] = this._decisionType.PublicationIndicator;
                s_emailPersonalization["besluittype.publicatietekst"] = this._decisionType.PublicationText;
                s_emailPersonalization["besluittype.toelichting"] = this._decisionType.Explanation;

                s_emailPersonalization["zaak.identificatie"] = this._case.Identification;
                s_emailPersonalization["zaak.omschrijving"] = this._case.Name;
                s_emailPersonalization["zaak.registratiedatum"] = $"{this._case.RegistrationDate.ConvertToDutchDateString()}";

                s_emailPersonalization["zaaktype.omschrijving"] = this._caseType.Name;
                s_emailPersonalization["zaaktype.omschrijvingGeneriek"] = this._caseType.Description;

                return s_emailPersonalization;
            }
        }
        #endregion

        #region Polymorphic (SMS logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override Guid GetSmsTemplateId()
            => this.Configuration.Notify.TemplateId.DecisionMade();  // NOTE: Decision has only one template

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalization(ZhvModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData)"/>
        protected override Dictionary<string, object> GetSmsPersonalization(CommonPartyData partyData)
        {
            return GetEmailPersonalization(partyData);  // NOTE: Both implementations are identical
        }
        #endregion

        #region Polymorphic (ProcessDataAsync)
        [GeneratedRegex("\\\\r\\\\n|\\r\\n|\r\n|\\\\n\\n|\\n\\\\n|\\\\n\\\\n|\\n\\n|\n\n|\\\\n|\\n|\n|\\\\r|\r", RegexOptions.Compiled)]
        private static partial Regex NewlinesCharsRegexPattern();

        private const string LogiusNewlines = "\\r\\n";

        internal static string ReplaceWhitespaces(string originalText)
        {
            return NewlinesCharsRegexPattern().Replace(originalText, LogiusNewlines)
                                              .Replace("\t", "\\t");
        }

        /// <inheritdoc cref="BaseScenario.ProcessDataAsync(NotificationEvent, IReadOnlyCollection{NotifyData})"/>
        protected override async Task<ProcessingDataResponse> ProcessDataAsync(NotificationEvent notification, IReadOnlyCollection<NotifyData> notifyData)
        {
            if (notifyData.IsEmpty())
            {
                return ProcessingDataResponse.Failure_Empty();
            }

            NotifyTemplateResponse templateResponse =
                // NOTE: Most likely there will be only a single package of data received
                await this.NotifyService.GenerateTemplatePreviewAsync(notifyData.First());

            if (templateResponse.IsFailure)
            {
                return ProcessingDataResponse.Failure(templateResponse.Error);
            }

            // Adjusting the body for Logius system
            string modifiedResponseBody = ReplaceWhitespaces(templateResponse.Body);

            // Prepare HTTP Request Body
            this._queryContext = this.DataQuery.From(notification);

            string commaSeparatedUris = await GetValidInfoObjectUrisAsync(this._queryContext);
            if (commaSeparatedUris.IsNullOrEmpty())
            {
                return ProcessingDataResponse.Failure(ApiResources.Processing_ERROR_Scenario_MissingInfoObjectsURIs);
            }

            string dataJson = PrepareDataJson(templateResponse.Subject, modifiedResponseBody, commaSeparatedUris);

            HttpRequestResponse requestResponse = await this._queryContext.CreateObjectAsync(
                                                        this._queryContext.PrepareObjectJsonBody(dataJson));
            return requestResponse.IsFailure
                ? ProcessingDataResponse.Failure(requestResponse.JsonResponse)
                : ProcessingDataResponse.Success();
        }

        /// <summary>
        /// Gets references in <see cref="Uri"/> format to <see cref="ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.InfoObject"/>s meeting certain criteria.
        /// </summary>
        /// <returns>
        ///   The comma-separated absolute <see cref="Uri"/> references to valid <see cref="ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.InfoObject"/>s in the given format:
        ///   <code>
        ///     ""absoluteUri", "absoluteUri", "absoluteUri""
        ///   </code>
        /// </returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        private async Task<string> GetValidInfoObjectUrisAsync(IQueryContext queryContext)
        {
            // Retrieve documents
            List<Document> documents = (await queryContext.GetDocumentsAsync(this._decisionResource))
                                       .Results;
            // Prepare URIs
            List<string> validInfoObjectsUris = new(documents.Count);

            foreach (Document document in documents)
            {
                // Retrieve info objects from documents
                InfoObject infoObject = await queryContext.GetInfoObjectAsync(document);

                // Filter out info objects not meeting the specified criteria
                if (infoObject.Confidentiality != PrivacyNotices.NonConfidential &&
                    infoObject.Status          != MessageStatus.Definitive)
                {
                    continue;
                }

                // Keep the URI references to the valid info objects
                validInfoObjectsUris.Add($"\"{document.InfoObjectUri}\"");
            }

            return validInfoObjectsUris.Join();
        }

        /// <summary>
        ///   Prepares a block of code (responsible for message object creation) to be sent together with final JSON payload.
        /// </summary>
        /// <exception cref="KeyNotFoundException"/>
        private string PrepareDataJson(string subject, string body, string commaSeparatedUris)
        {
            const string missing = "-";

            string identificationType = (this._bsnNumber.IsNullOrEmpty() ? IdTypes.Unknown : IdTypes.Bsn).GetEnumName();  // TODO: Which type should be used for organization?
            string identificationValue = this._bsnNumber.IsNullOrEmpty() ? missing : this._bsnNumber;                     // TODO: Which value should be used for organization?

            return $"\"onderwerp\":\"{(subject.IsNullOrEmpty() ? missing : subject)}\"," +
                   $"\"berichtTekst\":\"{(body.IsNullOrEmpty() ? missing : body)}\"," +
                   $"\"publicatiedatum\":\"{this._decision.PublicationDate.ConvertToDutchDateString()}\"," +
                   $"\"referentie\":\"{this._decisionResource.DecisionUri}\"," +
                   $"\"handelingsperspectief\":\"informatie verstrekken\"," + 
                   $"\"geopend\":false," +
                   $"\"berichtType\":\"notificatie\"," + 
                   $"\"identificatie\":{{" +
                     $"\"type\":\"{identificationType}\"," +
                     $"\"value\":\"{identificationValue}\"" +
                   $"}}," +
                   $"\"bijlages\":[{commaSeparatedUris}]";
        }
        #endregion

        #region Polymorphic (GetWhitelistEnvVarName)
        /// <inheritdoc cref="BaseScenario.GetWhitelistEnvVarName()"/>
        protected override string GetWhitelistEnvVarName() => this.Configuration.ZGW.Whitelist.DecisionMade_IDs().ToString();
        #endregion
    }
}