// © 2023, Worth Systems.

namespace EventsHandler.Constants
{
    /// <summary>
    /// The <see cref="ApiValues"/> root.
    /// </summary>
    internal static class ApiValues
    {
        /// <summary>
        /// The predefined immutable values distributed all over the project.
        /// </summary>
        internal static class Default
        {
            #region API Controller
            /// <summary>
            /// The API Controller constants.
            /// </summary>
            internal static class ApiController
            {
                /// <summary>
                /// The controller route.
                /// </summary>
                internal const string Route = "[controller]";

                /// <summary>
                /// The HTTP Request content type.
                /// </summary>
                internal const string ContentType = "application/json";
            }
            #endregion
        }
    }
}