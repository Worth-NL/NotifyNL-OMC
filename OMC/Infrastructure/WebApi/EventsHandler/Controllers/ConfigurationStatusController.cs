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
        /// Returns an interactive scenario flow diagram for the Case Created (Zaak aangemaakt) scenario.
        /// </summary>
        [HttpGet("/status/flow")]
        public ContentResult Flow() => Content(FlowHtml, "text/html; charset=utf-8");

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

        private const string Html = """
            <!DOCTYPE html>
            <html lang="nl">
            <head>
              <meta charset="UTF-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>OMC — Configuration Check</title>
              <link rel="preconnect" href="https://fonts.googleapis.com">
              <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
              <link href="https://fonts.googleapis.com/css2?family=Sora:wght@400;500;600;700&display=swap" rel="stylesheet">
              <style>
                *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

                :root {
                  --orange:        #FF7A59;
                  --orange-light:  #FFF0EC;
                  --orange-border: #FF8E72;
                  --dark:          #1E2238;
                  --charcoal:      #2B2E43;
                  --muted:         #80818C;
                  --light-muted:   #C3C6D0;
                  --bg:            #F6F7FA;
                  --surface:       #FFFFFF;
                  --border:        #EEF0F3;
                  --border-strong: #D5DAE1;
                  --green:         #16a34a;
                  --green-bg:      #f0fdf4;
                  --green-border:  #bbf7d0;
                  --red:           #dc2626;
                  --red-bg:        #fff5f5;
                  --red-border:    #fecaca;
                }

                body {
                  font-family: 'Sora', ui-sans-serif, system-ui, sans-serif;
                  background: var(--bg);
                  color: var(--charcoal);
                  min-height: 100vh;
                }

                /* ── Top nav bar ── */
                .topbar {
                  background: var(--orange);
                  padding: 0 2rem;
                  height: 56px;
                  display: flex;
                  align-items: center;
                  justify-content: space-between;
                }
                .topbar-brand {
                  display: flex; align-items: center; gap: 0.6rem;
                  color: #fff; font-weight: 700; font-size: 0.85rem;
                  letter-spacing: 0.06em; text-transform: uppercase;
                  text-decoration: none;
                }
                .topbar-brand svg { flex-shrink: 0; }
                .topbar-link {
                  color: rgba(255,255,255,0.85); font-size: 0.8rem; font-weight: 500;
                  text-decoration: none; border: 1px solid rgba(255,255,255,0.4);
                  padding: 0.3rem 0.8rem; border-radius: 6px;
                  transition: background 0.15s, color 0.15s;
                }
                .topbar-link:hover { background: rgba(255,255,255,0.15); color: #fff; }

                /* ── Page layout ── */
                .container { max-width: 880px; margin: 0 auto; padding: 2.5rem 1.5rem 4rem; }

                /* ── Page header ── */
                .page-header { margin-bottom: 2rem; }
                .page-label {
                  font-size: 0.7rem; font-weight: 600; letter-spacing: 0.1em;
                  text-transform: uppercase; color: var(--orange); margin-bottom: 0.4rem;
                }
                .page-header h1 {
                  font-size: 1.75rem; font-weight: 700; color: var(--dark); line-height: 1.2;
                }
                .page-header p {
                  margin-top: 0.4rem; color: var(--muted); font-size: 0.875rem; font-weight: 400;
                }

                /* ── Progress ── */
                .progress-wrap { margin-top: 1.5rem; }
                .progress-track {
                  height: 5px; background: var(--border-strong); border-radius: 99px; overflow: hidden;
                }
                .progress-fill {
                  height: 100%; width: 0%; background: var(--orange); border-radius: 99px;
                  transition: width 0.35s ease;
                }
                .progress-label {
                  margin-top: 0.45rem; font-size: 0.78rem; font-weight: 500; color: var(--muted);
                }

                /* ── Group cards ── */
                .group {
                  background: var(--surface); border-radius: 12px;
                  border: 1px solid var(--border); margin-bottom: 0.75rem;
                  overflow: hidden;
                  box-shadow: 0 1px 4px rgb(30 34 56 / 0.05);
                }
                .group-header {
                  display: flex; align-items: center; gap: 0.65rem;
                  padding: 0.9rem 1.25rem; cursor: pointer; user-select: none;
                  border-bottom: 1px solid transparent;
                  transition: background 0.12s;
                }
                .group-header:hover { background: var(--bg); }
                .group-icon { font-size: 1rem; flex-shrink: 0; }
                .group-name {
                  flex: 1; font-size: 0.875rem; font-weight: 600; color: var(--dark);
                }
                .group-badge {
                  font-size: 0.7rem; font-weight: 600; padding: 0.2rem 0.6rem;
                  border-radius: 99px; white-space: nowrap;
                }
                .badge-checking { background: var(--border); color: var(--muted); }
                .badge-ok   { background: var(--green-bg); color: var(--green); border: 1px solid var(--green-border); }
                .badge-fail { background: var(--red-bg);   color: var(--red);   border: 1px solid var(--red-border); }

                /* ── Check rows ── */
                .checks { }
                .check {
                  display: flex; align-items: flex-start;
                  padding: 0.55rem 1.25rem; border-top: 1px solid var(--border);
                  font-size: 0.825rem; animation: fadeIn 0.15s ease;
                }
                .check-icon {
                  width: 1.3rem; flex-shrink: 0; font-style: normal;
                  font-weight: 700; padding-top: 1px;
                }
                .icon-ok   { color: var(--green); }
                .icon-fail { color: var(--red); }
                .check-body { flex: 1; min-width: 0; }
                .check-main { display: flex; align-items: baseline; }
                .check-name {
                  flex: 1; color: var(--charcoal); font-weight: 500;
                  padding-right: 0.75rem; white-space: nowrap;
                }
                .check-detail {
                  color: var(--muted); font-size: 0.75rem;
                  font-family: 'SFMono-Regular', 'Consolas', monospace;
                  max-width: 380px; overflow: hidden;
                  text-overflow: ellipsis; white-space: nowrap;
                }
                .check-detail.ok { color: var(--green); font-weight: 500; }
                .check-detail.fail {
                  color: var(--red); white-space: pre-wrap; word-break: break-all;
                  text-overflow: unset; max-width: 100%;
                  max-height: 8rem; overflow-y: auto; display: block;
                  margin-top: 0.25rem;
                  background: var(--red-bg); border: 1px solid var(--red-border);
                  padding: 0.4rem 0.6rem; border-radius: 6px;
                }
                .check-hint {
                  margin-top: 0.3rem; font-size: 0.7rem; font-weight: 500;
                  color: var(--orange); font-family: 'SFMono-Regular', 'Consolas', monospace;
                }
                .check-hint::before { content: "→ set: "; color: var(--muted); font-weight: 400; }

                /* ── Summary card ── */
                .summary {
                  margin-top: 1.5rem; padding: 1.75rem; text-align: center;
                  background: var(--surface); border-radius: 12px;
                  border: 1px solid var(--border);
                  box-shadow: 0 1px 4px rgb(30 34 56 / 0.05);
                  display: none;
                }
                .summary.visible { display: block; }
                .summary-title { font-size: 1.1rem; font-weight: 700; color: var(--dark); margin-bottom: 0.4rem; }
                .summary-sub   { color: var(--muted); font-size: 0.85rem; }

                @keyframes fadeIn {
                  from { opacity: 0; transform: translateY(-2px); }
                  to   { opacity: 1; transform: translateY(0); }
                }
              </style>
            </head>
            <body>

              <nav class="topbar">
                <a class="topbar-brand" href="https://www.notificatie.nl" target="_blank">
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 0 1-3.46 0"/>
                  </svg>
                  Notificatie.nl — OMC
                </a>
                <div style="display:flex;gap:0.5rem">
                  <a class="topbar-link" href="/status/flow">Scenario flow →</a>
                  <a class="topbar-link" href="/swagger" target="_blank">API docs ↗</a>
                </div>
              </nav>

              <div class="container">
                <div class="page-header">
                  <div class="page-label">Configuration status</div>
                  <h1>OMC Configuration Check</h1>
                  <p>Live status of all required settings and service connections. Secret values are never shown.</p>
                  <div class="progress-wrap">
                    <div class="progress-track"><div class="progress-fill" id="pFill"></div></div>
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
                const state = {};
                let total = 0, passed = 0;
                const EST = 44;

                function groupKey(n) { return n.replace(/[^a-z0-9]/gi, '-'); }

                function getGroup(name, icon) {
                  if (state[name]) return state[name];
                  const key = groupKey(name);
                  const div = document.createElement('div');
                  div.className = 'group';
                  div.innerHTML =
                    `<div class="group-header" onclick="toggle('${key}')">` +
                      `<span class="group-icon">${icon}</span>` +
                      `<span class="group-name">${name}</span>` +
                      `<span class="group-badge badge-checking" id="b-${key}">checking…</span>` +
                    `</div>` +
                    `<div class="checks" id="c-${key}"></div>`;
                  document.getElementById('groups').appendChild(div);
                  state[name] = { badgeEl: div.querySelector(`#b-${key}`), checksEl: div.querySelector(`#c-${key}`), ok: 0, fail: 0 };
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
                  const ok     = d.ok;
                  const detail = d.detail ? `<span class="check-detail ${ok ? 'ok' : 'fail'}">${esc(d.detail)}</span>` : '';
                  const hint   = (!ok && d.hint) ? `<div class="check-hint">${esc(d.hint)}</div>` : '';
                  row.innerHTML =
                    `<i class="check-icon ${ok ? 'icon-ok' : 'icon-fail'}">${ok ? '✓' : '✗'}</i>` +
                    `<div class="check-body">` +
                      `<div class="check-main"><span class="check-name">${esc(d.name)}</span>${ok ? detail : ''}</div>` +
                      (!ok ? detail : '') + hint +
                    `</div>`;
                  g.checksEl.appendChild(row);
                  ok ? g.ok++ : g.fail++;
                  total++; if (ok) passed++;
                  updateBadge(g);
                  updateProgress();
                }

                function esc(s) {
                  return String(s).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
                }

                function updateBadge(g) {
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
                  document.getElementById('pLabel').textContent = `All done — ${d.passed} passed, ${d.failed} failed`;

                  const s = document.getElementById('summary');
                  s.classList.add('visible');
                  const title = document.getElementById('sTitle');
                  const sub   = document.getElementById('sSub');
                  if (d.failed === 0) {
                    title.style.color = 'var(--green)';
                    title.textContent = '✅ Everything looks good';
                    sub.textContent = 'All required settings are present and services are reachable.';
                  } else {
                    title.style.color = 'var(--orange)';
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

        private const string FlowHtml = """
            <!DOCTYPE html>
            <html lang="nl">
            <head>
              <meta charset="UTF-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>OMC — Scenario Flows</title>
              <link rel="preconnect" href="https://fonts.googleapis.com">
              <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
              <link href="https://fonts.googleapis.com/css2?family=Sora:wght@400;500;600;700&display=swap" rel="stylesheet">
              <style>
                *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
                :root {
                  --orange:   #FF7A59;
                  --dark:     #1E2238;
                  --charcoal: #2B2E43;
                  --muted:    #80818C;
                  --bg:       #F6F7FA;
                  --surface:  #FFFFFF;
                  --border:   #EEF0F3;
                }
                body {
                  font-family: 'Sora', ui-sans-serif, system-ui, sans-serif;
                  background: var(--bg);
                  color: var(--charcoal);
                  min-height: 100vh;
                  display: flex;
                  flex-direction: column;
                }

                /* ── Topbar ── */
                .topbar {
                  background: var(--orange); padding: 0 2rem; height: 56px;
                  display: flex; align-items: center; justify-content: space-between;
                  flex-shrink: 0;
                }
                .topbar-brand {
                  display: flex; align-items: center; gap: 0.6rem;
                  color: #fff; font-weight: 700; font-size: 0.85rem;
                  letter-spacing: 0.06em; text-transform: uppercase; text-decoration: none;
                }
                .topbar-links { display: flex; gap: 0.5rem; }
                .topbar-link {
                  color: rgba(255,255,255,0.85); font-size: 0.8rem; font-weight: 500;
                  text-decoration: none; border: 1px solid rgba(255,255,255,0.4);
                  padding: 0.3rem 0.8rem; border-radius: 6px; transition: background 0.15s;
                }
                .topbar-link:hover { background: rgba(255,255,255,0.15); color: #fff; }

                /* ── Body layout: sidebar + main ── */
                .layout {
                  display: flex;
                  flex: 1;
                  overflow: hidden;
                  height: calc(100vh - 56px);
                }

                /* ── Sidebar ── */
                .sidebar {
                  width: 260px;
                  flex-shrink: 0;
                  background: var(--surface);
                  border-right: 1px solid var(--border);
                  display: flex;
                  flex-direction: column;
                  overflow-y: auto;
                }
                .sidebar-header {
                  padding: 1.25rem 1.25rem 0.6rem;
                  border-bottom: 1px solid var(--border);
                }
                .sidebar-label {
                  font-size: 0.65rem; font-weight: 700; letter-spacing: 0.1em;
                  text-transform: uppercase; color: var(--orange); margin-bottom: 0.25rem;
                }
                .sidebar-title {
                  font-size: 0.875rem; font-weight: 700; color: var(--dark);
                }
                .sidebar-sub {
                  font-size: 0.72rem; color: var(--muted); margin-top: 0.2rem; line-height: 1.4;
                }
                .scenario-list { padding: 0.6rem 0; flex: 1; }
                .scenario-btn {
                  display: flex; align-items: flex-start; gap: 0.65rem;
                  width: 100%; padding: 0.7rem 1.25rem;
                  background: none; border: none; cursor: pointer;
                  text-align: left; transition: background 0.1s;
                  border-left: 3px solid transparent;
                }
                .scenario-btn:hover { background: var(--bg); }
                .scenario-btn.active {
                  background: #FFF0EC;
                  border-left-color: var(--orange);
                }
                .scenario-icon { font-size: 1.1rem; flex-shrink: 0; margin-top: 1px; }
                .scenario-label { flex: 1; }
                .scenario-name {
                  font-size: 0.8rem; font-weight: 600; color: var(--dark); line-height: 1.3;
                }
                .scenario-nl {
                  font-size: 0.7rem; color: var(--muted); margin-top: 0.1rem;
                }
                .scenario-channel {
                  display: inline-block; font-size: 0.6rem; font-weight: 600;
                  letter-spacing: 0.05em; padding: 0.1rem 0.4rem; border-radius: 4px;
                  margin-top: 0.3rem; text-transform: uppercase;
                }
                .ch-zaken    { background: #dbeafe; color: #1d4ed8; }
                .ch-objecten { background: #d1fae5; color: #065f46; }
                .ch-besluiten{ background: #ede9fe; color: #6d28d9; }

                /* ── Legend strip ── */
                .legend {
                  display: flex; flex-wrap: wrap; gap: 0.4rem 1rem;
                  padding: 0.7rem 1.5rem;
                  background: var(--surface);
                  border-bottom: 1px solid var(--border);
                  font-size: 0.72rem; flex-shrink: 0;
                }
                .legend-item { display: flex; align-items: center; gap: 0.35rem; color: var(--muted); }
                .leg { width: 9px; height: 9px; border-radius: 50%; flex-shrink: 0; }

                /* ── Main panel ── */
                .main {
                  flex: 1;
                  display: flex;
                  flex-direction: column;
                  overflow: hidden;
                }
                .main-header {
                  padding: 1rem 1.5rem 0.8rem;
                  background: var(--surface);
                  border-bottom: 1px solid var(--border);
                  flex-shrink: 0;
                }
                .main-label {
                  font-size: 0.65rem; font-weight: 700; letter-spacing: 0.1em;
                  text-transform: uppercase; color: var(--orange); margin-bottom: 0.2rem;
                }
                .main-title {
                  font-size: 1.2rem; font-weight: 700; color: var(--dark);
                }
                .main-desc {
                  font-size: 0.78rem; color: var(--muted); margin-top: 0.25rem;
                }

                .diagram-area {
                  flex: 1;
                  overflow: auto;
                  padding: 1.5rem;
                  display: flex;
                  justify-content: center;
                  align-items: flex-start;
                }
                #diagram-wrap {
                  background: var(--surface);
                  border: 1px solid var(--border);
                  border-radius: 12px;
                  padding: 2rem;
                  box-shadow: 0 1px 8px rgb(30 34 56 / 0.06);
                  min-width: 300px;
                  transform-origin: top center;
                  transition: transform 0.15s ease;
                }
                #diagram-wrap svg { max-width: 100%; }
                #loading {
                  display: flex; align-items: center; gap: 0.6rem;
                  padding: 3rem; color: var(--muted); font-size: 0.85rem;
                }
                .spinner {
                  width: 18px; height: 18px;
                  border: 2px solid var(--border);
                  border-top-color: var(--orange);
                  border-radius: 50%;
                  animation: spin 0.6s linear infinite;
                }
                @keyframes spin { to { transform: rotate(360deg); } }

                /* ── Zoom bar ── */
                .zoom-bar {
                  position: fixed; bottom: 1.25rem; right: 1.25rem;
                  display: flex; gap: 0.3rem;
                  background: var(--surface); border: 1px solid var(--border);
                  border-radius: 8px; padding: 0.25rem;
                  box-shadow: 0 2px 8px rgb(0 0 0 / 0.08); z-index: 10;
                }
                .zoom-btn {
                  background: none; border: none; cursor: pointer;
                  width: 28px; height: 28px; border-radius: 5px;
                  font-size: 0.95rem; font-weight: 700; color: var(--charcoal);
                  display: flex; align-items: center; justify-content: center;
                  transition: background 0.1s;
                }
                .zoom-btn:hover { background: var(--bg); }
              </style>
            </head>
            <body>
              <nav class="topbar">
                <a class="topbar-brand" href="https://www.notificatie.nl" target="_blank">
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 0 1-3.46 0"/>
                  </svg>
                  Notificatie.nl — OMC
                </a>
                <div class="topbar-links">
                  <a class="topbar-link" href="/status">← Configuration</a>
                  <a class="topbar-link" href="/swagger" target="_blank">API docs ↗</a>
                </div>
              </nav>

              <div class="layout">
                <!-- Sidebar -->
                <aside class="sidebar">
                  <div class="sidebar-header">
                    <div class="sidebar-label">Decision flows</div>
                    <div class="sidebar-title">Scenarios</div>
                    <div class="sidebar-sub">Click a scenario to trace its decision path from webhook to outcome.</div>
                  </div>
                  <div class="scenario-list" id="scenarioList"></div>
                </aside>

                <!-- Main -->
                <div class="main">
                  <div class="legend">
                    <span class="legend-item"><span class="leg" style="background:#FF7A59"></span>Entry / webhook</span>
                    <span class="legend-item"><span class="leg" style="background:#1E2238"></span>Scenario selected</span>
                    <span class="legend-item"><span class="leg" style="background:#16a34a"></span>Success</span>
                    <span class="legend-item"><span class="leg" style="background:#dc2626"></span>Aborted (silent, no retry)</span>
                    <span class="legend-item"><span class="leg" style="background:#f59e0b"></span>Failure (retried)</span>
                    <span class="legend-item"><span class="leg" style="background:#6b7280"></span>Skipped</span>
                    <span class="legend-item"><span class="leg" style="background:#dbeafe;border:1px solid #93c5fd"></span>Processing step</span>
                  </div>
                  <div class="main-header">
                    <div class="main-label" id="mainLabel">Select a scenario</div>
                    <div class="main-title" id="mainTitle">—</div>
                    <div class="main-desc" id="mainDesc">Choose a scenario from the list on the left to see its decision flow.</div>
                  </div>
                  <div class="diagram-area">
                    <div id="diagram-wrap">
                      <div id="loading"><div class="spinner"></div>Initialising Mermaid…</div>
                    </div>
                  </div>
                </div>
              </div>

              <div class="zoom-bar">
                <button class="zoom-btn" onclick="zoom(1.2)" title="Zoom in">+</button>
                <button class="zoom-btn" onclick="zoom(1/1.2)" title="Zoom out">−</button>
                <button class="zoom-btn" onclick="resetZoom()" title="Reset" style="font-size:0.65rem;font-weight:800">⊙</button>
              </div>

              <script type="module">
                import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs';

                const THEME = {
                  theme: 'base',
                  themeVariables: {
                    primaryColor:       '#dbeafe',
                    primaryTextColor:   '#1E2238',
                    primaryBorderColor: '#93c5fd',
                    lineColor:          '#80818C',
                    secondaryColor:     '#F6F7FA',
                    tertiaryColor:      '#fff3cd',
                    fontSize:           '13px',
                    fontFamily:         "'Sora', ui-sans-serif, system-ui, sans-serif",
                  }
                };

                mermaid.initialize({ startOnLoad: false, ...THEME });

                // ── Shared tail used by case/task/message scenarios ─────────────
                const CHANNEL_TAIL = `
                    CHANNEL -- Email --> BUILD_E["Build NotifyData — Email\\ntemplate configured in Notify__TemplateId"]
                    CHANNEL -- SMS --> BUILD_S["Build NotifyData — SMS"]
                    CHANNEL -- Letter --> BUILD_L["Build NotifyData — Letter\\n+ full postal address"]
                    CHANNEL -- Both --> BUILD_B["Build NotifyData — Email + SMS (sequentially)"]
                    CHANNEL -- Unknown --> FAIL_C(["🔁 Failure\\nNo valid channel — retry"])
                    BUILD_E & BUILD_S & BUILD_L & BUILD_B --> NOTIFY["POST to Notify NL API"]
                    NOTIFY --> NR{"HTTP 201?"}
                    NR -- No --> RETRY(["🔁 Failure — retry"])
                    NR -- Yes --> SUCCESS(["✅ Citizen notified"])
                    style FAIL_C  fill:#f59e0b,color:#fff,stroke:none
                    style RETRY   fill:#f59e0b,color:#fff,stroke:none
                    style SUCCESS fill:#16a34a,color:#fff,stroke:none`;

                const SCENARIOS = [
                  {
                    key: 'case-created',
                    icon: '📄',
                    name: 'Case Created',
                    nl: 'Zaak aangemaakt',
                    channel: 'zaken',
                    desc: 'Triggered when a new case status is created with SerialNumber = 1 (first status ever). Routes via OpenZaak and OpenKlant to notify the citizen.',
                    diagram: `flowchart TD
                    WEBHOOK(["📨 Webhook\\nAction=Create / Channel=zaken / Resource=status"])
                    WEBHOOK --> GS["GET CaseStatus\\nOpenZaak — ResourceUri"]
                    GS --> GT["GET CaseStatusType\\nOpenZaak — status.TypeUri"]
                    GT --> CI{"IsNotification\\nExpected?"}
                    CI -- No --> AB1(["⏹ Aborted\\ninformeren = false"])
                    CI -- Yes --> SN{"StatusType\\nSerialNumber?"}
                    SN -- "> 1" --> OTHER["→ Updated / Closed scenario"]
                    SN -- "= 1" --> SCENARIO["🆕 CaseCreatedScenario"]
                    SCENARIO --> WL{"Case type in\\nZaakCreate_IDs\\n(or wildcard *)?"}
                    WL -- No --> AB2(["⏹ Aborted\\nnot whitelisted"])
                    WL -- Yes --> GC["GET Case\\nOpenZaak — MainObjectUri"]
                    GC --> GP["GET Party data\\nOpenKlant — case.Uri\\npersonalisation: naam + zaak.identificatie + zaak.omschrijving"]
                    GP --> CHANNEL{"Distribution\\nChannel?"}
                    ${CHANNEL_TAIL}
                    style WEBHOOK   fill:#FF7A59,color:#fff,stroke:none
                    style SCENARIO  fill:#1E2238,color:#fff,stroke:none
                    style AB1       fill:#dc2626,color:#fff,stroke:none
                    style AB2       fill:#dc2626,color:#fff,stroke:none`
                  },
                  {
                    key: 'case-updated',
                    icon: '🔄',
                    name: 'Case Status Updated',
                    nl: 'Zaakstatus bijgewerkt',
                    channel: 'zaken',
                    desc: 'Triggered when a new status is added to an existing case (SerialNumber > 1) that is not yet the final status. Includes status description in personalisation.',
                    diagram: `flowchart TD
                    WEBHOOK(["📨 Webhook\\nAction=Create / Channel=zaken / Resource=status"])
                    WEBHOOK --> GS["GET CaseStatus\\nOpenZaak — ResourceUri"]
                    GS --> GT["GET CaseStatusType\\nOpenZaak — status.TypeUri"]
                    GT --> CI{"IsNotification\\nExpected?"}
                    CI -- No --> AB1(["⏹ Aborted\\ninformeren = false"])
                    CI -- Yes --> SN{"StatusType\\nSerialNumber?"}
                    SN -- "= 1" --> OTHER1["→ CaseCreatedScenario"]
                    SN -- "> 1, IsFinal" --> OTHER2["→ CaseClosedScenario"]
                    SN -- "> 1, not final" --> SCENARIO["🔄 CaseStatusUpdatedScenario"]
                    SCENARIO --> WL{"Case type in\\nZaakUpdate_IDs\\n(or wildcard *)?"}
                    WL -- No --> AB2(["⏹ Aborted\\nnot whitelisted"])
                    WL -- Yes --> GC["GET Case\\nOpenZaak — MainObjectUri"]
                    GC --> GP["GET Party data\\nOpenKlant\\npersonalisation: naam + zaak + status.omschrijving"]
                    GP --> CHANNEL{"Distribution\\nChannel?"}
                    ${CHANNEL_TAIL}
                    style WEBHOOK   fill:#FF7A59,color:#fff,stroke:none
                    style SCENARIO  fill:#1E2238,color:#fff,stroke:none
                    style AB1       fill:#dc2626,color:#fff,stroke:none
                    style AB2       fill:#dc2626,color:#fff,stroke:none`
                  },
                  {
                    key: 'case-closed',
                    icon: '✅',
                    name: 'Case Closed',
                    nl: 'Zaak afgesloten',
                    channel: 'zaken',
                    desc: 'Triggered when the final status is reached (IsFinalStatus = true). Optionally fetches a CaseResultType to include the outcome description in the notification.',
                    diagram: `flowchart TD
                    WEBHOOK(["📨 Webhook\\nAction=Create / Channel=zaken / Resource=status"])
                    WEBHOOK --> GS["GET CaseStatus\\nOpenZaak — ResourceUri"]
                    GS --> GT["GET CaseStatusType\\nOpenZaak — status.TypeUri"]
                    GT --> CI{"IsNotification\\nExpected?"}
                    CI -- No --> AB1(["⏹ Aborted\\ninformeren = false"])
                    CI -- Yes --> SN{"StatusType\\nSerialNumber?"}
                    SN -- "= 1" --> OTHER1["→ CaseCreatedScenario"]
                    SN -- "> 1, not final" --> OTHER2["→ CaseStatusUpdatedScenario"]
                    SN -- "> 1, IsFinalStatus" --> SCENARIO["✅ CaseClosedScenario"]
                    SCENARIO --> WL{"Case type in\\nZaakClose_IDs\\n(or wildcard *)?"}
                    WL -- No --> AB2(["⏹ Aborted\\nnot whitelisted"])
                    WL -- Yes --> GC["GET Case\\nOpenZaak — MainObjectUri\\n(includes expanded.result.resultType)"]
                    GC --> CR{"case.Expanded\\n.Result.ResultType\\n!= null?"}
                    CR -- Yes --> GRT["GET CaseResultType\\nOpenZaak — resultType URI"]
                    CR -- No --> GP
                    GRT --> GP["GET Party data\\nOpenKlant\\npersonalisation: naam + zaak + status\\n+ zaak.resultaat.resultaatType.omschrijving"]
                    GP --> CHANNEL{"Distribution\\nChannel?"}
                    ${CHANNEL_TAIL}
                    style WEBHOOK   fill:#FF7A59,color:#fff,stroke:none
                    style SCENARIO  fill:#1E2238,color:#fff,stroke:none
                    style AB1       fill:#dc2626,color:#fff,stroke:none
                    style AB2       fill:#dc2626,color:#fff,stroke:none`
                  },
                  {
                    key: 'task-assigned',
                    icon: '📋',
                    name: 'Task Assigned',
                    nl: 'Taak toegewezen',
                    channel: 'objecten',
                    desc: 'Triggered when a new Object with ObjectType matching TaskObjectType_Uuid is received. Validates task status, identification type, case type whitelist, and notification permission before notifying.',
                    diagram: `flowchart TD
                    WEBHOOK(["📨 Webhook\\nAction=Create / Channel=objecten / Resource=object"])
                    WEBHOOK --> OT{"ObjectType UUID\\n== TaskObjectType_Uuid?"}
                    OT -- No --> OTHER["→ Message / KTO / Abort"]
                    OT -- Yes --> SCENARIO["📋 TaskAssignedScenario"]
                    SCENARIO --> GT["GET Task object\\nObjecten API — ResourceUri"]
                    GT --> CS{"task.Status\\n== Open?"}
                    CS -- No --> AB1(["⏹ Aborted\\nTask already closed"])
                    CS -- Yes --> IT{"task.Identification\\n.Type == BSN or KVK?"}
                    IT -- No --> AB2(["⏹ Aborted\\nUnsupported identification type"])
                    IT -- Yes --> GCS["GET CaseStatuses\\nOpenZaak — task.CaseUri"]
                    GCS --> GCT["GET last CaseType\\nOpenZaak — from statuses"]
                    GCT --> WL{"Case type in\\nTaskAssigned_IDs?"}
                    WL -- No --> AB3(["⏹ Aborted\\nnot whitelisted"])
                    WL -- Yes --> NP{"caseType\\n.IsNotificationExpected?"}
                    NP -- No --> AB4(["⏹ Aborted\\ninformeren = false on caseType"])
                    NP -- Yes --> GC["GET Case\\nOpenZaak — task.CaseUri"]
                    GC --> BSN{"Identification\\nType == BSN?"}
                    BSN -- Yes --> GPB["GET Party data\\nOpenKlant — case.Uri + BSN"]
                    BSN -- No --> GPK["GET Party data\\nOpenKlant — case.Uri + case.Identification"]
                    GPB & GPK --> CHANNEL{"Distribution\\nChannel?"}
                    ${CHANNEL_TAIL}
                    style WEBHOOK fill:#FF7A59,color:#fff,stroke:none
                    style SCENARIO fill:#1E2238,color:#fff,stroke:none
                    style AB1 fill:#dc2626,color:#fff,stroke:none
                    style AB2 fill:#dc2626,color:#fff,stroke:none
                    style AB3 fill:#dc2626,color:#fff,stroke:none
                    style AB4 fill:#dc2626,color:#fff,stroke:none`
                  },
                  {
                    key: 'message-received',
                    icon: '💬',
                    name: 'Message Received',
                    nl: 'Bericht ontvangen',
                    channel: 'objecten',
                    desc: 'Triggered when a direct message Object arrives (MessageObjectType_Uuid). Requires ZGW__Whitelist__Message_Allowed = true. Looks up the citizen by BSN from the message payload — no case is involved.',
                    diagram: `flowchart TD
                    WEBHOOK(["📨 Webhook\\nAction=Create / Channel=objecten / Resource=object"])
                    WEBHOOK --> OT{"ObjectType UUID\\n== MessageObjectType_Uuid?"}
                    OT -- No --> OTHER["→ Task / KTO / Abort"]
                    OT -- Yes --> SCENARIO["💬 MessageReceivedScenario"]
                    SCENARIO --> MA{"ZGW__Whitelist\\n__Message_Allowed\\n== true?"}
                    MA -- No --> AB1(["⏹ Aborted\\nMessages globally disabled"])
                    MA -- Yes --> GM["GET Message object\\nObjecten API — ResourceUri"]
                    GM --> EX["Extract message.Record.Data\\n→ Subject, ActionsPerspective, BSN"]
                    EX --> GP["GET Party data\\nOpenKlant — BSN from message\\n(no case URI — message has no linked case)"]
                    GP --> CHANNEL{"Distribution\\nChannel?"}
                    ${CHANNEL_TAIL}
                    style WEBHOOK  fill:#FF7A59,color:#fff,stroke:none
                    style SCENARIO fill:#1E2238,color:#fff,stroke:none
                    style AB1      fill:#dc2626,color:#fff,stroke:none`
                  },
                  {
                    key: 'decision-made',
                    icon: '⚖️',
                    name: 'Decision Made',
                    nl: 'Besluit genomen',
                    channel: 'besluiten',
                    desc: 'Most complex scenario. Validates document type, status, and confidentiality before processing. Unlike other scenarios, does NOT send an email/SMS directly — instead it writes a structured message object to the Objecten API for citizens to retrieve via their portal.',
                    diagram: `flowchart TD
                    WEBHOOK(["📨 Webhook\\nAction=Create / Channel=besluiten / Resource=besluit"])
                    WEBHOOK --> SCENARIO["⚖️ DecisionMadeScenario"]
                    SCENARIO --> GDR["GET DecisionResource\\nOpenBesluiten — ResourceUri"]
                    GDR --> GIO["GET InfoObject\\nOpenZaak — decisionResource.InfoObjectUri"]
                    GIO --> CIT{"infoObject.TypeUri UUID\\nin DecisionInfoObjectType_Uuids?"}
                    CIT -- No --> AB1(["⏹ Aborted\\nInfoObject type not in allowed set"])
                    CIT -- Yes --> CST{"infoObject.Status\\n== Definitive?"}
                    CST -- No --> AB2(["⏹ Aborted\\nDocument not yet definitive"])
                    CST -- Yes --> CCV{"infoObject.Confidentiality\\n== NonConfidential?"}
                    CCV -- No --> AB3(["⏹ Aborted\\nDocument is confidential"])
                    CCV -- Yes --> GD["GET Decision\\nOpenBesluiten — decisionResource.DecisionUri"]
                    GD --> GCS["GET CaseStatuses → last CaseType\\nOpenZaak — decision.CaseUri"]
                    GCS --> WL{"Case type in\\nDecisionMade_IDs?"}
                    WL -- No --> AB4(["⏹ Aborted\\nnot whitelisted"])
                    WL -- Yes --> NP{"caseType\\n.IsNotificationExpected?"}
                    NP -- No --> AB5(["⏹ Aborted\\ninformeren = false"])
                    NP -- Yes --> GB["GET BSN number\\nOpenZaak — decision.CaseUri\\n(empty if organisation)"]
                    GB --> GDT["GET DecisionType\\nOpenBesluiten"]
                    GDT --> GC["GET Case\\nOpenZaak — decision.CaseUri"]
                    GC --> GP["GET Party data\\nOpenKlant — case.Uri + BSN"]
                    GP --> CH{"Distribution\\nChannel?"}
                    CH -- Any --> PREV["GenerateTemplatePreviewAsync\\nNotify NL — render template locally"]
                    CH -- Unknown --> FAIL_C(["🔁 Failure\\nNo valid channel"])
                    PREV --> PK{"Preview OK?"}
                    PK -- No --> FAIL_P(["🔁 Failure\\nTemplate preview failed"])
                    PK -- Yes --> NL["ReplaceWhitespaces\\nnormalise newlines for Logius"]
                    NL --> GDOC["GET Documents\\nOpenBesluiten — linked to decision"]
                    GDOC --> FILT["Filter InfoObjects\\nkeep: Definitive + NonConfidential"]
                    FILT --> VU{"Valid URIs\\nfound?"}
                    VU -- No --> FAIL_D(["🔁 Failure\\nNo valid attachments"])
                    VU -- Yes --> CO["POST CreateObject\\nObjecten API\\nmessage JSON: onderwerp + berichtTekst\\n+ publicatiedatum + referentie + bijlages"]
                    CO --> OK{"HTTP 201?"}
                    OK -- No --> RETRY(["🔁 Failure — retry"])
                    OK -- Yes --> SUCCESS(["✅ Decision message stored\\nCitizen retrieves via portal"])
                    style WEBHOOK  fill:#FF7A59,color:#fff,stroke:none
                    style SCENARIO fill:#1E2238,color:#fff,stroke:none
                    style AB1 fill:#dc2626,color:#fff,stroke:none
                    style AB2 fill:#dc2626,color:#fff,stroke:none
                    style AB3 fill:#dc2626,color:#fff,stroke:none
                    style AB4 fill:#dc2626,color:#fff,stroke:none
                    style AB5 fill:#dc2626,color:#fff,stroke:none
                    style FAIL_C fill:#f59e0b,color:#fff,stroke:none
                    style FAIL_P fill:#f59e0b,color:#fff,stroke:none
                    style FAIL_D fill:#f59e0b,color:#fff,stroke:none
                    style RETRY  fill:#f59e0b,color:#fff,stroke:none
                    style SUCCESS fill:#16a34a,color:#fff,stroke:none`
                  },
                  {
                    key: 'kto',
                    icon: '⭐',
                    name: 'Customer Satisfaction (KTO)',
                    nl: 'Klanttevredenheidsonderzoek',
                    channel: 'objecten',
                    desc: 'Triggered when a KTO Object arrives (KtoObjectType_Uuid). Handled entirely outside the normal Notify NL pipeline — authenticates with OAuth2 and posts to an external KTO provider (e.g. Expoints).',
                    diagram: `flowchart TD
                    WEBHOOK(["📨 Webhook\\nAction=Create / Channel=objecten / Resource=object"])
                    WEBHOOK --> OT{"ObjectType UUID\\n== KtoObjectType_Uuid?"}
                    OT -- No --> OTHER["→ Task / Message / Abort"]
                    OT -- Yes --> SCENARIO["⭐ KtoScenario\\n(detected in NotifyProcessor\\nbefore normal pipeline)"]
                    SCENARIO --> FAC["KtoScenarioFactory.Create()\\nBuild HTTP client with\\nOAuth2 client-credentials config"]
                    FAC --> TOK["GET OAuth2 token\\nKTO__Auth__JWT__Issuer\\nClient ID + Secret → access_token"]
                    TOK --> TK{"Token acquired?"}
                    TK -- No --> AB1(["🔁 Failure\\nCannot authenticate with KTO provider"])
                    TK -- Yes --> POST["POST to KTO provider\\nKTO__Url\\nnotification payload + Bearer token"]
                    POST --> OK{"HTTP 2xx?"}
                    OK -- No --> RETRY(["🔁 Failure — retry"])
                    OK -- Yes --> SUCCESS(["✅ KTO survey triggered\\nat provider (e.g. Expoints)"])
                    NOTE["ℹ️ KTO bypasses OpenKlant,\\nOpenZaak, Notify NL entirely.\\nNo DistributionChannel routing."]
                    style WEBHOOK  fill:#FF7A59,color:#fff,stroke:none
                    style SCENARIO fill:#1E2238,color:#fff,stroke:none
                    style AB1      fill:#dc2626,color:#fff,stroke:none
                    style RETRY    fill:#f59e0b,color:#fff,stroke:none
                    style SUCCESS  fill:#16a34a,color:#fff,stroke:none
                    style NOTE     fill:#fff3cd,stroke:#ffc107,color:#555`
                  }
                ];

                // Build sidebar
                const list = document.getElementById('scenarioList');
                SCENARIOS.forEach(s => {
                  const btn = document.createElement('button');
                  btn.className = 'scenario-btn';
                  btn.id = 'btn-' + s.key;
                  const chClass = { zaken: 'ch-zaken', objecten: 'ch-objecten', besluiten: 'ch-besluiten' }[s.channel] || '';
                  btn.innerHTML =
                    `<span class="scenario-icon">${s.icon}</span>` +
                    `<span class="scenario-label">` +
                      `<span class="scenario-name">${s.name}</span>` +
                      `<span class="scenario-nl">${s.nl}</span>` +
                      `<span class="scenario-channel ${chClass}">${s.channel}</span>` +
                    `</span>`;
                  btn.onclick = () => selectScenario(s.key);
                  list.appendChild(btn);
                });

                let currentKey = null;
                let renderCounter = 0;

                async function selectScenario(key) {
                  if (key === currentKey) return;
                  currentKey = key;

                  // Update sidebar active state
                  document.querySelectorAll('.scenario-btn').forEach(b => b.classList.remove('active'));
                  document.getElementById('btn-' + key).classList.add('active');

                  const s = SCENARIOS.find(x => x.key === key);
                  document.getElementById('mainLabel').textContent = s.channel + ' / ' + s.nl;
                  document.getElementById('mainTitle').textContent = s.icon + ' ' + s.name;
                  document.getElementById('mainDesc').textContent = s.desc;

                  // Show loading
                  const wrap = document.getElementById('diagram-wrap');
                  wrap.innerHTML = '<div id="loading"><div class="spinner"></div>Rendering diagram…</div>';
                  resetZoom();

                  // Render
                  const id = 'mmd-' + (++renderCounter);
                  try {
                    const { svg } = await mermaid.render(id, s.diagram);
                    wrap.innerHTML = svg;
                  } catch (err) {
                    wrap.innerHTML = `<pre style="color:red;font-size:0.75rem;padding:1rem">${err}</pre>`;
                  }
                }

                // Auto-select first scenario
                selectScenario(SCENARIOS[0].key);
              </script>

              <script>
                let scale = 1;
                function zoom(factor) {
                  scale = Math.min(Math.max(scale * factor, 0.25), 3);
                  document.getElementById('diagram-wrap').style.transform = `scale(${scale})`;
                }
                function resetZoom() {
                  scale = 1;
                  const w = document.getElementById('diagram-wrap');
                  if (w) w.style.transform = 'scale(1)';
                }
              </script>
            </body>
            </html>
            """;
    }
}
