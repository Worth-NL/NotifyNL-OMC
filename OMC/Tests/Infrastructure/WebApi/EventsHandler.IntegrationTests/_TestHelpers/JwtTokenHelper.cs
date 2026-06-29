// © 2024, Worth Systems.

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EventsHandler.Tests.Integration._TestHelpers
{
    internal static class JwtTokenHelper
    {
        /// <summary>
        /// Mints a valid OMC JWT token using the same algorithm as <c>SymmetricEncryptionStrategy</c>.
        /// </summary>
        internal static string GenerateToken(Dictionary<string, string> envVars)
            => BuildToken(envVars, DateTime.UtcNow.AddHours(1));

        internal static string GenerateExpiredToken(Dictionary<string, string> envVars)
            => BuildToken(envVars, DateTime.UtcNow.AddHours(-1));

        private static string BuildToken(Dictionary<string, string> envVars, DateTime expires)
        {
            string secret   = envVars["OMC_AUTH_JWT_SECRET"];
            string issuer   = envVars["OMC_AUTH_JWT_ISSUER"];
            string audience = envVars["OMC_AUTH_JWT_AUDIENCE"];
            string userId   = envVars["OMC_AUTH_JWT_USERID"];
            string userName = envVars["OMC_AUTH_JWT_USERNAME"];

            // Replicate SymmetricEncryptionStrategy.GetHashedSecret()
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            byte[] key = hmac.Key;
            Array.Resize(ref key, 64);
            var securityKey = new SymmetricSecurityKey(key);

            // Replicate JwtTokenHandler.GetJwtToken()
            var handler = new JsonWebTokenHandler { SetDefaultTimesOnTokenCreation = false };

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer    = issuer,
                IssuedAt  = DateTime.UtcNow,
                Expires   = expires,
                Subject   = new ClaimsIdentity([
                    new Claim("client_id",          issuer),
                    new Claim("user_id",            userId),
                    new Claim("user_representation", userName)
                ]),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
            };

            if (!string.IsNullOrWhiteSpace(audience))
                descriptor.Audience = audience;

            return handler.CreateToken(descriptor);
        }
    }
}
