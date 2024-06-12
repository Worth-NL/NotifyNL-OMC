// © 2024, Worth Systems.

using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Mapping.Enums;
using EventsHandler.Behaviors.Mapping.Enums.NotifyNL;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotifyNL;
using EventsHandler.Controllers.Base;
using EventsHandler.Extensions;
using EventsHandler.Properties;
using EventsHandler.Services.Serialization.Interfaces;
using EventsHandler.Services.Telemetry.Interfaces;
using EventsHandler.Services.UserCommunication.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventsHandler.Services.UserCommunication.v1
{
    /// <inheritdoc cref="NotifyResponder"/>
    /// <remarks>
    ///   Version: "OpenKlant" (1.0) Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="IRespondingService{TResult, TDetails}"/>
    internal sealed class NotifyCallbackResponder : NotifyResponder
    {
        private readonly ISerializationService _serializer;
        private readonly IRespondingService<ProcessingResult, string> _responder;
        private readonly ITelemetryService _telemetry;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyCallbackResponder"/> class.
        /// </summary>
        /// <param name="serializer">The input de(serializing) service.</param>
        /// <param name="telemetry">The telemetry service registering API events.</param>
        public NotifyCallbackResponder(ISerializationService serializer, ITelemetryService telemetry)
        {
            this._serializer = serializer;
            this._responder = this;  // NOTE: Shortcut to use interface methods faster (parent of this class derives from this interface)
            this._telemetry = telemetry;
        }

        internal override async Task<IActionResult> HandleNotifyCallbackAsync(object json)
        {
            DeliveryReceipt callback = DeliveryReceipt.Default;
            string callbackDetails = string.Empty;

            try
            {
                // Deserialize received JSON payload
                callback = this._serializer.Deserialize<DeliveryReceipt>(json);

                return callback.Statuses is not (DeliveryStatuses.PermanentFailure or
                                                 DeliveryStatuses.TemporaryFailure or
                                                 DeliveryStatuses.TechnicalFailure)

                    // Positive status was returned by Notify NL
                    ? OmcController.LogApiResponse(LogLevel.Information,
                        this._responder.Get_Processing_Status_ActionResult(ProcessingResult.Success, callbackDetails = GetCallbackDetails(callback)))

                    // Failure status was returned by Notify NL
                    : OmcController.LogApiResponse(LogLevel.Error,
                        this._responder.Get_Processing_Status_ActionResult(ProcessingResult.Failure, callbackDetails = GetCallbackDetails(callback)));
            }
            catch (Exception exception)
            {
                // NOTE: If callback.Id == Guid.Empty then it might be suspected that exception occurred during DeliveryReceipt deserialization
                callbackDetails = GetErrorDetails(callback, exception);

                return OmcController.LogApiResponse(exception,
                    this._responder.Get_Exception_ActionResult(exception));
            }
            finally
            {
                await LogContactRegistrationAsync(callback, callbackDetails);
            }
        }

        #region Helper methods
        private static string GetCallbackDetails(DeliveryReceipt callback)
        {
            return $"{Resources.Feedback_NotifyNL_SUCCESS_NotificationStatus} {callback.Id}: {callback.Statuses}.";
        }

        private static string GetErrorDetails(DeliveryReceipt callback, Exception exception)
        {
            return $"{Resources.Feedback_NotifyNL_ERROR_UnexpectedFailure} {callback.Id}: {exception.Message}.";
        }
        
        private async Task LogContactRegistrationAsync(DeliveryReceipt callback, string callbackDetails)
        {
            if (callback.Reference != null)
            {
                string decodedNotification = callback.Reference.Base64Decode();
                NotificationEvent notification = this._serializer.Deserialize<NotificationEvent>(decodedNotification);
                NotifyMethods notificationMethod = callback.Type.ConvertToNotifyMethod();

                try
                {
                    OmcController.LogApiResponse(LogLevel.Information,
                        await this._telemetry.ReportCompletionAsync(notification, notificationMethod, callbackDetails));
                }
                catch (Exception exception)
                {
                    // It wasn't possible to report completion because of issue with Telemetry Service
                    OmcController.LogApiResponse(exception,
                        this._responder.Get_Exception_ActionResult(exception));
                }
            }
        }
        #endregion
    }
}