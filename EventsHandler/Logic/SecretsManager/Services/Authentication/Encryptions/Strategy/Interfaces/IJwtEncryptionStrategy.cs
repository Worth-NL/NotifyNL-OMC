// © 2023, Worth Systems.

using Microsoft.IdentityModel.Tokens;

namespace SecretsManager.Services.Authentication.Encryptions.Strategy.Interfaces
{
    /// <summary>
    /// The strategy used to set up and generate JSON Web Tokens.
    /// </summary>
    public interface IJwtEncryptionStrategy
    {
        /// <summary>
        /// Gets the security key.
        /// </summary>
        /// <param name="secret">An optional secret (password) to be used to generate the security key.</param>
        internal SecurityKey GetSecurityKey(string secret = "");

        /// <summary>
        /// Generates a JSON Web Token based on the provided claims.
        /// </summary>
        /// <param name="securityKey">JWT: The security key to be used in verify signature.</param>
        /// <param name="issuer">JWT: The issuer (who created this token).</param>
        /// <param name="audience">JWT: The audience (who is allowed to receive this token).</param>
        /// <param name="expiresAt">JWT: For how long the token should be valid.</param>
        /// <param name="userId">JWT: The user identifier to be used for logging.</param>
        /// <param name="userName">JWT: The user representation to be used for logging.</param>
        /// <returns>
        ///   The new JSON Web Token as a <see langword="string"/> (3 components of it are: Header.Payload.Signature).
        /// </returns>
        internal string GetJwtToken(SecurityKey securityKey, string issuer, string audience, DateTime expiresAt, string userId, string userName);

        /// <inheritdoc cref="GetJwtToken(SecurityKey, string, string, DateTime, string, string)"/>
        /// <param name="securityKey">JWT: The security key to be used in verify signature.</param>
        /// <param name="issuer">JWT: The issuer (who created this token).</param>
        /// <param name="audience">JWT: The audience (who is allowed to receive this token).</param>
        /// <param name="expiresInMinutes">JWT: For how long (in minutes from now) the token should be valid.</param>
        /// <param name="userId">JWT: The user identifier to be used for logging.</param>
        /// <param name="userName">JWT: The user representation to be used for logging.</param>
        internal string GetJwtToken(SecurityKey securityKey, string issuer, string audience, double expiresInMinutes, string userId, string userName);

        /// <summary>
        /// Saves the JSON Web Token.
        /// </summary>
        internal void SaveJwtToken(string jwtToken);
    }
}