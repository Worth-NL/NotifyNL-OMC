﻿// © 2024, Worth Systems.

using EventsHandler.Exceptions;
using EventsHandler.Extensions;
using EventsHandler.Mapping.Enums.NotificatieApi;
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
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataProcessing.Strategy.Implementations
{
    /// <summary>
    /// <inheritdoc cref="INotifyScenario"/>
    /// The strategy for "Decision made" scenario.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    internal sealed class DecisionMadeScenario : BaseScenario
    {
        private IQueryContext? _queryContext;
        private DecisionResource _decisionResource;
        private Decision _decision;
        private DecisionType _decisionType;
        private Case _case;
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

            // Validation #1: The message needs to be of a specific type
            if (infoObject.TypeUri.GetGuid() !=
                this.Configuration.User.Whitelist.MessageObjectType_Uuid())
            {
                throw new AbortedNotifyingException(Resources.Processing_ABORT_DoNotSendNotification_MessageType);
            }

            // Validation #2: Status needs to be definitive
            if (infoObject.Status != MessageStatus.Definitive)
            {
                throw new AbortedNotifyingException(Resources.Processing_ABORT_DoNotSendNotification_DecisionStatus);
            }

            // Validation #3: Confidentiality needs to be acceptable
            if (infoObject.Confidentiality != PrivacyNotices.NonConfidential)  // TODO: First version would only check confidential status (why array?)
            {
                throw new AbortedNotifyingException(
                    string.Format(Resources.Processing_ABORT_DoNotSendNotification_DecisionConfidentiality,
                        infoObject.Confidentiality));
            }

            this._decision = await this._queryContext.GetDecisionAsync(this._decisionResource);
            this._decisionType = await this._queryContext.GetDecisionTypeAsync(this._decision);
            this._case = await this._queryContext.GetCaseAsync(this._decision.CaseUri);

            // Validation #4: The case identifier must be whitelisted
            ValidateCaseId(
                this.Configuration.User.Whitelist.DecisionMade_IDs().IsAllowed,
                this._case.Identification, GetWhitelistName());
            
            this._caseType = await this._queryContext.GetLastCaseTypeAsync(  // 3. Case type
                             await this._queryContext.GetCaseStatusesAsync(  // 2. Case statuses
                                   this._decision.CaseUri));                 // 1. Case URI

            // Validation #5: The notifications must be enabled
            ValidateNotifyPermit(this._caseType.IsNotificationExpected);

            // Preparing citizen details
            this._bsnNumber = await this._queryContext.GetBsnNumberAsync(  // 2. BSN number
                                    this._case.CaseTypeUri);               // 1. Case Type URI

            return await this._queryContext.GetPartyDataAsync(this._bsnNumber);  // 3. Citizen details
        }
        #endregion

        #region Polymorphic (Email logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override Guid GetEmailTemplateId()
            => Guid.Empty;  // NOTE: This scenario is not sending notifications

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

                s_emailPersonalization["besluit.identificatie"] = this._decision.Identification;
                s_emailPersonalization["besluit.datum"] = $"{this._decision.Date}";
                s_emailPersonalization["besluit.toelichting"] = this._decision.Explanation;
                s_emailPersonalization["besluit.bestuursorgaan"] = this._decision.GoverningBody;
                s_emailPersonalization["besluit.ingangsdatum"] = $"{this._decision.EffectiveDate}";
                s_emailPersonalization["besluit.vervaldatum"] = $"{this._decision.ExpirationDate}";
                s_emailPersonalization["besluit.vervalreden"] = this._decision.ExpirationReason;
                s_emailPersonalization["besluit.publicatiedatum"] = $"{this._decision.PublicationDate}";
                s_emailPersonalization["besluit.verzenddatum"] = $"{this._decision.ShippingDate}";
                s_emailPersonalization["besluit.uiterlijkereactiedatum"] = $"{this._decision.ResponseDate}";

                s_emailPersonalization["besluittype.omschrijving"] = this._decisionType.Name;
                s_emailPersonalization["besluittype.omschrijvinggeneriek"] = this._decisionType.Description;
                s_emailPersonalization["besluittype.besluitcategorie"] = this._decisionType.Category;
                s_emailPersonalization["besluittype.reactietermijn"] = $"{this._decisionType.ResponseDeadline}";
                s_emailPersonalization["besluittype.publicatieindicatie"] = this._decisionType.PublicationIndicator;
                s_emailPersonalization["besluittype.publicatietekst"] = this._decisionType.PublicationText;
                s_emailPersonalization["besluittype.publicatietermijn"] = $"{this._decisionType.PublicationDeadline}";
                s_emailPersonalization["besluittype.toelichting"] = this._decisionType.Explanation;

                s_emailPersonalization["zaak.identificatie"] = this._case.Identification;
                s_emailPersonalization["zaak.omschrijving"] = this._case.Name;
                s_emailPersonalization["zaak.registratiedatum"] = $"{this._case.RegistrationDate}";

                s_emailPersonalization["zaaktype.omschrijving"] = this._caseType.Name;
                s_emailPersonalization["zaaktype.omschrijvinggeneriek"] = this._caseType.Description;

                return s_emailPersonalization;
            }
        }
        #endregion

        #region Polymorphic (SMS logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override Guid GetSmsTemplateId()
          => Guid.Empty;  // NOTE: This scenario is not sending notifications

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalization(CommonPartyData)"/>
        protected override Dictionary<string, object> GetSmsPersonalization(CommonPartyData partyData)
        {
            return GetEmailPersonalization(partyData);  // NOTE: Both implementations are identical
        }
        #endregion

        #region Polymorphic (ProcessDataAsync)
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
            string modifiedResponseBody = templateResponse.Body.Replace("\n\n", "\r\n");

            // Prepare HTTP Request Body
            this._queryContext ??= this.DataQuery.From(notification);

            string commaSeparatedUris = await GetValidInfoObjectUrisAsync(this._queryContext);
            if (commaSeparatedUris.IsEmpty())
            {
                return ProcessingDataResponse.Failure(Resources.Processing_ERROR_Scenario_MissingInfoObjectsURIs);
            }

            string objectDataJson = PrepareObjectData(templateResponse.Subject, modifiedResponseBody, commaSeparatedUris);

            RequestResponse requestResponse = await this._queryContext.CreateMessageObjectAsync(objectDataJson);

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
                if (infoObject.Status          != MessageStatus.Definitive &&
                    infoObject.Confidentiality != PrivacyNotices.NonConfidential)  // TODO: First version would only check confidential status (why array?)
                {
                    continue;
                }

                // Keep the URI references to the valid info objects
                validInfoObjectsUris.Add($"\"{document.InfoObjectUri}\"");
            }

            return string.Join(", ", validInfoObjectsUris);
        }

        /// <summary>
        ///   Prepares a block of code (responsible for message object creation) to be sent together with final JSON payload.
        /// </summary>
        /// <exception cref="KeyNotFoundException"/>
        private string PrepareObjectData(string subject, string body, string commaSeparatedUris)
        {
            return $"{{" +
                   $"  \"onderwerp\": \"{subject}\"," +
                   $"  \"berichttekst\": \"{body}\"," +
                   $"  \"publicatiedatum\": \"{this._decision.PublicationDate}\"," +
                   $"  \"referentie\": \"{this._decisionResource.DecisionUri}\"," +
                   $"  \"handelingsperspectief\": \"{string.Empty}\"," +  // TODO: To be filled
                   $"  \"geopend\": false," +
                   $"  \"berichttype\": \"{this.Configuration.AppSettings.Variables.Objecten.MessageObjectType_Name()}\"," +
                   $"  \"identificatie\": {{" +
                   $"    \"type\": \"bsn\"," +
                   $"    \"value\": \"{this._bsnNumber}\"" +
                   $"  }}," +
                   $"  \"bijlages\": [" +
                   $"    {commaSeparatedUris}" +
                   $"  ]" +
                   $"}}";
        }
        #endregion

        #region Polymorphic (GetWhitelistName)
        /// <inheritdoc cref="BaseScenario.GetWhitelistName"/>
        protected override string GetWhitelistName() => this.Configuration.User.Whitelist.DecisionMade_IDs().ToString();
        #endregion
    }
}