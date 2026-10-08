// © 2026, Worth Systems.

using Common.Settings.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EventsHandler.Attributes.Dashboard
{
    /// <summary>
    /// Answers 404 for the decorated endpoints unless the status dashboard is switched on with
    /// <c>DASHBOARD_ENABLED</c>.
    /// </summary>
    /// <remarks>
    ///   404 rather than 403, so an environment with the dashboard off does not even reveal that the
    ///   endpoints exist. Checked per request, so the flag can't be bypassed by an endpoint that was
    ///   mapped regardless.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    internal sealed class DashboardEnabledAttribute : Attribute, IResourceFilter
    {
        /// <summary>Short-circuits the request with a 404 when the dashboard is off.</summary>
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (!ConfigExtensions.IsDashboardEnabled())
            {
                context.Result = new NotFoundResult();
            }
        }

        /// <summary>Nothing to do after the endpoint ran.</summary>
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }
    }
}
