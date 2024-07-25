// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Responding.Messages.Models.Base;
using EventsHandler.Behaviors.Responding.Messages.Models.Details.Base;
using EventsHandler.Constants;
using EventsHandler.Controllers;
using EventsHandler.Properties;
using EventsHandler.Services.Responding.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Notify.Models.Responses;
using System.Collections.Concurrent;

namespace EventsHandler.Attributes.Validation
{
    /// <summary>
    /// The UX wrapper to handle and display ASP.NET Core MVC framework errors into a standardized human-friendly API responses.
    /// </summary>
    /// <seealso cref="ActionFilterAttribute"/>
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class StandardizeApiResponsesAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Binding map of API Controllers to IRespondingService{T,...}.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Type> s_controllerToResponderBinding = new();

        /// <summary>
        /// Initializes the <see cref="StandardizeApiResponsesAttribute"/> class.
        /// </summary>
        static StandardizeApiResponsesAttribute()
        {
            s_controllerToResponderBinding.TryAdd(typeof(EventsController), typeof(IRespondingService<NotificationEvent>));
            s_controllerToResponderBinding.TryAdd(typeof(TestController), typeof(IRespondingService<NotificationResponse, BaseSimpleDetails>));
        }

        /// <summary>
        /// Intercepts the <see cref="IActionResult"/> error messages from the validation of
        /// <see cref="NotificationEvent"/> to display <see cref="BaseStandardResponseBody"/>.
        /// </summary>
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            // Check if validation problems occurred
            if (ContainsValidationProblems(context, out ValidationProblemDetails? details))
            {
                // Check if responder service is registered
                if (s_controllerToResponderBinding.TryGetValue(context.Controller.GetType(), out Type? serviceType))
                {
                    // Resolving which responder service should be used (depends on API Controller)
                    var responder = context.HttpContext.RequestServices.GetRequiredService(serviceType) as IRespondingService;

                    // Intercepting and replacing native error messages by user-friendly API responses
                    context = responder?.Get_Exception_ActionResult(context, details!.Errors) ?? context;
                }
                else
                {
                    throw new ArgumentException(Resources.Processing_ERROR_ExecutingContext_UnregisteredApiController);
                }
            }

            base.OnResultExecuting(context);
        }

        #region Helper methods        
        /// <summary>
        /// Performs a series of checks on <see cref="ResultExecutingContext"/> to determine
        /// whether a specific problem (<see cref="ValidationProblemDetails"/>) has occurred.
        /// </summary>
        /// <param name="context">The context to be checked.</param>
        /// <param name="validationProblemDetails">The retrieved details.</param>
        /// <returns>
        ///   <see langword="true"/> if specific problem with model binding was encountered; otherwise, <see langword="false"/>.
        /// </returns>
        private static bool ContainsValidationProblems(ResultExecutingContext context, out ValidationProblemDetails? validationProblemDetails)
        {
            if (!context.ModelState.IsValid &&
                context.Result is BadRequestObjectResult { Value: ValidationProblemDetails details } &&
                details.Title == DefaultValues.Validation.ErrorsOccurred)
            {
                validationProblemDetails = details;

                return true;
            }

            validationProblemDetails = default;

            return false;
        }
        #endregion
    }
}