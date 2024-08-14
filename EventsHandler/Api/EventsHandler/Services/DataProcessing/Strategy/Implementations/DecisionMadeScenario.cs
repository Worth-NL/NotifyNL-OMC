// © 2024, Worth Systems.

using EventsHandler.Exceptions;
using EventsHandler.Extensions;
using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Enums.OpenZaak;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Mapping.Models.POCOs.OpenZaak.Decision;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Interfaces;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
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
        /// <inheritdoc cref="Decision"/>
        private Decision CachedDecision { get; set; }

        /// <inheritdoc cref="Case"/>
        private Case CachedCase { get; set; }

        /// <inheritdoc cref="CaseType"/>
        private CaseType CachedCaseType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecisionMadeScenario"/> class.
        /// </summary>
        public DecisionMadeScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
            : base(configuration, dataQuery)
        {
        }

        #region Polymorphic (PrepareDataAsync)
        /// <inheritdoc cref="BaseScenario.PrepareDataAsync(NotificationEvent)"/>
        protected override async Task<CommonPartyData> PrepareDataAsync(NotificationEvent notification)
        {
            // Setup
            IQueryContext queryContext = this.DataQuery.From(notification);
            
            DecisionResource decisionResource = await queryContext.GetDecisionResourceAsync();
            InfoObject infoObject = await queryContext.GetInfoObjectAsync(decisionResource);

            // Validation #1: The message needs to be of a specific type
            if (infoObject.TypeUri.GetGuid() !=
                this.Configuration.User.Whitelist.MessageType_Uuid())
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

            this.CachedDecision = await queryContext.GetDecisionAsync(decisionResource);
            this.CachedCase = await queryContext.GetCaseAsync(this.CachedDecision.CaseUri);

            // Validation #4: The case identifier must be whitelisted
            ValidateCaseId(
                this.Configuration.User.Whitelist.DecisionMade_IDs().IsAllowed,
                this.CachedCase.Identification, GetWhitelistName());
            
            this.CachedCaseType = await queryContext.GetLastCaseTypeAsync(  // 3. Case type
                                  await queryContext.GetCaseStatusesAsync(  // 2. Case statuses
                                        this.CachedDecision.CaseUri));      // 1. Case URI

            // Validation #5: The notifications must be enabled
            ValidateNotifyPermit(this.CachedCaseType.IsNotificationExpected);

            // Preparing citizen details
            return await queryContext.GetPartyDataAsync(  // 3. Citizen details
                   await queryContext.GetBsnNumberAsync(  // 2. BSN number
                         this.CachedCase.CaseTypeUri));   // 1. Case Type URI
        }
        #endregion

        #region Polymorphic (Email logic)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override Guid GetEmailTemplateId()
            => this.Configuration.User.TemplateIds.Email.DecisionMade();

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalization(CommonPartyData)"/>
        protected override Dictionary<string, object> GetEmailPersonalization(CommonPartyData partyData)
        {
            return new Dictionary<string, object>
            {
                { "besluit.identificatie", this.CachedDecision.Identification },
                { "besluit.datum", this.CachedDecision.Date },
                { "besluit.toelichting", this.CachedDecision.Explanation },
                { "besluit.bestuursorgaan", this.CachedDecision.GoverningBody },
                { "besluit.ingangsdatum", this.CachedDecision.EffectiveDate },
                { "besluit.vervaldatum", this.CachedDecision.ExpirationDate },
                { "besluit.vervalreden", this.CachedDecision.ExpirationReason },
                { "besluit.publicatiedatum", this.CachedDecision.PublicationDate },
                { "besluit.verzenddatum", this.CachedDecision.ShippingDate },
                { "besluit.uiterlijkereactiedatum", this.CachedDecision.ResponseDate },

                { "besluittype.omschrijving", "" },
                { "besluittype.omschrijvinggeneriek", "" },
                { "besluittype.besluitcategorie", "" },
                { "besluittype.reactietermijn", "" },
                { "besluittype.publicatieindicatie", "" },
                { "besluittype.publicatietekst", "" },
                { "besluittype.publicatietermijn", "" },
                { "besluittype.toelichting", "" },

                { "zaak.identificatie", this.CachedCase.Identification },
                { "zaak.omschrijving", this.CachedCase.Name },
                { "zaak.registratiedatum", this.CachedCase.RegistrationDate },

                { "zaaktype.omschrijving", this.CachedCaseType.Name },
                { "zaaktype.omschrijvinggeneriek", this.CachedCaseType.Description }
            };
        }

        /// <summary>
        /// Gets references in <see cref="Uri"/> format to <see cref="InfoObject"/>s meeting certain criteria.
        /// </summary>
        /// <returns>
        ///   The references to valid <see cref="InfoObject"/>s.
        /// </returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        private static async Task<IReadOnlyCollection<Uri>> GetValidInfoObjectUrisAsync(IQueryContext queryContext, DecisionResource decisionResource)
        {
            // Retrieve documents
            List<Document> documents = (await queryContext.GetDocumentsAsync(decisionResource))
                                       .Results;

            if (documents.IsEmpty())
            {
                return Array.Empty<Uri>();
            }

            // Prepare URIs
            List<Uri> validInfoObjectsUris = new(documents.Count);

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
                validInfoObjectsUris.Add(document.InfoObjectUri);
            }

            return validInfoObjectsUris.ToArray();
        }
        #endregion

        #region Polymorphic (SMS logic)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override Guid GetSmsTemplateId()
          => this.Configuration.User.TemplateIds.Sms.DecisionMade();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalization(CommonPartyData)"/>
        protected override Dictionary<string, object> GetSmsPersonalization(CommonPartyData partyData)
        {
            return GetEmailPersonalization(partyData);  // NOTE: Both implementations are identical
        }
        #endregion

        #region Polymorphic (GetWhitelistName)
        /// <inheritdoc cref="BaseScenario.GetWhitelistName"/>
        protected override string GetWhitelistName() => this.Configuration.User.Whitelist.DecisionMade_IDs().ToString();
        #endregion
    }
}