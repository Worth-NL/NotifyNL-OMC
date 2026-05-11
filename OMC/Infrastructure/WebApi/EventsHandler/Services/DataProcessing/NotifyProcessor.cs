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
using EventsHandler.Services.DataProcessing.Strategy.Implementations.Kto;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.KTO.Interfaces;
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
        private readonly IKtoScenarioFactory _ktoScenarioFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyProcessor"/> class.
        /// </summary>
        /// <param name="serializer">The input de(serializing) service.</param>
        /// <param name="validator">The input validating service.</param>
        /// <param name="resolver">The strategies resolving service.</param>
        /// <param name="ktoScenarioFactory">The strategy to send Kto</param>
        public NotifyProcessor(
            ISerializationService serializer,
            IValidationService<NotificationEvent> validator,
            IScenariosResolver<INotifyScenario, NotificationEvent> resolver, IKtoScenarioFactory ktoScenarioFactory)  // Dependency Injection (DI)
        {
            this._serializer = serializer;
            this._validator = validator;
            this._resolver = resolver;
            this._ktoScenarioFactory = ktoScenarioFactory;
        }

        /// <inheritdoc cref="IProcessingService.ProcessAsync(object)"/>
        async Task<ProcessingResult> IProcessingService.ProcessAsync(object json)
        {
            BaseEnhancedDetails details = InfoDetails.Empty;

            try
            {
                // Step 1: Convert incoming object to JsonElement for inspection
                JsonElement jsonElement = json is JsonElement je
                    ? je
                    : JsonDocument.Parse(JsonSerializer.Serialize(json)).RootElement;

                // Step 2: Try to extract the actual payload if this is a CloudEvent wrapper
                JsonElement? actualPayload = TryExtractPayloadFromCloudEvent(jsonElement);
                if (actualPayload == null)
                {
                    // CloudEvent without 'data' – nothing to process
                    return ProcessingResult.Skipped(
                        "Received CloudEvent missing required 'data' property.", json, details);
                }

                // Step 3: Deserialize the (possibly extracted) payload into the expected NotificationEvent
                NotificationEvent notification = this._serializer.Deserialize<NotificationEvent>(actualPayload.Value);
                details = notification.Details;

                // Step 4: Validate deserialized notification
                if (this._validator.Validate(ref notification) is HealthCheck.ERROR_Invalid)
                {
                    // STOP: The notification is incomplete
                    return ProcessingResult.NotPossible(
                        ZhvResources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Message,
                        json, notification.Details);
                }

                // Step 5: Ping/test detection – silently skip
                if (IsTest(notification))
                {
                    return ProcessingResult.Skipped(
                        ApiResources.Processing_ERROR_Notification_Test, json, details);
                }

                // Step 6: Determine business scenario
                INotifyScenario scenario = await this._resolver.DetermineScenarioAsync(notification);

                // Step 7: Special handling for Kto scenario (Customer satisfaction survey)
                if (scenario is KtoScenario)
                {
                    try
                    {
                        WebQueries.KTO.Models.KtoScenario ktoScenario = _ktoScenarioFactory.Create();
                        HttpRequestResponse ktoResponse = await ktoScenario.SendKtoAsync(notification);

                        return ktoResponse.IsFailure
                            ? ProcessingResult.Failure(ktoResponse.JsonResponse, json, details)
                            : ProcessingResult.Success("Successfully sent KTO to KTO provider", json, details);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }

                // Step 8: For all other scenarios – query external data (OpenZaak, etc.)
                QueryingDataResponse queryDataResponse;

                if ((queryDataResponse = await scenario.TryGetDataAsync(notification)).IsFailure)
                {
                    string message = string.Format(
                        ApiResources.Processing_ERROR_Scenario_NotificationNotSent,
                        queryDataResponse.Message);

                    return ProcessingResult.Failure(message, json, details);
                }

                // Step 9: Process the data (e.g., send to Notify NL)
                ProcessingDataResponse processingDataResponse = await scenario.ProcessDataAsync(notification, queryDataResponse.Content);

                return processingDataResponse.IsFailure
                    ? ProcessingResult.Failure(
                        string.Format(ApiResources.Processing_ERROR_Scenario_NotificationNotSent,
                            processingDataResponse.Message), json, details)
                    : ProcessingResult.Success(
                        ApiResources.Processing_SUCCESS_Scenario_NotificationSent, json, details);
            }
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

        private JsonElement? TryExtractPayloadFromCloudEvent(JsonElement potentialCloudEvent)
        {
            // Minimal CloudEvent required attributes (CNCF specification)
            bool hasSpecVersion = potentialCloudEvent.TryGetProperty("specversion", out _);
            bool hasType = potentialCloudEvent.TryGetProperty("type", out _);
            bool hasSource = potentialCloudEvent.TryGetProperty("source", out _);
            bool hasId = potentialCloudEvent.TryGetProperty("id", out _);

            if (hasSpecVersion && hasType && hasSource && hasId)
            {
                // This is a CloudEvent – extract the 'data' property
                if (potentialCloudEvent.TryGetProperty("data", out JsonElement dataElement))
                {
                    return dataElement;
                }
                // CloudEvent without data – cannot proceed
                return null;
            }

            // Not a CloudEvent – return the whole payload as-is
            return potentialCloudEvent;
        }
        #endregion
    }
}