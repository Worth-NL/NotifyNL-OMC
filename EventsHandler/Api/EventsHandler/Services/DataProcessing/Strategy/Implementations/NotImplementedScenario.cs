// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
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
        public NotImplementedScenario(
            WebApiConfiguration configuration,
            IDataQueryService<NotificationEvent> dataQuery,
            INotifyService<NotificationEvent, NotifyData> notifyService)
            : base(configuration, dataQuery, notifyService)
        {
        }

        #region Polymorphic
        private const string ExclusionJustificationMessage = $"This method is unreachable, since it is dependent on {nameof(PrepareDataAsync)} => throwing exception.";

        /// <inheritdoc cref="BaseScenario.PrepareDataAsync(NotificationEvent)"/>
        protected override async Task<CommonPartyData> PrepareDataAsync(NotificationEvent notification)
            => await Task.FromResult(NotImplemented<CommonPartyData>());

        /// <inheritdoc cref="BaseScenario.GetSmsNotifyDataAsync(CommonPartyData)"/>
        [ExcludeFromCodeCoverage(Justification = ExclusionJustificationMessage)]
        protected override async Task<NotifyData> GetSmsNotifyDataAsync(CommonPartyData partyData)
            => await Task.FromResult(NotImplemented<NotifyData>()); // NOTE: Only for compilation purposes

        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        [ExcludeFromCodeCoverage(Justification = ExclusionJustificationMessage)]
        protected override Guid GetSmsTemplateId()
            => NotImplemented<Guid>(); // NOTE: Only for compilation purposes

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalizationAsync(CommonPartyData)"/>
        [ExcludeFromCodeCoverage(Justification = ExclusionJustificationMessage)]
        protected override async Task<Dictionary<string, object>> GetSmsPersonalizationAsync(CommonPartyData partyData)
            => await Task.FromResult(NotImplemented<Dictionary<string, object>>()); // NOTE: Only for compilation purposes

        /// <inheritdoc cref="BaseScenario.GetEmailNotifyDataAsync(CommonPartyData)"/>
        [ExcludeFromCodeCoverage(Justification = ExclusionJustificationMessage)]
        protected override async Task<NotifyData> GetEmailNotifyDataAsync(CommonPartyData partyData)
            => await Task.FromResult(NotImplemented<NotifyData>()); // NOTE: Only for compilation purposes

        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        [ExcludeFromCodeCoverage(Justification = ExclusionJustificationMessage)]
        protected override Guid GetEmailTemplateId()
            => NotImplemented<Guid>(); // NOTE: Only for compilation purposes

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalizationAsync(CommonPartyData)"/>
        [ExcludeFromCodeCoverage(Justification = ExclusionJustificationMessage)]
        protected override async Task<Dictionary<string, object>> GetEmailPersonalizationAsync(CommonPartyData partyData)
            => await Task.FromResult(NotImplemented<Dictionary<string, object>>()); // NOTE: Only for compilation purposes

        /// <inheritdoc cref="BaseScenario.GetWhitelistEnvVarName()"/>
        [ExcludeFromCodeCoverage(Justification = ExclusionJustificationMessage)]
        protected override string GetWhitelistEnvVarName()
            => NotImplemented<string>(); // NOTE: Only for compilation purposes
        #endregion

        /// <summary>
        /// This result is expected when using <see cref="NotImplementedScenario"/>.
        /// </summary>
        /// <returns>
        ///   The <see cref="NotImplementedException"/>.
        /// </returns>
        /// <exception cref="NotImplementedException"/>
        private static TResult NotImplemented<TResult>()
            => throw new NotImplementedException();
    }
}