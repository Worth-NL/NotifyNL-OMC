// © 2024, Worth Systems.

namespace EventsHandler.Services.Versioning.Interfaces
{
    /// <summary>
    /// Manages the information about versions of registered services (implementing <see cref="IVersionDetails"/>).
    /// </summary>
    internal interface IVersionsRegister
    {
        /// <summary>
        /// Gets the versions of registered APIs services (implementing <see cref="IVersionDetails"/>).
        /// </summary>
        internal string GetApisVersions();

        /// <summary>
        /// Gets the version of "OMC" Web API service
        /// </summary>
        internal string GetOmcVersion(string componentsVersions);
    }
}