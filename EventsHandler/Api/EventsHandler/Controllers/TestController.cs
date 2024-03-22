// © 2024, Worth Systems.

using Asp.Versioning;
using EventsHandler.Attributes.Authorization;
using EventsHandler.Attributes.Validation;
using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Mapping.Enums;
using EventsHandler.Behaviors.Responding.Messages.Models.Errors;
using EventsHandler.Configuration;
using EventsHandler.Constants;
using EventsHandler.Properties;
using EventsHandler.Services.UserCommunication.Interfaces;
using EventsHandler.Utilities.Swagger.Examples;
using Microsoft.AspNetCore.Mvc;
using Notify.Client;
using Notify.Models.Responses;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.InteropServices;

namespace EventsHandler.Controllers
{
    /// <summary>
    /// Controller used to test other API services from which NotifyNL OMC is dependent.
    /// </summary>
    /// <seealso cref="Controller"/>
    [ApiController]
    [Route(DefaultValues.ApiController.Route)]
    [Consumes(DefaultValues.Request.ContentType)]
    [Produces(DefaultValues.Request.ContentType)]
    [ApiVersion(DefaultValues.ApiController.Version)]
    public sealed class TestController : Controller  // TODO: Use OmcController
    {
        private readonly WebApiConfiguration _configuration;
        private readonly IRespondingService<ProcessingResult, string> _responder;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TestController"/> class.
        /// </summary>
        /// <param name="configuration">The service handling Data Provider (DAO) loading strategies.</param>
        /// <param name="responder">The output standardization service (UX/UI).</param>
        public TestController(
            WebApiConfiguration configuration,
            IRespondingService<ProcessingResult, string> responder)
        {
            this._configuration = configuration;
            this._responder = responder;
        }

        /// <summary>
        /// Checks the status of NotifyNL API web service.
        /// </summary>
        [HttpGet]
        [Route("Notify/HealthCheck")]
        // Security
        [ApiAuthorization]
        // Swagger UI
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HealthCheckAsync()
        {
            // Health Check URL
            string healthCheckUrl = $"{this._configuration.Notify.API.BaseUrl()}/_status?simple=true";

            // Request
            using var httpClient = new HttpClient();
            HttpResponseMessage result = await httpClient.GetAsync(healthCheckUrl);

            // Response
            return result.IsSuccessStatusCode
                // HttpStatus Code: 200 OK
                ? Ok(result)
                // HttpStatus Code: 500 Internal Server Error
                : StatusCode((int)HttpStatusCode.InternalServerError, result);
        }

        /// <summary>
        /// Sending Email messages to the NotifyNL API web service.
        /// </summary>
        /// <param name="emailAddress">The email address (required) where the notification should be sent.</param>
        /// <param name="emailTemplateId">The email template ID (optional) to be used from NotifyNL API service.
        ///   <para>
        ///     If empty the ID of a very first looked up email template will be used.
        ///   </para>
        /// </param>
        /// <param name="personalization">The map (optional) of keys and values to be used as message personalization.
        ///   <para>
        ///     Example of personalization in template from NotifyNL Admin Portal: "This is ((placeholderText)) information".
        ///   </para>
        ///   <para>
        ///     Example of personalization values to be provided: { "placeholderText": "good" }
        ///   </para>
        ///   <para>
        ///     Resulting message would be: "This is good information" (or exception, if personalization is required but not provided).
        ///   </para>
        /// </param>
        [HttpPost]
        [Route("Notify/SendEmail")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]  // NOTE: Replace errors raised by ASP.NET Core with standardized API responses
        // Swagger UI
        [SwaggerRequestExample(typeof(Dictionary<string, object>), typeof(PersonalizationExample))]
        [ProducesResponseType(StatusCodes.Status202Accepted)]                                                         // REASON: The notification successfully sent to NotifyNL API service
        [ProducesResponseType(StatusCodes.Status400BadRequest,          Type = typeof(ProcessingFailed.Simplified))]  // REASON: Issues on the NotifyNL API service side
        [ProducesResponseType(StatusCodes.Status401Unauthorized,        Type = typeof(string))]                       // REASON: JWT Token is invalid or expired
        [ProducesResponseType(StatusCodes.Status403Forbidden,           Type = typeof(ProcessingFailed.Simplified))]  // REASON: Base URL or API key to NotifyNL API service were incorrect
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ProcessingFailed.Detailed))]    // REASON: The JSON structure is invalid
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProcessingFailed.Simplified))]  // REASON: Unexpected internal error, not yet handled by Notify OMC logic
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
        /// Sending SMS text messages to the NotifyNL API web service.
        /// </summary>
        /// <param name="mobileNumber">The mobile phone number (required) where the notification should be sent.</param>
        /// <param name="smsTemplateId">The SMS template ID (optional) to be used from NotifyNL API service.
        ///   <para>
        ///     If empty the ID of a very first looked up SMS template will be used.
        ///   </para>
        /// </param>
        /// <param name="personalization">
        ///   <inheritdoc cref="SendEmailAsync" path="/param[@name='personalization']"/>
        /// </param>
        [HttpPost]
        [Route("Notify/SendSms")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]  // NOTE: Replace errors raised by ASP.NET Core with standardized API responses
        // Swagger UI
        [SwaggerRequestExample(typeof(Dictionary<string, object>), typeof(PersonalizationExample))]
        [ProducesResponseType(StatusCodes.Status202Accepted)]                                                         // REASON: The notification successfully sent to NotifyNL API service
        [ProducesResponseType(StatusCodes.Status400BadRequest,          Type = typeof(ProcessingFailed.Simplified))]  // REASON: Issues on the NotifyNL API service side
        [ProducesResponseType(StatusCodes.Status401Unauthorized,        Type = typeof(string))]                       // REASON: JWT Token is invalid or expired
        [ProducesResponseType(StatusCodes.Status403Forbidden,           Type = typeof(ProcessingFailed.Simplified))]  // REASON: Base URL or API key to NotifyNL API service were incorrect
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ProcessingFailed.Detailed))]    // REASON: The JSON structure is invalid
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProcessingFailed.Simplified))]  // REASON: Unexpected internal error, not yet handled by Notify OMC logic
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

        #region Helper methods
        private static readonly Dictionary<NotifyMethods, string> s_templateTypes = new()
        {
            { NotifyMethods.Sms,   "sms"   },
            { NotifyMethods.Email, "email" }
        };

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
                // Initialize the .NET client of NotifyNL API service
                var notifyClient = new NotificationClient(    // TODO: Client to be resolved by IClientFactory (to be testable)
                    this._configuration.Notify.API.BaseUrl(),
                    this._configuration.User.API.Key.NotifyNL());

                // Determine template type
                string templateType = s_templateTypes[notifyMethod];

                // Determine first possible Email template ID if nothing was provided
                List<TemplateResponse>? allTemplates = (await notifyClient.GetAllTemplatesAsync(templateType)).templates; // NOTE: Assign to variables for debug purposes
                templateId ??= allTemplates.First().id;

                // TODO: To be extracted into a dedicated service
                // NOTE: Empty personalization
                if (personalization.Count <= 1 &&
                    personalization.TryGetValue(PersonalizationExample.Key, out object? value) &&
                    Equals(value, PersonalizationExample.Value))
                {
                    NotificationResponse _ = notifyMethod switch
                    {
                        NotifyMethods.Email => await notifyClient.SendEmailAsync(contactDetails, templateId),
                        NotifyMethods.Sms   => await notifyClient.SendSmsAsync(contactDetails, templateId),
                        _ => NotImplementedNotifyMethod()
                    };
                }
                // NOTE: Personalization was provided by the user
                else
                {
                    NotificationResponse _ = notifyMethod switch
                    {
                        NotifyMethods.Email => await notifyClient.SendEmailAsync(contactDetails, templateId, personalization),
                        NotifyMethods.Sms   => await notifyClient.SendSmsAsync(contactDetails, templateId, personalization),
                        _ => NotImplementedNotifyMethod()
                    };
                }

                return this._responder.GetStandardized_Processing_ActionResult(ProcessingResult.Success,
                    $"{templateType} {Resources.Test_NotifyNL_SUCCESS_NotificationSent}");
            }
            catch (Exception exception)
            {
                return this._responder.GetStandardized_Exception_ActionResult(exception);
            }
        }

        private static NotificationResponse NotImplementedNotifyMethod()
            => throw new ArgumentException(Resources.Test_NotifyNL_ERROR_NotSupportedMethod);
        #endregion
    }
}
