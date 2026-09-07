// © 2024, Worth Systems.

namespace EventsHandler.Services.Configuration
{
    /// <summary>Describes a single scenario's decision flow for the /status/flow diagram viewer.</summary>
    /// <param name="Key">Stable identifier used for client-side routing and diagram click-navigation.</param>
    /// <param name="Icon">Emoji icon for the sidebar entry.</param>
    /// <param name="Name">English display name.</param>
    /// <param name="Nl">Dutch display name.</param>
    /// <param name="Channel">Grouping used for the sidebar badge (e.g. "zaken", "objecten", "besluiten", "overview").</param>
    /// <param name="Desc">One-paragraph description shown above the diagram.</param>
    /// <param name="Diagram">Mermaid flowchart source.</param>
    public sealed record ScenarioFlow(string Key, string Icon, string Name, string Nl, string Channel, string Desc, string Diagram);
}
