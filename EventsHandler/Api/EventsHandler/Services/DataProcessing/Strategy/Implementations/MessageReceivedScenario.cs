﻿// © 2024, Worth Systems.

using EventsHandler.Exceptions;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.Objecten.Message;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
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
    /// The strategy for "Message received" scenario.
    /// </summary>
    /// <seealso cref="BaseScenario"/>
    internal sealed class MessageReceivedScenario : BaseScenario
    {
        private Data _messageData;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageReceivedScenario"/> class.
        /// </summary>
        public MessageReceivedScenario(
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
            // Validation #1: Sending messages should be allowed
            if (!this.Configuration.User.Whitelist.Message_Allowed())
            {
                throw new AbortedNotifyingException(
                    string.Format(Resources.Processing_ABORT_DoNotSendNotification_MessagesForbidden, GetWhitelistName()));
            }

            // Setup
            IQueryContext queryContext = this.DataQuery.From(notification);

            this._messageData = (await queryContext.GetMessageAsync()).Record.Data;
            
            // Preparing citizen details
            return await queryContext.GetPartyDataAsync(           // 2. Citizen details
                         this._messageData.Identification.Value);  // 1. BSN number
        }
        #endregion

        #region Polymorphic (Email logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override Guid GetEmailTemplateId()
            => this.Configuration.User.TemplateIds.Email.MessageReceived();

        private static readonly object s_padlock = new();
        private static readonly Dictionary<string, object> s_emailPersonalization = new();  // Cached dictionary no need to be initialized every time

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalization(CommonPartyData)"/>
        protected override Dictionary<string, object> GetEmailPersonalization(CommonPartyData partyData)
        {
            lock (s_padlock)
            {
                // TODO: Names of parameters can be taken from models and properties(?)
                s_emailPersonalization["klant.voornaam"] = partyData.Name;
                s_emailPersonalization["klant.voorvoegselAchternaam"] = partyData.SurnamePrefix;
                s_emailPersonalization["klant.achternaam"] = partyData.Surname;

                s_emailPersonalization["message.onderwerp"] = this._messageData.Subject;
                s_emailPersonalization["message.handelingsperspectief"] = this._messageData.ActionsPerspective;

                return s_emailPersonalization;
            }
        }
        #endregion

        #region Polymorphic (SMS logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetSmsTemplateId()"/>
        protected override Guid GetSmsTemplateId()
            => this.Configuration.User.TemplateIds.Sms.MessageReceived();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalization(CommonPartyData)"/>
        protected override Dictionary<string, object> GetSmsPersonalization(CommonPartyData partyData)
        {
            return GetEmailPersonalization(partyData);  // NOTE: Both implementations are identical
        }
        #endregion

        #region Polymorphic (GetWhitelistName)
        private static string? s_environmentVariableName;

        /// <inheritdoc cref="BaseScenario.GetWhitelistName"/>
        protected override string GetWhitelistName()
        {
            lock (s_padlock)
            {
                return s_environmentVariableName ??= $"{nameof(this.Configuration.User).ToUpper()}_" +
                                                     $"{nameof(this.Configuration.User.Whitelist).ToUpper()}_" +
                                                     $"{nameof(this.Configuration.User.Whitelist.Message_Allowed).ToUpper()}";
            }
        }
        #endregion
    }
}