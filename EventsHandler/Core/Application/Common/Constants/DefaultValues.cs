// © 2023, Worth Systems.

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
            #region Web API (Controllers)
            /// <summary>
            /// The API Controller constants.
            /// </summary>
            public static class ApiController
            {
                /// <summary>
                /// The controller route.
                /// </summary>
                public const string Route = "[controller]";

                /// <summary>
                /// The controller version.
                /// </summary>
                public const string Version = "1.125";
            }
            #endregion

            #region HTTP
            /// <summary>
            /// The HTTP Request constants.
            /// </summary>
            public static class Request
            {
                /// <summary>
                /// The HTTP Request content type.
                /// </summary>
                public const string ContentType = "application/json";  // Constant only

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

            #region Security
            /// <summary>
            /// The authorization constants.
            /// </summary>
            public static class Authorization
            {
                /// <summary>
                /// The authorization HTTP Request header.
                /// </summary>
                public static string Name => nameof(Authorization);
                
                /// <summary>
                /// The token HTTP Request header.
                /// </summary>
                public static string Token => nameof(Token);

                /// <summary>
                /// The Open API security scheme constants.
                /// </summary>
                public static class OpenApiSecurityScheme
                {
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
            #endregion

            #region Business logic
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
                public const string DefaultStringValue = "-";  // Constant only
                
                /// <summary>
                /// The default <see cref="Enum"/> value.
                /// </summary>
                public const string DefaultEnumValueName = "-";  // Constant only
                
                /// <summary>
                /// The default <see cref="Uri"/> value.
                /// </summary>
                public static Uri EmptyUri { get; } = new(@"http://0.0.0.0:0/");
                
                /// <summary>
                /// The default organization number.
                /// </summary>
                public const string DefaultOrganization = "000000000";
            }

            // ReSharper disable InconsistentNaming => Allow underscores
            /// <summary>
            /// The validation constants.
            /// </summary>
            public static class Validation
            {
                /// <summary>
                /// The validation error message.
                /// </summary>
                public static string ErrorsOccurred => "One or more validation errors occurred.";
                
                /// <summary>
                /// The "missing property" deserialization error message.
                /// </summary>
                public static string Deserialization_MissingProperty => "JSON deserialization";
                
                /// <summary>
                /// The "invalid value" deserialization error message.
                /// </summary>
                public static string Deserialization_InvalidValue => "The JSON value";
                
                /// <summary>
                /// The "HTTP Request" error message.
                /// </summary>
                public static string HttpRequest_ErrorMessage => "HTTP Request";
            }
            #endregion
        }
    }
}