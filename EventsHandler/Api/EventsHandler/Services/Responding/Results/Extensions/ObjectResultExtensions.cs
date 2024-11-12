// © 2023, Worth Systems.

using EventsHandler.Services.Responding.Messages.Models.Base;
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
        internal static ObjectResult AsResult_403(this BaseStandardResponseBody response)
        {
            return new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
        #endregion

        #region HTTP Status Code 412
        /// <summary>
        /// Creates <see cref="HttpStatusCode.PreconditionFailed"/> object result.
        /// </summary>
        /// <param name="response">The specific custom response to be passed into <see cref="IActionResult"/>.</param>
        internal static ObjectResult AsResult_412(this BaseStandardResponseBody response)
        {
            return new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status412PreconditionFailed
            };
        }
        #endregion

        #region HTTP Status Code 422
        /// <summary>
        /// Creates <see cref="HttpStatusCode.UnprocessableEntity"/> object result.
        /// </summary>
        /// <param name="response">The specific custom response to be passed into <see cref="IActionResult"/>.</param>
        internal static ObjectResult AsResult_422(this BaseStandardResponseBody response)
        {
            return new UnprocessableEntityObjectResult(response);
        }
        #endregion

        #region HTTP Status Code 500
        /// <summary>
        /// Creates <see cref="HttpStatusCode.InternalServerError"/> object result.
        /// </summary>
        /// <param name="response">The specific custom response to be passed into <see cref="IActionResult"/>.</param>
        internal static ObjectResult AsResult_500(this BaseStandardResponseBody response)
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
        /// <param name="response">The specific custom response to be passed into <see cref="IActionResult"/>.</param>
        internal static ObjectResult AsResult_501(this BaseStandardResponseBody response)
        {
            return new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status501NotImplemented
            };
        }
        #endregion
    }
}