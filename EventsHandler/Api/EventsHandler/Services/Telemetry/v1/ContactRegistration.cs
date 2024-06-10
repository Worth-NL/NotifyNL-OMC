// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Behaviors.Versioning;
using EventsHandler.Constants;
using EventsHandler.Extensions;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.Telemetry.Interfaces;
using System.Text;

namespace EventsHandler.Services.Telemetry.v1
{
    /// <summary>
    /// <inheritdoc cref="ITelemetryService"/>
    /// </summary>
    /// <remarks>
    ///   Version: "Contactmomenten" Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    internal sealed class ContactRegistration : ITelemetryService
    {
        private readonly IQueryContext _queryContext;

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "Contactmomenten";
        
        /// <inheritdoc cref="IVersionDetails.Version"/>
        string IVersionDetails.Version => "1.0.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactRegistration"/> class.
        /// </summary>
        public ContactRegistration(IQueryContext queryContext)
        {
            this._queryContext = queryContext;
        }

        /// <inheritdoc cref="ITelemetryService.ReportCompletionAsync(NotificationEvent, NotifyMethods, string[])"/>
        async Task<string> ITelemetryService.ReportCompletionAsync(NotificationEvent notification, NotifyMethods notificationMethod, string[] messages)
        {
            // NOTE: Feedback from "OpenKlant" will be passed to "OpenZaak"
            return await SendFeedbackToOpenZaakAsync(this._queryContext, notification,
                   await SendFeedbackToOpenKlantAsync(this._queryContext, notification, notificationMethod, messages));
        }

        #region Helper methods
        private static async Task<ContactMoment> SendFeedbackToOpenKlantAsync(
            IQueryContext queryContext, NotificationEvent notification, NotifyMethods notificationMethod, IReadOnlyList<string> messages)
        {
            // Prepare the body
            CaseStatus caseStatus = (await queryContext.GetCaseStatusesAsync()).LastStatus();
            string logMessage = messages.Count > 0 ? messages[0] : string.Empty;

            string jsonBody =
                $"{{" +
                $"  \"bronorganisatie\": {notification.GetOrganizationId()}, " +  // ENG: Source organization
                $"  \"registratiedatum\": {caseStatus.Created}, " +               // ENG: Date of registration (of the case)
                $"  \"kanaal\": \"{notificationMethod}\", " +                     // ENG: Channel (of communication / notification)
                $"  \"tekst\": \"{logMessage}\", " +                              // ENG: Text (to be logged)
                $"  \"initiatief\": \"gemeente\", " +                             // ENG: Initiator (of the case)
                $"  \"medewerker\": \"{DefaultValues.Models.EmptyUri}\"" +        // ENG: Worker / collaborator / contributor
                $"}}";

            HttpContent body = new StringContent(jsonBody, Encoding.UTF8, DefaultValues.Request.ContentType);

            return await queryContext.SendFeedbackToOpenKlantAsync(body);
        }

        private static async Task<string> SendFeedbackToOpenZaakAsync(
            IQueryContext queryContext, NotificationEvent notification, ContactMoment contactMoment)
        {
            // Prepare the body
            string jsonBody =
                $"{{" +
                $"  \"zaak\": \"{notification.MainObject}\", " +   // ENG: Case
                $"  \"contactmoment\": \"{contactMoment.Url}\"" +  // ENG: Moment of contact
                $"}}";

            HttpContent body = new StringContent(jsonBody, Encoding.UTF8, DefaultValues.Request.ContentType);

            return await queryContext.SendFeedbackToOpenZaakAsync(body);
        }
        #endregion
    }
}