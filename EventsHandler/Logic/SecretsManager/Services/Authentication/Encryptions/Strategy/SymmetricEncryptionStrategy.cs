// © 2023, Worth Systems.

using Microsoft.IdentityModel.Tokens;
using SecretsManager.Constants;
using SecretsManager.Services.Authentication.Encryptions.Strategy.Interfaces;
using SecretsManager.Services.Authentication.Encryptions.Utilities;
using System.Security.Cryptography;
using System.Text;

namespace SecretsManager.Services.Authentication.Encryptions.Strategy
{
    /// <inheritdoc cref="IJwtEncryptionStrategy"/>
    public sealed class SymmetricEncryptionStrategy : IJwtEncryptionStrategy
    {
        /// <summary>
        /// Gets security symmetric key from a provided secret (password).
        /// </summary>
        SecurityKey IJwtEncryptionStrategy.GetSecurityKey(string secret)
        {
            byte[] privateRsaKey = GetHashedSecret(secret);

            return new SymmetricSecurityKey(privateRsaKey);
        }

        private static byte[] GetHashedSecret(string secret)
        {
            // Creates HMAC SHA 256 key handler with the given secret (encoding string => byte[])
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));

            // Retrieves HMAC key
            byte[] hmacKey = hmac.Key;
            
            /* NOTE: "0-padding" of the HMAC key (if the secret was shorter than 64 bytes). Thanks to that
                     even the keys from short passwords (secrets) can be used with HMAC SHA 256 algorithm */
            Array.Resize(ref hmacKey, 64);  // Good, long key = "shortKey__________________"
            
            return hmacKey;  // Valid 64 bytes key
        }

        /// <inheritdoc cref="IJwtEncryptionStrategy.GetJwtToken(SecurityKey, string, string, DateTime, string, string)"/>
        string IJwtEncryptionStrategy.GetJwtToken(SecurityKey securityKey, string issuer, string audience, DateTime expiresAt, string userId, string userName)
        {
            return JwtTokenHandler.GetJwtToken(securityKey, issuer, audience, expiresAt, SecurityAlgorithms.HmacSha256, userId, userName);
        }

        /// <inheritdoc cref="IJwtEncryptionStrategy.GetJwtToken(SecurityKey, string, string, double, string, string)"/>
        string IJwtEncryptionStrategy.GetJwtToken(SecurityKey securityKey, string issuer, string audience, double expiresInMinutes, string userId, string userName)
        {
            return JwtTokenHandler.GetJwtToken(securityKey, issuer, audience, expiresInMinutes, SecurityAlgorithms.HmacSha256, userId, userName);
        }

        /// <inheritdoc cref="IJwtEncryptionStrategy.SaveJwtToken(string)"/>
        void IJwtEncryptionStrategy.SaveJwtToken(string jwtToken)
        {
            // Save the tokens in [...]/bin/Debug/net7.0
            File.WriteAllText(DefaultValues.FileNames.JwtTokensPath, jwtToken);
        }
    }
}