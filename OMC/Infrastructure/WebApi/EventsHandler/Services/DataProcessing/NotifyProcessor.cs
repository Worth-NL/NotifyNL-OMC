// © 2023, Worth Systems.

using Common.Models.Messages.Details;
using Common.Models.Messages.Details.Base;
using Common.Models.Responses;
using EventsHandler.Exceptions;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Interfaces;
using EventsHandler.Services.DataProcessing.Models.Responses;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces;
using EventsHandler.Services.Validation.Interfaces;
using Notify.Exceptions;
using System.Text.Json;
using WebQueries.DataQuerying.Models.Responses;
using ZhvModels.Enums;
using ZhvModels.Mapping.Enums.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Properties;
using ZhvModels.Serialization.Interfaces;

namespace EventsHandler.Services.DataProcessing
{
    /// <inheritdoc cref="IProcessingService"/>
    internal sealed class NotifyProcessor : IProcessingService
    {
        private readonly ISerializationService _serializer;
        private readonly IValidationService<NotificationEvent> _validator;
        private readonly IScenariosResolver<INotifyScenario, NotificationEvent> _resolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyProcessor"/> class.
        /// </summary>
        /// <param name="serializer">The input de(serializing) service.</param>
        /// <param name="validator">The input validating service.</param>
        /// <param name="resolver">The strategies resolving service.</param>
        public NotifyProcessor(
            ISerializationService serializer,
            IValidationService<NotificationEvent> validator,
            IScenariosResolver<INotifyScenario, NotificationEvent> resolver)  // Dependency Injection (DI)
        {
            this._serializer = serializer;
            this._validator = validator;
            this._resolver = resolver;
        }

        /// <inheritdoc cref="IProcessingService.ProcessAsync(object)"/>
        async Task<ProcessingResult> IProcessingService.ProcessAsync(object json)
        {
            BaseEnhancedDetails details = InfoDetails.Empty;

            try
            {
                // Deserialize received JSON payload
                NotificationEvent notification = this._serializer.Deserialize<NotificationEvent>(json);
                details = notification.Details;

                // Validate deserialized notification to check if it's sufficiently complete to be processed
                if (this._validator.Validate(ref notification) is HealthCheck.ERROR_Invalid)
                {
                    // STOP: The notification is not complete; any further processing of it would be pointless
                    return ProcessingResult.NotPossible(ZhvResources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Message, json, notification.Details);
                }

                // Determine if the received notification is "test" (ping) event => In this case, do nothing
                if (IsTest(notification))
                {
                    // STOP: The notification SHOULD not be sent; it's just a connectivity test not a failure
                    return ProcessingResult.Skipped(ApiResources.Processing_ERROR_Notification_Test, json, details);
                }

                // Choose an adequate business-scenario (strategy) to process the notification
                INotifyScenario scenario = await this._resolver.DetermineScenarioAsync(notification);  // TODO: If failure, return ProcessingResult here (response pattern)

                // Get data from external services (e.g., "OpenZaak", "OpenKlant", other APIs)
                QueryingDataResponse queryDataResponse;

                if ((queryDataResponse = await scenario.TryGetDataAsync(notification)).IsFailure)
                {
                    string message = string.Format(ApiResources.Processing_ERROR_Scenario_NotificationNotSent, queryDataResponse.Message);

                    // RETRY: The notification COULD not be sent due to missing or inconsistent data
                    return ProcessingResult.Failure(message, json, details);
                }

                // Processing the prepared data in a specific way (e.g., sending to "Notify NL")
                ProcessingDataResponse processingDataResponse = await scenario.ProcessDataAsync(notification, queryDataResponse.Content);

                return processingDataResponse.IsFailure
                    // RETRY: Something bad happened and "Notify NL" did not send the notification as expected
                    ? ProcessingResult.Failure(
                        string.Format(ApiResources.Processing_ERROR_Scenario_NotificationNotSent, processingDataResponse.Message), json, details)

                    // SUCCESS: The notification was sent and the completion status was reported to the telemetry API
                    : ProcessingResult.Success(ApiResources.Processing_SUCCESS_Scenario_NotificationSent, json, details);
            }
            // Handling errors in a specific way depends on their types or severities
            catch (Exception exception)
            {
                return HandleException(exception, json, details);
            }
        }

        #region Helper methods
        /// <summary>
        /// Determines whether the received <see cref="NotificationEvent"/> is just a "test" ping.
        /// </summary>
        private static bool IsTest(NotificationEvent notification)
        {
            const string testUrl = "http://some.hoofdobject.nl/";

            return notification is
            {
                Channel: Channels.Unknown,
                Resource: Resources.Unknown
            } &&
            string.Equals(notification.MainObjectUri.AbsoluteUri, testUrl) &&
            string.Equals(notification.ResourceUri.AbsoluteUri, testUrl);
        }

        private static ProcessingResult HandleException(Exception exception, object json, BaseEnhancedDetails details)
        {
            return exception switch
            {
                // STOP: The JSON payload COULD not be deserialized; any further processing of it would be pointless
                JsonException => ProcessingResult.Skipped(exception.Message, json, details),

                // STOP: The notification COULD not be sent, but it's not a failure
                NotImplementedException => ProcessingResult.Skipped(ApiResources.Processing_ERROR_Scenario_NotImplemented, json, details),

                // STOP: The notification SHOULD not be sent due to internal condition
                AbortedNotifyingException => ProcessingResult.Aborted(exception.Message, json, details),

                // RETRY: The notification COULD not be sent because of issues with "Notify NL" (e.g., authorization or service being down)
                NotifyClientException => ProcessingResult.Failure(
                    string.Format(ApiResources.Processing_ERROR_Exception_Notify, exception.Message), json, details),

                // RETRY: The notification COULD not be sent
                _ => ProcessingResult.Failure(
                    string.Format(ApiResources.Processing_ERROR_Exception_Unhandled, exception.GetType().Name, exception.Message), json, details)
            };
        }
        #endregion
    }
}