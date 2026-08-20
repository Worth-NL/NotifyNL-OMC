// © 2024, Worth Systems.

using Common.Models.Responses;
using Common.Settings.Configuration;
using EventsHandler.Controllers.Base;
using EventsHandler.Services.Responding.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.DataSending.Models.Reponses;
using WebQueries.MOBB.Interfaces;
using WebQueries.MOBB.Models;
using WebQueries.Register.Interfaces;
using WebQueries.Tracing;
using ZgwModels.Enums;
using ZgwModels.Extensions;
using ZgwModels.Mapping.Enums.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
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
        private readonly IMessageBoxScenario _messageBoxScenario;
        private readonly TraceEmitter _traceEmitter;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyCallbackResponder"/> class.
        /// </summary>
        /// <param name="configuration">The configuration of the application.</param>
        /// <param name="serializer">The input de(serializing) service.</param>
        /// <param name="telemetry">The telemetry service registering API events.</param>
        /// <param name="notifyService"></param>
        /// <param name="messageBoxScenario">Used to continue the MOBB fallback chain when a delivery-receipt callback reports failure.</param>
        /// <param name="traceEmitter">Broadcasts this callback's outcome to the dashboard, correlated back to the original trace via <see cref="NotifyReference.TraceId"/>.</param>
        public NotifyCallbackResponder(OmcConfiguration configuration, ISerializationService serializer, ITelemetryService telemetry, INotifyService<NotifyData> notifyService, IMessageBoxScenario messageBoxScenario, TraceEmitter traceEmitter)  // Dependency Injection (DI)
            : base(serializer)
        {
            this._configuration = configuration;
            this._responder = this;  // NOTE: Shortcut to use interface methods faster ("NotifyResponder" parent derives from "IRespondingService<T>" interface)
            this._telemetry = telemetry;
            this._notifyService = notifyService;
            this._messageBoxScenario = messageBoxScenario;
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
                    // First-version draft: MOBB/Berichtenbox callbacks carry a MessageBoxNotifyReference
                    // instead of the standard NotifyReference, so they need their own contactmoment path.
                    if (await IsMessageBoxCallbackAsync(callback))
                    {
                        informResult = await InformUserAboutMessageBoxStatusAsync(callback, status);
                    }
                    else
                    {
                        (NotifyReference reference, NotifyMethods notificationMethod) = await ExtractCallbackDataAsync(callback);

                        EmitChannelConfirmationTrace(reference, notificationMethod, status);

                        HttpRequestResponse contactMomentResult = await InformUserAboutStatusAsync(callback, reference, notificationMethod, status);
                        EmitContactMomentTrace(reference, contactMomentResult);

                        informResult = contactMomentResult;
                    }
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
        /// and reports the channel-send stage's real outcome back to it — the stage that's been
        /// sitting in "pending" since the original scenario handed off to "Notify NL" (see
        /// EventsHandler.Services.DataProcessing.Strategy.Base.BaseScenario.ProcessDataAsync).
        /// This is genuinely the moment Notify NL confirms delivery, not the synchronous send.
        /// </summary>
        private void EmitChannelConfirmationTrace(NotifyReference reference, NotifyMethods notificationMethod, FeedbackTypes feedbackType)
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
            (long elapsedMs, string detail) = DescribeElapsedSinceSend(reference.SentAtUnixMs, "confirmed by Notify NL");

            this._traceEmitter.Emit(new TraceEvent(reference.TraceId, channelStage, status, Scenario: null, Detail: detail, ElapsedMs: elapsedMs));
        }

        /// <summary>
        /// Reports the "contactmoment" stage's real outcome, based on whether the write to
        /// Contactmomenten/Klantinteracties itself succeeded — deliberately independent of the
        /// channel-send stage's status above: Notify NL confirming delivery doesn't guarantee
        /// the contactmoment write also succeeded, and a failed send still gets its own
        /// "delivery failed" message registered as a (successful) contactmoment.
        /// </summary>
        private void EmitContactMomentTrace(NotifyReference reference, HttpRequestResponse contactMomentResult)
        {
            if (string.IsNullOrEmpty(reference.TraceId) || !this._traceEmitter.HasSubscribers)
            {
                return;
            }

            string status = contactMomentResult.IsSuccess ? "ok" : "fail";
            (long elapsedMs, string detail) = DescribeElapsedSinceSend(reference.SentAtUnixMs,
                contactMomentResult.IsSuccess ? "contactmoment registered" : $"failed to register contactmoment: {contactMomentResult.JsonResponse}");

            this._traceEmitter.Emit(new TraceEvent(reference.TraceId, "contactmoment", status, Scenario: null, Detail: detail, ElapsedMs: elapsedMs));
        }

        /// <summary>
        /// Computes how long it's been since the original scenario handed this notification off
        /// to "Notify NL" (if that moment was recorded), appending the elapsed time to
        /// <paramref name="outcomeText"/> for the trace's detail message.
        /// </summary>
        private static (long ElapsedMs, string Detail) DescribeElapsedSinceSend(long? sentAtUnixMs, string outcomeText)
        {
            if (sentAtUnixMs is not { } sentAt)
            {
                return (0, outcomeText);
            }

            long elapsedMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - sentAt;
            return (elapsedMs, $"{outcomeText} after {TimeSpan.FromMilliseconds(elapsedMs):mm\\:ss}");
        }

        /// <summary>
        /// MOBB / Berichtenbox counterpart of <see cref="InformUserAboutStatusAsync"/>.
        /// </summary>
        /// <remarks>
        ///   On a failure callback, this also continues the MOBB -> digitale-post -> letter fallback
        ///   chain (see <see cref="IMessageBoxScenario.HandleDeliveryFailureAsync"/>) - the "(Permanent)
        ///   Success?" / "Notificatie gelukt?" gateways in the BPMN evaluate this asynchronous delivery
        ///   status, not the synchronous send-call response already handled when the message was first sent.
        ///   The fallback's own outcome is logged but does not change this method's return value: the HTTP
        ///   response for this callback still reflects whether the contactmoment registration itself
        ///   succeeded, independent of whether a fallback send was also attempted.
        /// </remarks>
        private async Task<HttpRequestResponse> InformUserAboutMessageBoxStatusAsync(DeliveryReceipt callback, FeedbackTypes feedbackType)
        {
            (MessageBoxNotifyReference reference, NotifyMethods notificationMethod) = await ExtractMessageBoxCallbackDataAsync(callback);

            OmcController.LogApiResponse(
                feedbackType == FeedbackTypes.Failure ? LogLevel.Warning : LogLevel.Information,
                $"[MOBB callback] Message {reference.MessageId} (party {reference.PartyId}): channel {notificationMethod} reported {feedbackType}.");

            NotificationData notificationData = await GetMessageBoxNotificationDataAsync(notificationMethod, callback.Id);

            HttpRequestResponse result = await _telemetry.ReportMessageBoxCompletionAsync(
                reference,
                notificationMethod,
                callback.Recipient,
                messages: [
                    DetermineUserMessageSubject(_configuration, feedbackType, notificationMethod,
                        notificationData.IsSuccess ? notificationData.Subject : string.Empty),
                    TruncateContactMomentBody(DetermineUserMessageBody(_configuration, feedbackType, notificationMethod,
                        notificationData.IsSuccess ? notificationData.Body : string.Empty)),
                    feedbackType == FeedbackTypes.Success ? True : False,
                    notificationData.IsSuccess ? notificationData.SentAt : string.Empty
                ]);

            if (feedbackType == FeedbackTypes.Failure)
            {
                await ContinueMessageBoxFallbackAsync(reference, notificationMethod);
            }

            return result;
        }

        /// <summary>
        /// Caps a MOBB contactmoment's "inhoud" (content) at <see cref="MaxContactMomentBodyLength"/>
        /// characters, regardless of source channel (MOBB/e-mail/letter).
        /// </summary>
        private const int MaxContactMomentBodyLength = 1000;

        private static string TruncateContactMomentBody(string messageText)
        {
            return messageText.Length > MaxContactMomentBodyLength
                ? messageText[..MaxContactMomentBodyLength]
                : messageText;
        }

        /// <summary>
        /// Attempts the next fallback channel for a MOBB Bericht after its delivery-receipt callback
        /// reported failure, and logs the outcome. Never throws - a fallback failure must not break the
        /// callback's own HTTP response to Notify NL.
        /// </summary>
        private async Task ContinueMessageBoxFallbackAsync(MessageBoxNotifyReference reference, NotifyMethods failedChannel)
        {
            try
            {
                HttpRequestResponse fallbackResult = await _messageBoxScenario.HandleDeliveryFailureAsync(reference, failedChannel);

                OmcController.LogApiResponse(
                    fallbackResult.IsFailure ? LogLevel.Error : LogLevel.Information,
                    new ObjectResult(new { message = $"[MOBB fallback] {fallbackResult.JsonResponse}" })
                    {
                        StatusCode = fallbackResult.IsFailure
                            ? StatusCodes.Status500InternalServerError
                            : StatusCodes.Status201Created
                    });
            }
            catch (Exception exception)
            {
                OmcController.LogApiResponse(
                    LogLevel.Error,
                    new ObjectResult(new { message = $"[MOBB fallback] Unhandled exception: {exception.Message}" })
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    });
            }
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

        /// <summary>
        /// MOBB / Berichtenbox counterpart of <see cref="GetNotificationDataAsync"/>.
        /// </summary>
        /// <remarks>
        ///   <see cref="NotifyData"/> (and, transitively, <see cref="INotifyService{TPackage}.GetNotificationDataAsync"/>'s
        ///   static <c>INotifyClient</c> cache) requires a <see cref="NotifyReference"/> wrapping a real
        ///   <c>NotificationEvent</c>, which a MOBB/CloudEvent-driven send does not have. A throwaway, empty
        ///   <see cref="NotificationEvent"/> is built here purely to satisfy that type requirement - it is
        ///   never persisted or sent anywhere, only used in-memory for this one call.
        ///   <para>
        ///   Its <c>Channel</c> is deliberately set to <see cref="Channels.Objects"/> rather than left at the
        ///   default <c>Unknown</c>: <see cref="NotificationEventExtensions.GetOrganizationId"/> throws for
        ///   any channel it doesn't recognize (only <c>Cases</c>/<c>Decisions</c>/<c>Objects</c> resolve),
        ///   and for MOBB - which never has a real "Cases"/"Decisions" channel - this would otherwise throw
        ///   on every single call, not just an occasional cold-start. Confirmed safe: <see cref="NotificationClientFactory"/>
        ///   only uses this value for a log line, never to select which Notify NL account/credentials are used
        ///   (there is exactly one, configured globally via <c>NOTIFY_API_BASEURL</c>/<c>NOTIFY_API_KEY</c>), so
        ///   the returned "missing" organization ID has no effect beyond that log line.
        ///   </para>
        /// </remarks>
        private async Task<NotificationData> GetMessageBoxNotificationDataAsync(NotifyMethods notificationMethod, Guid notificationId)
        {
            try
            {
                var dummyReference = new NotifyReference
                {
                    Notification = new NotificationEvent { Channel = Channels.Objects }
                };

                var data = new NotifyData(notificationMethod, string.Empty, Guid.Empty, [], dummyReference);

                return await this._notifyService.GetNotificationDataAsync(data, notificationId);
            }
            catch (Exception exception)
            {
                return NotificationData.Failure(exception.Message);
            }
        }
        #endregion
    }
}