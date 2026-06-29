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
                  background: var(--bg); color: var(--charcoal);
                  min-height: 100vh; display: flex; flex-direction: column;
                }
                .topbar {
                  background: var(--orange); padding: 0 2rem; height: 56px;
                  display: flex; align-items: center; justify-content: space-between; flex-shrink: 0;
                }
                .topbar-brand {
                  display: flex; align-items: center; gap: 0.6rem; color: #fff;
                  font-weight: 700; font-size: 0.85rem; letter-spacing: 0.06em;
                  text-transform: uppercase; text-decoration: none;
                }
                .topbar-links { display: flex; gap: 0.5rem; }
                .topbar-link {
                  color: rgba(255,255,255,0.85); font-size: 0.8rem; font-weight: 500;
                  text-decoration: none; border: 1px solid rgba(255,255,255,0.4);
                  padding: 0.3rem 0.8rem; border-radius: 6px; transition: background 0.15s;
                }
                .topbar-link:hover { background: rgba(255,255,255,0.15); color: #fff; }

                .layout { display: flex; flex: 1; overflow: hidden; height: calc(100vh - 56px); }

                /* ── Sidebar ── */
                .sidebar {
                  width: 248px; flex-shrink: 0; background: var(--surface);
                  border-right: 1px solid var(--border); display: flex;
                  flex-direction: column; overflow-y: auto;
                }
                .sidebar-hd {
                  padding: 1.1rem 1.1rem 0.6rem; border-bottom: 1px solid var(--border);
                }
                .sidebar-label {
                  font-size: 0.62rem; font-weight: 700; letter-spacing: 0.12em;
                  text-transform: uppercase; color: var(--orange); margin-bottom: 0.2rem;
                }
                .sidebar-title { font-size: 0.85rem; font-weight: 700; color: var(--dark); }
                .sidebar-hint  { font-size: 0.68rem; color: var(--muted); margin-top: 0.25rem; line-height: 1.4; }
                .scenario-list { padding: 0.4rem 0; }
                .sbtn {
                  display: flex; align-items: flex-start; gap: 0.55rem;
                  width: 100%; padding: 0.6rem 1.1rem; background: none; border: none;
                  cursor: pointer; text-align: left; border-left: 3px solid transparent;
                  transition: background 0.1s;
                }
                .sbtn:hover { background: var(--bg); }
                .sbtn.active { background: #FFF0EC; border-left-color: var(--orange); }
                .sbtn-icon { font-size: 1rem; flex-shrink: 0; margin-top: 1px; }
                .sbtn-name { font-size: 0.78rem; font-weight: 600; color: var(--dark); line-height: 1.3; }
                .sbtn-nl   { font-size: 0.68rem; color: var(--muted); }
                .sbtn-ch {
                  display: inline-block; font-size: 0.58rem; font-weight: 700;
                  letter-spacing: 0.04em; padding: 0.1rem 0.35rem; border-radius: 3px;
                  margin-top: 0.25rem; text-transform: uppercase;
                }
                .ch-zaken     { background: #dbeafe; color: #1d4ed8; }
                .ch-objecten  { background: #d1fae5; color: #065f46; }
                .ch-besluiten { background: #ede9fe; color: #6d28d9; }
                .ch-overview  { background: var(--border); color: var(--muted); }

                /* ── Main panel ── */
                .main { flex: 1; display: flex; flex-direction: column; overflow: hidden; }

                .legend {
                  display: flex; flex-wrap: wrap; gap: 0.35rem 1rem;
                  padding: 0.6rem 1.4rem; background: var(--surface);
                  border-bottom: 1px solid var(--border); font-size: 0.7rem; flex-shrink: 0;
                }
                .li { display: flex; align-items: center; gap: 0.3rem; color: var(--muted); }
                .ld { width: 9px; height: 9px; border-radius: 50%; flex-shrink: 0; }
                .ld-clickable {
                  width: 9px; height: 9px; border-radius: 2px; flex-shrink: 0;
                  background: var(--dark); outline: 2px dashed #FF7A59; outline-offset: 1px;
                }

                .main-hd {
                  padding: 0.9rem 1.4rem 0.7rem; background: var(--surface);
                  border-bottom: 1px solid var(--border); flex-shrink: 0;
                }
                .main-label { font-size: 0.62rem; font-weight: 700; letter-spacing: 0.12em; text-transform: uppercase; color: var(--orange); margin-bottom: 0.15rem; }
                .main-title { font-size: 1.15rem; font-weight: 700; color: var(--dark); }
                .main-desc  { font-size: 0.75rem; color: var(--muted); margin-top: 0.2rem; }

                .diagram-area {
                  flex: 1; overflow: auto; padding: 1.25rem;
                  display: flex; justify-content: flex-start; align-items: flex-start;
                }
                #dwrap {
                  background: var(--surface); border: 1px solid var(--border);
                  border-radius: 12px; padding: 1.75rem;
                  box-shadow: 0 1px 8px rgb(30 34 56 / 0.06);
                  transform-origin: top left; transition: transform 0.12s ease;
                  width: max-content; min-width: calc(100% - 2.5rem);
                }
                #dwrap svg { display: block; }

                #loading {
                  display: flex; align-items: center; gap: 0.6rem;
                  padding: 3rem; color: var(--muted); font-size: 0.82rem;
                }
                .spin {
                  width: 16px; height: 16px; border: 2px solid var(--border);
                  border-top-color: var(--orange); border-radius: 50%;
                  animation: sp 0.6s linear infinite; flex-shrink: 0;
                }
                @keyframes sp { to { transform: rotate(360deg); } }

                .zoom-bar {
                  position: fixed; bottom: 1.1rem; right: 1.1rem; display: flex; gap: 0.25rem;
                  background: var(--surface); border: 1px solid var(--border);
                  border-radius: 7px; padding: 0.2rem;
                  box-shadow: 0 2px 8px rgb(0 0 0 / 0.08); z-index: 20;
                }
                .zbtn {
                  background: none; border: none; cursor: pointer; width: 26px; height: 26px;
                  border-radius: 4px; font-size: 0.9rem; font-weight: 700; color: var(--charcoal);
                  display: flex; align-items: center; justify-content: center; transition: background 0.1s;
                }
                .zbtn:hover { background: var(--bg); }

                /* make Mermaid clickable nodes show pointer cursor */
                #dwrap svg g.node.clickable { cursor: pointer !important; }
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
                <aside class="sidebar">
                  <div class="sidebar-hd">
                    <div class="sidebar-label">Scenario flows</div>
                    <div class="sidebar-title">Decision diagrams</div>
                    <div class="sidebar-hint">Click a scenario — or click a dark node inside any diagram — to navigate between flows.</div>
                  </div>
                  <div class="scenario-list" id="sList"></div>
                </aside>

                <div class="main">
                  <div class="legend">
                    <span class="li"><span class="ld" style="background:#FF7A59"></span>Webhook / entry</span>
                    <span class="li"><span class="ld" style="background:#1E2238"></span>Scenario node</span>
                    <span class="li"><span class="ld" style="background:#16a34a"></span>Success</span>
                    <span class="li"><span class="ld" style="background:#dc2626"></span>Aborted (silent)</span>
                    <span class="li"><span class="ld" style="background:#f59e0b"></span>Failure (retry)</span>
                    <span class="li"><span class="ld" style="background:#6b7280"></span>Skipped</span>
                    <span class="li"><span class="ld-clickable"></span>Click to navigate →</span>
                  </div>
                  <div class="main-hd">
                    <div class="main-label" id="mLabel">overview</div>
                    <div class="main-title" id="mTitle">—</div>
                    <div class="main-desc"  id="mDesc">Select a scenario from the list.</div>
                  </div>
                  <div class="diagram-area">
                    <div id="dwrap">
                      <div id="loading"><div class="spin"></div>Initialising…</div>
                    </div>
                  </div>
                </div>
              </div>

              <div class="zoom-bar">
                <button class="zbtn" onclick="zoom(1.2)">+</button>
                <button class="zbtn" onclick="zoom(1/1.2)">−</button>
                <button class="zbtn" onclick="resetZoom()" style="font-size:0.6rem;font-weight:900">⊙</button>
              </div>

              <script type="module">
                import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs';

                mermaid.initialize({
                  startOnLoad: false,
                  securityLevel: 'loose',
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
                });

                // ── Global dispatcher called by Mermaid click events ──────────
                window.go = function(nodeId) {
                  const map = {
                    routing:       'routing',
                    case_created:  'case-created',
                    case_updated:  'case-updated',
                    case_closed:   'case-closed',
                    task_assigned: 'task-assigned',
                    msg_received:  'message-received',
                    decision_made: 'decision-made',
                    kto:           'kto',
                  };
                  if (map[nodeId]) select(map[nodeId]);
                };

                // ── Shared channel-routing + send tail ───────────────────────
                const TAIL = `
                    CHANNEL -- Email  --> BE["Build Email NotifyData\\ntemplate + naam + zaak fields"]
                    CHANNEL -- SMS    --> BS["Build SMS NotifyData\\nsame personalisation"]
                    CHANNEL -- Letter --> BL["Build Letter NotifyData\\n+ full postal address"]
                    CHANNEL -- Both   --> BB["Build Email + SMS\\n(sent sequentially)"]
                    CHANNEL -- Unknown --> FC(["🔁 Failure — unknown channel"])
                    BE & BS & BL & BB --> NL["POST Notify NL API\\n/v2/notifications/{method}"]
                    NL --> NR{"HTTP 201?"}
                    NR -- No  --> RT(["🔁 Failure — retry"])
                    NR -- Yes --> OK(["✅ Citizen notified"])
                    style FC fill:#f59e0b,color:#fff,stroke:none
                    style RT fill:#f59e0b,color:#fff,stroke:none
                    style OK fill:#16a34a,color:#fff,stroke:none`;

                // ── Clickable routing node appended to top of each diagram ───
                const HOME = `
                    routing(["🗺 Routing overview"])
                    click routing go
                    style routing fill:#EEF0F3,color:#2B2E43,stroke:#D5DAE1`;

                // ── Scenario definitions ─────────────────────────────────────
                const SCENARIOS = [
                  {
                    key: 'routing', icon: '🗺', name: 'Routing Overview',
                    nl: 'Gebeurtenisrouting', channel: 'overview',
                    desc: 'Top-level event routing. Click any dark scenario node to drill into its decision flow.',
                    diagram: `flowchart TD
                    WEBHOOK(["📨 OpenNotificaties webhook\\nPOST /api/v1/notifications"])
                    WEBHOOK --> DV["Deserialize + validate\\nNotificationEvent"]
                    DV --> TP{"Test ping?\\nChannel=Unknown, Resource=Unknown"}
                    TP -- Yes --> SK(["⏹ Skipped — connectivity check only"])
                    TP -- No  --> RV{"Route by\\nAction / Channel / Resource"}

                    RV -- "Channel = zaken\\nResource = status" --> CB["Case routing\\n──────────────\\nGET CaseStatus → CaseStatusType\\ncheck IsNotificationExpected"]
                    RV -- "Channel = objecten\\nResource = object" --> OB["Object routing\\n──────────────\\nmatch ObjectType UUID"]
                    RV -- "Channel = besluiten\\nResource = besluit" --> decision_made["⚖️ Decision Made\\n(Besluit genomen)"]
                    RV -- "No match" --> NI(["⏹ NotImplemented"])

                    CB -- "volgnummer = 1" --> case_created["🆕 Case Created\\n(Zaak aangemaakt)"]
                    CB -- "volgnummer > 1\\\\nniet eindStatus" --> case_updated["🔄 Case Status Updated\\n(Zaakstatus bijgewerkt)"]
                    CB -- "volgnummer > 1\\\\neindStatus = true" --> case_closed["✅ Case Closed\\n(Zaak afgesloten)"]

                    OB -- "= TaskObjectType UUID" --> task_assigned["📋 Task Assigned\\n(Taak toegewezen)"]
                    OB -- "= MessageObjectType UUID" --> msg_received["💬 Message Received\\n(Bericht ontvangen)"]
                    OB -- "= KtoObjectType UUID" --> kto["⭐ KTO\\n(Klanttevredenheidsonderzoek)"]
                    OB -- "Unknown UUID" --> AB(["⏹ Aborted"])

                    click case_created go
                    click case_updated go
                    click case_closed go
                    click task_assigned go
                    click msg_received go
                    click decision_made go
                    click kto go

                    style WEBHOOK       fill:#FF7A59,color:#fff,stroke:none
                    style case_created  fill:#1E2238,color:#fff,stroke:none
                    style case_updated  fill:#1E2238,color:#fff,stroke:none
                    style case_closed   fill:#1E2238,color:#fff,stroke:none
                    style task_assigned fill:#1E2238,color:#fff,stroke:none
                    style msg_received  fill:#1E2238,color:#fff,stroke:none
                    style decision_made fill:#1E2238,color:#fff,stroke:none
                    style kto           fill:#1E2238,color:#fff,stroke:none
                    style SK fill:#6b7280,color:#fff,stroke:none
                    style NI fill:#6b7280,color:#fff,stroke:none
                    style AB fill:#dc2626,color:#fff,stroke:none`
                  },
                  {
                    key: 'case-created', icon: '📄', name: 'Case Created',
                    nl: 'Zaak aangemaakt', channel: 'zaken',
                    desc: 'SerialNumber = 1 (first status ever on the case). Whitelist check → fetch case + party → notify by channel.',
                    diagram: `flowchart TD
                    ${HOME}
                    routing --> WEBHOOK
                    WEBHOOK(["📨 Webhook — Channel=zaken / Resource=status"])
                    WEBHOOK --> GS["GET CaseStatus\\nOpenZaak — ResourceUri"]
                    GS --> GT["GET CaseStatusType\\nOpenZaak — status.TypeUri"]
                    GT --> NE{"informeren?"}
                    NE -- No --> AB1(["⏹ Aborted — informeren = false"])
                    NE -- Yes --> SN{"statustype\\\\nvolgnummer?"}
                    SN -- "volgnummer > 1\\\\nniet eindStatus" --> case_updated["🔄 Case Status Updated →"]
                    SN -- "eindStatus = true" --> case_closed["✅ Case Closed →"]
                    SN -- "= 1" --> SCENARIO["🆕 CaseCreatedScenario"]
                    SCENARIO --> WL{"Case type in\\nZaakCreate_IDs\\n(or wildcard *)?"}
                    WL -- No --> AB2(["⏹ Aborted — not whitelisted"])
                    WL -- Yes --> GC["GET Case\\nOpenZaak — MainObjectUri"]
                    GC --> GP["GET Party data\\nOpenKlant — case.Uri\\npersonalisation: naam + zaak.identificatie + zaak.omschrijving"]
                    GP --> CHANNEL{"Distribution\\nChannel?"}
                    ${TAIL}
                    click case_updated go
                    click case_closed go
                    style WEBHOOK   fill:#FF7A59,color:#fff,stroke:none
                    style SCENARIO  fill:#1E2238,color:#fff,stroke:none
                    style case_updated fill:#1E2238,color:#fff,stroke:none
                    style case_closed  fill:#1E2238,color:#fff,stroke:none
                    style AB1 fill:#dc2626,color:#fff,stroke:none
                    style AB2 fill:#dc2626,color:#fff,stroke:none`
                  },
                  {
                    key: 'case-updated', icon: '🔄', name: 'Case Status Updated',
                    nl: 'Zaakstatus bijgewerkt', channel: 'zaken',
                    desc: 'SerialNumber > 1 and not IsFinalStatus. Includes status.omschrijving (CaseStatusType.Name) in personalisation.',
                    diagram: `flowchart TD
                    ${HOME}
                    routing --> WEBHOOK
                    WEBHOOK(["📨 Webhook — Channel=zaken / Resource=status"])
                    WEBHOOK --> GS["GET CaseStatus\\nOpenZaak — ResourceUri"]
                    GS --> GT["GET CaseStatusType\\nOpenZaak — status.TypeUri"]
                    GT --> NE{"informeren?"}
                    NE -- No --> AB1(["⏹ Aborted — informeren = false"])
                    NE -- Yes --> SN{"statustype\\\\nvolgnummer?"}
                    SN -- "volgnummer = 1" --> case_created["🆕 Case Created →"]
                    SN -- "eindStatus = true" --> case_closed["✅ Case Closed →"]
                    SN -- "volgnummer > 1\\\\nniet eindStatus" --> SCENARIO["🔄 CaseStatusUpdatedScenario"]
                    SCENARIO --> WL{"Case type in\\nZaakUpdate_IDs\\n(or wildcard *)?"}
                    WL -- No --> AB2(["⏹ Aborted — not whitelisted"])
                    WL -- Yes --> GC["GET Case\\nOpenZaak — MainObjectUri"]
                    GC --> GP["GET Party data\\nOpenKlant — case.Uri\\npersonalisation: naam + zaak + status.omschrijving"]
                    GP --> CHANNEL{"Distribution\\nChannel?"}
                    ${TAIL}
                    click case_created go
                    click case_closed go
                    style WEBHOOK   fill:#FF7A59,color:#fff,stroke:none
                    style SCENARIO  fill:#1E2238,color:#fff,stroke:none
                    style case_created fill:#1E2238,color:#fff,stroke:none
                    style case_closed  fill:#1E2238,color:#fff,stroke:none
                    style AB1 fill:#dc2626,color:#fff,stroke:none
                    style AB2 fill:#dc2626,color:#fff,stroke:none`
                  },
                  {
                    key: 'case-closed', icon: '✅', name: 'Case Closed',
                    nl: 'Zaak afgesloten', channel: 'zaken',
                    desc: 'IsFinalStatus = true. Optionally fetches CaseResultType if a result is set on the case — adds zaak.resultaat.resultaatType.omschrijving to personalisation.',
                    diagram: `flowchart TD
                    ${HOME}
                    routing --> WEBHOOK
                    WEBHOOK(["📨 Webhook — Channel=zaken / Resource=status"])
                    WEBHOOK --> GS["GET CaseStatus\\nOpenZaak — ResourceUri"]
                    GS --> GT["GET CaseStatusType\\nOpenZaak — status.TypeUri"]
                    GT --> NE{"informeren?"}
                    NE -- No --> AB1(["⏹ Aborted — informeren = false"])
                    NE -- Yes --> SN{"statustype\\\\nvolgnummer?"}
                    SN -- "volgnummer = 1" --> case_created["🆕 Case Created →"]
                    SN -- "volgnummer > 1\\\\nniet eindStatus" --> case_updated["🔄 Case Status Updated →"]
                    SN -- "eindStatus = true" --> SCENARIO["✅ CaseClosedScenario"]
                    SCENARIO --> WL{"Case type in\\nZaakClose_IDs\\n(or wildcard *)?"}
                    WL -- No --> AB2(["⏹ Aborted — not whitelisted"])
                    WL -- Yes --> GC["GET Case\\nOpenZaak — MainObjectUri\\n(includes expanded.result.resultType)"]
                    GC --> CR{"zaak.resultaat\\\\n.resultaattype != null?"}
                    CR -- Yes --> GR["GET CaseResultType\\nOpenZaak — resultType URI"]
                    CR -- No  --> GP
                    GR --> GP["GET Party data\\nOpenKlant — case.Uri\\npersonalisation: naam + zaak + status\\n+ zaak.resultaat.resultaatType.omschrijving"]
                    GP --> CHANNEL{"Distribution\\nChannel?"}
                    ${TAIL}
                    click case_created go
                    click case_updated go
                    style WEBHOOK   fill:#FF7A59,color:#fff,stroke:none
                    style SCENARIO  fill:#1E2238,color:#fff,stroke:none
                    style case_created fill:#1E2238,color:#fff,stroke:none
                    style case_updated fill:#1E2238,color:#fff,stroke:none
                    style AB1 fill:#dc2626,color:#fff,stroke:none
                    style AB2 fill:#dc2626,color:#fff,stroke:none`
                  },
                  {
                    key: 'task-assigned', icon: '📋', name: 'Task Assigned',
                    nl: 'Taak toegewezen', channel: 'objecten',
                    desc: 'ObjectType UUID matches TaskObjectType_Uuid. Four sequential validations: task open, BSN/KVK identification, case type whitelist, and IsNotificationExpected.',
                    diagram: `flowchart TD
                    ${HOME}
                    routing --> WEBHOOK
                    WEBHOOK(["📨 Webhook — Channel=objecten / Resource=object"])
                    WEBHOOK --> OT{"ObjectType UUID?"}
                    OT -- "= MessageObjectType UUID" --> msg_received["💬 Message Received →"]
                    OT -- "= KtoObjectType UUID" --> kto["⭐ KTO →"]
                    OT -- "Unknown" --> AB0(["⏹ Aborted — UUID not configured"])
                    OT -- "= TaskObjectType UUID" --> SCENARIO["📋 TaskAssignedScenario"]
                    SCENARIO --> GT["GET Task object\\nObjecten API — ResourceUri"]
                    GT --> CS{"taak.status\\\\n== Open?"}
                    CS -- No --> AB1(["⏹ Aborted — task already closed"])
                    CS -- Yes --> IT{"taak.identificatie.type\\\\n== BSN of KVK?"}
                    IT -- No --> AB2(["⏹ Aborted — unsupported id type"])
                    IT -- Yes --> GCS["GET CaseStatuses → last CaseType\\nOpenZaak — task.CaseUri"]
                    GCS --> WL{"Case type in\\nTaskAssigned_IDs?"}
                    WL -- No --> AB3(["⏹ Aborted — not whitelisted"])
                    WL -- Yes --> NP{"zaaktype\\\\n.informeren?"}
                    NP -- No --> AB4(["⏹ Aborted — informeren = false"])
                    NP -- Yes --> GC["GET Case\\nOpenZaak — task.CaseUri"]
                    GC --> BSN{"taak.identificatie.type\\\\n== BSN?"}
                    BSN -- Yes --> GPB["GET Party data\\nOpenKlant — case.Uri + BSN"]
                    BSN -- No  --> GPK["GET Party data\\nOpenKlant — case.Uri + case.Id\\npersonalisation: naam + taak.verloopdatum + taak.title + zaak"]
                    GPB & GPK --> CHANNEL{"Distribution\\nChannel?"}
                    ${TAIL}
                    click msg_received go
                    click kto go
                    style WEBHOOK   fill:#FF7A59,color:#fff,stroke:none
                    style SCENARIO  fill:#1E2238,color:#fff,stroke:none
                    style msg_received fill:#1E2238,color:#fff,stroke:none
                    style kto          fill:#1E2238,color:#fff,stroke:none
                    style AB0 fill:#dc2626,color:#fff,stroke:none
                    style AB1 fill:#dc2626,color:#fff,stroke:none
                    style AB2 fill:#dc2626,color:#fff,stroke:none
                    style AB3 fill:#dc2626,color:#fff,stroke:none
                    style AB4 fill:#dc2626,color:#fff,stroke:none`
                  },
                  {
                    key: 'message-received', icon: '💬', name: 'Message Received',
                    nl: 'Bericht ontvangen', channel: 'objecten',
                    desc: 'ObjectType UUID matches MessageObjectType_Uuid. Requires ZGW__Whitelist__Message_Allowed = true. Party is looked up by BSN from the message object — no case is involved.',
                    diagram: `flowchart TD
                    ${HOME}
                    routing --> WEBHOOK
                    WEBHOOK(["📨 Webhook — Channel=objecten / Resource=object"])
                    WEBHOOK --> OT{"ObjectType UUID?"}
                    OT -- "= TaskObjectType UUID" --> task_assigned["📋 Task Assigned →"]
                    OT -- "= KtoObjectType UUID" --> kto["⭐ KTO →"]
                    OT -- "Unknown" --> AB0(["⏹ Aborted — UUID not configured"])
                    OT -- "= MessageObjectType UUID" --> SCENARIO["💬 MessageReceivedScenario"]
                    SCENARIO --> MA{"ZGW__Whitelist\\n__Message_Allowed?"}
                    MA -- No --> AB1(["⏹ Aborted — messages disabled globally"])
                    MA -- Yes --> GM["GET Message object\\nObjecten API — ResourceUri"]
                    GM --> EX["Extract message.Record.Data\\n→ Subject, ActionsPerspective, BSN"]
                    EX --> GP["GET Party data\\nOpenKlant — BSN from message\\n(no case URI — message has no linked case)\\npersonalisation: naam + message.onderwerp + handelingsperspectief"]
                    GP --> CHANNEL{"Distribution\\nChannel?"}
                    ${TAIL}
                    click task_assigned go
                    click kto go
                    style WEBHOOK   fill:#FF7A59,color:#fff,stroke:none
                    style SCENARIO  fill:#1E2238,color:#fff,stroke:none
                    style task_assigned fill:#1E2238,color:#fff,stroke:none
                    style kto           fill:#1E2238,color:#fff,stroke:none
                    style AB0 fill:#dc2626,color:#fff,stroke:none
                    style AB1 fill:#dc2626,color:#fff,stroke:none`
                  },
                  {
                    key: 'decision-made', icon: '⚖️', name: 'Decision Made',
                    nl: 'Besluit genomen', channel: 'besluiten',
                    desc: 'Five validations on the decision document before processing. Does NOT call Notify NL directly — instead writes a structured message to the Objecten API for the citizen portal.',
                    diagram: `flowchart TD
                    ${HOME}
                    routing --> WEBHOOK
                    WEBHOOK(["📨 Webhook — Channel=besluiten / Resource=besluit"])
                    WEBHOOK --> SCENARIO["⚖️ DecisionMadeScenario"]
                    SCENARIO --> GDR["GET DecisionResource\\nOpenBesluiten — ResourceUri"]
                    GDR --> GIO["GET InfoObject\\nOpenZaak — decisionResource.InfoObjectUri"]
                    GIO --> CIT{"informatieobject.informatieobjecttype\\\\nUUID in DecisionInfoObjectType_Uuids?"}
                    CIT -- No --> AB1(["⏹ Aborted — InfoObject type not in allowed set"])
                    CIT -- Yes --> CST{"informatieobject.status\\\\n== definitief?"}
                    CST -- No --> AB2(["⏹ Aborted — document not yet definitive"])
                    CST -- Yes --> CCV{"informatieobject.vertrouwelijkheid\\\\n== openbaar?"}
                    CCV -- No --> AB3(["⏹ Aborted — document is confidential"])
                    CCV -- Yes --> GD["GET Decision\\nOpenBesluiten — decisionResource.DecisionUri"]
                    GD --> GCS["GET CaseStatuses → last CaseType\\nOpenZaak — decision.CaseUri"]
                    GCS --> WL{"Case type in\\nDecisionMade_IDs?"}
                    WL -- No --> AB4(["⏹ Aborted — not whitelisted"])
                    WL -- Yes --> NP{"zaaktype\\\\n.informeren?"}
                    NP -- No --> AB5(["⏹ Aborted — informeren = false"])
                    NP -- Yes --> GB["GET BSN number\\nOpenZaak — decision.CaseUri\\n(empty if organisation — no BSN)"]
                    GB --> GDT["GET DecisionType\\nOpenBesluiten"]
                    GDT --> GC["GET Case\\nOpenZaak — decision.CaseUri"]
                    GC --> GP["GET Party data\\nOpenKlant — case.Uri + BSN"]
                    GP --> CH{"Distribution\\nChannel?"}
                    CH -- "Any channel" --> PV["GenerateTemplatePreviewAsync\\nNotify NL — render besluit template locally"]
                    CH -- Unknown --> FC(["🔁 Failure — unknown channel"])
                    PV --> PK{"Preview OK?"}
                    PK -- No --> FP(["🔁 Failure — template preview failed"])
                    PK -- Yes --> NWL["ReplaceWhitespaces\\nnormalise newlines for Logius (\\\\r\\\\n)"]
                    NWL --> GDOC["GET Documents\\nOpenBesluiten — all docs linked to decision"]
                    GDOC --> FLT["Filter InfoObjects\\nkeep: Status=Definitive AND NonConfidential"]
                    FLT --> VU{"Valid attachment\\nURIs found?"}
                    VU -- No --> FD(["🔁 Failure — no valid attachments"])
                    VU -- Yes --> CO["POST CreateObject\\nObjecten API — message JSON\\n(onderwerp + berichtTekst + publicatiedatum\\n+ referentie + identificatie + bijlages)"]
                    CO --> OR{"HTTP 201?"}
                    OR -- No --> RT(["🔁 Failure — retry"])
                    OR -- Yes --> SUC(["✅ Decision message stored in Objecten\\nCitizen reads via portal"])
                    style WEBHOOK  fill:#FF7A59,color:#fff,stroke:none
                    style SCENARIO fill:#1E2238,color:#fff,stroke:none
                    style AB1 fill:#dc2626,color:#fff,stroke:none
                    style AB2 fill:#dc2626,color:#fff,stroke:none
                    style AB3 fill:#dc2626,color:#fff,stroke:none
                    style AB4 fill:#dc2626,color:#fff,stroke:none
                    style AB5 fill:#dc2626,color:#fff,stroke:none
                    style FC  fill:#f59e0b,color:#fff,stroke:none
                    style FP  fill:#f59e0b,color:#fff,stroke:none
                    style FD  fill:#f59e0b,color:#fff,stroke:none
                    style RT  fill:#f59e0b,color:#fff,stroke:none
                    style SUC fill:#16a34a,color:#fff,stroke:none`
                  },
                  {
                    key: 'kto', icon: '⭐', name: 'Customer Satisfaction (KTO)',
                    nl: 'Klanttevredenheidsonderzoek', channel: 'objecten',
                    desc: 'ObjectType UUID matches KtoObjectType_Uuid. Handled before the normal pipeline in NotifyProcessor — authenticates with OAuth2 client credentials and posts to an external KTO provider.',
                    diagram: `flowchart TD
                    ${HOME}
                    routing --> WEBHOOK
                    WEBHOOK(["📨 Webhook — Channel=objecten / Resource=object"])
                    WEBHOOK --> OT{"ObjectType UUID?"}
                    OT -- "= TaskObjectType UUID" --> task_assigned["📋 Task Assigned →"]
                    OT -- "= MessageObjectType UUID" --> msg_received["💬 Message Received →"]
                    OT -- "Unknown" --> AB0(["⏹ Aborted — UUID not configured"])
                    OT -- "= KtoObjectType UUID" --> SCENARIO["⭐ KtoScenario\\n(detected in NotifyProcessor\\nbefore normal pipeline)"]
                    SCENARIO --> FAC["KtoScenarioFactory.Create()\\nBuild HTTP client with OAuth2 config\\nKTO__Auth__JWT__ClientId + Secret + Scope"]
                    FAC --> TOK["POST OAuth2 token request\\nKTO__Auth__JWT__Issuer\\n→ access_token (client credentials)"]
                    TOK --> TK{"Token acquired?"}
                    TK -- No --> AB1(["🔁 Failure — cannot authenticate"])
                    TK -- Yes --> POST["POST to KTO provider\\nKTO__Url\\nnotification payload + Bearer token"]
                    POST --> OK{"HTTP 2xx?"}
                    OK -- No  --> RT(["🔁 Failure — retry"])
                    OK -- Yes --> SUC(["✅ KTO survey triggered at provider"])
                    NOTE["ℹ️ KTO bypasses OpenKlant, OpenZaak, and\\nNotify NL entirely. No DistributionChannel routing."]
                    click task_assigned go
                    click msg_received go
                    style WEBHOOK   fill:#FF7A59,color:#fff,stroke:none
                    style SCENARIO  fill:#1E2238,color:#fff,stroke:none
                    style task_assigned fill:#1E2238,color:#fff,stroke:none
                    style msg_received  fill:#1E2238,color:#fff,stroke:none
                    style AB0 fill:#dc2626,color:#fff,stroke:none
                    style AB1 fill:#dc2626,color:#fff,stroke:none
                    style RT  fill:#f59e0b,color:#fff,stroke:none
                    style SUC fill:#16a34a,color:#fff,stroke:none
                    style NOTE fill:#fff3cd,stroke:#ffc107,color:#555`
                  }
                ];

                // ── Build sidebar ─────────────────────────────────────────────
                const list = document.getElementById('sList');
                SCENARIOS.forEach(s => {
                  const btn = document.createElement('button');
                  btn.className = 'sbtn'; btn.id = 'btn-' + s.key;
                  const chCls = { zaken:'ch-zaken', objecten:'ch-objecten', besluiten:'ch-besluiten', overview:'ch-overview' }[s.channel] || '';
                  btn.innerHTML =
                    `<span class="sbtn-icon">${s.icon}</span>` +
                    `<span><span class="sbtn-name">${s.name}</span>` +
                    `<br><span class="sbtn-nl">${s.nl}</span>` +
                    (s.channel !== 'overview' ? `<br><span class="sbtn-ch ${chCls}">${s.channel}</span>` : '') +
                    `</span>`;
                  btn.onclick = () => select(s.key);
                  list.appendChild(btn);
                });

                // ── Rendering ─────────────────────────────────────────────────
                let current = null;
                let ctr = 0;

                // Wire native click listeners onto Mermaid-rendered SVG nodes.
                // Mermaid marks click-enabled nodes with class "clickable" on the <g>.
                // It also generates IDs like "flowchart-nodeId-N" from which we recover the node ID.
                function wireClicks(wrap) {
                  const svg = wrap.querySelector('svg');
                  if (!svg) return;
                  const scenarioKeys = {
                    routing:       'routing',
                    case_created:  'case-created',
                    case_updated:  'case-updated',
                    case_closed:   'case-closed',
                    task_assigned: 'task-assigned',
                    msg_received:  'message-received',
                    decision_made: 'decision-made',
                    kto:           'kto',
                  };
                  svg.querySelectorAll('g.node.clickable').forEach(g => {
                    const m = (g.id || '').match(/^flowchart-(.+)-\d+$/);
                    if (!m) return;
                    const key = scenarioKeys[m[1]];
                    if (!key) return;
                    g.addEventListener('click', () => select(key));
                  });
                }

                async function select(key) {
                  if (key === current) return;
                  current = key;
                  document.querySelectorAll('.sbtn').forEach(b => b.classList.remove('active'));
                  document.getElementById('btn-' + key)?.classList.add('active');

                  const s = SCENARIOS.find(x => x.key === key);
                  document.getElementById('mLabel').textContent = s.channel === 'overview' ? 'overview' : s.channel + ' channel';
                  document.getElementById('mTitle').textContent = s.icon + ' ' + s.name;
                  document.getElementById('mDesc').textContent  = s.desc;

                  const wrap = document.getElementById('dwrap');
                  wrap.innerHTML = '<div id="loading"><div class="spin"></div>Rendering…</div>';
                  resetZoom();

                  const id = 'mmd' + (++ctr);
                  try {
                    const { svg } = await mermaid.render(id, s.diagram);
                    wrap.innerHTML = svg;
                    wireClicks(wrap);
                    applyZoom(1.5);
                  } catch (err) {
                    wrap.innerHTML = `<pre style="color:red;font-size:0.72rem;white-space:pre-wrap;padding:1rem">${err}</pre>`;
                  }
                }

                // expose so zoom script can call resetZoom
                window._selectScenario = select;
                select('routing');
              </script>

              <script>
                let scale = 1;
                function applyZoom(s) {
                  scale = Math.min(Math.max(s, 0.3), 5);
                  document.getElementById('dwrap').style.transform = `scale(${scale})`;
                }
                function zoom(f) { applyZoom(scale * f); }
                function resetZoom() { applyZoom(1.5); }
              </script>
            </body>
            </html>
            """;
    }
}
