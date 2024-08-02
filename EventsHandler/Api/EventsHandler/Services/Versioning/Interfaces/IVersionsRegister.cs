// © 2024, Worth Systems.

namespace EventsHandler.Services.Versioning.Interfaces
{
    /// <summary>
    /// Manages the information about versions of registered services (implementing <see cref="IVersionDetails"/>).
    /// </summary>
    public interface IVersionsRegister
    {
        /// <summary>
        /// Gets the versions of registered services (implementing <see cref="IVersionDetails"/>).
        /// </summary>
        internal string GetVersions();
    }
}