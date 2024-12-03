// © 2023, Worth Systems.

using Microsoft.IdentityModel.Tokens;
using SecretsManager.Services.Authentication.Encryptions.Strategy.Interfaces;

namespace SecretsManager.Services.Authentication.Encryptions.Strategy.Context
{
    /// <summary>
    /// The context on which a specific <see cref="IJwtEncryptionStrategy"/> will be executed.
    /// </summary>
    public sealed class EncryptionContext
    {
        /// <summary>
        /// The current encryption strategy.
        /// </summary>
        private IJwtEncryptionStrategy Strategy { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionContext"/> class.
        /// </summary>
        /// <param name="strategy">The encryption strategy to be used.</param>
        public EncryptionContext(IJwtEncryptionStrategy strategy)
        {
            this.Strategy = strategy;
        }

        /// <inheritdoc cref="IJwtEncryptionStrategy.GetSecurityKey(string)"/>
        public SecurityKey GetSecurityKey(string secret = "")
        {
            return this.Strategy.GetSecurityKey(secret);
        }

        /// <inheritdoc cref="IJwtEncryptionStrategy.GetJwtToken(SecurityKey, string, string, DateTime, string, string)"/>
        public string GetJwtToken(SecurityKey securityKey, string issuer, string audience, DateTime expiresAt, string userId, string userName)
        {
            return this.Strategy.GetJwtToken(securityKey, issuer, audience, expiresAt, userId, userName);
        }

        /// <inheritdoc cref="IJwtEncryptionStrategy.GetJwtToken(SecurityKey, string, string, double, string, string)"/>
        public string GetJwtToken(SecurityKey securityKey, string issuer, string audience, double expiresInMinutes, string userId, string userName)
        {
            return this.Strategy.GetJwtToken(securityKey, issuer, audience, expiresInMinutes, userId, userName);
        }

        /// <inheritdoc cref="IJwtEncryptionStrategy.SaveJwtToken(string)"/>
        internal void SaveJwtToken(string jwtToken)
        {
            this.Strategy.SaveJwtToken(jwtToken);
        }
    }
}