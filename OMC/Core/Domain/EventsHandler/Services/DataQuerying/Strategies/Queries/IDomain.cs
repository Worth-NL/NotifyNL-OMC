// © 2024, Worth Systems.

namespace EventsHandler.Services.DataQuerying.Strategies.Queries
{
    /// <summary>
    /// The interface defining that the service will have domain component of the URI.
    /// </summary>
    internal interface IDomain
    {
        /// <summary>
        /// Gets the organization-specific (e.g., municipality) domain part of the specific Web API service URI:
        /// <code>
        ///   http(s)://[DOMAIN]/ApiEndpoint
        /// </code>
        /// </summary>
        /// <exception cref="KeyNotFoundException"/>
        internal string GetDomain();
    }
}