// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Enums.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Interfaces;
using CitizenData = EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.CitizenData;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Behaviors.Communication.Strategy.Base
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
        /// <inheritdoc cref="WebApiConfiguration"/>
        protected WebApiConfiguration Configuration { get; private set; }

        /// <inheritdoc cref="IDataQueryService{TModel}"/>
        protected IDataQueryService<NotificationEvent> DataQuery { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseScenario"/> class.
        /// </summary>
        protected BaseScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
        {
            this.Configuration = configuration;
            this.DataQuery = dataQuery;
        }

        #region Interface
        /// <inheritdoc cref="INotifyScenario.GetAllNotifyDataAsync(NotificationEvent)"/>
        async Task<NotifyData[]> INotifyScenario.GetAllNotifyDataAsync(NotificationEvent notification)
            => await GetAllNotifyDataAsync(notification);
        #endregion

        #region Virtual
        /// <inheritdoc cref="INotifyScenario.GetAllNotifyDataAsync(NotificationEvent)"/>
        internal virtual async Task<NotifyData[]> GetAllNotifyDataAsync(NotificationEvent notification)
        {
            Case @case = await this.DataQuery.From(notification).GetCaseAsync();
            CitizenData citizen = (await this.DataQuery.From(notification).GetCitizenDetailsAsync()).Citizen;

            // TODO: Introduce unit tests
            // Determine which types of notifications should be published
            if (citizen.DistributionChannel == DistributionChannels.Sms)
            {
                return new[] { GetSmsNotifyData(@case, citizen) };
            }

            if (citizen.DistributionChannel == DistributionChannels.Email)
            {
                return new[] { GetEmailNotifyData(@case, citizen) };
            }

            if (citizen.DistributionChannel == DistributionChannels.Both)
            {
                return new[]
                {
                    GetSmsNotifyData(@case, citizen),
                    GetEmailNotifyData(@case, citizen)
                };
            }

            if (citizen.DistributionChannel == DistributionChannels.None)
            {
                return Array.Empty<NotifyData>();
            }

            // Notification method cannot be unknown. Fill the data properly in "OpenKlant"
            throw new InvalidOperationException(Resources.Processing_ERROR_Notification_DeliveryMethodUnknown);
        }

        /// <summary>
        /// Gets the SMS notify data to be used with "Notify NL" API Client.
        /// </summary>
        /// <param name="case">The <see cref="Case"/> the notification to be sent will be about.</param>
        /// <param name="citizen">The data associated to a specific citizen.</param>
        /// <returns>
        ///   The SMS data for "Notify NL" Web service.
        /// </returns>
        protected virtual NotifyData GetSmsNotifyData(Case @case, CitizenData citizen)
        {
            return new NotifyData
            (
                notificationMethod: NotifyMethods.Sms,
                contactDetails: citizen.TelephoneNumber,
                templateId: GetSmsTemplateId(),
                personalization: GetSmsPersonalization(@case, citizen)
            );
        }

        /// <summary>
        /// Gets the e-mail notify data to be used with "Notify NL" API Client.
        /// </summary>
        /// <param name="case">The <see cref="Case"/> the notification to be sent will be about.</param>
        /// <param name="citizen">The data associated to a specific citizen.</param>
        /// <returns>
        ///   The e-mail data for "Notify NL" Web service.
        /// </returns>
        protected virtual NotifyData GetEmailNotifyData(Case @case, CitizenData citizen)
        {
            return new NotifyData
            (
                notificationMethod: NotifyMethods.Email,
                contactDetails: citizen.EmailAddress,
                templateId: GetEmailTemplateId(),
                personalization: GetEmailPersonalization(@case, citizen)
            );
        }
        #endregion

        #region Abstract
        /// <summary>
        /// Gets the SMS template ID for this strategy.
        /// </summary>
        /// <returns>
        ///   The template ID from "Notify NL" Web service in format "00000000-0000-0000-0000-00000000".
        /// </returns>
        protected abstract string GetSmsTemplateId();

        /// <summary>
        /// Gets the SMS "personalization" for this strategy.
        /// </summary>
        /// <returns>
        ///   The dictionary of &lt;placeholder, value&gt; used for personalization of "Notify NL" Web service notification.
        /// </returns>
        protected abstract Dictionary<string, object> GetSmsPersonalization(Case @case, CitizenData citizen);

        /// <summary>
        /// Gets the e-mail template ID for this strategy.
        /// </summary>
        /// <returns>
        ///   The template ID from "Notify NL" Web service in format "00000000-0000-0000-0000-00000000" (UUID).
        /// </returns>
        protected abstract string GetEmailTemplateId();

        /// <summary>
        /// Gets the e-mail "personalization" for this strategy.
        /// </summary>
        /// <returns>
        ///   The dictionary of &lt;placeholder, value&gt; used for personalization of "Notify NL" Web service notification.
        /// </returns>
        protected abstract Dictionary<string, object> GetEmailPersonalization(Case @case, CitizenData citizen);
        #endregion
    }
}