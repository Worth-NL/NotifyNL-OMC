﻿// © 2024, Worth Systems.

using Asp.Versioning;
using EventsHandler.Attributes.Authorization;
using EventsHandler.Attributes.Validation;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Responding.Messages.Models.Informations;
using EventsHandler.Configuration;
using EventsHandler.Constants;
using EventsHandler.Services.UserCommunication.Interfaces;
using EventsHandler.Utilities.Swagger.Examples;
using Microsoft.AspNetCore.Mvc;
using Notify.Client;
using Notify.Exceptions;
using Notify.Models.Responses;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace EventsHandler.Controllers
{
    /// <summary>
    /// Controller used to test other API services from which NotifyNL OMC is dependent.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    [Route(DefaultValues.ApiController.Route)]
    [Consumes(DefaultValues.Request.ContentType)]
    [Produces(DefaultValues.Request.ContentType)]
    [ApiVersion(DefaultValues.ApiController.Version)]
    public sealed class TestController : ControllerBase
    {
        private readonly WebApiConfiguration _configuration;
        private readonly IRespondingService<NotificationEvent> _responder;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TestController"/> class.
        /// </summary>
        /// <param name="configuration">The service handling Data Provider (DAO) loading strategies.</param>
        /// <param name="responder">The output standardization service (UX/UI).</param>
        public TestController(
            WebApiConfiguration configuration,
            IRespondingService<NotificationEvent> responder)
        {
            this._configuration = configuration;
            this._responder = responder;
        }

        /// <summary>
        /// Sending Email messages to the NotifyNL API service. 
        /// Ensure that base URL and API key for NotifyNL API service are provided in the configuration.
        /// </summary>
        /// <param name="emailAddress">The email address (required) where the notification should be sent.</param>
        /// <param name="personalization">The map (optional) of keys and values to be used as email personalization.
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
        /// <param name="emailTemplateId">The email template ID (optional) to be used from NotifyNL API service.
        ///   <para>
        ///     If empty the ID of a very first looked up email template will be used.
        ///   </para>
        /// </param>
        /// <returns></returns>
        [HttpPost]
        [Route("Notify/SendEmail")]
        // Security
        [ApiAuthorization]
        // User experience
        [StandardizeApiResponses]  // NOTE: Replace errors raised by ASP.NET Core with standardized API responses
        // Swagger UI
        [SwaggerRequestExample(typeof(Dictionary<string, object>), typeof(PersonalizationExample))]
        [ProducesResponseType(StatusCodes.Status202Accepted)]                                                         // REASON: The notification successfully sent to NotifyNL API service
        [ProducesResponseType(StatusCodes.Status401Unauthorized,        Type = typeof(string))]                       // REASON: JWT Token is invalid or expired
        [ProducesResponseType(StatusCodes.Status400BadRequest,          Type = typeof(ProcessingFailed.Simplified))]  // REASON: Issues on the NotifyNL API service side
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProcessingFailed.Simplified))]  // REASON: Unexpected internal error, not handled by API logic
        public async Task<IActionResult> SendEmailAsync(
            [Required, FromQuery] string emailAddress,
            [Optional, FromQuery] string? emailTemplateId,
            [Optional, FromBody] Dictionary<string, object> personalization)
        {
            try
            {
                // Initialize the .NET client of NotifyNL API service
                var notifyClient = new NotificationClient(
                    this._configuration.Notify.API.BaseUrl(),
                    this._configuration.User.API.Key.NotifyNL());
                
                // Determine first possible Email template ID if nothing was provided
                List<TemplateResponse>? allTemplates = (await notifyClient.GetAllTemplatesAsync("email")).templates;
                emailTemplateId ??= allTemplates.First().id;

                // NOTE: Empty personalization
                if (personalization.Count == 1 || personalization.ContainsKey("key"))
                {
                    _ = await notifyClient.SendEmailAsync(emailAddress, emailTemplateId);
                }
                // NOTE: Personalization was provided by the user
                else
                {
                    _ = await notifyClient.SendEmailAsync(emailAddress, emailTemplateId, personalization);
                }

                return Ok(new ProcessingFailed.Simplified(HttpStatusCode.Accepted, "Email was successfully send to NotifyNL"));
            }
            catch (NotifyClientException exception)
            {
                string message;
                Match match;

                // NOTE: Personalization is required by NotifyNL message template, but it was not provided
                if ((match = _personalisationPattern.Match(exception.Message)).Success)
                {
                    message = match.Value;
                }
                else
                {
                    message = exception.Message;
                }

                return BadRequest(new ProcessingFailed.Simplified(HttpStatusCode.BadRequest, message));
                //return this._responder.GetStandardized_Exception_ActionResult(message);
            }
            catch (Exception exception)
            {
                return BadRequest(new ProcessingFailed.Simplified(HttpStatusCode.InternalServerError, exception.Message));
                //return this._responder.GetStandardized_Exception_ActionResult(exception);
            }
        }

        private readonly Regex _personalisationPattern = new("Missing personalisation\\:[a-z.,\\ ]+", RegexOptions.Compiled);
    }
}
