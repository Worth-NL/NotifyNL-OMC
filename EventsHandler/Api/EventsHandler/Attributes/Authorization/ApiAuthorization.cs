// © 2023, Worth Systems.

using EventsHandler.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics.CodeAnalysis;

namespace EventsHandler.Attributes.Authorization
{
    /// <summary>
    /// Custom authorization used by <see cref="EventsController"/> endpoints.
    /// </summary>
    /// <seealso cref="AuthorizeAttribute"/>
    /// <seealso cref="IAuthorizationFilter"/>
    [AttributeUsage(AttributeTargets.Method)]
    [ExcludeFromCodeCoverage]
    internal sealed class ApiAuthorization : AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // NOTE: Keep this method empty for now. It's main purpose is to overwrite OnAuthorization() method from plain [Authorize] annotation
        }
    }
}