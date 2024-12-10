// © 2024, Worth Systems.

namespace Common.Versioning.Interfaces
{
    /// <summary>
    /// Manages the information about version of registered service or application component (implementing <see cref="IVersionDetails"/>).
    /// </summary>
    public interface IVersionRegister
    {
        /// <summary>
        /// Gets the specific version of a service or application.
        /// </summary>
        /// <param name="parameters">The optional parameters to be included.</param>
        /// <returns>
        ///   The requested version in a predefined format.
        /// </returns>
        public string GetVersion(string parameters = "");
    }
}