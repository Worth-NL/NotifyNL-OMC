// © 2024, Worth Systems.

using EventsHandler.Controllers.Base;
using EventsHandler.Extensions;
using EventsHandler.Mapping.Enums;
using EventsHandler.Mapping.Enums.NotifyNL;
using EventsHandler.Mapping.Models.POCOs.NotifyNL;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataSending.Responses;
using EventsHandler.Services.Register.Interfaces;
using EventsHandler.Services.Responding.Enums.v2;
using EventsHandler.Services.Responding.Interfaces;
using EventsHandler.Services.Serialization.Interfaces;
using EventsHandler.Services.Settings.Configuration;
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
        private readonly WebApiConfiguration _configuration;
        private readonly IRespondingService<ProcessingStatus, string> _responder;
        private readonly ITelemetryService _telemetry;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyCallbackResponder"/> class.
        /// </summary>
        /// <param name="configuration">The configuration of the application.</param>
        /// <param name="serializer">The input de(serializing) service.</param>
        /// <param name="telemetry">The telemetry service registering API events.</param>
        public NotifyCallbackResponder(WebApiConfiguration configuration, ISerializationService serializer, ITelemetryService telemetry)
            : base(serializer)
        {
            this._configuration = configuration;
            this._responder = this;  // NOTE: Shortcut to use interface methods faster (parent of this class derives from this interface)
            this._telemetry = telemetry;
        }

        /// <inheritdoc cref="NotifyResponder.HandleNotifyCallbackAsync(object)"/>
        internal override async Task<IActionResult> HandleNotifyCallbackAsync(object json)
        {
            DeliveryReceipt callback = DeliveryReceipt.Default;
            FeedbackTypes status = FeedbackTypes.Unknown;

            try
            {
                // Deserialize received JSON payload
                callback = this.Serializer.Deserialize<DeliveryReceipt>(json);
                status = callback.Status.ConvertToFeedbackStatus();

                return callback.Status is not (DeliveryStatuses.PermanentFailure or
                                               DeliveryStatuses.TemporaryFailure or
                                               DeliveryStatuses.TechnicalFailure)

                    // Positive status was returned by Notify NL
                    ? OmcController.LogApiResponse(LogLevel.Information,
                        this._responder.GetResponse(ProcessingStatus.Success, GetDeliveryStatusLogMessage(callback)))

                    // Failure status was returned by Notify NL
                    : OmcController.LogApiResponse(LogLevel.Error,
                        this._responder.GetResponse(ProcessingStatus.Failure, GetDeliveryStatusLogMessage(callback)));
            }
            catch (Exception exception)
            {
                return OmcController.LogApiResponse(exception,
                    this._responder.GetExceptionResponse(exception));
            }
            finally
            {
                await LogContactRegistrationAsync(callback, status);
            }
        }

        #region Helper methods
        private async Task LogContactRegistrationAsync(DeliveryReceipt callback, FeedbackTypes feedbackType)
        {
            try
            {
                (NotifyReference reference, NotifyMethods notificationMethod) = await ExtractCallbackDataAsync(callback);
                RequestResponse response = await this._telemetry.ReportCompletionAsync(reference, notificationMethod, messages:
                    new []
                    {
                        // User message body
                        DetermineUserMessageBody(this._configuration, feedbackType, notificationMethod),

                    });

                OmcController.LogApiResponse(LogLevel.Information, response.JsonResponse);
            }
            catch (Exception exception)
            {
                // It wasn't possible to report completion because of issue with Telemetry Service
                OmcController.LogApiResponse(exception,
                    this._responder.GetExceptionResponse(
                        GetDeliveryErrorLogMessage(callback, exception)));
            }
        }

        private static string DetermineUserMessageBody(
            WebApiConfiguration configuration, FeedbackTypes feedbackType, NotifyMethods notificationMethod)
        {
            return feedbackType switch
            {
                FeedbackTypes.Success =>
                    notificationMethod switch
                    {
                        NotifyMethods.Email => configuration.AppSettings.Variables.UxMessages.Email_Success_Body(),
                        NotifyMethods.Sms => configuration.AppSettings.Variables.UxMessages.SMS_Success_Body(),
                        _ => string.Empty
                    },

                FeedbackTypes.Failure =>
                    notificationMethod switch
                    {
                        NotifyMethods.Email => configuration.AppSettings.Variables.UxMessages.Email_Failure_Body(),
                        NotifyMethods.Sms => configuration.AppSettings.Variables.UxMessages.SMS_Failure_Body(),
                        _ => string.Empty
                    },

                _ => string.Empty
            };
        }
        #endregion
    }
}