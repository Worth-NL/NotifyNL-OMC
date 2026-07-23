// © 2024, Worth Systems.

using EventsHandler.Services.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventsHandler.Controllers
{
    /// <summary>
    /// Serves configuration health-check and scenario flow data consumed by the standalone dashboard frontend.
    /// No authentication required — values of secrets are never exposed.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class ConfigurationStatusController : ControllerBase
    {
        private static readonly JsonSerializerOptions s_jsonOptions = new()
        {
            PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
        };

        /// <summary>
        /// Returns the static scenario flow definitions (Mermaid diagrams + metadata) used by the /status/flow viewer.
        /// </summary>
        [HttpGet("/status/scenarios")]
        public ActionResult<IReadOnlyList<ScenarioFlow>> Scenarios([FromServices] ScenarioFlowService scenarioFlowService)
            => Ok(scenarioFlowService.GetAll());

        /// <summary>
        /// Streams configuration check results as Server-Sent Events.
        /// </summary>
        [HttpGet("/status/stream")]
        public async Task StreamAsync([FromServices] ConfigurationCheckService checkService, CancellationToken ct)
        {
            Response.ContentType = "text/event-stream; charset=utf-8";
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("X-Accel-Buffering", "no");

            int total = 0, passed = 0;

            try
            {
                string startPayload = JsonSerializer.Serialize(new { total = checkService.GetExpectedTotal() }, s_jsonOptions);
                await Response.WriteAsync($"event: start\ndata: {startPayload}\n\n", ct);
                await Response.Body.FlushAsync(ct);

                await foreach (CheckResult check in checkService.RunChecksAsync(ct))
                {
                    string json = JsonSerializer.Serialize(check, s_jsonOptions);
                    await Response.WriteAsync($"data: {json}\n\n", ct);
                    await Response.Body.FlushAsync(ct);

                    total++;
                    if (check.Ok) passed++;
                }

                string summary = JsonSerializer.Serialize(
                    new { total, passed, failed = total - passed }, s_jsonOptions);
                await Response.WriteAsync($"event: complete\ndata: {summary}\n\n", ct);
                await Response.Body.FlushAsync(ct);
            }
            catch (OperationCanceledException)
            {
                // Client disconnected (tab closed / navigated away) — not an error.
            }
        }
    }
}
