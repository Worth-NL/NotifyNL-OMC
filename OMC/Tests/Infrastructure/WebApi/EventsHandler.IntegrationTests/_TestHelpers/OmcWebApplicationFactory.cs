// © 2024, Worth Systems.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EventsHandler.Tests.Integration._TestHelpers
{
    internal sealed class OmcWebApplicationFactory : WebApplicationFactory<OmcApplication>
    {
        private readonly Dictionary<string, string> _envVars;

        internal OmcWebApplicationFactory(Dictionary<string, string> envVars)
        {
            _envVars = envVars;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Inject all env vars from launchSettings into the process before the app starts
            foreach ((string key, string value) in _envVars)
                Environment.SetEnvironmentVariable(key, value);

            builder.UseEnvironment("Development");
        }
    }
}
