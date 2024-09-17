// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Exceptions;
using EventsHandler.Extensions;
using EventsHandler.Mapping.Enums.Objecten;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.Objecten.Task;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.Settings.Configuration;

namespace EventsHandler.Services.DataProcessing.Strategy.Implementations
{
    /// <summary>
    /// <inheritdoc cref="INotifyScenario"/>
    /// The strategy for "Task assigned" scenario.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    internal sealed class TaskAssignedScenario : BaseScenario
    {
        private IQueryContext _queryContext = null!;
        private CommonTaskData _taskData;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskAssignedScenario"/> class.
        /// </summary>
        public TaskAssignedScenario(
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

            // Validation #1: The task needs to be of a specific type
            if (notification.Attributes.ObjectTypeUri.GetGuid() !=
                this.Configuration.User.Whitelist.TaskObjectType_Uuid())
            {
                throw new AbortedNotifyingException(Resources.Processing_ABORT_DoNotSendNotification_TaskType);
            }

            this._taskData = await this._queryContext.GetTaskAsync();

            // Validation #2: The task needs to have an open status
            if (this._taskData.Status != TaskStatuses.Open)
            {
                throw new AbortedNotifyingException(Resources.Processing_ABORT_DoNotSendNotification_TaskClosed);
            }

            // Validation #3: The task needs to be assigned to a person
            if (this._taskData.Identification.Type != IdTypes.Bsn)
            {
                throw new AbortedNotifyingException(Resources.Processing_ABORT_DoNotSendNotification_TaskNotPerson);
            }
            
            CaseType caseType = await this._queryContext.GetLastCaseTypeAsync(  // 3. Case type
                                await this._queryContext.GetCaseStatusesAsync(  // 2. Case statuses
                                      this._taskData.CaseUri));                 // 1. Case URI
            
            // Validation #4: The case type identifier must be whitelisted
            ValidateCaseId(
                this.Configuration.User.Whitelist.TaskAssigned_IDs().IsAllowed,
                caseType.Identification, GetWhitelistName());

            // Validation #5: The notifications must be enabled
            ValidateNotifyPermit(caseType.IsNotificationExpected);

            // Preparing citizen details
            return await this._queryContext.GetPartyDataAsync(
                this._taskData.Identification.Value);  // BSN number
        }
        #endregion

        #region Polymorphic (Email logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override Guid GetEmailTemplateId()
            => this.Configuration.User.TemplateIds.Email.TaskAssigned();

        private static readonly object s_padlock = new();
        private static readonly Dictionary<string, object> s_emailPersonalization = new();  // Cached dictionary no need to be initialized every time

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalizationAsync(CommonPartyData)"/>
        protected override async Task<Dictionary<string, object>> GetEmailPersonalizationAsync(CommonPartyData partyData)
        {
            bool isValid = IsValid(this._taskData.ExpirationDate);
            string formattedExpirationDate = GetFormattedExpirationDate(isValid, this._taskData.ExpirationDate);
            string expirationDateProvided = GetExpirationDateProvided(isValid);
            
            Case @case = await this._queryContext.GetCaseAsync(this._taskData.CaseUri);
            
            lock (s_padlock)
            {
                s_emailPersonalization["klant.voornaam"] = partyData.Name;
                s_emailPersonalization["klant.voorvoegselAchternaam"] = partyData.SurnamePrefix;
                s_emailPersonalization["klant.achternaam"] = partyData.Surname;

                s_emailPersonalization["taak.verloopdatum"] = formattedExpirationDate;
                s_emailPersonalization["taak.heeft_verloopdatum"] = expirationDateProvided;
                s_emailPersonalization["taak.record.data.title"] = this._taskData.Title;

                s_emailPersonalization["zaak.identificatie"] = @case.Identification;
                s_emailPersonalization["zaak.omschrijving"] = @case.Name;

                return s_emailPersonalization;
            }
        }

        private static string GetFormattedExpirationDate(bool isValid, DateTime expirationDate)
        {
            return isValid
                ? expirationDate.ConvertToDutchDateString()
                : DefaultValues.Models.DefaultEnumValueName;
        }

        private static string GetExpirationDateProvided(bool isValid)
        {
            return isValid ? "yes" : "no";
        }

        private static bool IsValid(DateTime expirationDate)
        {
            return expirationDate != default;  // 0001-01-01, 00:00:00
        }
        #endregion

        #region Polymorphic (SMS logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override Guid GetSmsTemplateId()
            => this.Configuration.User.TemplateIds.Sms.TaskAssigned();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalizationAsync(CommonPartyData)"/>
        protected override async Task<Dictionary<string, object>> GetSmsPersonalizationAsync(CommonPartyData partyData)
        {
            return await GetEmailPersonalizationAsync(partyData);  // NOTE: Both implementations are identical
        }
        #endregion

        #region Polymorphic (GetWhitelistName)
        /// <inheritdoc cref="BaseScenario.GetWhitelistName()"/>
        protected override string GetWhitelistName() => this.Configuration.User.Whitelist.TaskAssigned_IDs().ToString();
        #endregion
    }
}