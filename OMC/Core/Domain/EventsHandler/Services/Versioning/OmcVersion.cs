// // © 2024, Worth Systems.

namespace EventsHandler.Services.Versioning
{
    /// <summary>
    /// The model representing OMC version (with its subcomponents).
    /// </summary>
    internal readonly struct OmcVersion
    {
        private static int Major => 1;

        private static int Minor => 12;

        private static int Patch => 5;

        /// <summary>
        /// Gets the .NET version of the software, accepted by API Controllers, e.g.:
        /// <code>1.101</code>
        /// </summary>
        internal static string GetNetVersion() => $"{Major}.{Minor}{Patch}";
        
        /// <summary>
        /// Gets the three-part version of the software, more detailed and user-friendly, e.g.:
        /// <code>1.10.1</code>
        /// </summary>
        internal static string GetExpandedVersion() => $"{Major}.{Minor}.{Patch}";

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString() => GetExpandedVersion();
    }
}