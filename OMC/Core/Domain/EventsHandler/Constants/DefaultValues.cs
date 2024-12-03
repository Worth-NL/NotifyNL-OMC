// © 2023, Worth Systems.

namespace EventsHandler.Constants
{
    /// <summary>
    /// The <see cref="ApiValues"/> root.
    /// </summary>
    internal static class ApiValues
    {
        /// <summary>
        /// The predefined immutable values distributed all over the application.
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

            #region API Security
            /// <summary>
            /// The authorization constants.
            /// </summary>
            internal static class Authorization
            {
                /// <summary>
                /// The authorization HTTP Request header.
                /// </summary>
                internal static string Name => "Authorization";
                
                /// <summary>
                /// The token HTTP Request header.
                /// </summary>
                internal static string Token => "Token";

                /// <summary>
                /// The Open API security scheme constants.
                /// </summary>
                internal static class OpenApiSecurityScheme
                {
                    /// <summary>
                    /// The bearer token format.
                    /// </summary>
                    internal static string BearerFormat => "JWT";

                    /// <summary>
                    /// The bearer token schema.
                    /// </summary>
                    internal static string BearerSchema => "Bearer";
                }
            }
            #endregion

            #region Domain values
            /// <summary>
            /// The models constants.
            /// </summary>
            internal static class Models
            {
                /// <summary>
                /// The default organization number.
                /// </summary>
                internal const string DefaultOrganization = "000000000";
            }
            #endregion
        }
    }
}