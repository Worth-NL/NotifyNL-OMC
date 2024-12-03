// © 2023, Worth Systems.

using Microsoft.IdentityModel.Tokens;
using SecretsManager.Services.Authentication.Encryptions.Strategy.Interfaces;
using SecretsManager.Services.Authentication.Encryptions.Utilities;
using System.Security.Cryptography;

namespace SecretsManager.Services.Authentication.Encryptions.Strategy
{
    /// <inheritdoc cref="IJwtEncryptionStrategy"/>
    public sealed class AsymmetricEncryptionStrategy : IJwtEncryptionStrategy
    {
        /// <summary>
        /// Gets security asymmetric key from a generated private RSA key.
        /// </summary>
        SecurityKey IJwtEncryptionStrategy.GetSecurityKey(string secret)
        {
            var rsaKey = RSA.Create();

            byte[] privateRsaKey = GenerateRsaKey();

            // Loads the key into RSA component
            rsaKey.ImportRSAPrivateKey(privateRsaKey, out _);

            return new RsaSecurityKey(rsaKey);  // Asymmetric security key
        }

        private static byte[] GenerateRsaKey()
        {
            var rsaKey = RSA.Create();

            // NOTE: The "Private Key" contains private and public RSA keys
            byte[] privateKeyBytes = rsaKey.ExportRSAPrivateKey();

            SavePrivateKey(privateKeyBytes);

            return privateKeyBytes;
        }

        private static void SavePrivateKey(byte[] privateKeyBytes)
        {
            // Save into file
            File.WriteAllBytes(IJwtEncryptionStrategy.PrivateKeyPath, privateKeyBytes);
        }

        /// <inheritdoc cref="IJwtEncryptionStrategy.GetJwtToken(SecurityKey, string, string, DateTime, string, string)"/>
        string IJwtEncryptionStrategy.GetJwtToken(SecurityKey securityKey, string issuer, string audience, DateTime expiresAt, string userId, string userName)
        {
            return JwtTokenHandler.GetJwtToken(securityKey, issuer, audience, expiresAt, SecurityAlgorithms.RsaSha256, userId, userName);
        }

        /// <inheritdoc cref="IJwtEncryptionStrategy.GetJwtToken(SecurityKey, string, string, double, string, string)"/>
        string IJwtEncryptionStrategy.GetJwtToken(SecurityKey securityKey, string issuer, string audience, double expiresInMinutes, string userId, string userName)
        {
            return JwtTokenHandler.GetJwtToken(securityKey, issuer, audience, expiresInMinutes, SecurityAlgorithms.RsaSha256, userId, userName);
        }

        /// <inheritdoc cref="IJwtEncryptionStrategy.SaveJwtToken(string)"/>
        void IJwtEncryptionStrategy.SaveJwtToken(string jwtToken)
        {
            // Save the tokens in [...]/bin/Debug/net8.0
            File.WriteAllText(IJwtEncryptionStrategy.JwtTokensPath, jwtToken);
        }
    }
}