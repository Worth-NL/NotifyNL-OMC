using EventsHandler.Services.Settings.DAO.Interfaces;

namespace EventsHandler.Services.Settings.DAO
{
    /// <inheritdoc cref="IEnvironment"/>
    internal sealed class EnvironmentReader : IEnvironment
    {
        /// <inheritdoc cref="IEnvironment.GetEnvironmentVariable(string)"/>
        public string? GetEnvironmentVariable(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }
    }
}