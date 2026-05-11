using Common.Settings.Configuration;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;

namespace EventsHandler.Services.DataProcessing.Strategy.Implementations
{
    /// <summary>
    /// <inheritdoc cref="INotifyScenario"/>
    /// Marker strategy for "Message" scenario.
    /// This scenario does NOT send notifications via Notify NL.
    /// Instead, it is intercepted in <see cref="NotifyProcessor"/> and handled by a separate worker.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    internal sealed class MessageScenario : BaseScenario
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageScenario"/> class.
        /// </summary>
        public MessageScenario(
            OmcConfiguration configuration,
            IDataQueryService<NotificationEvent> dataQuery,
            INotifyService<NotifyData> notifyService)
            : base(configuration, dataQuery, notifyService)
        {
        }

        #region Polymorphic (PrepareDataAsync)
        /// <inheritdoc cref="BaseScenario.PrepareDataAsync(NotificationEvent)"/>
        protected override Task<PreparedData> PrepareDataAsync(NotificationEvent notification)
        {
            // This method should never be called because MessageScenario is intercepted in NotifyProcessor.
            throw new NotSupportedException("MessageScenario is a marker and does not support PrepareDataAsync. Use the dedicated MessageForwarder instead.");
        }
        #endregion

        #region Polymorphic (Email logic)
        protected override Guid GetEmailTemplateId()
            => throw new NotSupportedException("MessageScenario does not send emails.");

        protected override Dictionary<string, object> GetEmailPersonalization(CommonPartyData partyData)
            => throw new NotSupportedException("MessageScenario does not send emails.");
        #endregion

        #region Polymorphic (SMS logic)
        protected override Guid GetSmsTemplateId()
            => throw new NotSupportedException("MessageScenario does not send SMS.");

        protected override Dictionary<string, object> GetSmsPersonalization(CommonPartyData partyData)
            => throw new NotSupportedException("MessageScenario does not send SMS.");
        #endregion

        #region Polymorphic (Letter logic)
        protected override Guid GetLetterTemplateId()
            => throw new NotSupportedException("MessageScenario does not send letters.");

        protected override Dictionary<string, object> GetLetterPersonalization(CommonPartyData partyData)
            => throw new NotSupportedException("MessageScenario does not send letters.");
        #endregion

        #region Polymorphic (GetWhitelistEnvVarName)
        protected override string GetWhitelistEnvVarName()
            => "MessageScenario_NotUsed"; // Not used because no validation occurs
        #endregion
    }
}