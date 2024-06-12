// © 2024, Worth Systems.

namespace EventsHandler.Behaviors.Versioning
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