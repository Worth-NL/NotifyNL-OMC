// © 2023, Worth Systems.

namespace EventsHandler.Constants
{
    /// <summary>
    /// Predefined immutable values distributed all over the application.
    /// </summary>
    internal static class DefaultValues
    {
        #region Web API (Controllers)
        internal static class ApiController
        {
            internal const string Route = "[controller]";

            internal const string Version = "1.70";
        }
        #endregion

        #region HTTP
        internal static class Request
        {
            internal const string ContentType = "application/json";  // Constant only

            internal static string HttpProtocol => "http";

            internal static string HttpsProtocol => "https";
        }
        #endregion

        #region Settings (Program.cs)
        // Telemetry
        internal static class Logging
        {
            internal static string CloudRoleName => "omc";

            internal static string Category => "notifynl-omc";
        }

        // Security
        internal static class Authorization
        {
            internal static string Name => "Authorization";

            internal static class OpenApiSecurityScheme
            {
                internal static string BearerFormat => "JWT";

                internal static string BearerSchema => "Bearer";
            }
        }
        #endregion

        #region Business logic
        internal static class Models
        {
            internal static string EmptyJson => "{}";

            internal const string DefaultEnumValueName = "-";  // Constant only

            internal static Uri EmptyUri { get; } = new(@"http://0.0.0.0:0/");

            internal const string DefaultOrganization = "000000000";
        }

        // ReSharper disable InconsistentNaming => Allow underscores
        internal static class Validation
        {
            internal static string ErrorsOccurred => "One or more validation errors occurred.";
            
            internal static string Deserialization_MissingProperty => "JSON deserialization";

            internal static string Deserialization_InvalidValue => "The JSON value";
        }
        #endregion
    }
}