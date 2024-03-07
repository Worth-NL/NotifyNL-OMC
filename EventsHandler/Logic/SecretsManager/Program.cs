// © 2023, Worth Systems.

using Microsoft.IdentityModel.Tokens;
using SecretsManager.Services.Authentication.Encryptions.Strategy;
using SecretsManager.Services.Authentication.Encryptions.Strategy.Context;
using System.Globalization;

namespace SecretsManager
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // Determine for how long the JWT token should be valid
            DateTime validDateTime = GetJwtTokenValidity(args);

            // Create context
            var context = new EncryptionContext(new SymmetricEncryptionStrategy());

            // Get security key
            SecurityKey securityKey = context.GetSecurityKey(GetConfigValue("NOTIFY_AUTHORIZATION_JWT_SECRET"));

            // Generate JSON Web Token
            string jwtToken = context.GetJwtToken(
                securityKey,
                issuer: GetConfigValue("NOTIFY_AUTHORIZATION_JWT_ISSUER"),
                audience: GetConfigValue("NOTIFY_AUTHORIZATION_JWT_AUDIENCE"),
                expiresAt: validDateTime,
                userId: GetConfigValue("NOTIFY_AUTHORIZATION_JWT_USERID"),
                userRepresentation: GetConfigValue("NOTIFY_AUTHORIZATION_JWT_USERNAME"));

            // Write JWT tokens
            context.SaveJwtToken(jwtToken);
        }

        // TODO: Reuse the same DAO service to obtain these configurations from there. Avoid hardcoded variable names
        private static string GetConfigValue(string key)
        {
            return Environment.GetEnvironmentVariable(key)
                ?? throw new KeyNotFoundException($"The value of environment variable \"{key}\" is empty or missing");
        }

        private static DateTime GetJwtTokenValidity(IReadOnlyList<string> args)
        {
            double validForNextMinutes = 60;  // NOTE: Standard value, unless specified otherwise
            DateTime currentDateTime = DateTime.UtcNow;
            DateTime futureDateTime;

            // Using custom validity for JWT token
            if (args.Count != 0)
            {
                // Variant #1: Use specific validity date (e.g., valid until 31st of December 2023 at 23:59:59)
                if (DateTime.TryParse(args[0], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out futureDateTime))
                {
                    if (futureDateTime <= currentDateTime)
                    {
                        throw new ArgumentException("The expected date time should be from the future!");
                    }

                    Console.WriteLine($"The token will be valid until: {futureDateTime:F}");

                    return futureDateTime;
                }
                // Variant #2: Use specific range in minutes (e.g., valid for 120 minutes from now)
                else if (double.TryParse(args[0], out double minutes))
                {
                    validForNextMinutes = minutes;
                }
                else
                {
                    throw new FormatException("The expected program argument should be date time (e.g., 2023-12-31T23:59:59) or minutes!");
                }
            }

            futureDateTime = currentDateTime.AddMinutes(validForNextMinutes);
            
            Console.WriteLine($"The token will be valid for the next {validForNextMinutes} minutes");

            return futureDateTime;
        }
    }
}