// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Enums;
using EventsHandler.Behaviors.Mapping.Enums.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Exceptions;
using EventsHandler.Extensions;
using EventsHandler.Services.DataProcessing.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using Notify.Exceptions;
using ResourcesEnum = EventsHandler.Behaviors.Mapping.Enums.NotificatieApi.Resources;
using ResourcesText = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataProcessing
{
    /// <inheritdoc cref="IProcessingService{TModel}"/>
    internal sealed class NotifyProcessor : IProcessingService<NotificationEvent>
    {
        private readonly IScenariosResolver _resolver;
        private readonly ISendingService<NotificationEvent, NotifyData> _sender;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyProcessor"/> class.
        /// </summary>
        public NotifyProcessor(
            IScenariosResolver resolver,
            ISendingService<NotificationEvent, NotifyData> sender)
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
                INotifyScenario scenario = await this._resolver.DetermineScenarioAsync(notification);

                // Get data from external services (e.g., "OpenZaak", "OpenKlant", other APIs)
                NotifyData[] allNotifyData = await scenario.GetAllNotifyDataAsync(notification);

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
                            await this._sender.SendEmailAsync(notification, notifyData);
                            break;

                        case NotifyMethods.Sms:
                            await this._sender.SendSmsAsync(notification, notifyData);
                            break;

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
                   string.Equals(notification.MainObject.AbsoluteUri, testUrl) &&
                   string.Equals(notification.ResourceUrl.AbsoluteUri, testUrl);
        }
    }
}