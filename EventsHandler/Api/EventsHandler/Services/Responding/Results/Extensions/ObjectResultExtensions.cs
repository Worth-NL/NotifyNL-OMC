// © 2023, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Services.Responding.Messages.Models.Base;
using EventsHandler.Services.Responding.Messages.Models.Details.Base;
using EventsHandler.Services.Responding.Messages.Models.Errors;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventsHandler.Services.Responding.Results.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="ObjectResult"/>s.
    /// </summary>
    internal static class ObjectResultExtensions
    {
        #region HTTP Status Code 202
        /// <summary>
        /// Creates <see cref="HttpStatusCode.Accepted"/> object result.
        /// </summary>
        /// <param name="response">The specific custom response to be passed into <see cref="IActionResult"/>.</param>
        internal static ObjectResult AsResult_202(this BaseStandardResponseBody response)
        {
            return new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status202Accepted
            };
        }

        /// <summary>
        /// Creates <see cref="HttpStatusCode.Accepted"/> object result.
        /// </summary>
        /// <param name="response">The specific custom response to be passed into <see cref="IActionResult"/>.</param>
        internal static ObjectResult AsResult_202(string response)
        {
            return new ObjectResult(new ProcessingFailed.Simplified(HttpStatusCode.Accepted, response))
            {
                StatusCode = StatusCodes.Status202Accepted
            };
        }
        #endregion

        #region HTTP Status Code 206
        /// <summary>
        /// Creates <see cref="HttpStatusCode.PartialContent"/> object result.
        /// </summary>
        /// <param name="response">The specific custom response to be passed into <see cref="IActionResult"/>.</param>
        internal static ObjectResult AsResult_206(this BaseStandardResponseBody response)
        {
            return new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status206PartialContent
            };
        }
        #endregion

        #region HTTP Status Code 400
        /// <summary>
        /// Creates <see cref="HttpStatusCode.BadRequest"/> object result.
        /// </summary>
        /// <param name="errorDetails">The error details to be passed into <see cref="IActionResult"/>.</param>
        internal static ObjectResult AsResult_400(this BaseEnhancedDetails errorDetails)
        {
            return errorDetails.Cases.IsNotNullOrEmpty() &&  // NOTE: Not enough details
                   errorDetails.Reasons.Any()
                ? new HttpRequestFailed.Detailed(errorDetails).AsResult_400()
                : new HttpRequestFailed.Simplified(errorDetails.Trim()).AsResult_400();
        }

        /// <summary>
        /// Creates <see cref="HttpStatusCode.BadRequest"/> object result.
        /// </summary>
        /// <param name="response">The specific custom response to be passed into <see cref="IActionResult"/>.</param>
        internal static ObjectResult AsResult_400(string response)
        {
            return new ProcessingFailed.Simplified(HttpStatusCode.BadRequest, response).AsResult_400();
        }

        /// <summary>
        /// Creates <see cref="HttpStatusCode.BadRequest"/> object result.
        /// </summary>
        /// <param name="response">The specific custom response to be passed into <see cref="IActionResult"/>.</param>
        internal static ObjectResult AsResult_400(this BaseStandardResponseBody response)
        {
            return new BadRequestObjectResult(response);
        }
        #endregion

        #region HTTP Status Code 403
        /// <summary>
        /// Creates <see cref="HttpStatusCode.Forbidden"/> object result.
        /// </summary>
        /// <param name="response">The specific custom response to be passed into <see cref="IActionResult"/>.</param>
        internal static ObjectResult AsResult_403(string response)
        {
            return new ProcessingFailed.Simplified(HttpStatusCode.Forbidden, response).AsResult_403();
        }

        /// <summary>
        /// Creates <see cref="HttpStatusCode.Forbidden"/> object result.
        /// </summary>
        /// <param name="response">The specific custom response to be passed into <see cref="IActionResult"/>.</param>
        private static ObjectResult AsResult_403(this BaseStandardResponseBody response)
        {
            return new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
        #endregion

        #region HTTP Status Code 422
        /// <summary>
        /// Creates <see cref="HttpStatusCode.UnprocessableEntity"/> object result.
        /// </summary>
        /// <param name="errorDetails">The error details to be passed into <see cref="IActionResult"/>.</param>
        internal static ObjectResult AsResult_422(this BaseEnhancedDetails errorDetails)
        {
            return new DeserializationFailed(errorDetails).AsResult_422();
        }

        /// <summary>
        /// Creates <see cref="HttpStatusCode.UnprocessableEntity"/> object result.
        /// </summary>
        /// <param name="response">The specific custom response to be passed into <see cref="IActionResult"/>.</param>
        internal static ObjectResult AsResult_422(this DeserializationFailed response)
        {
            return new UnprocessableEntityObjectResult(response);
        }
        #endregion

        #region HTTP Status Code 500
        /// <summary>
        /// Creates <see cref="HttpStatusCode.InternalServerError"/> object result.
        /// </summary>
        /// <param name="details">The specific custom details to be passed into <see cref="IActionResult"/>.</param>
        internal static ObjectResult AsResult_500(this BaseSimpleDetails details)
        {
            return new InternalError(details).AsResult_500();
        }

        /// <summary>
        /// Creates <see cref="HttpStatusCode.InternalServerError"/> object result.
        /// </summary>
        /// <param name="response">The specific custom response to be passed into <see cref="IActionResult"/>.</param>
        internal static ObjectResult AsResult_500(string response)
        {
            return new InternalError(response).AsResult_500();
        }

        /// <summary>
        /// Creates <see cref="HttpStatusCode.InternalServerError"/> object result.
        /// </summary>
        /// <param name="response">The specific custom response to be passed into <see cref="IActionResult"/>.</param>
        private static ObjectResult AsResult_500(this BaseStandardResponseBody response)
        {
            return new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        #endregion

        #region HTTP Status Code 501
        /// <summary>
        /// Creates <see cref="HttpStatusCode.NotImplemented"/> object result.
        /// </summary>
        internal static ObjectResult AsResult_501()
        {
            return new ObjectResult(new NotImplemented())
            {
                StatusCode = StatusCodes.Status501NotImplemented
            };
        }
        #endregion

        #region Trim()
        // ReSharper disable once SuggestBaseTypeForParameter => Do not allow simplifying simple details (redundancy)
        /// <summary>
        /// Trims the missing details.
        /// </summary>
        /// <param name="details">The enhanced details.</param>
        private static BaseSimpleDetails Trim(this BaseEnhancedDetails details)
        {
            // Details without Cases and Reasons
            return new SimpleDetails(details.Message);
        }
        #endregion
    }
}