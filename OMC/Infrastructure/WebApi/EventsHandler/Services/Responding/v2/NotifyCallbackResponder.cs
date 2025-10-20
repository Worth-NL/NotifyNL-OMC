// © 2024, Worth Systems.

using Common.Extensions;
using Common.Models.Responses;
using Common.Settings.Configuration;
using EventsHandler.Controllers.Base;
using EventsHandler.Services.Responding.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Diagnostics;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.DataSending.Models.Reponses;
using WebQueries.Register.Interfaces;
using ZhvModels.Enums;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Models.POCOs.NotifyNL;
using ZhvModels.Serialization.Interfaces;

namespace EventsHandler.Services.Responding.v2
{
    /// <inheritdoc cref="GeneralResponder"/>
    /// <remarks>
    ///   Version: "OpenKlant" (2.0) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IRespondingService"/>
    internal sealed class NotifyCallbackResponder : GeneralResponder
    {
        private readonly OmcConfiguration _configuration;
        private readonly IRespondingService<ProcessingResult> _responder;
        private readonly ITelemetryService _telemetry;
        private readonly INotifyService<NotifyData> _notifyService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyCallbackResponder"/> class.
        /// </summary>
        /// <param name="configuration">The configuration of the application.</param>
        /// <param name="serializer">The input de(serializing) service.</param>
        /// <param name="telemetry">The telemetry service registering API events.</param>
        /// <param name="notifyService"></param>
        public NotifyCallbackResponder(OmcConfiguration configuration, ISerializationService serializer, ITelemetryService telemetry, INotifyService<NotifyData> notifyService)  // Dependency Injection (DI)
            : base(serializer)
        {
            this._configuration = configuration;
            this._responder = this;  // NOTE: Shortcut to use interface methods faster ("NotifyResponder" parent derives from "IRespondingService<T>" interface)
            this._telemetry = telemetry;
            this._notifyService = notifyService;
        }

        /// <inheritdoc cref="GeneralResponder.HandleNotifyCallbackAsync(object)"/>
        internal override async Task<IActionResult> HandleNotifyCallbackAsync(object json)
        {
            DeliveryReceipt callback;
            FeedbackTypes status;

            try
            {
                callback = this.Serializer.Deserialize<DeliveryReceipt>(json);
                status = callback.Status.ConvertToFeedbackStatus();

                if (status is FeedbackTypes.Success or FeedbackTypes.Failure)
                {
                    // Register contactmoment
                    try
                    {
                        await InformUserAboutStatusAsync(callback, status);
                    }
                    catch (Exception ex)
                    {
                        // You could also log this internally if you want to track partial failures
                        throw new Exception($"Failed to inform user about status for callback ID {callback.Id}: {ex.Message}", ex);
                    }
                }

                // Log the contact registration / response
                return LogContactRegistration(callback, status);
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    statusCode = 500,
                    statusDescription = $"[ERROR] Failed to process Notify callback",
                    details = new { message = ex.Message }
                };

                return new ObjectResult(errorResponse)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        #region Helper methods
        private const string True = "true";
        private const string False = "false";

        private async Task InformUserAboutStatusAsync(DeliveryReceipt callback, FeedbackTypes feedbackType)
        {
            (NotifyReference reference, NotifyMethods notificationMethod) = await ExtractCallbackDataAsync(callback);

            NotificationData notificationData = await GetNotificationDataAsync(reference, notificationMethod, callback.Id);

            // Registering new status of the notification (for user)
            var result = await this._telemetry.ReportCompletionAsync(reference, notificationMethod, callback.Recipient, messages:
            [
                // User message subject
                DetermineUserMessageSubject(this._configuration, feedbackType, notificationMethod, notificationData.IsSuccess ? notificationData.Subject : string.Empty),

                // User message body
                DetermineUserMessageBody(this._configuration, feedbackType, notificationMethod, notificationData.IsSuccess ? notificationData.Body : string.Empty),

                // Is successfully sent
                feedbackType == FeedbackTypes.Success ? True : False,

                // Sent Timestamp
                notificationData.IsSuccess ? notificationData.SentAt : String.Empty
            ]);

            if (result.IsFailure)
            {
                throw new Exception(result.JsonResponse);
            }
        }

        private ObjectResult LogContactRegistration(DeliveryReceipt callback, FeedbackTypes feedbackType)
        {
            try
            {
                var response = this._responder.GetResponse(
                    feedbackType is FeedbackTypes.Success or FeedbackTypes.Info or FeedbackTypes.Failure
                        ? ProcessingResult.Success(GetDeliveryStatusLogMessage(callback))
                        : ProcessingResult.Failure(GetDeliveryStatusLogMessage(callback))
                );

                // Extract HttpRequestResponse if wrapped
                if (response is ObjectResult objResult && objResult.Value is HttpRequestResponse r && r.IsFailure)
                {
                    return new ObjectResult(r.JsonResponse)
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
                }

                return OmcController.LogApiResponse(
                    feedbackType == FeedbackTypes.Failure ? LogLevel.Error : LogLevel.Information,
                    response
                );
            }
            catch (Exception exception)
            {
                return new ObjectResult(GetDeliveryErrorLogMessage(callback, exception))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        private static string DetermineUserMessageSubject(
            OmcConfiguration configuration, FeedbackTypes feedbackType, NotifyMethods notificationMethod, string subject)
        {
            return feedbackType switch
            {
                FeedbackTypes.Success =>
                    subject == string.Empty
                        ? notificationMethod switch
                        {
                            NotifyMethods.Email => configuration.AppSettings.Variables.UxMessages.Email_Success_Subject(),
                            NotifyMethods.Sms => configuration.AppSettings.Variables.UxMessages.SMS_Success_Subject(),
                            _ => string.Empty
                        }
                        : subject  // Use the subject from the original notification if available
                    ,

                FeedbackTypes.Failure =>
                    notificationMethod switch
                    {
                        NotifyMethods.Email => configuration.AppSettings.Variables.UxMessages.Email_Failure_Subject(),
                        NotifyMethods.Sms => configuration.AppSettings.Variables.UxMessages.SMS_Failure_Subject(),
                        _ => string.Empty
                    },

                _ => string.Empty
            };
        }

        private static string DetermineUserMessageBody(
            OmcConfiguration configuration, FeedbackTypes feedbackType, NotifyMethods notificationMethod, string body)
        {
            return feedbackType switch
            {
                FeedbackTypes.Success =>
                    body == string.Empty
                        ? notificationMethod switch
                        {
                            NotifyMethods.Email => configuration.AppSettings.Variables.UxMessages.Email_Success_Body(),
                            NotifyMethods.Sms => configuration.AppSettings.Variables.UxMessages.SMS_Success_Body(),
                            _ => string.Empty
                        }
                        : body  // Use the body from the original notification if available
                ,
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

        private async Task<NotificationData> GetNotificationDataAsync(
            NotifyReference reference, NotifyMethods notificationMethod, Guid notificationId)
        {
            var data = new NotifyData(notificationMethod, String.Empty, Guid.Empty, [], reference);
            return await this._notifyService.GetNotificationDataAsync(data, notificationId);
        }
        #endregion
    }
}