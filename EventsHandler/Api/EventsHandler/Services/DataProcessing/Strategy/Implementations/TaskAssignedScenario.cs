// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Exceptions;
using EventsHandler.Extensions;
using EventsHandler.Mapping.Enums.Objecten;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.Objecten;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Interfaces;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.Settings.Configuration;
using System.Globalization;

namespace EventsHandler.Services.DataProcessing.Strategy.Implementations
{
    /// <summary>
    /// <inheritdoc cref="INotifyScenario"/>
    /// The strategy for "Task assigned" scenario.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    internal sealed class TaskAssignedScenario : BaseScenario
    {
        private Data _cachedTaskData;
        private Case _cachedCase;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskAssignedScenario"/> class.
        /// </summary>
        public TaskAssignedScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
            : base(configuration, dataQuery)
        {
        }

        #region Polymorphic (PrepareDataAsync)
        /// <inheritdoc cref="BaseScenario.PrepareDataAsync(NotificationEvent)"/>
        protected override async Task<CommonPartyData> PrepareDataAsync(NotificationEvent notification)
        {
            // Setup
            IQueryContext queryContext = this.DataQuery.From(notification);

            // Validation #1: The task needs to be of a specific type
            if (notification.Attributes.ObjectTypeUri.GetGuid() !=
                this.Configuration.User.Whitelist.TaskType_Uuid())
            {
                throw new AbortedNotifyingException(Resources.Processing_ABORT_DoNotSendNotification_TaskType);
            }

            this._cachedTaskData = (await queryContext.GetTaskAsync()).Record.Data;

            // Validation #2: The task needs to have an open status
            if (this._cachedTaskData.Status != TaskStatuses.Open)
            {
                throw new AbortedNotifyingException(Resources.Processing_ABORT_DoNotSendNotification_TaskClosed);
            }

            // Validation #3: The task needs to be assigned to a person
            if (this._cachedTaskData.Identification.Type != IdTypes.Bsn)
            {
                throw new AbortedNotifyingException(Resources.Processing_ABORT_DoNotSendNotification_TaskNotPerson);
            }
            
            this._cachedCase = await queryContext.GetCaseAsync(this._cachedTaskData);
            
            // Validation #4: The case identifier must be whitelisted
            ValidateCaseId(
                this.Configuration.User.Whitelist.TaskAssigned_IDs().IsAllowed,
                this._cachedCase.Identification, GetWhitelistName());
            
            CaseType caseType = await queryContext.GetLastCaseTypeAsync(  // 3. Case type
                                await queryContext.GetCaseStatusesAsync(  // 2. Case statuses
                                      this._cachedTaskData.CaseUri));     // 1. Case URI

            // Validation #5: The notifications must be enabled
            ValidateNotifyPermit(caseType.IsNotificationExpected);

            // Preparing citizen details
            return await queryContext.GetPartyDataAsync(
                this._cachedTaskData.Identification.Value);  // BSN number
        }
        #endregion

        #region Polymorphic (Email logic)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override Guid GetEmailTemplateId()
            => this.Configuration.User.TemplateIds.Email.TaskAssigned();

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalization(CommonPartyData)"/>
        protected override Dictionary<string, object> GetEmailPersonalization(CommonPartyData partyData)
        {
            string formattedExpirationDate = GetFormattedExpirationDate(this._cachedTaskData.ExpirationDate);
            string expirationDateProvided = GetExpirationDateProvided(this._cachedTaskData.ExpirationDate);

            return new Dictionary<string, object>
            {
                { "taak.verloopdatum", formattedExpirationDate },
                { "taak.heeft_verloopdatum", expirationDateProvided },
                { "taak.record.data.title", this._cachedTaskData.Title },

                { "zaak.identificatie", this._cachedCase.Identification },
                { "zaak.omschrijving", this._cachedCase.Name },

                { "klant.voornaam", partyData.Name },
                { "klant.voorvoegselAchternaam", partyData.SurnamePrefix },
                { "klant.achternaam", partyData.Surname }
            };
        }

        private static readonly TimeZoneInfo s_cetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        private static readonly CultureInfo s_dutchCulture = new("nl-NL");

        private static string GetFormattedExpirationDate(DateTime expirationDate)
        {
            if (!IsValid(expirationDate))
            {
                return DefaultValues.Models.DefaultEnumValueName;
            }

            // Convert time zone from UTC to CET (if necessary)
            if (expirationDate.Kind == DateTimeKind.Utc)
            {
                expirationDate = TimeZoneInfo.ConvertTimeFromUtc(expirationDate, s_cetTimeZone);
            }

            // Formatting the date and time
            return expirationDate.ToString("f", s_dutchCulture);
        }

        private static string GetExpirationDateProvided(DateTime expirationDate)
        {
            return IsValid(expirationDate) ? "yes" : "no";
        }

        private static bool IsValid(DateTime expirationDate)
        {
            return expirationDate != default;  // 0001-01-01, 00:00:00
        }
        #endregion

        #region Polymorphic (SMS logic)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override Guid GetSmsTemplateId()
            => this.Configuration.User.TemplateIds.Sms.TaskAssigned();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalization(CommonPartyData)"/>
        protected override Dictionary<string, object> GetSmsPersonalization(CommonPartyData partyData)
        {
            return GetEmailPersonalization(partyData);  // NOTE: Both implementations are identical
        }
        #endregion

        #region Polymorphic (GetWhitelistName)
        /// <inheritdoc cref="BaseScenario.GetWhitelistName"/>
        protected override string GetWhitelistName() => this.Configuration.User.Whitelist.TaskAssigned_IDs().ToString();
        #endregion
    }
}