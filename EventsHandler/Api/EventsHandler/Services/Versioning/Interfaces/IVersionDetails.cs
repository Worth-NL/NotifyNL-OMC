// © 2024, Worth Systems.

namespace EventsHandler.Services.Versioning.Interfaces
{
    /// <summary>
    /// Stores information about version of a specific service or component.
    /// </summary>
    public interface IVersionDetails
    {
        /// <summary>
        /// Gets the name of a specific service or component.
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// Gets the version of a specific service or component.
        /// </summary>
        internal string Version { get; }
    }
}