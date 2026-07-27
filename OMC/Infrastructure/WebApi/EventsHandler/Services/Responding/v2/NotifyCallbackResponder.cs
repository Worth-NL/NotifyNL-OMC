// © 2024, Worth Systems.

using Common.Models.Responses;
using Common.Settings.Configuration;
using EventsHandler.Controllers.Base;
using EventsHandler.Services.Responding.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.DataSending.Models.Reponses;
using WebQueries.Register.Interfaces;
using WebQueries.Tracing;
using ZgwModels.Enums;
using ZgwModels.Extensions;
using ZgwModels.Mapping.Models.POCOs.NotifyNL;
using ZgwModels.Serialization.Interfaces;

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
        private readonly TraceEmitter _traceEmitter;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyCallbackResponder"/> class.
        /// </summary>
        /// <param name="configuration">The configuration of the application.</param>
        /// <param name="serializer">The input de(serializing) service.</param>
        /// <param name="telemetry">The telemetry service registering API events.</param>
        /// <param name="notifyService"></param>
        /// <param name="traceEmitter">Broadcasts this callback's outcome to the dashboard, correlated back to the original trace via <see cref="NotifyReference.TraceId"/>.</param>
        public NotifyCallbackResponder(OmcConfiguration configuration, ISerializationService serializer, ITelemetryService telemetry, INotifyService<NotifyData> notifyService, TraceEmitter traceEmitter)  // Dependency Injection (DI)
            : base(serializer)
        {
            this._configuration = configuration;
            this._responder = this;  // NOTE: Shortcut to use interface methods faster ("NotifyResponder" parent derives from "IRespondingService<T>" interface)
            this._telemetry = telemetry;
            this._notifyService = notifyService;
            this._traceEmitter = traceEmitter;
        }

        /// <inheritdoc cref="GeneralResponder.HandleNotifyCallbackAsync(object)"/>
        internal override async Task<IActionResult> HandleNotifyCallbackAsync(object json)
        {
            try
            {
                DeliveryReceipt callback = Serializer.Deserialize<DeliveryReceipt>(json);
                FeedbackTypes status = callback.Status.ConvertToFeedbackStatus();

                if (string.IsNullOrEmpty(callback.Reference))
                {
                    return new AcceptedResult
                    {
                        Value = new { message = "No reference found. Unable to send telemetry." }
                    };
                }

                LogContactRegistration(callback, status);

                HttpRequestResponse? informResult = null;

                if (status is FeedbackTypes.Success or FeedbackTypes.Failure)
                {
                    (NotifyReference reference, NotifyMethods notificationMethod) = await ExtractCallbackDataAsync(callback);

                    EmitDeliveryConfirmationTrace(reference, notificationMethod, status);

                    informResult = await InformUserAboutStatusAsync(callback, reference, notificationMethod, status);
                }

                // If we have a telemetry result, base the HTTP response on that
                if (informResult != null)
                {
                    return new ObjectResult(new
                    {
                        success = informResult.Value.IsSuccess,
                        message = informResult.Value.JsonResponse
                    })
                    {
                        StatusCode = informResult.Value.IsFailure
                            ? StatusCodes.Status500InternalServerError
                            : StatusCodes.Status201Created
                    };
                }

                // Fallback when no telemetry call was made
                return new ObjectResult(new
                {
                    success = true,
                    message = "Callback processed successfully (no telemetry sent)."
                })
                {
                    StatusCode = StatusCodes.Status202Accepted
                };
            }
            catch (Exception ex)
            {
                // Centralized fallback for any unexpected errors
                var errorResponse = new
                {
                    statusCode = StatusCodes.Status500InternalServerError,
                    statusDescription = "[ERROR] Failed to process Notify callback",
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

        private async Task<HttpRequestResponse> InformUserAboutStatusAsync(
            DeliveryReceipt callback, NotifyReference reference, NotifyMethods notificationMethod, FeedbackTypes feedbackType)
        {
            NotificationData notificationData = await GetNotificationDataAsync(reference, notificationMethod, callback.Id);

            // Register the new status of the notification (for the user)
            return await _telemetry.ReportCompletionAsync(
                reference,
                notificationMethod,
                callback.Recipient,
                messages: [
                    DetermineUserMessageSubject(_configuration, feedbackType, notificationMethod,
                        notificationData.IsSuccess ? notificationData.Subject : string.Empty),
                    DetermineUserMessageBody(_configuration, feedbackType, notificationMethod,
                        notificationData.IsSuccess ? notificationData.Body : string.Empty),
                    feedbackType == FeedbackTypes.Success ? True : False,
                    notificationData.IsSuccess ? notificationData.SentAt : string.Empty
                ]);
        }

        /// <summary>
        /// Resolves the dashboard trace this delivery receipt belongs to (if any — older
        /// in-flight sends from before this correlation existed won't have a <see cref="NotifyReference.TraceId"/>)
        /// and reports the real outcome back to it: the channel-send stage that's been sitting
        /// in "pending" since the original scenario handed off to "Notify NL"
        /// (see EventsHandler.Services.DataProcessing.Strategy.Base.BaseScenario.ProcessDataAsync),
        /// and "contactmoment" right after it — this is genuinely the moment either of those
        /// resolve, not the synchronous send.
        /// </summary>
        private void EmitDeliveryConfirmationTrace(NotifyReference reference, NotifyMethods notificationMethod, FeedbackTypes feedbackType)
        {
            if (string.IsNullOrEmpty(reference.TraceId) || !this._traceEmitter.HasSubscribers)
            {
                return;
            }

            string channelStage = notificationMethod switch
            {
                NotifyMethods.Email => "notify-email",
                NotifyMethods.Sms => "notify-sms",
                NotifyMethods.Letter => "notify-post",
                _ => "kanaalresolutie"
            };
            string status = feedbackType == FeedbackTypes.Success ? "ok" : "fail";

            long elapsedMs = 0;
            string detail = "confirmed by Notify NL";
            if (reference.SentAtUnixMs is { } sentAtUnixMs)
            {
                elapsedMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - sentAtUnixMs;
                detail = $"confirmed by Notify NL after {TimeSpan.FromMilliseconds(elapsedMs):mm\\:ss}";
            }

            this._traceEmitter.Emit(new TraceEvent(reference.TraceId, channelStage, status, Scenario: null, Detail: detail, ElapsedMs: elapsedMs));
            this._traceEmitter.Emit(new TraceEvent(reference.TraceId, "contactmoment", status, Scenario: null, Detail: detail, ElapsedMs: elapsedMs));
        }

        private void LogContactRegistration(DeliveryReceipt callback, FeedbackTypes feedbackType)
        {
            try
            {
                ObjectResult response = this._responder.GetResponse(
                    feedbackType is FeedbackTypes.Success or FeedbackTypes.Info or FeedbackTypes.Failure
                        ? ProcessingResult.Success(GetDeliveryStatusLogMessage(callback))
                        : ProcessingResult.Failure(GetDeliveryStatusLogMessage(callback))
                );

                OmcController.LogApiResponse(
                    feedbackType == FeedbackTypes.Failure ? LogLevel.Error : LogLevel.Information,
                    response
                );
            }
            catch (Exception exception)
            {
                OmcController.LogApiResponse(
                    LogLevel.Error,
                    new ObjectResult(GetDeliveryErrorLogMessage(callback, exception))
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    }
                );
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
                            NotifyMethods.Letter => configuration.AppSettings.Variables.UxMessages.Letter_Success_Body(),
                            _ => string.Empty
                        }
                        : subject  // Use the subject from the original notification if available
                    ,

                FeedbackTypes.Failure =>
                    notificationMethod switch
                    {
                        NotifyMethods.Email => configuration.AppSettings.Variables.UxMessages.Email_Failure_Subject(),
                        NotifyMethods.Sms => configuration.AppSettings.Variables.UxMessages.SMS_Failure_Subject(),
                        NotifyMethods.Letter => configuration.AppSettings.Variables.UxMessages.Letter_Failure_Body(),
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