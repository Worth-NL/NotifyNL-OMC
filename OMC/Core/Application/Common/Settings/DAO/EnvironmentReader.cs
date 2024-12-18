using Common.Settings.DAO.Interfaces;

namespace Common.Settings.DAO
{
    /// <inheritdoc cref="IEnvironment"/>
    public sealed class EnvironmentReader : IEnvironment
    {
        /// <inheritdoc cref="IEnvironment.GetEnvironmentVariable(string)"/>
        public string? GetEnvironmentVariable(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }
    }
}