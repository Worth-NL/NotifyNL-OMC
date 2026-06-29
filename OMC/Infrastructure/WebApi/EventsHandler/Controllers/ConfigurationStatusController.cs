// © 2024, Worth Systems.

using EventsHandler.Services.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventsHandler.Controllers
{
    /// <summary>
    /// Serves a live configuration health-check dashboard for operations engineers.
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
        /// Returns the HTML dashboard page.
        /// </summary>
        [HttpGet("/status")]
        public ContentResult Index() => Content(Html, "text/html; charset=utf-8");

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

        private const string Html = """
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="UTF-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>OMC Configuration Check</title>
              <style>
                *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
                body {
                  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                  background: #0f172a;
                  color: #e2e8f0;
                  min-height: 100vh;
                  padding-bottom: 4rem;
                }
                .container { max-width: 860px; margin: 0 auto; padding: 2rem 1.25rem; }

                /* ── Header ── */
                .header { margin-bottom: 2rem; }
                .header h1 {
                  font-size: 1.6rem; font-weight: 700; color: #f8fafc;
                  display: flex; align-items: center; gap: 0.5rem;
                }
                .header p { color: #94a3b8; margin-top: 0.3rem; font-size: 0.9rem; }
                .progress-wrap { margin-top: 1.25rem; }
                .progress-bar {
                  height: 6px; background: #1e293b; border-radius: 3px; overflow: hidden;
                }
                .progress-fill {
                  height: 100%; width: 0%; background: #3b82f6; border-radius: 3px;
                  transition: width 0.4s ease;
                }
                .progress-label { font-size: 0.8rem; color: #64748b; margin-top: 0.4rem; }

                /* ── Group card ── */
                .group {
                  background: #1e293b; border-radius: 0.75rem;
                  margin-bottom: 0.75rem; overflow: hidden;
                  border: 1px solid #334155;
                }
                .group-header {
                  display: flex; align-items: center; gap: 0.6rem;
                  padding: 0.9rem 1.25rem; cursor: pointer;
                  font-weight: 600; font-size: 0.9rem; letter-spacing: 0.01em;
                  user-select: none;
                }
                .group-header:hover { background: #243347; }
                .group-name { flex: 1; }
                .group-badge {
                  font-size: 0.72rem; padding: 0.2rem 0.55rem;
                  border-radius: 1rem; font-weight: 600; white-space: nowrap;
                }
                .badge-checking { background: #1e3050; color: #64748b; }
                .badge-ok       { background: #14532d; color: #4ade80; }
                .badge-warn     { background: #422006; color: #fb923c; }
                .badge-fail     { background: #450a0a; color: #f87171; }

                /* ── Check rows ── */
                .checks { border-top: 1px solid #334155; }
                .check {
                  display: flex; align-items: baseline; gap: 0;
                  padding: 0.5rem 1.25rem; border-bottom: 1px solid #1e3050;
                  font-size: 0.85rem; animation: fadeIn 0.15s ease;
                }
                .check:last-child { border-bottom: none; }
                .check-icon { width: 1.4rem; flex-shrink: 0; font-style: normal; }
                .check-name { flex: 1; color: #cbd5e1; padding: 0 0.5rem; }
                .check-detail {
                  color: #475569; font-size: 0.78rem;
                  font-family: 'SFMono-Regular', 'Consolas', monospace;
                  max-width: 320px; overflow: hidden;
                  text-overflow: ellipsis; white-space: nowrap;
                }
                .check-detail.ok   { color: #22c55e; }
                .check-detail.fail { color: #ef4444; }

                /* ── Summary ── */
                .summary {
                  margin-top: 1.5rem; padding: 1.5rem; text-align: center;
                  background: #1e293b; border-radius: 0.75rem; border: 1px solid #334155;
                  display: none;
                }
                .summary.visible { display: block; }
                .summary-title { font-size: 1.15rem; font-weight: 700; margin-bottom: 0.4rem; }
                .summary-sub   { color: #94a3b8; font-size: 0.88rem; }

                @keyframes fadeIn {
                  from { opacity: 0; transform: translateY(-3px); }
                  to   { opacity: 1; transform: translateY(0); }
                }
              </style>
            </head>
            <body>
              <div class="container">
                <div class="header">
                  <h1>🔧 OMC Configuration Check</h1>
                  <p>Live status of all required settings and service connections. Secret values are never shown.</p>
                  <div class="progress-wrap">
                    <div class="progress-bar"><div class="progress-fill" id="pFill"></div></div>
                    <div class="progress-label" id="pLabel">Connecting…</div>
                  </div>
                </div>

                <div id="groups"></div>
                <div class="summary" id="summary">
                  <div class="summary-title" id="sTitle"></div>
                  <div class="summary-sub"   id="sSub"></div>
                </div>
              </div>

              <script>
                const state = {};   // groupName → { el, checksEl, badgeEl, ok, fail }
                let total = 0, passed = 0;
                const EST = 44;    // rough total for progress bar

                function groupKey(name) { return name.replace(/[^a-z0-9]/gi, '-'); }

                function getGroup(name, icon) {
                  if (state[name]) return state[name];
                  const key = groupKey(name);
                  const div = document.createElement('div');
                  div.className = 'group';
                  div.innerHTML =
                    `<div class="group-header" onclick="toggle('${key}')">` +
                      `<span>${icon}</span>` +
                      `<span class="group-name">${name}</span>` +
                      `<span class="group-badge badge-checking" id="b-${key}">checking…</span>` +
                    `</div>` +
                    `<div class="checks" id="c-${key}"></div>`;
                  document.getElementById('groups').appendChild(div);
                  state[name] = {
                    badgeEl:  div.querySelector(`#b-${key}`),
                    checksEl: div.querySelector(`#c-${key}`),
                    ok: 0, fail: 0
                  };
                  return state[name];
                }

                function toggle(key) {
                  const el = document.getElementById('c-' + key);
                  if (el) el.style.display = el.style.display === 'none' ? '' : 'none';
                }

                function addCheck(d) {
                  const g = getGroup(d.group, d.icon || '📋');
                  const row = document.createElement('div');
                  row.className = 'check';
                  row.innerHTML =
                    `<i class="check-icon">${d.ok ? '✓' : '✗'}</i>` +
                    `<span class="check-name">${d.name}</span>` +
                    `<span class="check-detail ${d.ok ? 'ok' : 'fail'}">${d.detail || ''}</span>`;
                  g.checksEl.appendChild(row);
                  d.ok ? g.ok++ : g.fail++;
                  total++; if (d.ok) passed++;
                  updateBadge(d.group, g);
                  updateProgress();
                }

                function updateBadge(name, g) {
                  const b = g.badgeEl, t = g.ok + g.fail;
                  if (g.fail > 0) {
                    b.className = 'group-badge badge-fail';
                    b.textContent = `${g.ok}/${t} — ${g.fail} missing`;
                  } else {
                    b.className = 'group-badge badge-ok';
                    b.textContent = `${t}/${t} ✓`;
                  }
                }

                function updateProgress() {
                  document.getElementById('pFill').style.width = Math.min(100, total / EST * 100) + '%';
                  document.getElementById('pLabel').textContent = `${total} checks complete…`;
                }

                const src = new EventSource('/status/stream');

                src.onmessage = e => { try { addCheck(JSON.parse(e.data)); } catch {} };

                src.addEventListener('complete', e => {
                  src.close();
                  const d = JSON.parse(e.data);
                  document.getElementById('pFill').style.width = '100%';
                  document.getElementById('pLabel').textContent =
                    `All done — ${d.passed} passed, ${d.failed} failed`;

                  const s = document.getElementById('summary');
                  s.classList.add('visible');
                  const title = document.getElementById('sTitle');
                  const sub   = document.getElementById('sSub');
                  if (d.failed === 0) {
                    title.style.color = '#22c55e';
                    title.textContent = '✅ Everything looks good';
                    sub.textContent = 'All required settings are present and services are reachable.';
                  } else {
                    title.style.color = '#f59e0b';
                    title.textContent = `⚠️ ${d.failed} issue${d.failed !== 1 ? 's' : ''} found`;
                    sub.textContent = `${d.passed} of ${d.total} checks passed. Review the items marked ✗ above.`;
                  }
                });

                src.onerror = () => {
                  src.close();
                  document.getElementById('pLabel').textContent = 'Connection lost — refresh to retry';
                };
              </script>
            </body>
            </html>
            """;
    }
}
