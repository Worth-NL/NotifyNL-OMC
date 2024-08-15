// © 2023, Worth Systems.

using EventsHandler.Exceptions;
using EventsHandler.Extensions;
using EventsHandler.Mapping.Enums;
using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.DataProcessing.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataSending.Interfaces;
using Notify.Exceptions;
using ResourcesEnum = EventsHandler.Mapping.Enums.NotificatieApi.Resources;
using ResourcesText = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataProcessing
{
    /// <inheritdoc cref="IProcessingService{TModel}"/>
    internal sealed class NotifyProcessor : IProcessingService<NotificationEvent>
    {
        private readonly IScenariosResolver _resolver;
        private readonly INotifyService<NotificationEvent, NotifyData> _sender;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyProcessor"/> class.
        /// </summary>
        public NotifyProcessor(
            IScenariosResolver resolver,
            INotifyService<NotificationEvent, NotifyData> sender)
        {
            this._resolver = resolver;
            this._sender = sender;
        }

        /// <inheritdoc cref="IProcessingService{TModel}.ProcessAsync(TModel)"/>
        async Task<(ProcessingResult, string)> IProcessingService<NotificationEvent>.ProcessAsync(NotificationEvent notification)
        {
            try
            {
                // Determine if the received notification is "test" (ping) event => In this case, do nothing
                if (IsTest(notification))
                {
                    // NOTE: The notification SHOULD not be sent, but it's not a failure and shouldn't be retried
                    return (ProcessingResult.Skipped, ResourcesText.Processing_ERROR_Notification_Test);
                }

                // Choose an adequate business-scenario (strategy) to process the notification
                INotifyScenario scenario = await this._resolver.DetermineScenarioAsync(notification);  // TODO: If failure, return ProcessingResult here

                // Get data from external services (e.g., "OpenZaak", "OpenKlant", other APIs)
                NotifyData[] allNotifyData = await scenario.GetAllNotifyDataAsync(notification);  // TODO: If failure, return ProcessingResult here

                if (!allNotifyData.HasAny())
                {
                    // NOTE: The notification COULD not be sent due to missing or inconsistent data. Retry is necessary
                    return (ProcessingResult.Failure, ResourcesText.Processing_ERROR_Scenario_DataNotFound);
                }

                // Sending notifications
                foreach (NotifyData notifyData in allNotifyData)
                {
                    // Determine how to handle certain types of notifications by "Notify NL"
                    switch (notifyData.NotificationMethod)
                    {
                        case NotifyMethods.Email:
                        {
                            if ((await this._sender.SendEmailAsync(notification, notifyData)).IsSuccess)
                            {
                                break;
                            }

                            // NOTE: Something bad happened and "Notify NL" did not send the notification as expected
                            return (ProcessingResult.Failure, ResourcesText.Processing_ERROR_Scenario_NotificationNotSent);
                        }

                        case NotifyMethods.Sms:
                        {
                            if ((await this._sender.SendSmsAsync(notification, notifyData)).IsSuccess)
                            {
                                break;
                            }

                            // NOTE: Something bad happened and "Notify NL" did not send the notification as expected
                            return (ProcessingResult.Failure, ResourcesText.Processing_ERROR_Scenario_NotificationNotSent);
                        }

                        case NotifyMethods.None:
                        default:
                            // NOTE: NotifyMethods cannot be "None" or undefined
                            return (ProcessingResult.Failure, ResourcesText.Processing_ERROR_Scenario_NotificationNotSent);
                    }
                }

                // NOTE: The notification was sent and the completion status was reported to the telemetry API
                return (ProcessingResult.Success, ResourcesText.Processing_SUCCESS_Scenario_NotificationSent);
            }
            // TODO: Replace exception handling by (ProcessingResult result, string message) value tuple to further optimize the OMC workflow
            catch (TelemetryException exception)
            {
                // NOTE: The notification was sent, but the communication with the telemetry API failed. Do not retry
                return (ProcessingResult.Success, $"{ResourcesText.Processing_ERROR_Telemetry_CompletionNotSent} | {exception.Message}");
            }
            catch (NotImplementedException)
            {
                // NOTE: The notification COULD not be sent, but it's not a failure, and it shouldn't be retried
                return (ProcessingResult.Skipped, ResourcesText.Processing_ERROR_Scenario_NotImplemented);
            }
            catch (AbortedNotifyingException exception)
            {
                // NOTE: The notification SHOULD not be sent due to internal condition, and it shouldn't be retried
                return (ProcessingResult.Aborted, exception.Message);
            }
            catch (NotifyClientException exception)
            {
                // NOTE: The notification COULD not be sent because of issues with "Notify NL" (e.g., authorization or service being down)
                return (ProcessingResult.Failure, $"Notify NL Exception | {exception.Message}");
            }
            catch (Exception exception)
            {
                // NOTE: The notification COULD not be sent. Retry is necessary
                return (ProcessingResult.Failure, $"{exception.GetType().Name} | {exception.Message}");
            }
        }

        /// <summary>
        /// Determines whether the received <see cref="NotificationEvent"/> is just a "test" ping.
        /// </summary>
        private static bool IsTest(NotificationEvent notification)
        {
            const string testUrl = "http://some.hoofdobject.nl/";

            return notification is
            {
                Channel: Channels.Unknown,
                Resource: ResourcesEnum.Unknown
            } &&
            string.Equals(notification.MainObjectUri.AbsoluteUri, testUrl) &&
            string.Equals(notification.ResourceUri.AbsoluteUri, testUrl);
        }
    }
}