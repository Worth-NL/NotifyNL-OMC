// © 2024, Worth Systems.

using Common.Models.Responses;
using Common.Settings.Configuration;
using EventsHandler.Controllers.Base;
using EventsHandler.Services.Responding.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.Register.Interfaces;
using ZhvModels.Enums;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Enums.NotifyNL;
using ZhvModels.Mapping.Models.POCOs.NotifyNL;
using ZhvModels.Serialization.Interfaces;

namespace EventsHandler.Services.Responding.v1
{
    /// <inheritdoc cref="NotifyResponder"/>
    /// <remarks>
    ///   Version: "OpenKlant" (1.0) Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="IRespondingService{TResult}"/>
    internal sealed class NotifyCallbackResponder : NotifyResponder
    {
        private readonly WebApiConfiguration _configuration;
        private readonly IRespondingService<ProcessingResult> _responder;
        private readonly ITelemetryService _telemetry;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyCallbackResponder"/> class.
        /// </summary>
        /// <param name="configuration">The configuration of the application.</param>
        /// <param name="serializer">The input de(serializing) service.</param>
        /// <param name="telemetry">The telemetry service registering API events.</param>
        public NotifyCallbackResponder(WebApiConfiguration configuration, ISerializationService serializer, ITelemetryService telemetry)  // Dependency Injection (DI)
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
                        this._responder.GetResponse(ProcessingResult.Success(GetDeliveryStatusLogMessage(callback))))

                    // Failure status was returned by Notify NL
                    : OmcController.LogApiResponse(LogLevel.Error,
                        this._responder.GetResponse(ProcessingResult.Failure(GetDeliveryStatusLogMessage(callback))));
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
                HttpRequestResponse response = await this._telemetry.ReportCompletionAsync(reference, notificationMethod, messages:
                [
                    // User message body
                    DetermineUserMessageBody(this._configuration, feedbackType, notificationMethod)

                ]);

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