// © 2023, Worth Systems.

using Common.Settings.Configuration;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using System.Diagnostics.CodeAnalysis;
using NotificatieApi;
using NotificatieApi;
using NotificatieApi;
using OpenKlant;
using OpenKlant;

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
            INotifyService<NotifyData> notifyService)
            : base(configuration, dataQuery, notifyService)
        {
        }

        #region Polymorphic
        private const string ExclusionJustificationMessage = $"This method is unreachable, since it is dependent on {nameof(PrepareDataAsync)} => throwing exception.";

        /// <inheritdoc cref="BaseScenario.PrepareDataAsync(ZhvModels.Mapping.Models.POCOs.NotificatieApi.NotificationEvent)"/>
        protected override async Task<PreparedData> PrepareDataAsync(NotificationEvent notification)
            => await Task.FromResult(NotImplemented<PreparedData>());

        /// <inheritdoc cref="BaseScenario.GetSmsNotifyData(ZhvModels.Mapping.Models.POCOs.NotificatieApi.NotificationEvent, PreparedData)"/>
        [ExcludeFromCodeCoverage(Justification = ExclusionJustificationMessage)]
        protected override NotifyData GetSmsNotifyData(NotificationEvent notification, PreparedData preparedData)
            => NotImplemented<NotifyData>();  // NOTE: Only for compilation purposes

        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        [ExcludeFromCodeCoverage(Justification = ExclusionJustificationMessage)]
        protected override Guid GetSmsTemplateId()
            => NotImplemented<Guid>();  // NOTE: Only for compilation purposes

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalization(ZhvModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData)"/>
        [ExcludeFromCodeCoverage(Justification = ExclusionJustificationMessage)]
        protected override Dictionary<string, object> GetSmsPersonalization(CommonPartyData partyData)
            => NotImplemented<Dictionary<string, object>>();  // NOTE: Only for compilation purposes

        /// <inheritdoc cref="BaseScenario.GetEmailNotifyData(ZhvModels.Mapping.Models.POCOs.NotificatieApi.NotificationEvent, PreparedData)"/>
        [ExcludeFromCodeCoverage(Justification = ExclusionJustificationMessage)]
        protected override NotifyData GetEmailNotifyData(NotificationEvent notification, PreparedData preparedData)
            => NotImplemented<NotifyData>();  // NOTE: Only for compilation purposes

        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        [ExcludeFromCodeCoverage(Justification = ExclusionJustificationMessage)]
        protected override Guid GetEmailTemplateId()
            => NotImplemented<Guid>();  // NOTE: Only for compilation purposes

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalization(ZhvModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData)"/>
        [ExcludeFromCodeCoverage(Justification = ExclusionJustificationMessage)]
        protected override Dictionary<string, object> GetEmailPersonalization(CommonPartyData partyData)
            => NotImplemented<Dictionary<string, object>>();  // NOTE: Only for compilation purposes

        /// <inheritdoc cref="BaseScenario.GetWhitelistEnvVarName()"/>
        [ExcludeFromCodeCoverage(Justification = ExclusionJustificationMessage)]
        protected override string GetWhitelistEnvVarName()
            => NotImplemented<string>();  // NOTE: Only for compilation purposes
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