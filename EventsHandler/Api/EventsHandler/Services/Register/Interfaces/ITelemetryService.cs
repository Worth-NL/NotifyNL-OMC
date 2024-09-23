// © 2023, Worth Systems.

using EventsHandler.Exceptions;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.DataSending.Responses;
using EventsHandler.Services.Versioning.Interfaces;
using System.Text.Json;

namespace EventsHandler.Services.Register.Interfaces
{
    /// <summary>
    /// The service to collect and send feedback about the current business activities to the dedicated external API endpoint.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    public interface ITelemetryService : IVersionDetails
    {
        /// <summary>
        /// Reports to external API service that notification of type <see cref="NotifyMethods"/> was sent to "Notify NL" service.
        /// </summary>
        /// <param name="notification">The notification from "OpenNotificaties" Web API service.</param>
        /// <param name="notificationMethod">The notification method.</param>
        /// <param name="messages">The messages.</param>
        /// <returns>
        ///   The response from an external Web API service.
        /// </returns>
        internal async Task<RequestResponse> ReportCompletionAsync(NotificationEvent notification, NotifyMethods notificationMethod, params string[] messages)
        {
            try
            {
                // Register processed notification
                ContactMoment contactMoment = await CreateContactMomentAsync(notification, notificationMethod, messages);

                RequestResponse requestResponse;

                // Linking to the case and the customer
                if ((requestResponse = await LinkCaseAsync(contactMoment)).IsFailure || 
                    (requestResponse = await LinkCustomerAsync(contactMoment)).IsFailure)
                {
                    return RequestResponse.Failure(requestResponse.JsonResponse);
                }

                return RequestResponse.Success(Resources.Register_NotifyNL_SUCCESS_NotificationSent);
            }
            catch (Exception exception)
            {
                return RequestResponse.Failure(exception.Message);
            }
        }

        /// <summary>
        /// Register the moment when the <see cref="NotificationEvent"/> was successfully processed.
        /// </summary>
        /// <param name="notification">The notification from "OpenNotificaties" Web API service.</param>
        /// <param name="notificationMethod">The notification method.</param>
        /// <param name="messages">The messages.</param>
        /// <returns>
        ///   The response from "OpenZaak" Web API service mapped into a <see cref="ContactMoment"/> object.
        /// </returns>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="TelemetryException"/>
        /// <exception cref="JsonException"/>
        protected Task<ContactMoment> CreateContactMomentAsync(NotificationEvent notification, NotifyMethods notificationMethod, IReadOnlyList<string> messages);

        /// <summary>
        /// Links the <see cref="ContactMoment"/> with the <see cref="Case"/>.
        /// </summary>
        /// <param name="contactMoment"><inheritdoc cref="ContactMoment" path="/summary"/></param>
        /// <returns>
        ///   The response from an external Web API service.
        /// </returns>
        protected Task<RequestResponse> LinkCaseAsync(ContactMoment contactMoment);

        /// <summary>
        /// Links the <see cref="ContactMoment"/> with the <see cref="CommonPartyData"/>.
        /// </summary>
        /// <param name="contactMoment"><inheritdoc cref="ContactMoment" path="/summary"/></param>
        /// <returns>
        ///   The response from an external Web API service.
        /// </returns>
        protected Task<RequestResponse> LinkCustomerAsync(ContactMoment contactMoment);
    }
}