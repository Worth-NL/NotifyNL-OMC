// © 2023, Worth Systems.

using Common.Enums.Processing;
using EventsHandler.Models.DTOs.Processing;
using EventsHandler.Models.Responses.Sending;
using EventsHandler.Properties;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.Versioning.Interfaces;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;

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
        /// <param name="reference"><inheritdoc cref="NotifyReference" path="/summary"/></param>
        /// <param name="notificationMethod">The notification method.</param>
        /// <param name="messages">The messages to be used during registration of this event.</param>
        /// <returns>
        ///   The response from an external Web API service.
        /// </returns>
        internal async Task<RequestResponse> ReportCompletionAsync(NotifyReference reference, NotifyMethods notificationMethod, params string[] messages)
        {
            try
            {
                this.QueryContext.SetNotification(reference.Notification);

                // Register processed notification
                ContactMoment contactMoment = await this.QueryContext.CreateContactMomentAsync(
                    GetCreateContactMomentJsonBody(reference.Notification, reference, notificationMethod, messages));

                RequestResponse requestResponse;

                // Linking to the case and the customer
                if ((requestResponse = await this.QueryContext.LinkCaseToContactMomentAsync(GetLinkCaseJsonBody(contactMoment, reference))).IsFailure || 
                    (requestResponse = await this.QueryContext.LinkCustomerToContactMomentAsync(GetLinkCustomerJsonBody(contactMoment, reference))).IsFailure)
                {
                    return RequestResponse.Failure(requestResponse.JsonResponse);
                }

                return RequestResponse.Success(ApiResources.Register_NotifyNL_SUCCESS_NotificationSent);
            }
            catch (Exception exception)
            {
                return RequestResponse.Failure(exception.Message);
            }
        }

        /// <summary>
        /// Prepares a dedicated JSON body.
        /// </summary>
        /// <param name="notification"><inheritdoc cref="NotificationEvent" path="/summary"/></param>
        /// <param name="reference"><inheritdoc cref="NotifyReference" path="/summary"/></param>
        /// <param name="notificationMethod">The notification method.</param>
        /// <param name="messages">The messages.</param>
        /// <returns>
        ///   The JSON content for HTTP Request Body.
        /// </returns>
        protected string GetCreateContactMomentJsonBody(NotificationEvent notification, NotifyReference reference, NotifyMethods notificationMethod, IReadOnlyList<string> messages);

        /// <summary>
        /// Prepares a dedicated JSON body.
        /// </summary>
        /// <param name="contactMoment"><inheritdoc cref="ContactMoment" path="/summary"/></param>
        /// <param name="reference"><inheritdoc cref="NotifyReference" path="/summary"/></param>
        /// <returns>
        ///   The JSON content for HTTP Request Body.
        /// </returns>
        protected string GetLinkCaseJsonBody(ContactMoment contactMoment, NotifyReference reference);

        /// <summary>
        /// Prepares a dedicated JSON body.
        /// </summary>
        /// <param name="contactMoment"><inheritdoc cref="ContactMoment" path="/summary"/></param>
        /// <param name="reference"><inheritdoc cref="NotifyReference" path="/summary"/></param>
        /// <returns>
        ///   The JSON content for HTTP Request Body.
        /// </returns>
        protected string GetLinkCustomerJsonBody(ContactMoment contactMoment, NotifyReference reference);
    }
}