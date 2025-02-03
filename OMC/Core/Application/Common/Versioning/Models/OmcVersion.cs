// © 2024, Worth Systems.

namespace Common.Versioning.Models
{
    /// <summary>
    /// The model representing OMC version (with its subcomponents).
    /// </summary>
    public readonly struct OmcVersion
    {
        private static int s_major;
        private static int s_minor;
        private static int s_patch;

        /// <summary>
        /// Gets the .NET version of the software, accepted by API Controllers, e.g.:
        /// <code>1.101</code>
        /// </summary>
        /// <remarks>
        ///   This standard is required by ASP.NET Web API controllers (leading to a potential incorrect format exception).
        /// </remarks>
        public static string GetNetVersion() => $"{s_major}.{s_minor}{s_patch}";

        /// <summary>
        /// Gets the three-part version of the software, more detailed and user-friendly, e.g.:
        /// <code>1.10.1</code>
        /// </summary>
        public static string GetExpandedVersion() => $"{s_major}.{s_minor}.{s_patch}";

        /// <summary>
        /// Sets Version by Event-handler version-prefix in Program.
        /// </summary>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="patch"></param>
        public static void SetVersion(int major, int minor, int patch)
        {
            s_major = major;
            s_minor = minor;
            s_patch = patch;
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString() => GetExpandedVersion();
    }
}
