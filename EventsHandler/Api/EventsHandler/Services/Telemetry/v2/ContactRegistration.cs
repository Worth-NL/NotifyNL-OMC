// © 2024, Worth Systems.

using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Configuration;
using EventsHandler.Constants;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.Telemetry.Interfaces;
using EventsHandler.Services.Versioning.Interfaces;
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
        private readonly WebApiConfiguration _configuration;
        private readonly IQueryContext _queryContext;

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "Klantcontacten";
        
        /// <inheritdoc cref="IVersionDetails.Version"/>
        string IVersionDetails.Version => "2.0.0";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ContactRegistration"/> class.
        /// </summary>
        public ContactRegistration(WebApiConfiguration configuration, IQueryContext queryContext)
        {
            this._configuration = configuration;
            this._queryContext = queryContext;
        }

        /// <inheritdoc cref="ITelemetryService.ReportCompletionAsync(NotificationEvent, NotifyMethods, string[])"/>
        async Task<string> ITelemetryService.ReportCompletionAsync(NotificationEvent notification, NotifyMethods notificationMethod, string[] messages)
        {
            this._queryContext.SetNotification(notification);

            // NOTE: Feedback from "OpenKlant" will be linked to the subject object
            return await SendFeedbackToSubjectObjectAsync(this._configuration, this._queryContext,
                   await SendFeedbackToOpenKlantAsync(this._queryContext, notificationMethod, messages));
        }

        #region Helper methods
        private static async Task<ContactMoment> SendFeedbackToOpenKlantAsync(
            IQueryContext queryContext, NotifyMethods notificationMethod, IReadOnlyList<string> messages)
        {
            string userMessageSubject = messages.Count > 0 ? messages[0] : string.Empty;
            string userMessageBody    = messages.Count > 1 ? messages[1] : string.Empty;
            string isSuccessfullySent = messages.Count > 2 ? messages[2] : string.Empty;

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

        private static async Task<string> SendFeedbackToSubjectObjectAsync(
            WebApiConfiguration configuration, IQueryContext queryContext, ContactMoment contactMoment)
        {
            string caseId = (await queryContext.GetMainObjectAsync()).Id;

            string jsonBody =
                $"{{" +
                $"  \"klantcontact\": {{" +                  // ENG: Customer contact
                $"    \"uuid\": \"{contactMoment.Id}\"" +
                $"  }}, " +
                $"  \"wasKlantcontact\": {{" +               // ENG: ???
                $"    \"uuid\": \"{contactMoment.Id}\"" +
                $"  }}, " +
                $"  \"onderwerpobjectidentificator\": {{" +  // ENG: Subject Object Identifier
                $"    \"objectId\": \"{caseId}\", " +
                $"    \"codeObjecttype\": \"{configuration.AppSettings.Variables.OpenKlant.CodeObjectType()}\", " +
                $"    \"codeRegister\": \"{configuration.AppSettings.Variables.OpenKlant.CodeRegister()}\", " +
                $"    \"codeSoortObjectId\": \"{configuration.AppSettings.Variables.OpenKlant.CodeObjectTypeId()}\"" +
                $"  }}" +
                $"}}";

            HttpContent body = new StringContent(jsonBody, Encoding.UTF8, DefaultValues.Request.ContentType);

            return await queryContext.LinkToSubjectObjectAsync(body);
        }
        #endregion
    }
}