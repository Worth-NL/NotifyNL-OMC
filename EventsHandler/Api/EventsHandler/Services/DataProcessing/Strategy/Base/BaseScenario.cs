// © 2023, Worth Systems.

using EventsHandler.Mapping.Enums.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.DataProcessing.Strategy.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.Settings.Configuration;
using Resources = EventsHandler.Properties.Resources;

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
        /// <inheritdoc cref="WebApiConfiguration"/>
        protected WebApiConfiguration Configuration { get; }

        /// <inheritdoc cref="IDataQueryService{TModel}"/>
        protected IDataQueryService<NotificationEvent> DataQuery { get; }

        /// <inheritdoc cref="IQueryContext"/>
        protected IQueryContext? QueryContext { get; set; }

        /// <inheritdoc cref="CommonPartyData"/>
        protected CommonPartyData? CachedCommonPartyData { get; set; }

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
        #endregion

        #region Virtual (GetAllNotifyDataAsync)
        /// <inheritdoc cref="INotifyScenario.GetAllNotifyDataAsync(NotificationEvent)"/>
        internal virtual async Task<NotifyData[]> GetAllNotifyDataAsync(NotificationEvent notification)
        {
            // Validation: This model needs to be set by a respective strategy
            if (this.CachedCommonPartyData == null ||
                this.CachedCommonPartyData.Value.IsDefault())
            {
                throw new InvalidOperationException(Resources.HttpRequest_ERROR_NoPartyData);
            }

            // Determine which types of notifications should be published
            NotifyData[] notifyData = this.CachedCommonPartyData.Value.DistributionChannel switch
            {
                DistributionChannels.Email => new[] { await GetEmailNotifyDataAsync(this.CachedCommonPartyData.Value) },

                DistributionChannels.Sms   => new[] { await GetSmsNotifyDataAsync(this.CachedCommonPartyData.Value) },

                // NOTE: Older version of "OpenKlant" was supporting option for many types of notifications
                DistributionChannels.Both  => new[] { await GetEmailNotifyDataAsync(this.CachedCommonPartyData.Value),
                                                      await GetSmsNotifyDataAsync(this.CachedCommonPartyData.Value) },

                DistributionChannels.None  => Array.Empty<NotifyData>(),

                // NOTE: Notification method cannot be unknown or undefined. Fill the data properly in "OpenKlant"
                DistributionChannels.Unknown
                  => throw new InvalidOperationException(Resources.Processing_ERROR_Notification_DeliveryMethodUnknown),
                _ => throw new InvalidOperationException(Resources.Processing_ERROR_Notification_DeliveryMethodUnknown)
            };

            DropCache();

            return notifyData;
        }
        #endregion

        #region Virtual (Email logic)
        /// <summary>
        /// Gets the e-mail notify data to be used with "Notify NL" API Client.
        /// </summary>
        /// <param name="partyData">The data associated to a specific party (e.g., citizen, organization).</param>
        /// <returns>
        ///   The e-mail data for "Notify NL" Web API service.
        /// </returns>
        protected virtual async Task<NotifyData> GetEmailNotifyDataAsync(CommonPartyData partyData)
        {
            return new NotifyData
            (
                notificationMethod: NotifyMethods.Email,
                contactDetails: partyData.EmailAddress,
                templateId: GetEmailTemplateId(),
                personalization: await GetEmailPersonalizationAsync(partyData)
            );
        }
        #endregion

        #region Virtual (SMS logic)
        /// <summary>
        /// Gets the SMS notify data to be used with "Notify NL" API Client.
        /// </summary>
        /// <param name="partyData">The data associated to a specific party (e.g., citizen, organization).</param>
        /// <returns>
        ///   The SMS data for "Notify NL" Web API service.
        /// </returns>
        protected virtual async Task<NotifyData> GetSmsNotifyDataAsync(CommonPartyData partyData)
        {
            return new NotifyData
            (
                notificationMethod: NotifyMethods.Sms,
                contactDetails: partyData.TelephoneNumber,
                templateId: GetSmsTemplateId(),
                personalization: await GetSmsPersonalizationAsync(partyData)
            );
        }
        #endregion

        #region Virtual (DropCache)
        /// <summary>
        /// Drops (clears) the scenario internal cache.
        /// <para>
        ///   Clears:
        /// </para>
        /// <list type="bullet">
        ///   <item><see cref="QueryContext"/></item>
        ///   <item><see cref="CachedCommonPartyData"/></item>
        /// </list>
        /// </summary>
        protected virtual void DropCache()
        {
            this.QueryContext = null;
            this.CachedCommonPartyData = null;
        }
        #endregion

        #region Abstract (Email logic)
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
        /// <param name="partyData">The data associated to a specific party (e.g., citizen, organization).</param>
        /// <returns>
        ///   The dictionary of &lt;placeholder, value&gt; used for personalization of "Notify NL" Web API service notification.
        /// </returns>
        protected abstract Task<Dictionary<string, object>> GetEmailPersonalizationAsync(CommonPartyData partyData);
        #endregion

        #region Abstract (SMS logic)
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
        /// <param name="partyData">The data associated to a specific party (e.g., citizen, organization).</param>
        /// <returns>
        ///   The dictionary of &lt;placeholder, value&gt; used for personalization of "Notify NL" Web API service notification.
        /// </returns>
        protected abstract Task<Dictionary<string, object>> GetSmsPersonalizationAsync(CommonPartyData partyData);
        #endregion
    }
}