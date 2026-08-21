// © 2024, Worth Systems.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;

namespace EventsHandler.Services.Configuration
{
    /// <summary>Serves the static scenario flow definitions used by the /status/flow diagram viewer.</summary>
    [ExcludeFromCodeCoverage]
    public sealed class ScenarioFlowService
    {
        private const string ResourceName = "EventsHandler.Services.Configuration.Data.ScenarioFlows.json";

        private static readonly Lazy<IReadOnlyList<ScenarioFlow>> s_scenarios = new(Load);

        /// <summary>All scenario flows, in display order.</summary>
        public IReadOnlyList<ScenarioFlow> GetAll() => s_scenarios.Value;

        private static IReadOnlyList<ScenarioFlow> Load()
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
                ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' was not found.");

            return JsonSerializer.Deserialize<List<ScenarioFlow>>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' deserialized to null.");
        }
    }
}
