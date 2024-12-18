// © 2023, Worth Systems.

using Common.Models.Messages.Base;
using EventsHandler.Controllers;
using EventsHandler.Properties;
using EventsHandler.Services.Responding;
using EventsHandler.Services.Responding.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Concurrent;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;

namespace EventsHandler.Attributes.Validation
{
    /// <summary>
    /// The UX wrapper to handle and display ASP.NET Core MVC framework errors that cannot be handled in API controllers.
    /// </summary>
    /// <seealso cref="ActionFilterAttribute"/>
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class AspNetExceptionsHandlerAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Binding map of API Controllers to IRespondingService{T,...}.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Type> s_mappedControllersToResponders = new();

        /// <summary>
        /// Initializes the <see cref="AspNetExceptionsHandlerAttribute"/> class.
        /// </summary>
        static AspNetExceptionsHandlerAttribute()
        {
            // NOTE: Concept similar to strategy design pattern => decide how and which API Controllers are responding to the end-user
            s_mappedControllersToResponders.TryAdd(typeof(EventsController), typeof(NotificationEventResponder));
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
                try
                {
                    // Check if responder service is registered
                    if (!s_mappedControllersToResponders.TryGetValue(context.Controller.GetType(), out Type? responderType))
                    {
                        // Set to default responder for general purposes
                        responderType = typeof(GeneralResponder);
                    }

                    // Resolving which responder service should be used (depends on API Controller)
                    var responder = (IRespondingService)context.HttpContext.RequestServices.GetRequiredService(responderType);

                    // Intercepting and replacing native error messages by user-friendly API responses
                    context = responder.GetExceptionResponse(context, details!.Errors);
                }
                catch (Exception exception) when (exception
                    is InvalidOperationException  // The looked-up responding service is not registered, or it's registered differently
                    or InvalidCastException)      // The looked-up service was resolved, but it's not deriving from IRespondingService
                {
                    throw new ArgumentException(ApiResources.Processing_ERROR_ExecutingContext_UnregisteredApiController);
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
            const string errorsOccurred = "One or more validation errors occurred.";

            if (!context.ModelState.IsValid &&
                context.Result is BadRequestObjectResult { Value: ValidationProblemDetails { Title: errorsOccurred } details })
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