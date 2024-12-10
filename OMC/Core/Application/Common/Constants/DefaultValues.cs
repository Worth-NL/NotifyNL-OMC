// © 2024, Worth Systems.

namespace Common.Constants
{
    /// <summary>
    /// The <see cref="CommonValues"/> root.
    /// </summary>
    public static class CommonValues
    {
        /// <summary>
        /// The predefined immutable values distributed all over the application.
        /// </summary>
        public static class Default
        {
            #region Security
            /// <summary>
            /// The authorization constants.
            /// </summary>
            public static class Authorization
            {
                /// <summary>
                /// The token HTTP Request header.
                /// </summary>
                public static string Token => "Token";

                /// <summary>
                /// The Open API constants.
                /// </summary>
                public static class OpenApi
                {
                    /// <summary>
                    /// The security scheme constants.
                    /// </summary>
                    public static class SecurityScheme
                    {
                        /// <summary>
                        /// The authorization HTTP Request header.
                        /// </summary>
                        public static string Name => "Authorization";

                        /// <summary>
                        /// The bearer token format.
                        /// </summary>
                        public static string BearerFormat => "JWT";

                        /// <summary>
                        /// The bearer token schema.
                        /// </summary>
                        public static string BearerSchema => "Bearer";
                    }
                }
            }
            #endregion

            #region Communication
            /// <summary>
            /// The HTTP Request constants.
            /// </summary>
            public static class Network
            {
                /// <summary>
                /// The HTTP protocol.
                /// </summary>
                public static string HttpProtocol => "http";

                /// <summary>
                /// The HTTPS protocol.
                /// </summary>
                public static string HttpsProtocol => "https";
            }
            #endregion

            #region Common values
            /// <summary>
            /// The models constants.
            /// </summary>
            public static class Models
            {
                /// <summary>
                /// The empty JSON.
                /// </summary>
                public static string EmptyJson => "{}";
                
                /// <summary>
                /// The default <see langword="string"/> value.
                /// </summary>
                public const string DefaultStringValue = "-";
                
                /// <summary>
                /// The default <see cref="Enum"/> value.
                /// </summary>
                public const string DefaultEnumValueName = "-";
                
                /// <summary>
                /// The default <see cref="Uri"/> value.
                /// </summary>
                public static Uri EmptyUri { get; } = new(@"http://0.0.0.0:0/");
            }
            #endregion
        }
    }
}