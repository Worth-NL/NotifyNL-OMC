// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.Settings.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace EventsHandler.Services.DataProcessing.Strategy.Implementations
{
    /// <summary>
    /// <inheritdoc cref="INotifyScenario"/>
    /// The strategy representing not implemented scenario.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    internal sealed class NotImplementedScenario : BaseScenario
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseScenario"/> class.
        /// </summary>
        public NotImplementedScenario(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
            : base(configuration, dataQuery)
        {
        }

        #region Polymorphic
        /// <inheritdoc cref="BaseScenario.GetAllNotifyDataAsync(NotificationEvent)"/>
        [ExcludeFromCodeCoverage]
        internal override async Task<NotifyData[]> GetAllNotifyDataAsync(NotificationEvent notification)
        {
            return await Task.FromResult(NotImplemented<NotifyData[]>());
        }

        /// <inheritdoc cref="BaseScenario.GetSmsNotifyDataAsync(CommonPartyData)"/>
        [ExcludeFromCodeCoverage]
        protected override async Task<NotifyData> GetSmsNotifyDataAsync(CommonPartyData partyData)
        {
            return await Task.FromResult(NotImplemented<NotifyData>());
        }

        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        [ExcludeFromCodeCoverage]
        protected override string GetSmsTemplateId()
        {
            return NotImplemented<string>();
        }

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalizationAsync(CommonPartyData)"/>
        [ExcludeFromCodeCoverage]
        protected override async Task<Dictionary<string, object>> GetSmsPersonalizationAsync(CommonPartyData partyData)
        {
            return await Task.FromResult(NotImplemented<Dictionary<string, object>>());
        }

        /// <inheritdoc cref="BaseScenario.GetEmailNotifyDataAsync(CommonPartyData)"/>
        [ExcludeFromCodeCoverage]
        protected override async Task<NotifyData> GetEmailNotifyDataAsync(CommonPartyData partyData)
        {
            return await Task.FromResult(NotImplemented<NotifyData>());
        }

        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        [ExcludeFromCodeCoverage]
        protected override string GetEmailTemplateId()
        {
            return NotImplemented<string>();
        }

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalizationAsync(CommonPartyData)"/>
        [ExcludeFromCodeCoverage]
        protected override async Task<Dictionary<string, object>> GetEmailPersonalizationAsync(CommonPartyData partyData)
        {
            return await Task.FromResult(NotImplemented<Dictionary<string, object>>());
        }

        /// <inheritdoc cref="BaseScenario.GetWhitelistName"/>
        [ExcludeFromCodeCoverage]
        protected override string GetWhitelistName()
        {
            return NotImplemented<string>();
        }
        #endregion

        /// <summary>
        /// This result is expected when using <see cref="NotImplementedScenario"/>.
        /// </summary>
        /// <returns>
        ///   The <see cref="NotImplementedException"/>.
        /// </returns>
        private static TResult NotImplemented<TResult>()
        {
            throw new NotImplementedException();
        }
    }
}