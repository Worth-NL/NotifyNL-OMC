// © 2024, Worth Systems.

namespace EventsHandler.Services.Settings.DAO.Interfaces
{
    /// <summary>
    /// An interface representing the <see cref="Environment"/> DAO.
    /// </summary>
    internal interface IEnvironment
    {
        /// <summary>
        /// Gets the environment variable using the provided key.
        /// </summary>
        /// <param name="key">The key used to look up for environment variable.</param>
        /// <returns>
        ///   The value of environment variable.
        /// </returns>
        internal string? GetEnvironmentVariable(string key);
    }
}