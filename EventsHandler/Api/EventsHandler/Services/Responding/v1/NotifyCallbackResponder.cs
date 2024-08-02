// © 2024, Worth Systems.

using EventsHandler.Controllers.Base;
using EventsHandler.Mapping.Enums;
using EventsHandler.Mapping.Enums.NotifyNL;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.NotifyNL;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.Responding.Interfaces;
using EventsHandler.Services.Serialization.Interfaces;
using EventsHandler.Services.Telemetry.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventsHandler.Services.Responding.v1
{
    /// <inheritdoc cref="NotifyResponder"/>
    /// <remarks>
    ///   Version: "OpenKlant" (1.0) Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="IRespondingService{TResult, TDetails}"/>
    internal sealed class NotifyCallbackResponder : NotifyResponder
    {
        private readonly IRespondingService<ProcessingResult, string> _responder;
        private readonly ITelemetryService _telemetry;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyCallbackResponder"/> class.
        /// </summary>
        /// <param name="serializer">The input de(serializing) service.</param>
        /// <param name="telemetry">The telemetry service registering API events.</param>
        public NotifyCallbackResponder(ISerializationService serializer, ITelemetryService telemetry)
            : base(serializer)
        {
            this._responder = this;  // NOTE: Shortcut to use interface methods faster (parent of this class derives from this interface)
            this._telemetry = telemetry;
        }

        /// <inheritdoc cref="NotifyResponder.HandleNotifyCallbackAsync(object)"/>
        internal override async Task<IActionResult> HandleNotifyCallbackAsync(object json)
        {
            DeliveryReceipt callback = DeliveryReceipt.Default;
            string callbackDetails = string.Empty;

            try
            {
                // Deserialize received JSON payload
                callback = this.Serializer.Deserialize<DeliveryReceipt>(json);

                return callback.Status is not (DeliveryStatuses.PermanentFailure or
                                               DeliveryStatuses.TemporaryFailure or
                                               DeliveryStatuses.TechnicalFailure)

                    // Positive status was returned by Notify NL
                    ? OmcController.LogApiResponse(LogLevel.Information,
                        this._responder.Get_Processing_Status_ActionResult(ProcessingResult.Success, callbackDetails = GetDeliveryStatusLogMessage(callback)))

                    // Failure status was returned by Notify NL
                    : OmcController.LogApiResponse(LogLevel.Error,
                        this._responder.Get_Processing_Status_ActionResult(ProcessingResult.Failure, callbackDetails = GetDeliveryStatusLogMessage(callback)));
            }
            catch (Exception exception)
            {
                // NOTE: If callback.Id == Guid.Empty then it might be suspected that exception occurred during DeliveryReceipt deserialization
                callbackDetails = GetDeliveryErrorLogMessage(callback, exception);

                return OmcController.LogApiResponse(exception,
                    this._responder.Get_Exception_ActionResult(exception));
            }
            finally
            {
                await LogContactRegistrationAsync(callback, callbackDetails);
            }
        }

        #region Helper methods
        private async Task LogContactRegistrationAsync(DeliveryReceipt callback, string callbackDetails)
        {
            try
            {
                (NotificationEvent notification, NotifyMethods notificationMethod) = ExtractNotificationData(callback);

                OmcController.LogApiResponse(LogLevel.Information,
                    await this._telemetry.ReportCompletionAsync(notification, notificationMethod, callbackDetails));
            }
            catch (Exception exception)
            {
                // It wasn't possible to report completion because of issue with Telemetry Service or Reference
                OmcController.LogApiResponse(exception,
                    this._responder.Get_Exception_ActionResult(exception));
            }
        }
        #endregion
    }
}