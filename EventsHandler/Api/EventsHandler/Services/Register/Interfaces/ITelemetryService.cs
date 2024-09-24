// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataSending.Responses;
using EventsHandler.Services.Versioning.Interfaces;

namespace EventsHandler.Services.Register.Interfaces
{
    /// <summary>
    /// The service to collect and send feedback about the current business activities to the dedicated external API endpoint.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    public interface ITelemetryService : IVersionDetails
    {
        /// <inheritdoc cref="IQueryContext"/>
        internal IQueryContext QueryContext { get; }

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
                this.QueryContext.SetNotification(notification);

                // Register processed notification
                ContactMoment contactMoment = await this.QueryContext.CreateContactMomentAsync(
                    GetCreateContactMomentJsonBody(notification, notificationMethod, messages));

                RequestResponse requestResponse;

                // Linking to the case and the customer
                if ((requestResponse = await this.QueryContext.LinkCaseToContactMomentAsync(GetLinkCaseJsonBody(contactMoment))).IsFailure || 
                    (requestResponse = await this.QueryContext.LinkCustomerToContactMomentAsync(GetLinkCustomerJsonBody(contactMoment))).IsFailure)
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
        /// Prepares a dedicated JSON body.
        /// </summary>
        /// <param name="notification">The notification from "OpenNotificaties" Web API service.</param>
        /// <param name="notificationMethod">The notification method.</param>
        /// <param name="messages">The messages.</param>
        /// <returns>
        ///   The JSON content for HTTP Request Body.
        /// </returns>
        protected string GetCreateContactMomentJsonBody(NotificationEvent notification, NotifyMethods notificationMethod, IReadOnlyList<string> messages);

        /// <summary>
        /// Prepares a dedicated JSON body.
        /// </summary>
        /// <param name="contactMoment"><inheritdoc cref="ContactMoment" path="/summary"/></param>
        /// <returns>
        ///   The JSON content for HTTP Request Body.
        /// </returns>
        protected string GetLinkCaseJsonBody(ContactMoment contactMoment);

        /// <summary>
        /// Prepares a dedicated JSON body.
        /// </summary>
        /// <param name="contactMoment"><inheritdoc cref="ContactMoment" path="/summary"/></param>
        /// <returns>
        ///   The JSON content for HTTP Request Body.
        /// </returns>
        protected string GetLinkCustomerJsonBody(ContactMoment contactMoment);
    }
}