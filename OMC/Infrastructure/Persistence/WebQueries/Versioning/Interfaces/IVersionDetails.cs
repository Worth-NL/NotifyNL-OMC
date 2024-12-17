// © 2024, Worth Systems.

namespace WebQueries.Versioning.Interfaces
{
    /// <summary>
    /// Stores information about version of a specific service or component.
    /// </summary>
    public interface IVersionDetails
    {
        /// <summary>
        /// Gets the name of a specific service or component.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the version of a specific service or component.
        /// </summary>
        public string Version { get; }  // TODO: Get version of the api from OpenApi schema in specific API Query services
    }
}