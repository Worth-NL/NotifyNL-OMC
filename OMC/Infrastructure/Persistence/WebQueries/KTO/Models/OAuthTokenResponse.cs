using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace WebQueries.KTO.Models
{
    /// <summary>
    /// Represents an OAuth 2.0 token response.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct OAuthTokenResponse : IJsonSerializable
    {
        /// <summary>
        /// The type of token (e.g., "Bearer").
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("token_type")]
        [JsonPropertyOrder(1)]
        public string TokenType { [UsedImplicitly] get; init; }

        /// <summary>
        /// The number of seconds until the token expires.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("expires_in")]
        [JsonPropertyOrder(2)]
        public int ExpiresIn { [UsedImplicitly] get; init; }

        /// <summary>
        /// The number of seconds until the extended expiration time.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("ext_expires_in")]
        [JsonPropertyOrder(3)]
        public int ExtExpiresIn { [UsedImplicitly] get; init; }

        /// <summary>
        /// The access token used for authentication.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("access_token")]
        [JsonPropertyOrder(4)]
        public string AccessToken { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthTokenResponse"/> struct.
        /// </summary>
        public OAuthTokenResponse()
        {
            TokenType = string.Empty;
            ExpiresIn = 0;
            ExtExpiresIn = 0;
            AccessToken = string.Empty;
        }
    }

    /// <summary>
    /// Interface to mark objects as JSON serializable.
    /// </summary>
    public interface IJsonSerializable
    {
    }
}