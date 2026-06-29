// © 2024, Worth Systems.

using Common.Settings.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;

namespace EventsHandler.Services.Configuration
{
    /// <summary>The result of a single configuration or connectivity check.</summary>
    /// <param name="Group">Display group this check belongs to.</param>
    /// <param name="Icon">Emoji icon representing the group.</param>
    /// <param name="Name">Human-readable check label.</param>
    /// <param name="Ok">Whether the check passed.</param>
    /// <param name="Detail">Display value or full error message (secrets are never included).</param>
    /// <param name="Hint">Environment variable(s) to set when this check fails.</param>
    public sealed record CheckResult(string Group, string Icon, string Name, bool Ok, string? Detail = null, string? Hint = null);

    /// <summary>Runs all configuration presence and service connectivity checks.</summary>
    [ExcludeFromCodeCoverage]
    public sealed class ConfigurationCheckService(OmcConfiguration config, IQueryContext queryContext)
    {
        /// <summary>Streams check results as they complete.</summary>
        public async IAsyncEnumerable<CheckResult> RunChecksAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            foreach (var r in OmcAuthChecks())         yield return r;
            foreach (var r in ZgwAuthChecks())          yield return r;
            foreach (var r in EndpointChecks())         yield return r;
            foreach (var r in NotifyConfigChecks())     yield return r;
            yield return await ConnectivityCheckAsync("Service Connectivity", "🔗", "OpenZaak",
                () => queryContext.GetZaakHealthCheckAsync(), ct,
                hint: "ZGW__Endpoint__OpenZaak, ZGW__Auth__JWT__Secret, ZGW__Auth__JWT__Issuer, ZGW__Auth__JWT__UserId");
            yield return await ConnectivityCheckAsync("Service Connectivity", "🔗", "OpenKlant",
                () => queryContext.GetKlantHealthCheckAsync(), ct,
                hint: "ZGW__Endpoint__OpenKlant, ZGW__Auth__Key__OpenKlant");
            yield return await ConnectivityCheckAsync("Service Connectivity", "🔗", "OpenBesluiten",
                () => queryContext.GetBesluitenHealthCheckAsync(), ct,
                hint: "ZGW__Endpoint__Besluiten, ZGW__Auth__JWT__Secret, ZGW__Auth__JWT__Issuer");
            yield return await ConnectivityCheckAsync("Service Connectivity", "🔗", "Objecten",
                () => queryContext.GetObjectenHealthCheckAsync(), ct,
                hint: "ZGW__Endpoint__Objecten, ZGW__Auth__Key__Objecten");
            yield return await ConnectivityCheckAsync("Service Connectivity", "🔗", "ObjectTypen",
                () => queryContext.GetObjectTypenHealthCheckAsync(), ct,
                hint: "ZGW__Endpoint__ObjectTypen, ZGW__Auth__Key__ObjectTypen");
            foreach (var r in CaseCreatedChecks())      yield return r;
            foreach (var r in CaseUpdatedChecks())      yield return r;
            foreach (var r in CaseClosedChecks())       yield return r;
            foreach (var r in TaskAssignedChecks())     yield return r;
            foreach (var r in MessageReceivedChecks())  yield return r;
            foreach (var r in DecisionMadeChecks())     yield return r;
            foreach (var r in KtoChecks())              yield return r;
        }

        // ── Config check groups ───────────────────────────────────────────────

        private IEnumerable<CheckResult> OmcAuthChecks()
        {
            const string g = "OMC Authentication", ic = "🔐";
            yield return Masked(g, ic, "JWT Secret",   () => config.OMC.Auth.JWT.Secret(),   "OMC__Auth__JWT__Secret");
            yield return Show(g,   ic, "JWT Issuer",   () => config.OMC.Auth.JWT.Issuer(),   "OMC__Auth__JWT__Issuer");
            yield return Option(g, ic, "JWT Audience", () => config.OMC.Auth.JWT.Audience(), "OMC__Auth__JWT__Audience");
            yield return Show(g,   ic, "JWT User ID",  () => config.OMC.Auth.JWT.UserId(),   "OMC__Auth__JWT__UserId");
            yield return Show(g,   ic, "JWT Username", () => config.OMC.Auth.JWT.UserName(), "OMC__Auth__JWT__UserName");
        }

        private IEnumerable<CheckResult> ZgwAuthChecks()
        {
            const string g = "ZGW Authentication", ic = "🔑";
            yield return Masked(g, ic, "JWT Secret",          () => config.ZGW.Auth.JWT.Secret(),      "ZGW__Auth__JWT__Secret");
            yield return Show(g,   ic, "JWT Issuer",          () => config.ZGW.Auth.JWT.Issuer(),      "ZGW__Auth__JWT__Issuer");
            yield return Masked(g, ic, "OpenKlant API Key",   () => config.ZGW.Auth.Key.OpenKlant(),   "ZGW__Auth__Key__OpenKlant");
            yield return Masked(g, ic, "Objecten API Key",    () => config.ZGW.Auth.Key.Objecten(),    "ZGW__Auth__Key__Objecten");
            yield return Masked(g, ic, "ObjectTypen API Key", () => config.ZGW.Auth.Key.ObjectTypen(), "ZGW__Auth__Key__ObjectTypen");
        }

        private IEnumerable<CheckResult> EndpointChecks()
        {
            const string g = "ZGW Endpoints", ic = "🌐";
            yield return Show(g, ic, "OpenNotificaties", () => config.ZGW.Endpoint.OpenNotificaties(), "ZGW__Endpoint__OpenNotificaties");
            yield return Show(g, ic, "OpenZaak",         () => config.ZGW.Endpoint.OpenZaak(),         "ZGW__Endpoint__OpenZaak");
            yield return Show(g, ic, "OpenKlant",        () => config.ZGW.Endpoint.OpenKlant(),        "ZGW__Endpoint__OpenKlant");
            yield return Show(g, ic, "Besluiten",        () => config.ZGW.Endpoint.Besluiten(),        "ZGW__Endpoint__Besluiten");
            yield return Show(g, ic, "Objecten",         () => config.ZGW.Endpoint.Objecten(),         "ZGW__Endpoint__Objecten");
            yield return Show(g, ic, "ObjectTypen",      () => config.ZGW.Endpoint.ObjectTypen(),      "ZGW__Endpoint__ObjectTypen");
        }

        private IEnumerable<CheckResult> NotifyConfigChecks()
        {
            const string g = "Notify NL", ic = "📬";
            yield return Show(g,   ic, "API URL", () => config.Notify.API.BaseUrl().ToString(), "Notify__API__BaseUrl");
            yield return Masked(g, ic, "API Key", () => config.Notify.API.Key(),               "Notify__API__Key");
        }

        private IEnumerable<CheckResult> CaseCreatedChecks()
        {
            const string g = "Case Created (Zaak aangemaakt)", ic = "📄";
            yield return Whitelist(g, ic, "Allowed case types",          () => config.ZGW.Whitelist.ZaakCreate_IDs(),            "ZGW__Whitelist__ZaakCreate_IDs");
            yield return Uuid(g,      ic, "Email notification template", () => config.Notify.TemplateId.Email.ZaakCreate(),      "Notify__TemplateId__Email__ZaakCreate");
            yield return Uuid(g,      ic, "SMS notification template",   () => config.Notify.TemplateId.Sms.ZaakCreate(),        "Notify__TemplateId__Sms__ZaakCreate");
        }

        private IEnumerable<CheckResult> CaseUpdatedChecks()
        {
            const string g = "Case Status Updated (Zaakstatus bijgewerkt)", ic = "🔄";
            yield return Whitelist(g, ic, "Allowed case types",          () => config.ZGW.Whitelist.ZaakUpdate_IDs(),            "ZGW__Whitelist__ZaakUpdate_IDs");
            yield return Uuid(g,      ic, "Email notification template", () => config.Notify.TemplateId.Email.ZaakUpdate(),      "Notify__TemplateId__Email__ZaakUpdate");
            yield return Uuid(g,      ic, "SMS notification template",   () => config.Notify.TemplateId.Sms.ZaakUpdate(),        "Notify__TemplateId__Sms__ZaakUpdate");
        }

        private IEnumerable<CheckResult> CaseClosedChecks()
        {
            const string g = "Case Closed (Zaak afgesloten)", ic = "✅";
            yield return Whitelist(g, ic, "Allowed case types",          () => config.ZGW.Whitelist.ZaakClose_IDs(),             "ZGW__Whitelist__ZaakClose_IDs");
            yield return Uuid(g,      ic, "Email notification template", () => config.Notify.TemplateId.Email.ZaakClose(),       "Notify__TemplateId__Email__ZaakClose");
            yield return Uuid(g,      ic, "SMS notification template",   () => config.Notify.TemplateId.Sms.ZaakClose(),         "Notify__TemplateId__Sms__ZaakClose");
        }

        private IEnumerable<CheckResult> TaskAssignedChecks()
        {
            const string g = "Task Assigned (Taak toegewezen)", ic = "📋";
            yield return Whitelist(g, ic, "Allowed case types",          () => config.ZGW.Whitelist.TaskAssigned_IDs(),          "ZGW__Whitelist__TaskAssigned_IDs");
            yield return Uuid(g,      ic, "Task object type UUID",       () => config.ZGW.Variable.ObjectType.TaskObjectType_Uuid(),  "ZGW__Variable__ObjectType__TaskObjectType_Uuid");
            yield return Uuid(g,      ic, "Email notification template", () => config.Notify.TemplateId.Email.TaskAssigned(),    "Notify__TemplateId__Email__TaskAssigned");
            yield return Uuid(g,      ic, "SMS notification template",   () => config.Notify.TemplateId.Sms.TaskAssigned(),      "Notify__TemplateId__Sms__TaskAssigned");
        }

        private IEnumerable<CheckResult> MessageReceivedChecks()
        {
            const string g = "Message Received (Bericht ontvangen)", ic = "💬";
            bool allowed;
            try { allowed = config.ZGW.Whitelist.Message_Allowed(); } catch { allowed = false; }

            if (!allowed)
            {
                yield return new CheckResult(g, ic, "Messages enabled", false,
                    "disabled — set to true to enable this scenario",
                    "ZGW__Whitelist__Message_Allowed");
                yield break;
            }

            yield return new CheckResult(g, ic, "Messages enabled", true, "enabled");
            yield return Uuid(g, ic, "Message object type UUID",    () => config.ZGW.Variable.ObjectType.MessageObjectType_Uuid(), "ZGW__Variable__ObjectType__MessageObjectType_Uuid");
            yield return Uuid(g, ic, "Email notification template", () => config.Notify.TemplateId.Email.MessageReceived(),        "Notify__TemplateId__Email__MessageReceived");
            yield return Uuid(g, ic, "SMS notification template",   () => config.Notify.TemplateId.Sms.MessageReceived(),          "Notify__TemplateId__Sms__MessageReceived");
        }

        private IEnumerable<CheckResult> DecisionMadeChecks()
        {
            const string g = "Decision Made (Besluit genomen)", ic = "⚖️";
            yield return Whitelist(g, ic, "Allowed case types",        () => config.ZGW.Whitelist.DecisionMade_IDs(),                     "ZGW__Whitelist__DecisionMade_IDs");
            yield return UuidSet(g,  ic, "Decision info object types", () => config.ZGW.Variable.ObjectType.DecisionInfoObjectType_Uuids(), "ZGW__Variable__ObjectType__DecisionInfoObjectType_Uuids");
            yield return Uuid(g,     ic, "Notification template",      () => config.Notify.TemplateId.DecisionMade(),                      "Notify__TemplateId__DecisionMade");
        }

        private IEnumerable<CheckResult> KtoChecks()
        {
            const string g = "Customer Satisfaction (KTO)", ic = "⭐";
            yield return Uuid(g,   ic, "KTO object type UUID", () => config.ZGW.Variable.ObjectType.KtoObjectType_Uuid(), "ZGW__Variable__ObjectType__KtoObjectType_Uuid");
            yield return Show(g,   ic, "KTO service URL",      () => config.KTO.Url(),                "KTO__Url");
            yield return Masked(g, ic, "KTO JWT Secret",       () => config.KTO.Auth.JWT.Secret(),    "KTO__Auth__JWT__Secret");
            yield return Show(g,   ic, "KTO JWT Issuer",       () => config.KTO.Auth.JWT.Issuer(),    "KTO__Auth__JWT__Issuer");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static CheckResult Show(string group, string icon, string name, Func<string> getter, string hint)
        {
            try
            {
                string v = getter();
                bool ok = !string.IsNullOrWhiteSpace(v);
                return new CheckResult(group, icon, name, ok, ok ? v : "not configured", ok ? null : hint);
            }
            catch { return new CheckResult(group, icon, name, false, "not configured", hint); }
        }

        private static CheckResult Masked(string group, string icon, string name, Func<string> getter, string hint)
        {
            try
            {
                string v = getter();
                bool ok = !string.IsNullOrWhiteSpace(v);
                return new CheckResult(group, icon, name, ok, ok ? "configured" : "not configured", ok ? null : hint);
            }
            catch { return new CheckResult(group, icon, name, false, "not configured", hint); }
        }

        private static CheckResult Option(string group, string icon, string name, Func<string> getter, string hint)
        {
            try
            {
                string v = getter();
                return new CheckResult(group, icon, name, true,
                    string.IsNullOrWhiteSpace(v) ? "(optional, not set)" : v);
            }
            catch { return new CheckResult(group, icon, name, true, "(optional, not set)"); }
        }

        private static CheckResult Uuid(string group, string icon, string name, Func<Guid> getter, string hint)
        {
            try
            {
                Guid g = getter();
                bool ok = g != Guid.Empty;
                return new CheckResult(group, icon, name, ok, ok ? g.ToString() : "not configured", ok ? null : hint);
            }
            catch { return new CheckResult(group, icon, name, false, "not configured", hint); }
        }

        private static CheckResult Whitelist(string group, string icon, string name,
            Func<OmcConfiguration.ZgwComponent.WhitelistComponent.IDs> getter, string hint)
        {
            try
            {
                var ids = getter();
                bool ok = ids.Count > 0;
                return new CheckResult(group, icon, name, ok,
                    ok ? $"{ids.Count} case type(s)" : "none configured — no notifications will be sent for this scenario",
                    ok ? null : hint);
            }
            catch { return new CheckResult(group, icon, name, false, "not configured", hint); }
        }

        private static CheckResult UuidSet(string group, string icon, string name, Func<HashSet<Guid>> getter, string hint)
        {
            try
            {
                var uuids = getter();
                bool ok = uuids.Count > 0;
                return new CheckResult(group, icon, name, ok, ok ? $"{uuids.Count} UUID(s)" : "not configured", ok ? null : hint);
            }
            catch { return new CheckResult(group, icon, name, false, "not configured", hint); }
        }

        private static async Task<CheckResult> ConnectivityCheckAsync(
            string group, string icon, string name,
            Func<Task<HttpRequestResponse>> fn, CancellationToken ct, string hint)
        {
            try
            {
                HttpRequestResponse r = await fn().WaitAsync(ct);
                if (r.IsSuccess)
                    return new CheckResult(group, icon, name, true, "reachable");

                string detail = string.IsNullOrWhiteSpace(r.JsonResponse)
                    ? "unreachable — no response body"
                    : r.JsonResponse;
                return new CheckResult(group, icon, name, false, detail, hint);
            }
            catch (OperationCanceledException)
            {
                return new CheckResult(group, icon, name, false,
                    "timed out — the service did not respond in time", hint);
            }
            catch (HttpRequestException ex)
            {
                string reason = ex.InnerException?.Message ?? ex.Message;
                return new CheckResult(group, icon, name, false, reason, hint);
            }
            catch (Exception ex)
            {
                return new CheckResult(group, icon, name, false, ex.Message, hint);
            }
        }
    }
}
