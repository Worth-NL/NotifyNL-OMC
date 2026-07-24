// Data model for the architecture/flow overview page. Registers and channels marked
// `active: false` are aspirational (from the design reference) — not real OMC integrations
// yet. They render greyed out, matching the "inactief" convention shown for real inactive
// scenarios (e.g. Message Received when ZGW_WHITELIST_MESSAGE_ALLOWED is off).

export interface ArchitectureNode {
  key: string;
  name: string;
  subtitle: string;
  version?: string;
  /** Real, wired-up OMC integration vs. a placeholder from the design reference. */
  active: boolean;
}

export const INPUT_NODES: ArchitectureNode[] = [
  { key: "opennotificaties", name: "Open Notificaties", subtitle: "Notificatie-API — NL-GOV", version: "v1.4.2", active: true },
  { key: "mendix", name: "Mendix", subtitle: "Low-code app-events", version: "v2.6", active: false },
];

export const REGISTER_NODES: ArchitectureNode[] = [
  { key: "openklant", name: "Open Klant", subtitle: "Klantinteracties — partij + digitaal adres", version: "v2.0", active: true },
  { key: "openzaak", name: "Open Zaak", subtitle: "Zaken, statussen, resultaten", version: "v1.3", active: true },
  { key: "objecten", name: "Objecten", subtitle: "Taken, berichten, KTO-triggers", version: "v2.0", active: true },
  { key: "objecttypen", name: "ObjectTypen", subtitle: "Schema's voor Objecten API", version: "v1.1", active: true },
  { key: "besluiten", name: "Besluiten", subtitle: "Besluitdocumenten en -typen", version: "v1.0", active: true },
  { key: "brp", name: "BRP", subtitle: "Haal Centraal — persoonsgegevens", version: "v2.2", active: true },
  { key: "kto", name: "KTO", subtitle: "Klanttevredenheidsonderzoek (Expoints)", active: true },
  { key: "documentstore", name: "Document store", subtitle: "Centrale documentopslag", active: false },
  { key: "profielservice", name: "Profiel service", subtitle: "Voorkeuren per burger", active: false },
  { key: "urn", name: "URN", subtitle: "OIN/RSIN bronidentificatie", active: false },
  { key: "vtb", name: "VTB", subtitle: "Verificatie toegangsbeheer", active: false },
];

// Replaces a generic 3-node guess ("Kanaal keuze" / "Privacy filter" / "Notificatie filter")
// with the gates that actually exist in the C# source, verified per scenario:
//   - "Privacy filter" as GDPR/consent never existed in this pipeline — the one natural-person
//     check in the codebase (CheckIfInitiatorIsNaturalPersonAsync) belongs to the separate
//     /MijnZaken forwarding pipeline, not these 7 scenarios.
//   - "Notificatie filter" (dedup/throttle) has zero basis in code — no send-log, no
//     "already notified" check anywhere in the repo.
// See BaseScenario.cs, and each ScenarioXyz.cs's PrepareDataAsync, for the real gates below.
export const FILTER_NODES: ArchitectureNode[] = [
  { key: "zaaktypewhitelist", name: "Zaaktype whitelist", subtitle: "ValidateCaseId — per-scenario env var", active: true },
  { key: "informerencheck", name: "Informeren-check", subtitle: "IsNotificationExpected", active: true },
  { key: "kanaalresolutie", name: "Kanaalresolutie", subtitle: "OpenKlant adres/voorkeur — e-mail of sms", active: true },
  { key: "berichtenschakelaar", name: "Berichten-schakelaar", subtitle: "Globale aan/uit-vlag — geen per-zaaktype whitelist", active: true },
  { key: "taakcheck", name: "Taak- & ID-typecheck", subtitle: "Taak open + BSN/KVK", active: true },
  { key: "documentcheck", name: "Documentstatus & vertrouwelijkheid", subtitle: "Definitief + niet-vertrouwelijk", active: true },
];

// "Letter"/"Both" branches exist in BaseScenario's DistributionChannel switch, but the live
// OpenKlant v2 integration (PartyResults.DetermineDistributionChannel) never returns them —
// only Email/Sms are reachable in production, so Notify Post stays wired but never "live" for
// any real flow. PostGuard is not part of this pipeline at all: it's a manual test endpoint
// (TestNotifyController) never invoked by NotifyProcessor or any scenario — marked inactive.
export const CHANNEL_NODES: ArchitectureNode[] = [
  { key: "notify-email", name: "Notify E-mail", subtitle: "Notify NL — e-mail", active: true },
  { key: "notify-sms", name: "Notify SMS", subtitle: "Notify NL — sms", active: true },
  { key: "notify-post", name: "Notify Post", subtitle: "Notify NL — brief (nooit bereikt door live OpenKlant-data)", active: true },
  { key: "postguard", name: "PostGuard", subtitle: "Handmatig testendpoint — geen scenario roept dit aan", active: false },
  { key: "berichtenbox", name: "Notify Berichtenbox", subtitle: "MijnOverheid berichtenbox", active: false },
  { key: "lokale-berichtenbox", name: "Lokale Berichtenbox", subtitle: "Gemeentelijk portaal", active: false },
];

export const CONFIRMATION_NODES: ArchitectureNode[] = [
  { key: "contactmoment", name: "Contactmoment", subtitle: "Vastgelegd in Open Klant", active: true },
  { key: "archief", name: "Archief", subtitle: "Langetermijnopslag bevestigingen", active: false },
  { key: "contactherstel", name: "Contact herstel", subtitle: "Terugvalkanaal bij falen", active: false },
];

export const PATTERN_ENGINE_KEY = "output-patronen";

export type EdgeCategory = "producten" | "zaken" | "taken" | "besluiten" | "verrijking" | "bevestiging";

export const EDGE_CATEGORY_COLOR: Record<EdgeCategory, string> = {
  producten: "var(--color-arch-indigo)",
  zaken: "var(--color-arch-blue)",
  taken: "var(--color-arch-amber)",
  besluiten: "var(--color-arch-emerald)",
  verrijking: "var(--color-arch-teal)",
  bevestiging: "var(--color-arch-violet)",
};

export interface FlowEdge {
  source: string;
  target: string;
  category: EdgeCategory;
}

// The static architecture graph — always the same shape, regardless of selected flow.
// Which edges appear "live" (vs. dimmed) is derived at render time from whether both
// endpoints are used by the currently selected FlowOption.
//
// Registers are never a pipeline stage of their own — OMC is the sole hub that calls each
// register and gets a response back before continuing, so every register edge is a round
// trip to/from Output Patronen (rendered below it), not a forward hop to Kanaal keuze.
export const EDGES: FlowEdge[] = [
  { source: "opennotificaties", target: PATTERN_ENGINE_KEY, category: "zaken" },
  { source: "mendix", target: PATTERN_ENGINE_KEY, category: "producten" },

  ...REGISTER_NODES.map((n) => ({ source: PATTERN_ENGINE_KEY, target: n.key, category: "verrijking" as const })),

  // Four real entry points into the filter stage, one per scenario family. Task's and
  // Decision's own checks run BEFORE the shared zaaktype whitelist in the real code
  // (TaskAssignedScenario/DecisionMadeScenario check their own things first, then call
  // ValidateCaseId), so they feed into it rather than branching off it. Case scenarios and
  // Message Received have no scenario-unique gate, so they enter the shared chain directly
  // (Message Received skips the whitelist/informeren chain entirely — its one gate is a
  // global flag, not a per-case-type check).
  { source: PATTERN_ENGINE_KEY, target: "zaaktypewhitelist", category: "producten" },
  { source: PATTERN_ENGINE_KEY, target: "taakcheck", category: "taken" },
  { source: PATTERN_ENGINE_KEY, target: "documentcheck", category: "besluiten" },
  { source: PATTERN_ENGINE_KEY, target: "berichtenschakelaar", category: "producten" },

  { source: "taakcheck", target: "zaaktypewhitelist", category: "taken" },
  { source: "documentcheck", target: "zaaktypewhitelist", category: "besluiten" },

  { source: "zaaktypewhitelist", target: "informerencheck", category: "producten" },
  { source: "informerencheck", target: "kanaalresolutie", category: "producten" },
  { source: "berichtenschakelaar", target: "kanaalresolutie", category: "producten" },

  // Only Email/Sms are ever actually selected by the live OpenKlant integration (see
  // CHANNEL_NODES comment) — Notify Post stays reachable in the diagram since the code path
  // is real, it just never lights up for any concrete flow (not in any FlowOption.channels).
  { source: "kanaalresolutie", target: "notify-email", category: "producten" },
  { source: "kanaalresolutie", target: "notify-sms", category: "producten" },
  { source: "kanaalresolutie", target: "notify-post", category: "producten" },
  // Berichtenbox/Lokale Berichtenbox are aspirational (active: false) — never real sends —
  // but still need an edge into the main graph, or Dagre treats {berichtenbox, archief} and
  // {lokale-berichtenbox, contactherstel} as disconnected components with nothing else
  // anchoring them, and dumps them off in a corner unrelated to the rest of Uitvoer.
  { source: "kanaalresolutie", target: "berichtenbox", category: "producten" },
  { source: "kanaalresolutie", target: "lokale-berichtenbox", category: "producten" },

  { source: "notify-email", target: "contactmoment", category: "bevestiging" },
  { source: "notify-sms", target: "contactmoment", category: "bevestiging" },
  { source: "notify-post", target: "contactmoment", category: "bevestiging" },
  { source: "postguard", target: "contactmoment", category: "bevestiging" },
  { source: "berichtenbox", target: "archief", category: "bevestiging" },
  { source: "lokale-berichtenbox", target: "contactherstel", category: "bevestiging" },
];

export interface FlowOption {
  /** Matches a ScenarioFlow.key from the API, or "all" for the overview state. */
  key: string;
  name: string;
  nl: string;
  registers: string[];
  filters: string[];
  channels: string[];
  confirmations: string[];
}

export const FLOW_OPTIONS: FlowOption[] = [
  {
    key: "all",
    name: "Alle flows",
    nl: "Volledig overzicht",
    registers: REGISTER_NODES.map((n) => n.key),
    filters: FILTER_NODES.map((n) => n.key),
    channels: CHANNEL_NODES.map((n) => n.key),
    confirmations: CONFIRMATION_NODES.map((n) => n.key),
  },
  {
    key: "case-created",
    name: "Case Created",
    nl: "Zaak aangemaakt",
    registers: ["openzaak", "openklant"],
    filters: ["zaaktypewhitelist", "informerencheck", "kanaalresolutie"],
    channels: ["notify-email", "notify-sms"],
    confirmations: ["contactmoment"],
  },
  {
    key: "case-updated",
    name: "Case Status Updated",
    nl: "Zaakstatus bijgewerkt",
    registers: ["openzaak", "openklant"],
    filters: ["zaaktypewhitelist", "informerencheck", "kanaalresolutie"],
    channels: ["notify-email", "notify-sms"],
    confirmations: ["contactmoment"],
  },
  {
    key: "case-closed",
    name: "Case Closed",
    nl: "Zaak afgesloten",
    registers: ["openzaak", "openklant"],
    filters: ["zaaktypewhitelist", "informerencheck", "kanaalresolutie"],
    channels: ["notify-email", "notify-sms"],
    confirmations: ["contactmoment"],
  },
  {
    key: "task-assigned",
    name: "Task Assigned",
    nl: "Taak toegewezen",
    registers: ["objecten", "openzaak", "openklant"],
    // Real order in TaskAssignedScenario.PrepareDataAsync: task-open + BSN/KVK check first,
    // then the shared zaaktype whitelist, then the informeren check.
    filters: ["taakcheck", "zaaktypewhitelist", "informerencheck", "kanaalresolutie"],
    channels: ["notify-email", "notify-sms"],
    confirmations: ["contactmoment"],
  },
  {
    key: "message-received",
    name: "Message Received",
    nl: "Bericht ontvangen",
    registers: ["objecten", "openklant"],
    // The only scenario with no zaaktype whitelist and no informeren-check — its single gate
    // is a global on/off flag (ZGW_WHITELIST_MESSAGE_ALLOWED), not per-case-type.
    filters: ["berichtenschakelaar", "kanaalresolutie"],
    channels: ["notify-email", "notify-sms"],
    confirmations: ["contactmoment"],
  },
  {
    key: "decision-made",
    name: "Decision Made",
    nl: "Besluit genomen",
    registers: ["besluiten", "openzaak", "openklant", "objecten"],
    // Real order in DecisionMadeScenario.PrepareDataAsync: InfoObject-type + status +
    // confidentiality checks first, then the shared zaaktype whitelist, then informeren.
    // Still resolves a channel (to pick which template to preview) — but ProcessDataAsync
    // is overridden to call GenerateTemplatePreviewAsync + POST to Objecten instead of an
    // actual Notify NL send, hence channels: [] below (nothing ever lights up downstream).
    filters: ["documentcheck", "zaaktypewhitelist", "informerencheck", "kanaalresolutie"],
    channels: [],
    confirmations: ["contactmoment"],
  },
  {
    key: "kto",
    name: "Customer Satisfaction (KTO)",
    nl: "Klanttevredenheidsonderzoek",
    registers: ["kto"],
    filters: [],
    channels: [],
    confirmations: [],
  },
];

// Node positions are no longer hand-placed here — see lib/layout.ts, which runs Dagre's
// graph layout algorithm over EDGES to get crossing-minimized positions automatically.
