// © 2023, Worth Systems.

using Common.Extensions;
using Common.Settings.Configuration;
using EventsHandler.Exceptions;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Models.Responses;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using JetBrains.Annotations;
using System.Text.Json;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.DataSending.Models.Reponses;
using ZhvModels.Enums;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Enums.OpenKlant;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;

namespace EventsHandler.Services.DataProcessing.Strategy.Base
{
    /// <summary>
    /// The intermediate layer between concrete scenario strategies and their <see cref="INotifyScenario"/> interface.
    /// <para>
    ///   This <see langword="abstract class"/> applies to all strategies, and it's defining the
    ///   mandatory strategies methods (which should not be visible outside on the other hand).
    /// </para>
    /// </summary>
    /// <seealso cref="INotifyScenario"/>
    internal abstract class BaseScenario : INotifyScenario
    {
        /// <inheritdoc cref="OmcConfiguration"/>
        protected OmcConfiguration Configuration { get; }

        /// <inheritdoc cref="IDataQueryService{TModel}"/>
        protected IDataQueryService<NotificationEvent> DataQuery { get; }

        /// <inheritdoc cref="INotifyService{TPackage}"/>
        protected INotifyService<NotifyData> NotifyService { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseScenario"/> class.
        /// </summary>
        protected BaseScenario(
            OmcConfiguration configuration,
            IDataQueryService<NotificationEvent> dataQuery,
            INotifyService<NotifyData> notifyService)
        {
            this.Configuration = configuration;
            this.DataQuery = dataQuery;
            this.NotifyService = notifyService;
        }

        #region Parent (TryGetDataAsync)
        /// <inheritdoc cref="INotifyScenario.TryGetDataAsync(NotificationEvent)"/>
        async Task<QueryingDataResponse> INotifyScenario.TryGetDataAsync(NotificationEvent notification)
        {
            PreparedData preparedData = await PrepareDataAsync(notification);

            // Determine which types of notifications should be published
            return preparedData.Party.DistributionChannel switch
            {
                DistributionChannels.Email
                    => QueryingDataResponse.Success([GetEmailNotifyData(notification, preparedData)]),

                DistributionChannels.Sms
                    => QueryingDataResponse.Success([GetSmsNotifyData(notification, preparedData)]),

                // NOTE: Older version of "OpenKlant" was supporting option for sending many types of notifications
                DistributionChannels.Both
                    => QueryingDataResponse.Success(
                    [
                        GetEmailNotifyData(notification, preparedData),
                            GetSmsNotifyData(notification, preparedData)
                    ]),

                // NOTE: Notification method cannot be unknown or undefined. Fill the data properly in "OpenKlant"
                _ => QueryingDataResponse.Failure()
            };
        }
        #endregion

        #region Parent (Validation)
        /// <summary>
        /// Validates whether the case identifier is whitelisted in <see cref="OmcConfiguration"/> settings.
        /// </summary>
        /// <param name="isCaseIdWhitelistedValidation">The scenario-specific validation delegate to be invoked.</param>
        /// <param name="caseId">The case identifier to be checked.</param>
        /// <param name="scenarioName">The name of the scenario used for logging.</param>
        /// <exception cref="AbortedNotifyingException"/>
        protected static void ValidateCaseId(Predicate<string> isCaseIdWhitelistedValidation, string caseId, string scenarioName)
        {
            if (isCaseIdWhitelistedValidation.Invoke(caseId))
            {
                return;
            }

            throw new AbortedNotifyingException(string.Format(ApiResources.Processing_ABORT_DoNotSendNotification_Whitelist_CaseTypeId, caseId, scenarioName));
        }
        
        /// <summary>
        /// Validates whether the inform field is set to false.
        /// </summary>
        /// <param name="isNotificationExpected">The value of the inform field.</param>
        /// <exception cref="AbortedNotifyingException"/>
        protected static void ValidateNotifyPermit(bool isNotificationExpected)
        {
            if (!isNotificationExpected)
            {
                throw new AbortedNotifyingException(ApiResources.Processing_ABORT_DoNotSendNotification_Informeren);
            }
        }
        #endregion

        #region Virtual (Email logic)
        /// <summary>
        /// Gets the e-mail notify data to be used with "Notify NL" API Client.
        /// </summary>
        /// <param name="notification"><inheritdoc cref="NotificationEvent" path="/summary"/></param>
        /// <param name="preparedData"><inheritdoc cref="PreparedData" path="/summary"/></param>
        /// <returns>
        ///   The e-mail data for "Notify NL" Web API service.
        /// </returns>
        protected virtual NotifyData GetEmailNotifyData(NotificationEvent notification, PreparedData preparedData)
        {
            return new NotifyData
            (
                notificationMethod: NotifyMethods.Email,
                contactDetails: preparedData.Party.EmailAddress,
                templateId: GetEmailTemplateId(),
                personalization: GetEmailPersonalization(preparedData.Party),
                reference: new NotifyReference
                {
                    Notification = notification,
                    CaseId = preparedData.CaseUri.GetGuid(),
                    PartyId = preparedData.Party.Uri.GetGuid()
                }
            );
        }
        #endregion

        #region Virtual (SMS logic)
        /// <summary>
        /// Gets the SMS notify data to be used with "Notify NL" API Client.
        /// </summary>
        /// <param name="notification"><inheritdoc cref="NotificationEvent" path="/summary"/></param>
        /// <param name="preparedData"><inheritdoc cref="PreparedData" path="/summary"/></param>
        /// <returns>
        ///   The SMS data for "Notify NL" Web API service.
        /// </returns>
        protected virtual NotifyData GetSmsNotifyData(NotificationEvent notification, PreparedData preparedData)
        {
            return new NotifyData
            (
                notificationMethod: NotifyMethods.Sms,
                contactDetails: preparedData.Party.TelephoneNumber,
                templateId: GetSmsTemplateId(),
                personalization: GetSmsPersonalization(preparedData.Party),
                reference: new NotifyReference
                {
                    Notification = notification,
                    CaseId = preparedData.CaseUri.GetGuid(),
                    PartyId = preparedData.Party.Uri.GetGuid()
                }
            );
        }
        #endregion

        #region Virtual (ProcessData)
        /// <inheritdoc cref="INotifyScenario.ProcessDataAsync(NotificationEvent, IReadOnlyCollection{NotifyData})"/>
        async Task<ProcessingDataResponse> INotifyScenario.ProcessDataAsync(NotificationEvent notification, IReadOnlyCollection<NotifyData> notifyData)
            => await ProcessDataAsync(notification, notifyData);

        /// <inheritdoc cref="INotifyScenario.ProcessDataAsync(NotificationEvent, IReadOnlyCollection{NotifyData})"/>
        protected virtual async Task<ProcessingDataResponse> ProcessDataAsync(NotificationEvent notification, IReadOnlyCollection<NotifyData> notifyData)
        {
            if (notifyData.IsEmpty())
            {
                return ProcessingDataResponse.Failure_Empty();
            }

            // Sending notifications (default behavior of the most scenarios/strategies)
            foreach (NotifyData data in notifyData)
            {
                NotifySendResponse response = data.NotificationMethod switch
                {
                    NotifyMethods.Email => await this.NotifyService.SendEmailAsync(data),
                    NotifyMethods.Sms   => await this.NotifyService.SendSmsAsync(data),
                    _ => NotifySendResponse.Failure_Unknown()
                };

                if (response.IsFailure)  // Fail early (if there are two packages given, failure of just single one of them is enough)
                {
                    return ProcessingDataResponse.Failure(response.Error);
                }
            }

            return ProcessingDataResponse.Success();
        }
        #endregion

        #region Abstract (PrepareData)        
        /// <summary>
        /// Prepares all the data required by this specific scenario.
        /// </summary>
        /// <param name="notification"><inheritdoc cref="NotificationEvent" path="/summary"/></param>
        /// <returns>
        ///   The data containing basic information required to send the notification.
        /// </returns>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        /// <exception cref="AbortedNotifyingException"/>
        protected abstract Task<PreparedData> PrepareDataAsync(NotificationEvent notification);
        #endregion

        #region Abstract (Email logic: template + personalization)
        /// <summary>
        /// Gets the e-mail template ID for this strategy.
        /// </summary>
        /// <returns>
        ///   The template ID from "Notify NL" Web API service in format "00000000-0000-0000-0000-00000000" (UUID).
        /// </returns>
        protected abstract Guid GetEmailTemplateId();

        /// <summary>
        /// Gets the e-mail "personalization" for this strategy.
        /// </summary>
        /// <param name="partyData">The data associated to a specific party (e.g., citizen, organization).</param>
        /// <returns>
        ///   The dictionary of &lt;placeholder, value&gt; used for personalization of "Notify NL" Web API service notification.
        /// </returns>
        protected abstract Dictionary<string, object> GetEmailPersonalization(CommonPartyData partyData);
        #endregion

        #region Abstract (SMS logic: template + personalization)
        /// <summary>
        /// Gets the SMS template ID for this strategy.
        /// </summary>
        /// <returns>
        ///   The template ID from "Notify NL" Web API service in format "00000000-0000-0000-0000-00000000".
        /// </returns>
        protected abstract Guid GetSmsTemplateId();

        /// <summary>
        /// Gets the SMS "personalization" for this strategy.
        /// </summary>
        /// <param name="partyData">The data associated to a specific party (e.g., citizen, organization).</param>
        /// <returns>
        ///   The dictionary of &lt;placeholder, value&gt; used for personalization of "Notify NL" Web API service notification.
        /// </returns>
        protected abstract Dictionary<string, object> GetSmsPersonalization(CommonPartyData partyData);
        #endregion

        #region Abstract (GetWhitelistEnvVarName)        
        /// <summary>
        /// Gets the name of this specific scenario.
        /// </summary>
        [UsedImplicitly]
        protected abstract string GetWhitelistEnvVarName();
        #endregion
    }
}