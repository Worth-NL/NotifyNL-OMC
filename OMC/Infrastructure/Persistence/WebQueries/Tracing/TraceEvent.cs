// © 2026, Worth Systems.

namespace WebQueries.Tracing
{
    /// <summary>
    /// A single step in a notification's real-time journey through the OMC pipeline, broadcast
    /// to the dashboard over Server-Sent Events. Structural only — no case identifiers, BSNs,
    /// or other citizen data ever leave the process through this channel.
    /// </summary>
    /// <param name="TraceId">Identifies which notification's journey this step belongs to.</param>
    /// <param name="Stage">Matches a node key in the dashboard's architecture graph (e.g. "zaaktypewhitelist", "kanaalresolutie").</param>
    /// <param name="Status">One of "start", "ok", "fail", "abort".</param>
    /// <param name="Scenario">The matched FlowOption key (e.g. "case-created"), once routing has determined it.</param>
    /// <param name="Detail">Optional short, non-sensitive context (e.g. which channel was resolved).</param>
    /// <param name="ElapsedMs">Milliseconds since this trace started, for client-side pacing.</param>
    public sealed record TraceEvent(
        string TraceId,
        string Stage,
        string Status,
        string? Scenario,
        string? Detail,
        long ElapsedMs);
}
