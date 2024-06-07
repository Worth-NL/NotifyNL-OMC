// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Configuration;
using EventsHandler.Constants;
using EventsHandler.Exceptions;
using EventsHandler.Extensions;
using EventsHandler.Properties;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;
using EventsHandler.Services.Telemetry.Interfaces;
using System.Text;
using System.Text.Json;

namespace EventsHandler.Services.Telemetry.v1
{
    /// <summary>
    /// <inheritdoc cref="ITelemetryService"/>
    /// </summary>
    /// <remarks>
    ///   Version: "ContactMomenten" Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="ITelemetryService"/>
    internal sealed class ContactRegistration : ITelemetryService
    {
        private readonly WebApiConfiguration _configuration;
        private readonly IDataQueryService<NotificationEvent> _dataQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactRegistration"/> class.
        /// </summary>
        public ContactRegistration(WebApiConfiguration configuration, IDataQueryService<NotificationEvent> dataQuery)
        {
            this._configuration = configuration;
            this._dataQuery = dataQuery;
        }

        /// <inheritdoc cref="ITelemetryService.ReportCompletionAsync(NotificationEvent, NotifyMethods, string)"/>
        /// <returns>
        ///   Response from "OpenZaak" Web API service.
        /// </returns>
        async Task<string> ITelemetryService.ReportCompletionAsync(NotificationEvent notification, NotifyMethods notificationMethod, string message)
        {
            // NOTE: Feedback from "OpenKlant" will be passed to "OpenZaak"
            return await SendFeedbackToOpenZaakAsync(
                notification,
                await SendFeedbackToOpenKlantAsync(notification, notificationMethod, message));
        }

        #region Helper methods
        /// <summary>
        /// Sends the completion feedback to "OpenKlant" Web API service.
        /// </summary>
        private async Task<ContactMoment> SendFeedbackToOpenKlantAsync(NotificationEvent notification, NotifyMethods notificationMethod, string message)
        {
            // Prepare the body
            CaseStatus caseStatus = (await this._dataQuery.From(notification).GetCaseStatusesAsync()).LastStatus();  // TODO: Regarding performance, think whether we can store such data in a kind of register

            string serialized = JsonSerializer.Serialize(new Dictionary<string, object>  // TODO: Optimization-wise, try to compose a simple JSON-like string directly in the code to skip the serialization
            {
                { "bronorganisatie", notification.GetOrganizationId() },
                { "registratiedatum", caseStatus.Created },
                { "kanaal", $"{notificationMethod}" },
                { "tekst", message },
                { "initiatief", "gemeente" },
                { "medewerker", "https://www.google.com/" }  // TODO: Check if this is the correct one
            });
            HttpContent body = new StringContent(serialized, Encoding.UTF8, DefaultValues.Request.ContentType);

            // Predefined URL components
            string specificOpenKlant = this._configuration.User.Domain.OpenKlant();
            Uri klantContactMomentUri = new($"https://{specificOpenKlant}/contactmomenten/api/v1/contactmomenten");

            // Sending the request and getting the response (combined internal logic)
            return await this._dataQuery
                .From(notification)
                .ProcessPostAsync<ContactMoment>(HttpClientTypes.Telemetry_ContactMomenten, klantContactMomentUri, body, Resources.HttpRequest_ERROR_NoFeedbackKlant);
        }

        /// <summary>
        /// Sends the completion feedback to "OpenZaak" Web API service.
        /// </summary>
        private async Task<string> SendFeedbackToOpenZaakAsync(NotificationEvent notification, ContactMoment contactMoment)
        {
            // Prepare the body
            string serialized = JsonSerializer.Serialize(new Dictionary<string, object>  // TODO: Optimization-wise, try to compose a simple JSON-like string directly in the code to skip the serialization
            {
                { "zaak", $"{notification.MainObject}" },
                { "contactmoment", $"{contactMoment.Url}" }
            });
            HttpContent body = new StringContent(serialized, Encoding.UTF8, DefaultValues.Request.ContentType);

            // Predefined URL components
            string specificOpenZaak = this._configuration.User.Domain.OpenZaak();
            Uri klantContactMomentUri = new($"https://{specificOpenZaak}/zaken/api/v1/zaakcontactmomenten");

            // Sending the request
            (bool success, string jsonResponse) =
                await this._dataQuery.HttpNetwork.PostAsync(HttpClientTypes.Telemetry_ContactMomenten, klantContactMomentUri, body);

            // Getting the response
            return success ? jsonResponse : throw new TelemetryException(jsonResponse);
        }
        #endregion
    }
}