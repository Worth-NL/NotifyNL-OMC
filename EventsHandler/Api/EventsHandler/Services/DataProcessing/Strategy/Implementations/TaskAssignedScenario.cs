// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Exceptions;
using EventsHandler.Mapping.Enums.Objecten;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.Objecten;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
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
        /// <inheritdoc cref="Data"/>
        private Data? CachedTaskData { get; set; }

        /// <inheritdoc cref="Case"/>
        private Case? CachedCase { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskAssignedScenario"/> class.
        /// </summary>
        public TaskAssignedScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
            : base(configuration, dataQuery)
        {
        }

        #region Polymorphic (GetAllNotifyDataAsync)
        /// <inheritdoc cref="BaseScenario.GetAllNotifyDataAsync(NotificationEvent)"/>
        internal override async Task<NotifyData[]> GetAllNotifyDataAsync(NotificationEvent notification)
        {
            this.QueryContext ??= this.DataQuery.From(notification);

            // Validation #1: The task needs to be of a specific type
            if (!this.QueryContext.IsValidType())
            {
                throw new AbortedNotifyingException(Resources.Processing_ABORT_DoNotSendNotification_TaskType);
            }

            this.CachedTaskData ??= (await this.QueryContext.GetTaskAsync()).Record.Data;

            // Validation #2: The task needs to have an open status
            if (this.CachedTaskData.Value.Status != TaskStatuses.Open)
            {
                throw new AbortedNotifyingException(Resources.Processing_ABORT_DoNotSendNotification_TaskClosed);
            }

            // Validation #3: The task needs to be assigned to a person
            if (this.CachedTaskData.Value.Identification.Type != IdTypes.Bsn)
            {
                throw new AbortedNotifyingException(Resources.Processing_ABORT_DoNotSendNotification_TaskNotPerson);
            }

            this.CachedCommonPartyData ??=
                await this.QueryContext.GetPartyDataAsync(this.CachedTaskData.Value.Identification.Value);

            return await base.GetAllNotifyDataAsync(notification);
        }
        #endregion

        #region Polymorphic (Email logic)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override string GetEmailTemplateId()
            => this.Configuration.User.TemplateIds.Email.TaskAssigned();

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalizationAsync(CommonPartyData)"/>
        protected override async Task<Dictionary<string, object>> GetEmailPersonalizationAsync(CommonPartyData partyData)
        {
            this.CachedCase ??= await this.QueryContext!.GetCaseAsync(this.CachedTaskData!.Value);

            ValidateCaseId(
                this.Configuration.User.Whitelist.TaskAssigned_IDs().IsAllowed,
                this.CachedCase.Value.Identification, GetWhitelistName());

            ValidateNotifyPermit((
                await this.QueryContext!.GetLastCaseTypeAsync(     // Case type
                await this.QueryContext!.GetCaseStatusesAsync()))  // Case status
                .IsNotificationExpected);

            string formattedExpirationDate = GetFormattedExpirationDate(this.CachedTaskData!.Value.ExpirationDate);
            string expirationDateProvided = GetExpirationDateProvided(this.CachedTaskData!.Value.ExpirationDate);

            return new Dictionary<string, object>
            {
                { "taak.verloopdatum", formattedExpirationDate },
                { "taak.heeft_verloopdatum", expirationDateProvided },
                { "taak.record.data.title", this.CachedTaskData!.Value.Title },
                { "zaak.omschrijving", this.CachedCase.Value.Name },
                { "zaak.identificatie", this.CachedCase.Value.Identification }
            };
        }

        private static bool IsValid(DateTime expirationDate)
        {
            return expirationDate != default;  // 0001-01-01, 00:00:00
        }

        private static string GetFormattedExpirationDate(DateTime expirationDate)
        {
            if (IsValid(expirationDate))
            {
                // Convert time zone from UTC to CET (if necessary)
                if (expirationDate.Kind == DateTimeKind.Utc)
                {
                    var cetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
                    expirationDate = TimeZoneInfo.ConvertTimeFromUtc(expirationDate, cetTimeZone);
                }

                // Formatting the date and time
                return expirationDate.ToString("f", new CultureInfo("nl-NL"));
            }

            return DefaultValues.Models.DefaultEnumValueName;
        }

        private static string GetExpirationDateProvided(DateTime expirationDate)
        {
            return IsValid(expirationDate) ? "yes" : "no";
        }
        #endregion

        #region Polymorphic (SMS logic)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override string GetSmsTemplateId()
            => this.Configuration.User.TemplateIds.Sms.TaskAssigned();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalizationAsync(CommonPartyData)"/>
        protected override async Task<Dictionary<string, object>> GetSmsPersonalizationAsync(CommonPartyData partyData)
        {
            return await GetEmailPersonalizationAsync(partyData);  // NOTE: Both implementations are identical
        }
        #endregion

        #region Polymorphic (DropCache)
        /// <inheritdoc cref="BaseScenario.DropCache()"/>
        /// <remarks>
        /// <list type="bullet">
        ///   <item><see cref="CachedTaskData"/></item>
        ///   <item><see cref="CachedCase"/></item>
        /// </list>
        /// </remarks>
        protected override void DropCache()
        {
            base.DropCache();

            this.CachedTaskData = null;
            this.CachedCase = null;
        }
        #endregion

        #region Polymorphic (GetWhitelistName)
        /// <inheritdoc cref="BaseScenario.GetWhitelistName"/>
        protected override string GetWhitelistName() => this.Configuration.User.Whitelist.TaskAssigned_IDs().ToString();
        #endregion
    }
}