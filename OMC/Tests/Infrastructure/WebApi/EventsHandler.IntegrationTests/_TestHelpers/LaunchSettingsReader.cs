// © 2024, Worth Systems.

using System.Text.Json;

namespace EventsHandler.Tests.Integration._TestHelpers
{
    internal static class LaunchSettingsReader
    {
        private const string ProfileName = "IIS Express (Workflow v2)";

        internal static Dictionary<string, string> GetEnvironmentVariables()
        {
            string path = ResolvelaunchSettingsPath();

            if (!File.Exists(path))
                return [];

            // JsonCommentHandling.Skip handles both // and /* */ style comments
            var options = new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip };
            using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(path), options);

            if (!doc.RootElement.TryGetProperty("profiles", out JsonElement profiles) ||
                !profiles.TryGetProperty(ProfileName, out JsonElement profile) ||
                !profile.TryGetProperty("environmentVariables", out JsonElement envVars))
            {
                return [];
            }

            return envVars.EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value.GetString() ?? string.Empty);
        }

        internal static bool HasRealValues(Dictionary<string, string> envVars)
        {
            return envVars.TryGetValue("OMC_AUTH_JWT_SECRET", out string? secret) &&
                   !string.IsNullOrWhiteSpace(secret);
        }

        private static string ResolvelaunchSettingsPath()
        {
            // Climb from bin/Debug|Release/net10.0 up to the solution root, then back down
            string? dir = AppContext.BaseDirectory;

            for (int i = 0; i < 10; i++)
            {
                string candidate = Path.Combine(dir ?? string.Empty,
                    "OMC", "Infrastructure", "WebApi", "EventsHandler", "Properties", "launchSettings.json");

                if (File.Exists(candidate))
                    return candidate;

                dir = Path.GetDirectoryName(dir);
            }

            return string.Empty;
        }
    }
}
