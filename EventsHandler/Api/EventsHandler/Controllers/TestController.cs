﻿// © 2024, Worth Systems.

using EventsHandler.Attributes.Authorization;
using EventsHandler.Attributes.Validation;
using EventsHandler.Controllers.Base;
using EventsHandler.Extensions;
using EventsHandler.Mapping.Enums;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataSending.Responses;
using EventsHandler.Services.Register.Interfaces;
using EventsHandler.Services.Responding.Interfaces;
using EventsHandler.Services.Responding.Messages.Models.Errors;
using EventsHandler.Services.Serialization.Interfaces;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Utilities.Swagger.Examples;
using Microsoft.AspNetCore.Mvc;
using Notify.Client;
using Notify.Models.Responses;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace EventsHandler.Controllers
{
    /// <summary>
    /// Controller used to test other Web API services from which "Notify NL" OMC is dependent.
    /// </summary>
    /// <seealso cref="OmcController"/>
    public sealed class TestController : OmcController
    {
        private readonly WebApiConfiguration _configuration;
        private readonly ISerializationService _serializer;
        private readonly ITelemetryService _telemetry;
        private readonly IRespondingService<ProcessingResult, string> _responder;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration of the application.</param>
        /// <param name="serializer">The input de(serializing) service.</param>
        /// <param name="telemetry">The telemetry service registering API events.</param>
        /// <param name="responder">The output standardization service (UX/UI).</param>
        public TestController(
            WebApiConfiguration configuration,
            ISerializationService serializer,
            ITelemetryService telemetry,
            IRespondingService<ProcessingResult, string> responder)
        {
            this._configuration = configuration;
            this._serializer = serializer;
            this._telemetry = telemetry;
            this._responder = responder;
        }

        /// <summary>
        /// Checks the status of "Notify NL" Web API service.
        /// </summary>
        [HttpGet]
        [Route("NotifyNL/HealthCheck")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]  // NOTE: Replace errors raised by ASP.NET Core with standardized API responses
        // Swagger UI
        [ProducesResponseType(StatusCodes.Status202Accepted)]                                                         // REASON: The API service is up and running
        [ProducesResponseType(StatusCodes.Status400BadRequest)]                                                       // REASON: The API service is currently down
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProcessingFailed.Simplified))]  // REASON: Unexpected internal error (if-else / try-catch-finally handle)
        public async Task<IActionResult> HealthCheckAsync()
        {
            try
            {
                // Health Check URL
                string healthCheckUrl = $"{this._configuration.OMC.API.BaseUrl.NotifyNL()}/_status?simple=true";

                // Request
                using HttpResponseMessage result = await new HttpClient().GetAsync(healthCheckUrl);

                // Response
                return result.IsSuccessStatusCode
                    // HttpStatus Code: 202 Accepted
                    ? LogApiResponse(LogLevel.Information, this._responder.GetResponse(ProcessingResult.Success, result.ToString()))
                    // HttpStatus Code: 400 Bad Request
                    : LogApiResponse(LogLevel.Error, this._responder.GetResponse(ProcessingResult.Failure, result.ToString()));
            }
            catch (Exception exception)
            {
                // HttpStatus Code: 500 Internal Server Error
                return LogApiResponse(exception,
                    this._responder.GetExceptionResponse(exception));
            }
        }

        /// <summary>
        /// Sending Email messages to the "Notify NL" Web API service.
        /// </summary>
        /// <remarks>
        ///   NOTE: This endpoint will send real email to the given email address.
        /// </remarks>
        /// <param name="emailAddress">The email address (required) where the notification should be sent.</param>
        /// <param name="emailTemplateId">The email template ID (optional) to be used from "Notify NL" API service.
        ///   <para>
        ///     NOTE: If empty the ID of a very first looked up email template will be used.
        ///   </para>
        /// </param>
        /// <param name="personalization">The map (optional) of keys and values to be used as message personalization.
        ///   <para>
        ///     Example of personalization in template from "Notify NL" Admin Portal: "This is ((placeholderText)) information".
        ///   </para>
        ///   <para>
        ///     Example of personalization values to be provided: { "placeholderText": "good" }
        ///   </para>
        ///   <para>
        ///     Resulting message would be: "This is good information" (or exception, if personalization is required but not provided).
        ///   </para>
        /// </param>
        [HttpPost]
        [Route("NotifyNL/SendEmail")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]  // NOTE: Replace errors raised by ASP.NET Core with standardized API responses
        // Swagger UI
        [SwaggerRequestExample(typeof(Dictionary<string, object>), typeof(PersonalizationExample))]
        [ProducesResponseType(StatusCodes.Status202Accepted)]                                                         // REASON: The notification successfully sent to "Notify NL" API service
        [ProducesResponseType(StatusCodes.Status400BadRequest,          Type = typeof(ProcessingFailed.Simplified))]  // REASON: Issues on the "Notify NL" API service side
        [ProducesResponseType(StatusCodes.Status403Forbidden,           Type = typeof(ProcessingFailed.Simplified))]  // REASON: Base URL or API key to "Notify NL" API service were incorrect
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ProcessingFailed.Simplified))]  // REASON: The JSON structure is invalid
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProcessingFailed.Simplified))]  // REASON: Unexpected internal error (if-else / try-catch-finally handle)
        [ProducesResponseType(StatusCodes.Status501NotImplemented,      Type = typeof(string))]                       // REASON: Operation is not implemented
        public async Task<IActionResult> SendEmailAsync(
            [Required, FromQuery] string emailAddress,
            [Optional, FromQuery] string? emailTemplateId,
            [Optional, FromBody] Dictionary<string, object> personalization)
        {
            return await SendAsync(
                NotifyMethods.Email,
                emailAddress,
                emailTemplateId,
                personalization);
        }

        /// <summary>
        /// Sending SMS text messages to the "Notify NL" Web API service.
        /// </summary>
        /// <remarks>
        ///   NOTE: This endpoint will send real SMS to the given mobile number.
        /// </remarks>
        /// <param name="mobileNumber">The mobile phone number (required) where the notification should be sent.
        ///   <para>
        ///     NOTE: International country code is expected, e.g.: +1 (USA), +81 (Japan), +351 (Portugal), etc.
        ///   </para>
        /// </param>
        /// <param name="smsTemplateId">The SMS template ID (optional) to be used from "Notify NL" API service.
        ///   <para>
        ///     NOTE: If empty the ID of a very first looked up SMS template will be used.
        ///   </para>
        /// </param>
        /// <param name="personalization">
        ///   <inheritdoc cref="SendEmailAsync" path="/param[@name='personalization']"/>
        /// </param>
        [HttpPost]
        [Route("NotifyNL/SendSms")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]  // NOTE: Replace errors raised by ASP.NET Core with standardized API responses
        // Swagger UI
        [SwaggerRequestExample(typeof(Dictionary<string, object>), typeof(PersonalizationExample))]
        [ProducesResponseType(StatusCodes.Status202Accepted)]                                                         // REASON: The notification successfully sent to "Notify NL" API service
        [ProducesResponseType(StatusCodes.Status400BadRequest,          Type = typeof(ProcessingFailed.Simplified))]  // REASON: Issues on the "Notify NL" API service side
        [ProducesResponseType(StatusCodes.Status403Forbidden,           Type = typeof(ProcessingFailed.Simplified))]  // REASON: Base URL or API key to "Notify NL" API service were incorrect
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ProcessingFailed.Simplified))]  // REASON: The JSON structure is invalid
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProcessingFailed.Simplified))]  // REASON: Unexpected internal error (if-else / try-catch-finally handle)
        [ProducesResponseType(StatusCodes.Status501NotImplemented,      Type = typeof(string))]                       // REASON: Operation is not implemented
        public async Task<IActionResult> SendSmsAsync(
            [Required, FromQuery] string mobileNumber,
            [Optional, FromQuery] string? smsTemplateId,
            [Optional, FromBody] Dictionary<string, object> personalization)
        {
            return await SendAsync(
            NotifyMethods.Sms,
                mobileNumber,
                smsTemplateId,
                personalization);
        }

        /// <summary>
        /// Simulates behavior of Notify/Confirm endpoint, mocking (with better control) the response from "Notify NL" Web API service.
        /// </summary>
        /// <remarks>
        ///   NOTE: This endpoint will attempt to create real Contact Moment object.
        /// </remarks>
        /// <param name="json">The content of 'reference' sent back from NotifyNL Web API service.</param>
        /// <param name="notifyMethod">The notification method to be used during this test.</param>
        /// <param name="messages">
        ///   The messages required by specific contact registration implementation.
        ///   <para>
        ///     NOTE: The provided values are already strings, you don't have to surround them with quotation marks!
        ///   </para>
        ///   <para>
        ///     For "OMC Workflow v1" use:
        ///     <list type="bullet">
        ///       <item>index 0 = Message body</item>
        ///     </list>
        ///   </para>
        ///   <para>
        ///     For "OMC Workflow v2" use:
        ///     <list type="bullet">
        ///       <item>index 0 = Message subject; </item>
        ///       <item>index 1 = Message body; </item>
        ///       <item>index 2 = Status of completion (true / false)</item>
        ///     </list>
        ///   </para>
        /// </param>
        [HttpPost]
        [Route("OMC/Confirm")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]  // NOTE: Replace errors raised by ASP.NET Core with standardized API responses
        [SwaggerRequestExample(typeof(NotifyReference), typeof(NotifyReferenceExample))]  // NOTE: Documentation of expected JSON schema with sample and valid payload values
        [ProducesResponseType(StatusCodes.Status202Accepted)]                                                         // REASON: The registration was successfully sent to "Contactmomenten" API Web API service
        [ProducesResponseType(StatusCodes.Status400BadRequest,          Type = typeof(ProcessingFailed.Simplified))]  // REASON: One of the HTTP Request calls wasn't successful
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ProcessingFailed.Simplified))]  // REASON: The JSON structure is invalid
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProcessingFailed.Simplified))]  // REASON: The registration wasn't sent / Unexpected internal error (if-else / try-catch-finally handle)
        public async Task<IActionResult> ConfirmAsync(
            [Required, FromBody] object json,
            [Required, FromQuery] NotifyMethods notifyMethod,
            [Required, FromQuery] string[] messages)
        {
            try
            {
                // Deserialize received JSON payload
                NotifyReference reference = this._serializer.Deserialize<NotifyReference>(json);
             
                // Processing reporting operation
                RequestResponse response = await this._telemetry.ReportCompletionAsync(reference, notifyMethod, messages);

                return response.IsSuccess
                    // HttpStatus Code: 202 Accepted
                    ? LogApiResponse(LogLevel.Information, this._responder.GetResponse(ProcessingResult.Success, response.JsonResponse))
                    // HttpStatus Code: 400 Bad Request
                    : LogApiResponse(LogLevel.Error, this._responder.GetResponse(ProcessingResult.Failure, response.JsonResponse));
            }
            catch (Exception exception)
            {
                // HttpStatus Code: 500 Internal Server Error
                return LogApiResponse(exception,
                    this._responder.GetExceptionResponse(exception));
            }
        }

        #region Helper methods
        /// <summary>
        /// Generic method sending notification through <see cref="NotificationClient"/> and handling its responses in a standardized way.
        /// </summary>
        /// <returns>
        ///   The standardized <see cref="ObjectResult"/> API response.
        /// </returns>
        private async Task<IActionResult> SendAsync(
            NotifyMethods notifyMethod,
            string contactDetails,
            string? templateId,
            Dictionary<string, object> personalization)
        {
            try
            {
                // Initialize the .NET client of "Notify NL" API service
                var notifyClient = new NotificationClient(  // TODO: Client to be resolved by IClientFactory (to be testable)
                    this._configuration.OMC.API.BaseUrl.NotifyNL().ToString(),
                    this._configuration.User.API.Key.NotifyNL());

                // Determine first possible Email template ID if nothing was provided
                List<TemplateResponse>? allTemplates = (await notifyClient.GetAllTemplatesAsync(notifyMethod.GetEnumName())).templates; // NOTE: Assign to variables for debug purposes
                templateId ??= allTemplates.First().id;

                // TODO: To be extracted into a dedicated service
                // Case #1: Empty personalization
                if (IsEmptyOrDefault(personalization))
                {
                    switch (notifyMethod)
                    {
                        case NotifyMethods.Email:
                            _ = await notifyClient.SendEmailAsync(contactDetails, templateId);
                            break;

                        case NotifyMethods.Sms:
                            _ = await notifyClient.SendSmsAsync(contactDetails, templateId);
                            break;

                        default:
                            return LogApiResponse(LogLevel.Error,
                                this._responder.GetResponse(ProcessingResult.Failure, Resources.Test_NotifyNL_ERROR_NotSupportedMethod));
                    }
                }
                // Case #2: Personalization was provided by the user
                else
                {
                    // Casting object values improperly converted by API endpoint to string and then convert:
                    // - ["Key", ValueKind = Number : "1234"] into just ["Key", 1234] or
                    // - ["Key", ValueKind = String : "Test"] into just ["Key", "Test"], etc.
                    foreach (KeyValuePair<string, object> keyValuePair in personalization)
                    {
                        personalization[keyValuePair.Key] = $"{keyValuePair.Value}";
                    }

                    switch (notifyMethod)
                    {
                        case NotifyMethods.Email:
                            _ = await notifyClient.SendEmailAsync(contactDetails, templateId, personalization);
                            break;

                        case NotifyMethods.Sms:
                            _ = await notifyClient.SendSmsAsync(contactDetails, templateId, personalization);
                            break;

                        default:
                            return LogApiResponse(LogLevel.Error,
                                this._responder.GetResponse(ProcessingResult.Failure, Resources.Test_NotifyNL_ERROR_NotSupportedMethod));
                    }
                }

                // HttpStatus Code: 202 Accepted
                return LogApiResponse(LogLevel.Information,
                    this._responder.GetResponse(ProcessingResult.Success,
                        string.Format(Resources.Test_NotifyNL_SUCCESS_NotificationSent, notifyMethod.GetEnumName())));
            }
            catch (Exception exception)
            {
                // HttpStatus Code: 500 Internal Server Error
                return LogApiResponse(exception,
                    this._responder.GetExceptionResponse(exception));
            }
        }

        private static bool IsEmptyOrDefault(IReadOnlyDictionary<string, object> personalization)
        {
            return personalization.Count <= 1 &&
                   personalization.TryGetValue(PersonalizationExample.Key, out object? value) &&
                   Equals(value, PersonalizationExample.Value);
        }
        #endregion
    }
}