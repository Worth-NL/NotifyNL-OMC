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
            #region HTTP
            /// <summary>
            /// The HTTP Request constants.
            /// </summary>
            public static class Request
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