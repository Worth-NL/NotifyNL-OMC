﻿// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Settings.Extensions;
using EventsHandler.Exceptions;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Strategy.Base;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.Objecten.Message;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;

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
            OmcConfiguration configuration,
            IDataQueryService<NotificationEvent> dataQuery,
            INotifyService<NotifyData> notifyService)  // Dependency Injection (DI)
            : base(configuration, dataQuery, notifyService)
        {
        }

        #region Polymorphic (PrepareDataAsync)
        /// <inheritdoc cref="BaseScenario.PrepareDataAsync(NotificationEvent)"/>
        protected override async Task<PreparedData> PrepareDataAsync(NotificationEvent notification)
        {
            // Validation #1: Sending messages should be allowed
            if (!this.Configuration.ZGW.Whitelist.Message_Allowed())
            {
                throw new AbortedNotifyingException(
                    string.Format(ApiResources.Processing_ABORT_DoNotSendNotification_Whitelist_MessagesForbidden, GetWhitelistEnvVarName()));
            }

            // Setup
            IQueryContext queryContext = this.DataQuery.From(notification);

            this._messageData = (await queryContext.GetMessageAsync()).Record.Data;
            
            // Preparing party details
            return new PreparedData(
                party: await queryContext.GetPartyDataAsync(
                    caseUri: null,
                    bsnNumber: this._messageData.Identification.Value),  // BSN number
                caseUri: null);  // NOTE: There is no case linked so, there is no case URI either
        }
        #endregion

        #region Polymorphic (Email logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetEmailTemplateId()"/>
        protected override Guid GetEmailTemplateId()
            => this.Configuration.Notify.TemplateId.Email.MessageReceived();

        private static readonly object s_padlock = new();
        private static readonly Dictionary<string, object> s_emailPersonalization = [];  // Cached dictionary no need to be initialized every time
        private static readonly Dictionary<string, object> s_letterPersonalization = [];  // Cached dictionary no need to be initialized every time

        /// <inheritdoc cref="BaseScenario.GetEmailPersonalization(ZhvModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData)"/>
        protected override Dictionary<string, object> GetEmailPersonalization(CommonPartyData partyData)
        {
            lock (s_padlock)
            {
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
            => this.Configuration.Notify.TemplateId.Sms.MessageReceived();

        /// <inheritdoc cref="BaseScenario.GetSmsPersonalization(ZhvModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData)"/>
        protected override Dictionary<string, object> GetSmsPersonalization(CommonPartyData partyData)
        {
            return GetEmailPersonalization(partyData);  // NOTE: Both implementations are identical
        }
        #endregion

        #region Polymorphic (Letter logic: template + personalization)
        /// <inheritdoc cref="BaseScenario.GetLetterTemplateId"/>
        protected override Guid GetLetterTemplateId()
            => this.Configuration.Notify.TemplateId.Letter.ZaakClose();

        /// <inheritdoc cref="BaseScenario.GetLetterPersonalization(ZhvModels.Mapping.Models.POCOs.OpenKlant.CommonPartyData)"/>
        protected override Dictionary<string, object> GetLetterPersonalization(CommonPartyData partyData)
        {
            lock (s_padlock)
            {
                s_letterPersonalization["klant.voornaam"] = partyData.Name;
                s_letterPersonalization["klant.voorvoegselAchternaam"] = partyData.SurnamePrefix;
                s_letterPersonalization["klant.achternaam"] = partyData.Surname;
                s_letterPersonalization["klant.street"] = partyData.LetterAddress.Street;
                s_letterPersonalization["klant.number"] = partyData.LetterAddress.Number;
                s_letterPersonalization["klant.zip"] = partyData.LetterAddress.Zip;
                s_letterPersonalization["klant.city"] = partyData.LetterAddress.City;
                s_letterPersonalization["klant.country"] = partyData.LetterAddress.Country;

                s_letterPersonalization["message.onderwerp"] = this._messageData.Subject;
                s_letterPersonalization["message.handelingsperspectief"] = this._messageData.ActionsPerspective;

                return s_letterPersonalization;
            }
        }
        #endregion

        #region Polymorphic (GetWhitelistEnvVarName)
        /// <inheritdoc cref="BaseScenario.GetWhitelistEnvVarName()"/>
        protected override string GetWhitelistEnvVarName()
            => ConfigExtensions.GetWhitelistMessageAllowedEnvVarName();
        #endregion
    }
}