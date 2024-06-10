// © 2024, Worth Systems.

using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Communication.Enums.v2;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Versioning;
using EventsHandler.Constants;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.Telemetry.Interfaces;
using System.Text;

namespace EventsHandler.Services.Telemetry.v2
{
    /// <inheritdoc cref="ITelemetryService"/>
    /// <remarks>
    ///   Version: "Klantcontacten" Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    internal sealed class ContactRegistration : ITelemetryService
    {
        private readonly IQueryContext _queryContext;

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "Klantcontacten";
        
        /// <inheritdoc cref="IVersionDetails.Version"/>
        string IVersionDetails.Version => "2.0.0";
        
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
            // NOTE: Feedback from "OpenKlant" will be linked to the subject object
            return await SendFeedbackToSubjectObjectAsync(this._queryContext,
                   await SendFeedbackToOpenKlantAsync(this._queryContext, notificationMethod, messages));
        }

        #region Helper methods
        private static async Task<ContactMoment> SendFeedbackToOpenKlantAsync(
            IQueryContext queryContext, NotifyMethods notificationMethod, IReadOnlyList<string> messages)
        {
            string userMessageSubject = messages.Count > 0 ? messages[0] : string.Empty;
            string userMessageBody    = messages.Count > 1 ? messages[1] : string.Empty;
            bool isSuccessfullySent   = messages.Count > 2 && Equals(messages[2], NotifyStatuses.Success.ToString());

            string jsonBody =
                $"{{" +
                $"  \"kanaal\": \"{notificationMethod}\", " +              // ENG: Channel of communication (notification)
                $"  \"onderwerp\": \"{userMessageSubject}\", " +           // ENG: Subject (of the message to be sent to the user)
                $"  \"inhoud\": \"{userMessageBody}\", " +                 // ENG: Content (of the message to be sent to the user)
                $"  \"indicatieContactGelukt\": {isSuccessfullySent}, " +  // ENG: Indication of successful contact
                $"  \"taal\": \"nl\", " +                                  // ENG: Language (of the notification)
                $"  \"vertrouwelijk\": false" +                            // ENG: Confidentiality (of the notification)
                $"}}";

            HttpContent body = new StringContent(jsonBody, Encoding.UTF8, DefaultValues.Request.ContentType);

            return await queryContext.SendFeedbackToOpenKlantAsync(body);
        }

        private static async Task<string> SendFeedbackToSubjectObjectAsync(IQueryContext queryContext, ContactMoment contactMoment)
        {
            string caseId = (await queryContext.GetMainObjectAsync()).Id;

            string jsonBody =
                $"{{" +
                $"  \"klantcontact\": {{" +
                $"    \"uuid\": \"{contactMoment.Url}\"" +
                $"  }}, " +
                $"  \"wasKlantcontact\": {{" +
                $"    \"uuid\": null" +
                $"  }}, " +
                $"  \"onderwerpobjectidentificator\": {{" +
                $"    \"objectId\": \"{caseId}\", " +
                $"    \"codeObjecttype\": \"\", " +  // TODO: Get from appsettings
                $"    \"codeRegister\": \"\", " +  // TODO: Get from appsettings
                $"    \"codeSoortObjectId\": \"\"" +  // TODO: Get from appsettings
                $"  }}" +
                $"}}";

            HttpContent body = new StringContent(jsonBody, Encoding.UTF8, DefaultValues.Request.ContentType);

            return await queryContext.LinkToSubjectObjectAsync(body);
        }
        #endregion
    }
}