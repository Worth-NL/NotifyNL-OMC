// © 2024, Worth Systems.

using Common.Models.Responses;
using Common.Settings.Configuration;
using EventsHandler.Controllers.Base;
using EventsHandler.Models.DTOs.Processing;
using EventsHandler.Services.Register.Interfaces;
using EventsHandler.Services.Responding.Interfaces;
using EventsHandler.Services.Serialization.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ZhvModels.Enums;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Models.POCOs.NotifyNL;

namespace EventsHandler.Services.Responding.v2
{
    /// <inheritdoc cref="NotifyResponder"/>
    /// <remarks>
    ///   Version: "OpenKlant" (2.0) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IRespondingService"/>
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
            this._responder = this;  // NOTE: Shortcut to use interface methods faster ("NotifyResponder" parent derives from "IRespondingService<T>" interface)
            this._telemetry = telemetry;
        }

        /// <inheritdoc cref="NotifyResponder.HandleNotifyCallbackAsync(object)"/>
        internal override async Task<IActionResult> HandleNotifyCallbackAsync(object json)
        {
            try
            {
                DeliveryReceipt callback = this.Serializer.Deserialize<DeliveryReceipt>(json);
                FeedbackTypes status = callback.Status.ConvertToFeedbackStatus();

                if (status is FeedbackTypes.Success
                           or FeedbackTypes.Failure)  // NOTE: Do not spam user with "intermediate" state messages => FeedbackTypes.Info
                {
                    // Inform users about the progress of their notification
                    await InformUserAboutStatusAsync(callback, status);
                }

                // Log event in the system
                return LogContactRegistration(callback, status);
            }
            catch (Exception exception)
            {
                // NOTE: If callback.IdUri == Guid.Empty then it might be suspected that exception occurred during DeliveryReceipt deserialization
                return OmcController.LogApiResponse(exception,
                    this._responder.GetExceptionResponse(exception));
            }
        }

        #region Helper methods
        private const string True = "true";
        private const string False = "false";

        private async Task InformUserAboutStatusAsync(DeliveryReceipt callback, FeedbackTypes feedbackType)
        {
            (NotifyReference reference, NotifyMethods notificationMethod) = await ExtractCallbackDataAsync(callback);

            // Registering new status of the notification (for user)
            await this._telemetry.ReportCompletionAsync(reference, notificationMethod, messages:
            [
                // User message subject
                DetermineUserMessageSubject(this._configuration, feedbackType, notificationMethod),

                // User message body
                DetermineUserMessageBody(this._configuration, feedbackType, notificationMethod),

                // Is successfully sent
                feedbackType == FeedbackTypes.Success ? True : False
            ]);
        }

        private ObjectResult LogContactRegistration(DeliveryReceipt callback, FeedbackTypes feedbackType)
        {
            try
            {
                return OmcController.LogApiResponse(
                    // Log level
                    feedbackType == FeedbackTypes.Failure ? LogLevel.Error : LogLevel.Information,
                    // IActionResult
                    this._responder.GetResponse(
                        feedbackType is FeedbackTypes.Success
                                     or FeedbackTypes.Info
                            // NOTE: Everything is good (either final or intermediate state)
                            ? ProcessingResult.Success(GetDeliveryStatusLogMessage(callback))
                            // NOTE: The notification couldn't be delivered as planned
                            : ProcessingResult.Failure(GetDeliveryStatusLogMessage(callback))
                        ));
            }
            catch (Exception exception)
            {
                // It wasn't possible to report completion because of issue with Telemetry Service
                return OmcController.LogApiResponse(exception,
                    this._responder.GetExceptionResponse(
                        GetDeliveryErrorLogMessage(callback, exception)));
            }
        }

        private static string DetermineUserMessageSubject(
            WebApiConfiguration configuration, FeedbackTypes feedbackType, NotifyMethods notificationMethod)
        {
            return feedbackType switch
            {
                FeedbackTypes.Success =>
                    notificationMethod switch
                    {
                        NotifyMethods.Email => configuration.AppSettings.Variables.UxMessages.Email_Success_Subject(),
                        NotifyMethods.Sms => configuration.AppSettings.Variables.UxMessages.SMS_Success_Subject(),
                        _ => string.Empty
                    },

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