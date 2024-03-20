// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Constants;
using EventsHandler.Exceptions;
using EventsHandler.Extensions;
using EventsHandler.Properties;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;
using EventsHandler.Services.Telemetry.Interfaces;
using System.Text;
using System.Text.Json;

namespace EventsHandler.Services.Telemetry
{
    /// <summary>
    /// <inheritdoc cref="IFeedbackService" />
    /// <para>
    ///   Informs external "Contactmomenten" API web service about completion of sending notifications to NotifyNL API web service.
    /// </para>
    /// </summary>
    /// <seealso cref="IFeedbackService" />
    internal sealed class ContactRegistration : IFeedbackService
    {
        private readonly IDataQueryService<NotificationEvent> _dataQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactRegistration"/> class.
        /// </summary>
        public ContactRegistration(IDataQueryService<NotificationEvent> dataQuery)
        {
            this._dataQuery = dataQuery;
        }

        /// <inheritdoc cref="IFeedbackService.ReportCompletionAsync(NotificationEvent, NotifyMethods)"/>
        async Task<string> IFeedbackService.ReportCompletionAsync(NotificationEvent notification, NotifyMethods notificationMethod)
        {
            // NOTE: Feedback from "OpenKlant" will be passed to "OpenZaak"
            return await SendFeedbackToOpenZaakAsync(
                notification,
                await SendFeedbackToOpenKlantAsync(notification, notificationMethod));
        }

        #region Helper methods
        /// <summary>
        /// Sends the completion feedback to "OpenKlant" Web service.
        /// </summary>
        private async Task<ContactMoment> SendFeedbackToOpenKlantAsync(NotificationEvent notification, NotifyMethods notificationMethod)
        {
            // Prepare the body
            CaseStatus caseStatus = (await this._dataQuery.From(notification).GetCaseStatusesAsync()).LastStatus();  // TODO: Regarding performance, think whether we can store such data in a kind of register

            string serialized = JsonSerializer.Serialize(new Dictionary<string, object>  // TODO: Optimization-wise, try to compose a simple JSON-like string directly in the code to skip the serialization
            {
                { "bronorganisatie", notification.GetOrganizationId() },
                { "registratiedatum", caseStatus.Created },
                { "kanaal", $"{notificationMethod}" },
                { "tekst", string.Empty },  // TODO: To be filled
                { "initiatief", "gemeente" }
            });
            HttpContent body = new StringContent(serialized, Encoding.UTF8, DefaultValues.Request.ContentType);

            // Predefined URL components
            string specificOpenKlant = this._dataQuery.HttpSupplier.Configuration.User.Domain.OpenKlant();
            Uri klantContactMomentUri = new($"https://{specificOpenKlant}/contactmomenten/api/v1/contactmomenten");

            // Sending the request and getting the response (combined internal logic)
            return await this._dataQuery
                .From(notification)
                .ProcessPostAsync<ContactMoment>(HttpClientTypes.Telemetry, klantContactMomentUri, body, Resources.HttpRequest_ERROR_NoFeedbackKlant);
        }

        /// <summary>
        /// Sends the completion feedback to "OpenZaak" Web service.
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
            string specificOpenZaak = this._dataQuery.HttpSupplier.Configuration.User.Domain.OpenZaak();
            Uri klantContactMomentUri = new($"https://{specificOpenZaak}/zaken/api/v1/zaakcontactmomenten");

            // Sending the request
            (bool success, string jsonResponse) =
                await this._dataQuery.HttpSupplier.PostAsync(HttpClientTypes.Telemetry, notification.GetOrganizationId(), klantContactMomentUri, body);

            // Getting the response
            return success ? jsonResponse : throw new TelemetryException(jsonResponse);
        }
        #endregion
    }
}