// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Enums.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Interfaces;
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
        protected Case? CachedCase { get; set; }

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
            => await this.GetAllNotifyDataAsync(notification);

        /// <inheritdoc cref="INotifyScenario.DropCache()"/>
        void INotifyScenario.DropCache()
            => this.DropCache();
        #endregion

        #region Virtual
        /// <inheritdoc cref="INotifyScenario.GetAllNotifyDataAsync(NotificationEvent)"/>
        internal virtual async Task<NotifyData[]> GetAllNotifyDataAsync(NotificationEvent notification)
        {
            CommonPartyData partyData = await this.DataQuery.From(notification).GetPartyDataAsync();

            // Determine which types of notifications should be published
            return partyData.DistributionChannel switch
            {
                DistributionChannels.Email => new[] { await GetEmailNotifyDataAsync(notification, partyData) },

                DistributionChannels.Sms   => new[] { await GetSmsNotifyDataAsync(notification, partyData) },
                
                // NOTE: Older version of "OpenKlant" was supporting option for many types of notifications
                DistributionChannels.Both  => new[] { await GetEmailNotifyDataAsync(notification, partyData),
                                                      await GetSmsNotifyDataAsync(notification, partyData) },
                
                DistributionChannels.None  => Array.Empty<NotifyData>(),
                
                // NOTE: Notification method cannot be unknown. Fill the data properly in "OpenKlant"
                _ => throw new InvalidOperationException(Resources.Processing_ERROR_Notification_DeliveryMethodUnknown)
            };
        }

        /// <summary>
        /// Gets the SMS notify data to be used with "Notify NL" API Client.
        /// </summary>
        /// <param name="notification">The initial notification from "OpenNotificaties" Web API service.</param>
        /// <param name="partyData">The data associated to a specific party (e.g., citizen, organization).</param>
        /// <returns>
        ///   The SMS data for "Notify NL" Web API service.
        /// </returns>
        protected virtual async Task<NotifyData> GetSmsNotifyDataAsync(NotificationEvent notification, CommonPartyData partyData)
        {
            return new NotifyData
            (
                notificationMethod: NotifyMethods.Sms,
                contactDetails: partyData.TelephoneNumber,
                templateId: GetSmsTemplateId(),
                personalization: await GetSmsPersonalizationAsync(notification, partyData)
            );
        }

        /// <summary>
        /// Gets the e-mail notify data to be used with "Notify NL" API Client.
        /// </summary>
        /// <param name="notification">The initial notification from "OpenNotificaties" Web API service.</param>
        /// <param name="partyData">The data associated to a specific party (e.g., citizen, organization).</param>
        /// <returns>
        ///   The e-mail data for "Notify NL" Web API service.
        /// </returns>
        protected virtual async Task<NotifyData> GetEmailNotifyDataAsync(NotificationEvent notification, CommonPartyData partyData)
        {
            return new NotifyData
            (
                notificationMethod: NotifyMethods.Email,
                contactDetails: partyData.EmailAddress,
                templateId: GetEmailTemplateId(),
                personalization: await GetEmailPersonalizationAsync(notification, partyData)
            );
        }
        #endregion

        #region Abstract
        /// <summary>
        /// Gets the SMS template ID for this strategy.
        /// </summary>
        /// <returns>
        ///   The template ID from "Notify NL" Web API service in format "00000000-0000-0000-0000-00000000".
        /// </returns>
        protected abstract string GetSmsTemplateId();

        /// <summary>
        /// Gets the SMS "personalization" for this strategy.
        /// </summary>
        /// <param name="notification">The initial notification from "OpenNotificaties" Web API service.</param>
        /// <param name="partyData">The data associated to a specific party (e.g., citizen, organization).</param>
        /// <returns>
        ///   The dictionary of &lt;placeholder, value&gt; used for personalization of "Notify NL" Web API service notification.
        /// </returns>
        protected abstract Task<Dictionary<string, object>> GetSmsPersonalizationAsync(
            NotificationEvent notification, CommonPartyData partyData);

        /// <summary>
        /// Gets the e-mail template ID for this strategy.
        /// </summary>
        /// <returns>
        ///   The template ID from "Notify NL" Web API service in format "00000000-0000-0000-0000-00000000" (UUID).
        /// </returns>
        protected abstract string GetEmailTemplateId();

        /// <summary>
        /// Gets the e-mail "personalization" for this strategy.
        /// </summary>
        /// <param name="notification">The initial notification from "OpenNotificaties" Web API service.</param>
        /// <param name="partyData">The data associated to a specific party (e.g., citizen, organization).</param>
        /// <returns>
        ///   The dictionary of &lt;placeholder, value&gt; used for personalization of "Notify NL" Web API service notification.
        /// </returns>
        protected abstract Task<Dictionary<string, object>> GetEmailPersonalizationAsync(
            NotificationEvent notification, CommonPartyData partyData);

        /// <inheritdoc cref="INotifyScenario.DropCache()"/>
        protected virtual void DropCache()
        {
            this.CachedCase = null;
        }
        #endregion
    }
}