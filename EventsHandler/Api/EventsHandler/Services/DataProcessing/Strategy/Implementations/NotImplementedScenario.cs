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
        /// <inheritdoc cref="BaseScenario.PrepareDataAsync(NotificationEvent)"/>
        protected override async Task<CommonPartyData> PrepareDataAsync(NotificationEvent notification)
        {
            return await Task.FromResult(NotImplemented<CommonPartyData>());
        }

        /// <inheritdoc cref="BaseScenario.GetSmsNotifyData(CommonPartyData)"/>
        [ExcludeFromCodeCoverage]
        protected override NotifyData GetSmsNotifyData(CommonPartyData partyData)
        {
            return NotImplemented<NotifyData>();
        }

        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        [ExcludeFromCodeCoverage]
        protected override Guid GetSmsTemplateId()
        {
            return NotImplemented<Guid>();
        }

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalization(CommonPartyData)"/>
        [ExcludeFromCodeCoverage]
        protected override Dictionary<string, object> GetSmsPersonalization(CommonPartyData partyData)
        {
            return NotImplemented<Dictionary<string, object>>();
        }

        /// <inheritdoc cref="BaseScenario.GetEmailNotifyData(CommonPartyData)"/>
        [ExcludeFromCodeCoverage]
        protected override NotifyData GetEmailNotifyData(CommonPartyData partyData)
        {
            return NotImplemented<NotifyData>();
        }

        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        [ExcludeFromCodeCoverage]
        protected override Guid GetEmailTemplateId()
        {
            return NotImplemented<Guid>();
        }

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalization(CommonPartyData)"/>
        [ExcludeFromCodeCoverage]
        protected override Dictionary<string, object> GetEmailPersonalization(CommonPartyData partyData)
        {
            return NotImplemented<Dictionary<string, object>>();
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