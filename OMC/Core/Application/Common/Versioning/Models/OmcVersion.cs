// © 2024, Worth Systems.

namespace Common.Versioning.Models
{
    /// <summary>
    /// The model representing OMC version (with its subcomponents).
    /// </summary>
    public readonly struct OmcVersion
    {
        private static int Major => 1;

        private static int Minor => 14;

        private static int Patch => 0;
        /// <summary>
        /// Gets the .NET version of the software, accepted by API Controllers, e.g.:
        /// <code>1.101</code>
        /// </summary>
        /// <remarks>
        ///   This standard is required by ASP.NET Web API controllers (leading to a potential incorrect format exception).
        /// </remarks>
        public static string GetNetVersion() => $"{Major}.{Minor}{Patch}";

        /// <summary>
        /// Gets the three-part version of the software, more detailed and user-friendly, e.g.:
        /// <code>1.10.1</code>
        /// </summary>
        public static string GetExpandedVersion() => $"{Major}.{Minor}.{Patch}";

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString() => GetExpandedVersion();
    }
}