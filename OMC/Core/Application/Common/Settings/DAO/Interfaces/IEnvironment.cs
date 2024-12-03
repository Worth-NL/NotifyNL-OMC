// © 2024, Worth Systems.

namespace Common.Settings.DAO.Interfaces
{
    /// <summary>
    /// An interface representing the <see cref="Environment"/> DAO.
    /// </summary>
    public interface IEnvironment
    {
        /// <summary>
        /// Gets the environment variable using the provided key.
        /// </summary>
        /// <param name="key">The key used to look up for environment variable.</param>
        /// <returns>
        ///   The value of environment variable.
        /// </returns>
        public string? GetEnvironmentVariable(string key);
    }
}