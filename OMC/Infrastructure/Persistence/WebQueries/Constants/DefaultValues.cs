// © 2024, Worth Systems.

namespace WebQueries.Constants
{
    /// <summary>
    /// The <see cref="QueryValues"/> root.
    /// </summary>
    internal static class QueryValues
    {
        /// <summary>
        /// The predefined immutable values distributed all over the project.
        /// </summary>
        internal static class Default
        {
            #region Communication
            /// <summary>
            /// The network constants.
            /// </summary>
            internal static class Network
            {
                /// <summary>
                /// The HTTP Request content type.
                /// </summary>
                internal const string ContentType = "application/json";
            }
            #endregion
        }
    }
}