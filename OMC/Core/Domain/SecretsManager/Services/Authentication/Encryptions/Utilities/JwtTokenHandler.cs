// © 2023, Worth Systems.

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace SecretsManager.Services.Authentication.Encryptions.Utilities
{
    /// <summary>
    /// Common JSON Web Token methods shared between all encryption strategies.
    /// </summary>
    internal static class JwtTokenHandler
    {
        /// <summary>
        /// Generates a new JSON Web Token based on the provided "claims".
        /// <para>
        ///   The token will be valid until specified date and time.
        /// </para>
        /// </summary>
        internal static string GetJwtToken(
            SecurityKey securityKey, string issuer, string audience, DateTime expiresAt,
            string securityAlgorithm, string userId, string userName)
        {
            // Creating JWT token
            return new JsonWebTokenHandler
            {
                SetDefaultTimesOnTokenCreation = false  // NOTE: Remove for example "nbf"
            }
            .CreateToken(
                GetJwtTokenDescriptor(
                    securityKey, issuer, audience, DateTime.UtcNow, expiresAt,
                    securityAlgorithm, userId, userName));
        }

        /// <summary>
        /// Generates a new JSON Web Token based on the provided "claims".
        /// <para>
        ///   The token will be valid for the specified time (starting from now).
        /// </para>
        /// </summary>
        internal static string GetJwtToken(
            SecurityKey securityKey, string issuer, string audience, double expiresInMinutes,
            string securityAlgorithm, string userId, string userName)
        {
            // Calculating validity time
            DateTime currentDateTime = DateTime.UtcNow;

            // Creating JWT token
            return new JsonWebTokenHandler
            {
                SetDefaultTimesOnTokenCreation = false  // NOTE: Remove for example "nbf"
            }
            .CreateToken(
                GetJwtTokenDescriptor(
                    securityKey, issuer, audience, currentDateTime,
                    currentDateTime.AddMinutes(expiresInMinutes),
                    securityAlgorithm, userId, userName));
        }

        /// <summary>
        /// Gets the JWT token descriptor - defining "claims" for JWT token.
        /// </summary>
        private static SecurityTokenDescriptor GetJwtTokenDescriptor(
            SecurityKey securityKey, string issuer, string audience, DateTime issuedAt,
            DateTime expiresAt, string securityAlgorithm, string userId, string userName)
        {
            // Claim types
            const string clientIdClaimType = "client_id";
            const string userIdClaimType = "user_id";
            const string userNameClaimType = "user_representation";

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                // Required
                Issuer = issuer,      // "iss"
                IssuedAt = issuedAt,  // "iat"
                Expires = expiresAt,  // "exp"

                Subject = new ClaimsIdentity(
                [
                    // Required
                    new Claim(clientIdClaimType, issuer),

                    // Optional: Used only for "audit trials"
                    new Claim(userIdClaimType, userId),
                    new Claim(userNameClaimType, userName)
                ]),

                // Required
                SigningCredentials = new SigningCredentials(securityKey, securityAlgorithm)
            };

            // TODO: Audience should be skipped because "OpenKlant" v1 is not yet ready to receive it
            if (!string.IsNullOrWhiteSpace(audience))
            {
                tokenDescriptor.Audience = audience;  // "aud"
            }

            return tokenDescriptor;
        }
    }
}