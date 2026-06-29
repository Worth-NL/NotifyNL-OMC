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
              <title>OMC — Case Created Flow</title>
              <link rel="preconnect" href="https://fonts.googleapis.com">
              <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
              <link href="https://fonts.googleapis.com/css2?family=Sora:wght@400;500;600;700&display=swap" rel="stylesheet">
              <style>
                *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

                :root {
                  --orange:  #FF7A59;
                  --dark:    #1E2238;
                  --charcoal:#2B2E43;
                  --muted:   #80818C;
                  --bg:      #F6F7FA;
                  --surface: #FFFFFF;
                  --border:  #EEF0F3;
                }

                body {
                  font-family: 'Sora', ui-sans-serif, system-ui, sans-serif;
                  background: var(--bg);
                  color: var(--charcoal);
                  min-height: 100vh;
                  display: flex;
                  flex-direction: column;
                }

                .topbar {
                  background: var(--orange);
                  padding: 0 2rem;
                  height: 56px;
                  display: flex;
                  align-items: center;
                  justify-content: space-between;
                  flex-shrink: 0;
                }
                .topbar-brand {
                  display: flex; align-items: center; gap: 0.6rem;
                  color: #fff; font-weight: 700; font-size: 0.85rem;
                  letter-spacing: 0.06em; text-transform: uppercase;
                  text-decoration: none;
                }
                .topbar-brand svg { flex-shrink: 0; }
                .topbar-links { display: flex; gap: 0.5rem; }
                .topbar-link {
                  color: rgba(255,255,255,0.85); font-size: 0.8rem; font-weight: 500;
                  text-decoration: none; border: 1px solid rgba(255,255,255,0.4);
                  padding: 0.3rem 0.8rem; border-radius: 6px;
                  transition: background 0.15s;
                }
                .topbar-link:hover { background: rgba(255,255,255,0.15); color: #fff; }

                .page-header {
                  padding: 2rem 2rem 1rem;
                  border-bottom: 1px solid var(--border);
                  background: var(--surface);
                }
                .page-label {
                  font-size: 0.7rem; font-weight: 600; letter-spacing: 0.1em;
                  text-transform: uppercase; color: var(--orange); margin-bottom: 0.35rem;
                }
                .page-header h1 {
                  font-size: 1.5rem; font-weight: 700; color: var(--dark);
                }
                .page-header p {
                  margin-top: 0.3rem; color: var(--muted); font-size: 0.85rem;
                }

                .legend {
                  display: flex; flex-wrap: wrap; gap: 0.5rem 1.25rem;
                  padding: 0.9rem 2rem; background: var(--surface);
                  border-bottom: 1px solid var(--border); font-size: 0.78rem;
                }
                .legend-item { display: flex; align-items: center; gap: 0.4rem; }
                .legend-dot {
                  width: 10px; height: 10px; border-radius: 50%; flex-shrink: 0;
                }
                .dot-start   { background: #FF7A59; }
                .dot-success { background: #16a34a; }
                .dot-stop    { background: #6b7280; }
                .dot-abort   { background: #dc2626; }
                .dot-retry   { background: #f59e0b; }
                .dot-scenario{ background: #1E2238; }
                .dot-step    { background: #dbeafe; border: 1px solid #93c5fd; }

                /* diagram container fills remaining height */
                .diagram-wrap {
                  flex: 1;
                  overflow: auto;
                  padding: 2rem;
                  display: flex;
                  justify-content: center;
                  align-items: flex-start;
                }
                .diagram-wrap .mermaid {
                  background: var(--surface);
                  border: 1px solid var(--border);
                  border-radius: 12px;
                  padding: 2rem;
                  box-shadow: 0 1px 8px rgb(30 34 56 / 0.06);
                  max-width: 100%;
                  overflow: auto;
                }

                /* zoom controls */
                .zoom-bar {
                  position: fixed; bottom: 1.5rem; right: 1.5rem;
                  display: flex; gap: 0.35rem;
                  background: var(--surface); border: 1px solid var(--border);
                  border-radius: 8px; padding: 0.3rem; box-shadow: 0 2px 8px rgb(0 0 0 / 0.08);
                }
                .zoom-btn {
                  background: none; border: none; cursor: pointer;
                  width: 30px; height: 30px; border-radius: 5px;
                  font-size: 1rem; font-weight: 600; color: var(--charcoal);
                  display: flex; align-items: center; justify-content: center;
                  transition: background 0.12s;
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
                  <a class="topbar-link" href="/status">← Configuration status</a>
                  <a class="topbar-link" href="/swagger" target="_blank">API docs ↗</a>
                </div>
              </nav>

              <div class="page-header">
                <div class="page-label">Scenario flow — decision diagram</div>
                <h1>Case Created (Zaak aangemaakt)</h1>
                <p>Traces every decision the OMC makes when OpenNotificaties delivers a <code>zaken / status / create</code> event — from raw webhook to citizen notification.</p>
              </div>

              <div class="legend">
                <span class="legend-item"><span class="legend-dot dot-start"></span>Entry / trigger</span>
                <span class="legend-item"><span class="legend-dot dot-scenario"></span>Scenario selected</span>
                <span class="legend-item"><span class="legend-dot dot-success"></span>Success</span>
                <span class="legend-item"><span class="legend-dot dot-abort"></span>Aborted (silent, no retry)</span>
                <span class="legend-item"><span class="legend-dot dot-retry"></span>Failure (will retry)</span>
                <span class="legend-item"><span class="legend-dot dot-stop"></span>Skipped / not applicable</span>
              </div>

              <div class="diagram-wrap">
                <div class="mermaid" id="diagram">
            flowchart TD
                WEBHOOK(["📨 OpenNotificaties webhook\nPOST /api/v1/notifications"])

                WEBHOOK --> DESER["Deserialize JSON\nto NotificationEvent"]
                DESER --> VALID{"Valid payload?\nAll required fields present?"}
                VALID -- "No" --> NOT_POSSIBLE(["⏹ NotPossible\nLog & discard"])
                VALID -- Yes --> IS_TEST{"Test ping?\nChannel=Unknown, Resource=Unknown\nmainObject = test URL"}
                IS_TEST -- Yes --> SKIPPED(["⏹ Skipped\nConnectivity ping only"])
                IS_TEST -- No --> RESOLVE{"Route by\nAction / Channel / Resource"}

                RESOLVE -- "Action=Create\nChannel=zaken\nResource=status" --> GET_STATUS
                RESOLVE -- "Action=Create\nChannel=objecten\nResource=object" --> OBJ_BRANCH["Match ObjectType UUID\n→ Task / Message / KTO scenario"]
                RESOLVE -- "Action=Create\nChannel=besluiten\nResource=besluit" --> DEC_BRANCH["⚖️ DecisionMadeScenario"]
                RESOLVE -- "No match" --> NOT_IMPL(["⏹ NotImplemented"])

                GET_STATUS["GET CaseStatus\nOpenZaak — ResourceUri"]
                GET_STATUS --> GET_STATUS_TYPE["GET CaseStatusType\nOpenZaak — status.TypeUri"]
                GET_STATUS_TYPE --> CHECK_INFORM{"IsNotificationExpected\n(informeren field)?"}
                CHECK_INFORM -- No --> ABORTED_INFORM(["⏹ Aborted\ninformeren = false"])
                CHECK_INFORM -- Yes --> CHECK_SERIAL{"StatusType\nSerialNumber?"}

                CHECK_SERIAL -- "= 1\n(first status ever)" --> SCENARIO_CREATED["🆕 CaseCreatedScenario"]
                CHECK_SERIAL -- "> 1  and  not final" --> SCENARIO_UPDATED["🔄 CaseStatusUpdatedScenario"]
                CHECK_SERIAL -- "> 1  and  IsFinalStatus" --> SCENARIO_CLOSED["✅ CaseClosedScenario"]

                SCENARIO_CREATED --> WHITELIST{"Case type ID in\nZGW__Whitelist__ZaakCreate_IDs?"}
                WHITELIST -- "No (not whitelisted)" --> ABORTED_WL(["⏹ Aborted\ncaseType not in whitelist"])
                WHITELIST -- "Yes — or wildcard *" --> GET_CASE["GET Case details\nOpenZaak — notification.MainObjectUri"]
                GET_CASE --> GET_PARTY["GET Party data\nOpenKlant — case.Uri + case.Identification"]
                GET_PARTY --> CHANNEL{"DistributionChannel\nfrom OpenKlant?"}

                CHANNEL -- Email --> BUILD_EMAIL["Build NotifyData — Email\ntemplate: Notify__TemplateId__Email__ZaakCreate\npersonalisation: naam, zaak.id, zaak.omschrijving"]
                CHANNEL -- SMS --> BUILD_SMS["Build NotifyData — SMS\ntemplate: Notify__TemplateId__Sms__ZaakCreate"]
                CHANNEL -- Letter --> BUILD_LETTER["Build NotifyData — Letter\ntemplate: Notify__TemplateId__Letter__ZaakCreate\n+ full postal address"]
                CHANNEL -- Both --> BUILD_BOTH["Build NotifyData — Email + SMS\n(two packages, sent sequentially)"]
                CHANNEL -- "Unknown / missing" --> FAIL_CHANNEL(["🔁 Failure\nNo valid channel — retry"])

                BUILD_EMAIL & BUILD_SMS & BUILD_LETTER & BUILD_BOTH --> SEND_CALL["POST to Notify NL API\n/v2/notifications/{method}\ntemplateId + personalisation + reference"]
                SEND_CALL --> SEND_RESP{"HTTP 201 Created?"}
                SEND_RESP -- "No (NotifyClientException)" --> RETRY(["🔁 Failure\nRetry later"])
                SEND_RESP -- Yes --> SUCCESS(["✅ Success\nCitizen notified"])

                style WEBHOOK         fill:#FF7A59,color:#fff,stroke:none
                style SUCCESS         fill:#16a34a,color:#fff,stroke:none
                style SCENARIO_CREATED fill:#1E2238,color:#fff,stroke:none
                style NOT_POSSIBLE    fill:#6b7280,color:#fff,stroke:none
                style SKIPPED         fill:#6b7280,color:#fff,stroke:none
                style NOT_IMPL        fill:#6b7280,color:#fff,stroke:none
                style ABORTED_INFORM  fill:#dc2626,color:#fff,stroke:none
                style ABORTED_WL      fill:#dc2626,color:#fff,stroke:none
                style FAIL_CHANNEL    fill:#f59e0b,color:#fff,stroke:none
                style RETRY           fill:#f59e0b,color:#fff,stroke:none
                </div>
              </div>

              <div class="zoom-bar">
                <button class="zoom-btn" onclick="zoom(1.2)" title="Zoom in">+</button>
                <button class="zoom-btn" onclick="zoom(1/1.2)" title="Zoom out">−</button>
                <button class="zoom-btn" onclick="resetZoom()" title="Reset zoom" style="font-size:0.7rem">⊙</button>
              </div>

              <script type="module">
                import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs';
                mermaid.initialize({
                  startOnLoad: true,
                  theme: 'base',
                  themeVariables: {
                    primaryColor:      '#dbeafe',
                    primaryTextColor:  '#1E2238',
                    primaryBorderColor:'#93c5fd',
                    lineColor:         '#80818C',
                    secondaryColor:    '#F6F7FA',
                    tertiaryColor:     '#F6F7FA',
                    fontSize:          '13px',
                    fontFamily:        "'Sora', ui-sans-serif, system-ui, sans-serif",
                  }
                });
              </script>

              <script>
                let scale = 1;
                const diagram = document.getElementById('diagram');
                function zoom(factor) {
                  scale = Math.min(Math.max(scale * factor, 0.3), 3);
                  diagram.style.transform = `scale(${scale})`;
                  diagram.style.transformOrigin = 'top center';
                }
                function resetZoom() {
                  scale = 1;
                  diagram.style.transform = 'scale(1)';
                }
              </script>
            </body>
            </html>
            """;
    }
}
