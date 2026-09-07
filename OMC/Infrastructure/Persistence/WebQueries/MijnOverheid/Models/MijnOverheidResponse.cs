// WebQueries/MijnOverheid/Models/MijnOverheidResponse.cs
namespace WebQueries.MijnOverheid.Models
{
    /// <summary>
    /// Represents the result of sending a CloudEvent to the MijnOverheid webhook.
    /// Contains the HTTP status code and response body from MijnOverheid,
    /// plus a success flag for convenience.
    /// </summary>
    public class MijnOverheidResponse
    {
        /// <summary>
        /// Indicates whether the HTTP request to MijnOverheid was successful
        /// (status code 2xx).
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// The HTTP status code returned by MijnOverheid (e.g., 200, 202, 400, 500).
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// The response body content returned by MijnOverheid, typically a plain text
        /// or JSON error message. May be empty if no body was provided.
        /// </summary>
        public string ResponseBody { get; set; } = string.Empty;
    }
}