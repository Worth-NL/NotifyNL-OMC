// © 2024, Worth Systems.

using EventsHandler.Exceptions;
using EventsHandler.Extensions;
using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Enums.Objecten;
using EventsHandler.Mapping.Enums.OpenZaak;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Mapping.Models.POCOs.OpenZaak.Decision;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataProcessing.Strategy.Responses;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.DataSending.Responses;
using EventsHandler.Services.Settings.Configuration;
using System.Text.Json;
using System.Text.RegularExpressions;
using Resources = EventsHandler.Properties.Resources;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="DecisionMadeScenario"/> class.
        /// </summary>
        public DecisionMadeScenario(
            WebApiConfiguration configuration,
            IDataQueryService<NotificationEvent> dataQuery,
            INotifyService<NotificationEvent, NotifyData> notifyService)
            : base(configuration, dataQuery, notifyService)
        {
        }

        #region Polymorphic (PrepareDataAsync)
        /// <inheritdoc cref="BaseScenario.PrepareDataAsync(NotificationEvent)"/>
        protected override async Task<CommonPartyData> PrepareDataAsync(NotificationEvent notification)
        {
            // Setup
            this._queryContext = this.DataQuery.From(notification);
            
            this._decisionResource = await this._queryContext.GetDecisionResourceAsync();
            InfoObject infoObject = await this._queryContext.GetInfoObjectAsync(this._decisionResource);

            // Validation #1: The info object needs to be of a specific type
            if (!this.Configuration.User.Whitelist.DecisionInfoObjectType_Uuids().Contains(infoObject.TypeUri.GetGuid()))
            {
                throw new AbortedNotifyingException(
                    string.Format(Resources.Processing_ABORT_DoNotSendNotification_Whitelist_InfoObjectType,
                        Settings.Extensions.ConfigurationExtensions.GetWhitelistInfoObjectsEnvVarName()));
            }

            // Validation #2: Status needs to be definitive
            if (infoObject.Status != MessageStatus.Definitive)
            {
                throw new AbortedNotifyingException(Resources.Processing_ABORT_DoNotSendNotification_DecisionStatus);
            }

            // Validation #3: Confidentiality needs to be acceptable
            if (infoObject.Confidentiality != PrivacyNotices.NonConfidential)
            {
                throw new AbortedNotifyingException(
                    string.Format(Resources.Processing_ABORT_DoNotSendNotification_DecisionConfidentiality,
                        infoObject.Confidentiality));
            }

            this._decision = await this._queryContext.GetDecisionAsync(this._decisionResource);
            this._caseType = await this._queryContext.GetLastCaseTypeAsync(  // 3. Case type
                             await this._queryContext.GetCaseStatusesAsync(  // 2. Case statuses
                                   this._decision.CaseUri));                 // 1. Case URI

            // Validation #4: The case identifier must be whitelisted
            ValidateCaseId(
                this.Configuration.User.Whitelist.DecisionMade_IDs().IsAllowed,
                this._caseType.Identification, GetWhitelistEnvVarName());

            // Validation #5: The notifications must be enabled
            ValidateNotifyPermit(this._caseType.IsNotificationExpected);

            // Preparing citizen details
            this._bsnNumber = await this._queryContext.GetBsnNumberAsync(  // 2. BSN number
                                    this._decision.CaseUri);               // 1. Case URI

            return await this._queryContext.GetPartyDataAsync(this._bsnNumber);  // 3. Citizen details
        }
        #endregion

        #region Polymorphic (Email logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override Guid GetEmailTemplateId()
            => this.Configuration.User.TemplateIds.Email.DecisionMade();

        private static readonly object s_padlock = new();
        private static readonly Dictionary<string, object> s_emailPersonalization = new();  // Cached dictionary no need to be initialized every time

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalizationAsync(CommonPartyData)"/>
        protected override async Task<Dictionary<string, object>> GetEmailPersonalizationAsync(CommonPartyData partyData)
        {
            DecisionType decisionType = await this._queryContext.GetDecisionTypeAsync(this._decision);
            Case @case = await this._queryContext.GetCaseAsync(this._decision.CaseUri);

            lock (s_padlock)
            {
                s_emailPersonalization["klant.voornaam"] = partyData.Name;
                s_emailPersonalization["klant.voorvoegselAchternaam"] = partyData.SurnamePrefix;
                s_emailPersonalization["klant.achternaam"] = partyData.Surname;

                s_emailPersonalization["besluit.identificatie"] = this._decision.Identification;
                s_emailPersonalization["besluit.datum"] = $"{this._decision.Date:O}";
                s_emailPersonalization["besluit.toelichting"] = this._decision.Explanation;
                s_emailPersonalization["besluit.bestuursorgaan"] = this._decision.GoverningBody;
                s_emailPersonalization["besluit.ingangsdatum"] = $"{this._decision.EffectiveDate:O}";
                s_emailPersonalization["besluit.vervaldatum"] = $"{this._decision.ExpirationDate:O}";
                s_emailPersonalization["besluit.vervalreden"] = this._decision.ExpirationReason;
                s_emailPersonalization["besluit.publicatiedatum"] = $"{this._decision.PublicationDate:O}";
                s_emailPersonalization["besluit.verzenddatum"] = $"{this._decision.ShippingDate:O}";
                s_emailPersonalization["besluit.uiterlijkereactiedatum"] = $"{this._decision.ResponseDate:O}";

                s_emailPersonalization["besluittype.omschrijving"] = decisionType.Name;
                s_emailPersonalization["besluittype.omschrijvingGeneriek"] = decisionType.Description;
                s_emailPersonalization["besluittype.besluitcategorie"] = decisionType.Category;
                s_emailPersonalization["besluittype.publicatieindicatie"] = decisionType.PublicationIndicator;
                s_emailPersonalization["besluittype.publicatietekst"] = decisionType.PublicationText;
                s_emailPersonalization["besluittype.toelichting"] = decisionType.Explanation;

                s_emailPersonalization["zaak.identificatie"] = @case.Identification;
                s_emailPersonalization["zaak.omschrijving"] = @case.Name;
                s_emailPersonalization["zaak.registratiedatum"] = $"{@case.RegistrationDate:O}";

                s_emailPersonalization["zaaktype.omschrijving"] = this._caseType.Name;
                s_emailPersonalization["zaaktype.omschrijvingGeneriek"] = this._caseType.Description;

                return s_emailPersonalization;
            }
        }
        #endregion

        #region Polymorphic (SMS logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override Guid GetSmsTemplateId()
            => this.Configuration.User.TemplateIds.Sms.DecisionMade();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalizationAsync(CommonPartyData)"/>
        protected override async Task<Dictionary<string, object>> GetSmsPersonalizationAsync(CommonPartyData partyData)
        {
            return await GetEmailPersonalizationAsync(partyData);  // NOTE: Both implementations are identical
        }
        #endregion

        #region Polymorphic (ProcessDataAsync)
        [GeneratedRegex("(\\n\\n)|(\\n)", RegexOptions.Compiled)]
        private static partial Regex NewlinesCharsRegexPattern();

        private const string LogiusNewlines = "\\r\\n";

        /// <inheritdoc cref="BaseScenario.ProcessDataAsync(NotificationEvent, IReadOnlyCollection{NotifyData})"/>
        protected override async Task<ProcessingDataResponse> ProcessDataAsync(NotificationEvent notification, IReadOnlyCollection<NotifyData> notifyData)
        {
            if (notifyData.IsEmpty())
            {
                return ProcessingDataResponse.Failure_Empty();
            }

            NotifyTemplateResponse templateResponse =
                // NOTE: Most likely there will be only a single package of data received
                await this.NotifyService.GenerateTemplatePreviewAsync(notification, notifyData.First());

            if (templateResponse.IsFailure)
            {
                return ProcessingDataResponse.Failure(templateResponse.Error);
            }

            // Adjusting the body for Logius system
            string modifiedResponseBody = NewlinesCharsRegexPattern().Replace(templateResponse.Body, LogiusNewlines);

            // Prepare HTTP Request Body
            this._queryContext = this.DataQuery.From(notification);

            string commaSeparatedUris = await GetValidInfoObjectUrisAsync(this._queryContext);
            if (commaSeparatedUris.IsEmpty())
            {
                return ProcessingDataResponse.Failure(Resources.Processing_ERROR_Scenario_MissingInfoObjectsURIs);
            }

            string dataJson = PrepareDataJson(templateResponse.Subject, modifiedResponseBody, commaSeparatedUris);

            RequestResponse requestResponse = await this._queryContext.CreateObjectAsync(
                                                    this._queryContext.PrepareObjectJsonBody(dataJson));
            return requestResponse.IsFailure
                ? ProcessingDataResponse.Failure(requestResponse.JsonResponse)
                : ProcessingDataResponse.Success();
        }

        /// <summary>
        /// Gets references in <see cref="Uri"/> format to <see cref="InfoObject"/>s meeting certain criteria.
        /// </summary>
        /// <returns>
        ///   The comma-separated absolute <see cref="Uri"/> references to valid <see cref="InfoObject"/>s in the given format:
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
            return $"\"onderwerp\":\"{subject}\"," +
                   $"\"berichttekst\":\"{body}\"," +
                   $"\"publicatiedatum\":\"{this._decision.PublicationDate:O}\"," +  // 2001-01-01
                   $"\"referentie\":\"{this._decisionResource.DecisionUri}\"," +
                   $"\"handelingsperspectief\":\"TODO\"," +  // TODO: To be filled
                   $"\"geopend\":false," +
                   $"\"berichttype\":\"TODO\"," +  // TODO: To be filled
                   $"\"identificatie\":{{" +
                     $"\"type\":\"{IdTypes.Bsn.GetEnumName()}\"," +
                     $"\"value\":\"{this._bsnNumber}\"" +
                   $"}}," +
                   $"\"bijlages\":[{commaSeparatedUris}]";
        }
        #endregion

        #region Polymorphic (GetWhitelistEnvVarName)
        /// <inheritdoc cref="BaseScenario.GetWhitelistEnvVarName()"/>
        protected override string GetWhitelistEnvVarName() => this.Configuration.User.Whitelist.DecisionMade_IDs().ToString();
        #endregion
    }
}