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
  /** Compact label, for where the full name doesn't fit (the check blocks in the OMC block). */
  short?: string;
  /** What a check checks, shown when it is clicked. Only checks have it. */
  details?: CheckDetails;
}

export interface CheckDetails {
  /** What is checked, in plain language. */
  what: string;
  /** Where it is configured: an env var, or the register field it reads. */
  config: string;
  /** What happens when it does not pass, including the HTTP status Open Notificaties gets. */
  onFail: string;
  /** Where it lives in the code. */
  source: string;
}

export const INPUT_NODES: ArchitectureNode[] = [
  { key: "opennotificaties", name: "Open Notificaties", subtitle: "Notificatie-API — NL-GOV", version: "v1.4.2", active: true },
  { key: "oneground", name: "Roxit Oneground", subtitle: "Zaaksysteem — MijnZaken events", active: true },
  { key: "mendix", name: "Mendix", subtitle: "Low-code app-events", version: "v2.6", active: false },
];

export const REGISTER_NODES: ArchitectureNode[] = [
  { key: "openklant", name: "KlantAPI (OpenKlant)", subtitle: "Klantinteracties — partij + digitaal adres", version: "v2.0", active: true },
  { key: "openzaak", name: "ZaakAPI (OpenZaak)", subtitle: "Zaken, statussen, resultaten", version: "v1.3", active: true },
  { key: "objecten", name: "ObjectenAPI (OpenObject)", subtitle: "Taken, berichten, KTO-triggers", version: "v2.0", active: true },
  { key: "objecttypen", name: "ObjectTypen", subtitle: "Schema's voor Objecten API", version: "v1.1", active: true },
  { key: "besluiten", name: "BesluitenAPI (OpenZaak)", subtitle: "Besluitdocumenten en -typen", version: "v1.0", active: true },
  { key: "brp", name: "BRP", subtitle: "Haal Centraal — persoonsgegevens", version: "v2.2", active: true },
  { key: "openproduct", name: "ProductenAPI (Open Product)", subtitle: "Producten, producttypen en eigenaren", version: "v1.7", active: true },
  { key: "documenten", name: "DocumentenAPI", subtitle: "Documenten en hun inhoud (pdf, bijlagen)", active: true },
  { key: "profielservice", name: "ProfielService (MoZa)", subtitle: "Voorkeuren per burger", active: false },
  { key: "vtb", name: "BerichtenAPI (OpenVTB)", subtitle: "Berichten voor de Berichtenbox", active: true },
];

// The checks each scenario runs before it sends anything, verified against the C# source (see
// BaseScenario.cs and each scenario's PrepareDataAsync). They are not nodes of their own in the
// diagram: they are blocks inside the OMC block, one row per flow in the order that flow runs
// them (see FlowOption.filters). That keeps the diagram the same shape however many scenarios
// exist, which a separate node per check could not (each new scenario added another lane of
// crossing lines). Their keys are still the trace stages the backend emits, so a live trace
// lights up the matching block, and every check keeps its own log next to its explanation.
//   - naturalpersoncheck/mijnzaken-staleness are MijnZaken-only (MijnOverheidForwarder.cs).
//   - producttypewhitelist/productgepubliceerd/productbezorging are Product created only
//     (ProductScenarioImplementation.cs).
export const CHECK_NODES: ArchitectureNode[] = [
  {
    key: "zaaktypewhitelist", name: "Zaaktype whitelist", short: "Zaaktype whitelist", active: true,
    subtitle: "ValidateCaseId — per-scenario env var",
    details: {
      what: "Staat het zaaktype (de identificatie uit de catalogus) op de whitelist van dit scenario? Een * staat alles toe.",
      config: "ZGW_WHITELIST_ZAAKCREATE_IDS, _ZAAKUPDATE_IDS, _ZAAKCLOSE_IDS, _TASKASSIGNED_IDS of _DECISIONMADE_IDS — per scenario",
      onFail: "Afgebroken (206): er wordt niets verstuurd en Open Notificaties biedt het event niet opnieuw aan. De reden noemt de env var.",
      source: "BaseScenario.ValidateCaseId",
    },
  },
  {
    key: "informerencheck", name: "Informeren-check", short: "Informeren", active: true,
    subtitle: "IsNotificationExpected",
    details: {
      what: "Staat \"informeren\" aan op het statustype (zaakscenario's) of zaaktype? Alleen dan wil de gemeente dat de burger bericht krijgt.",
      config: "Het veld informeren op het statustype/zaaktype in de OpenZaak-catalogus",
      onFail: "Afgebroken (206): bewust niet versturen, geen herlevering.",
      source: "NotifyScenariosResolver, BaseScenario.ValidateNotifyPermit",
    },
  },
  {
    key: "kanaalresolutie", name: "Kanaalresolutie", short: "Kanaal", active: true,
    subtitle: "OpenKlant adres/voorkeur — e-mail of sms",
    details: {
      what: "Kiest e-mail of sms op basis van de digitale adressen van de partij in OpenKlant: eerst een adres waarvan de referentie gelijk is aan de zaak, dan het voorkeursadres, dan het eerste bruikbare adres.",
      config: "Digitale adressen en voorkeursDigitaalAdres van de partij in OpenKlant",
      onFail: "Mislukt (412) als er geen bruikbaar adres is; bestaat de partij niet, dan afgebroken (206).",
      source: "BaseScenario.TryGetDataAsync, PartyResults.Party",
    },
  },
  {
    key: "berichtenschakelaar", name: "Berichten-schakelaar", short: "Berichten aan/uit", active: true,
    subtitle: "Globale aan/uit-vlag — geen per-zaaktype whitelist",
    details: {
      what: "Staat het scenario Bericht ontvangen aan? Eén vlag voor alle berichten, geen whitelist per type.",
      config: "ZGW_WHITELIST_MESSAGE_ALLOWED (true/false)",
      onFail: "Afgebroken (206): de reden noemt de env var.",
      source: "MessageReceivedScenario.PrepareDataAsync",
    },
  },
  {
    key: "taakcheck", name: "Taak- & ID-typecheck", short: "Taak & ID-type", active: true,
    subtitle: "Taak open + BSN/KVK",
    details: {
      what: "Is de taak nog open, en is de ontvanger geïdentificeerd met een BSN of KVK-nummer?",
      config: "De taak in de Objecten API (status en identificatie)",
      onFail: "Afgebroken (206): een gesloten taak of een ander identificatietype krijgt geen bericht.",
      source: "TaskAssignedScenario.PrepareDataAsync",
    },
  },
  {
    key: "documentcheck", name: "Documentcheck", short: "Documentcheck", active: true,
    subtitle: "Besluitdocument definitief/openbaar · pdfurl in eigen Documenten API",
    details: {
      what: "Besluit genomen: is het besluitdocument van een toegestaan informatieobjecttype, definitief en openbaar (niet vertrouwelijk)? Printstraat: wijst de pdfurl naar een document in de geconfigureerde Documenten API (zelfde schema, host en poort)?",
      config: "Besluit: ZGW_VARIABLE_OBJECTTYPE_DECISIONINFOOBJECTTYPE_UUIDS · Printstraat: ZGW_ENDPOINT_DOCUMENTEN",
      onFail: "Besluit genomen: afgebroken (206), de reden noemt wat niet klopt. Printstraat: mislukt (412).",
      source: "DecisionMadeScenario.PrepareDataAsync, PrintScenarioImplementation.TryResolveDocumentUuid",
    },
  },
  {
    key: "printschakelaar", name: "Printen aan/uit", short: "Printen aan/uit", active: true,
    subtitle: "Globale aan/uit-vlag voor de printstraat",
    details: {
      what: "Staat printen aan voor deze omgeving? Eén vlag voor alle printopdrachten.",
      config: "ZGW_WHITELIST_PRINT_ALLOWED (true/false)",
      onFail: "Mislukt (412): de reden noemt de env var.",
      source: "PrintScenarioImplementation.ProcessPrintAsync (stap 1)",
    },
  },
  {
    key: "betrokkeneurn", name: "BSN in betrokkene-URN", short: "BSN in URN", active: true,
    subtitle: "contact_betrokkene_urn moet een BSN dragen",
    details: {
      what: "Bevat contact_betrokkene_urn in het print-object een BSN? Alleen een BSN kan nu worden opgezocht; KVK volgt later.",
      config: "contact_betrokkene_urn in het print-object (Objecten API)",
      onFail: "Mislukt (412): de reden noemt welk soort URN het was.",
      source: "PrintScenarioImplementation.TryResolveBsn",
    },
  },
  {
    key: "vtbcloudeventtype", name: "CloudEvent-type", short: "Gepubliceerd-event", active: true,
    subtitle: "Alleen nl.overheid.berichten.bericht-gepubliceerd",
    details: {
      what: "Is het CloudEvent een bericht-gepubliceerd? Alleen dan staat het bericht echt klaar. Een bericht-geregistreerd (of een toekomstig type) wordt genegeerd.",
      config: "Vast in de code: nl.overheid.berichten.bericht-gepubliceerd",
      onFail: "Geen actie nodig: OMC antwoordt succes, dus Open VTB biedt het niet opnieuw aan.",
      source: "MessageBoxScenarioImplementation.ProcessCloudEventAsync (stap 0)",
    },
  },
  {
    key: "vtbberichtid", name: "Bericht-ID", short: "Bericht-ID", active: true,
    subtitle: "subject van het CloudEvent is een bericht-UUID",
    details: {
      what: "Heeft het CloudEvent een subject, en is dat een geldige UUID? Daarmee wordt het bericht bij Open VTB opgehaald.",
      config: "subject van het CloudEvent",
      onFail: "Mislukt (412).",
      source: "MessageBoxScenarioImplementation.ProcessCloudEventAsync (stap 1)",
    },
  },
  {
    key: "vtbberichttekst", name: "Berichttekst", short: "Berichttekst", active: true,
    subtitle: "Het opgehaalde bericht heeft tekst",
    details: {
      what: "Heeft het bericht uit Open VTB een tekst? Een leeg bericht wordt niet verstuurd, ook niet via een terugval.",
      config: "Het bericht in Open VTB",
      onFail: "Bewust niet verstuurd: OMC antwoordt succes, dus geen herlevering (opnieuw proberen verandert niets).",
      source: "MessageBoxScenarioImplementation.ProcessCloudEventAsync (stap 5)",
    },
  },
  {
    key: "vtbontvanger", name: "Ontvanger is burger", short: "Ontvanger is burger", active: true,
    subtitle: "BSN in de ontvanger-URN",
    details: {
      what: "Draagt de ontvanger-URN van het bericht een BSN? Alleen burgers kunnen een bericht in de Berichtenbox krijgen.",
      config: "De ontvanger (URN) van het bericht in Open VTB",
      onFail: "Bewust niet verstuurd: OMC antwoordt succes, dus geen herlevering.",
      source: "MessageBoxScenarioImplementation.ProcessCloudEventAsync (stap 6)",
    },
  },
  {
    key: "vtbberichttype", name: "Berichttype whitelist", short: "Berichttype whitelist", active: true,
    subtitle: "Berichttype van het Open VTB-bericht",
    details: {
      what: "Staat het berichttype van het Open VTB-bericht op de whitelist? Geldt voor de Berichtenbox én de terugval.",
      config: "ZGW_WHITELIST_VTBMESSAGE_TYPES",
      onFail: "Bewust niet verstuurd: OMC antwoordt succes, dus Open Notificaties biedt het niet opnieuw aan.",
      source: "MessageBoxScenarioImplementation.ProcessCloudEventAsync (stap 7)",
    },
  },
  {
    key: "mobbgeschiktheid", name: "Berichtenbox of terugval", short: "Berichtenbox of terugval", active: true,
    subtitle: "isInMijnOverheidBerichtenbox → Berichtenbox, anders e-mail of brief",
    details: {
      what: "Hoort het bericht in de MijnOverheid Berichtenbox? Zo ja: naar de Berichtenbox. Zo nee: terugval naar digitale post (e-mail), en zonder e-mailadres een brief naar het adres uit de BRP.",
      config: "Het bericht in Open VTB; BRP voor het briefadres",
      onFail: "Geen afkeuring: deze stap kiest alleen het kanaal. Weigert Notify NL de Berichtenbox-verzending, dan volgt dezelfde terugval.",
      source: "MessageBoxScenarioImplementation.ProcessCloudEventAsync (stap 8), SendDigitalePostFallbackAsync",
    },
  },
  {
    key: "mobbemailadres", name: "E-mailadres bekend", short: "E-mailadres bekend", active: true,
    subtitle: "Terugval 1: digitale post per e-mail",
    details: {
      what: "Heeft de partij een e-mailadres in OpenKlant? Dan gaat het bericht als digitale post per e-mail. Zo niet, of weigert Notify NL die e-mail, dan volgt een brief.",
      config: "Digitale adressen van de partij in OpenKlant · NOTIFY_TEMPLATEID_EMAIL_MESSAGEBOX",
      onFail: "Geen afkeuring: terugval naar een brief.",
      source: "MessageBoxScenarioImplementation.SendDigitalePostFallbackAsync",
    },
  },
  {
    key: "brpadres", name: "Bruikbaar BRP-adres", short: "BRP-adres", active: true,
    subtitle: "Terugval 2: brief naar het adres uit de BRP",
    details: {
      what: "Geeft de BRP een bruikbaar adres voor de ontvanger? Dat is nodig voor de brief, het laatste kanaal. Het adres wordt nooit gelogd of bewaard.",
      config: "BRP (Haal Centraal) · NOTIFY_TEMPLATEID_LETTER_MESSAGEBOX",
      onFail: "Mislukt (412): er is geen kanaal meer over.",
      source: "MessageBoxScenarioImplementation.SendLetterFallbackAsync",
    },
  },
  {
    key: "naturalpersoncheck", name: "Natuurlijk persoon-check", short: "Natuurlijk persoon", active: true,
    subtitle: "CheckIfInitiatorIsNaturalPersonAsync — MijnZaken",
    details: {
      what: "Is de initiator van de zaak een natuurlijk persoon? Alleen burgers krijgen MijnZaken-berichten.",
      config: "De rol initiator van de zaak in OpenZaak",
      onFail: "Overgeslagen: het event wordt niet doorgestuurd naar MijnOverheid.",
      source: "MijnOverheidForwarder.VerifyNaturalPersonWithTraceAsync",
    },
  },
  {
    key: "mijnzaken-staleness", name: "Verouderd-check", short: "Verouderd", active: true,
    subtitle: "laatstGemuteerd/laatstGeopend vs. event-tijd",
    details: {
      what: "Is het event niet ouder dan de laatste wijziging (laatstGemuteerd) of opening (laatstGeopend) van de zaak? Zo komt een oud event nooit over een nieuwere stand heen.",
      config: "laatstGemuteerd / laatstGeopend op de zaak in OpenZaak",
      onFail: "Overgeslagen: het verouderde event wordt niet doorgestuurd.",
      source: "MijnOverheidForwarder.HandleMutatedAsync / HandleOpenedAsync",
    },
  },
  {
    key: "producttypewhitelist", name: "Producttype whitelist", short: "Producttype whitelist", active: true,
    subtitle: "producttype.code — ZGW_WHITELIST_PRODUCTCREATE_IDS",
    details: {
      what: "Staat de code van het producttype (uit het opgehaalde product, niet uit de kenmerken) op de whitelist? Een * staat alles toe.",
      config: "ZGW_WHITELIST_PRODUCTCREATE_IDS",
      onFail: "Afgebroken (206): de reden noemt de producttype-code en de env var.",
      source: "ProductScenarioImplementation.ValidateProductTypeIsWhitelisted",
    },
  },
  {
    key: "productgepubliceerd", name: "Publicatiecheck", short: "Gepubliceerd", active: true,
    subtitle: "product.gepubliceerd",
    details: {
      what: "Is het product gepubliceerd? Een product dat (nog) niet getoond wordt, wordt ook niet aangekondigd.",
      config: "Het veld gepubliceerd op het product in Open Product",
      onFail: "Afgebroken (206).",
      source: "ProductScenarioImplementation.ValidateProductIsPublished",
    },
  },
  {
    key: "productbezorging", name: "Productbezorging", short: "Bezorging", active: true,
    subtitle: "Per eigenaar e-mailen — mislukt → contactmoment",
    details: {
      what: "Stuurt elke eigenaar een eigen e-mail. Pas nadat alle eigenaren een partij hebben: een eigenaar zonder e-mailadres, of een verzending die Notify NL weigert, wordt een mislukt contactmoment.",
      config: "NOTIFY_TEMPLATEID_EMAIL_PRODUCTCREATED",
      onFail: "De notificatie slaagt (202); per eigenaar wordt de mislukking als contactmoment vastgelegd. Ontbreekt het template: mislukt (412).",
      source: "ProductScenarioImplementation.DeliverAsync",
    },
  },
];

export const CHECK_KEYS = new Set(CHECK_NODES.map((s) => s.key));

// "Letter"/"Both" branches exist in BaseScenario's DistributionChannel switch, but the live
// OpenKlant v2 integration (PartyResults.DetermineDistributionChannel) never returns them —
// only Email/Sms are reachable in production, so Notify Post stays wired but never "live" for
// any real flow. PostGuard is not part of this pipeline at all: it's a manual test endpoint
// (TestNotifyController) never invoked by NotifyProcessor or any scenario — marked inactive.
// KTO sits here rather than among the registers: OMC hands the survey off to it, the same way
// it hands a message to Notify NL (matches the OMC3 "Stroom" design).
export const CHANNEL_NODES: ArchitectureNode[] = [
  { key: "notify-email", name: "Notify E-mail", subtitle: "Notify NL — e-mail", active: true },
  { key: "notify-sms", name: "Notify SMS", subtitle: "Notify NL — sms", active: true },
  { key: "notify-post", name: "Notify Post", subtitle: "Notify NL — brief", active: true },
  { key: "printstraat", name: "Printstraat", subtitle: "Notify NL — kant-en-klare brief (pdf)", active: true },
  { key: "berichtenbox", name: "Logius Berichtenbox", subtitle: "MijnOverheid Berichtenbox — via Notify NL", active: true },
  { key: "logius-mijnzaken", name: "Logius MijnZaken", subtitle: "CloudEvent — MijnOverheid burgerportaal", active: true },
  { key: "kto", name: "KTO", subtitle: "Klanttevredenheidsonderzoek (Expoints)", active: true },
  { key: "postguard", name: "PostGuard", subtitle: "Handmatig testendpoint — geen scenario roept dit aan", active: false },
  { key: "lokale-berichtenbox", name: "Lokale Berichtenbox", subtitle: "Gemeentelijk portaal", active: false },
];

export const CONFIRMATION_NODES: ArchitectureNode[] = [
  { key: "contactmoment", name: "Contactmoment", subtitle: "Vastgelegd in Open Klant", active: true },
  { key: "archief", name: "Archief", subtitle: "Langetermijnopslag bevestigingen", active: false },
  { key: "contactherstel", name: "Contact herstel", subtitle: "Terugvalkanaal bij falen", active: false },
];

export const PATTERN_ENGINE_KEY = "output-patronen";

/** Trace stages the backend names differently from the card they belong to. The print flow
 * reports its hand-off to Notify NL as "notifynl"; on the diagram that is the Printstraat. */
export const STAGE_ALIASES: Record<string, string> = { notifynl: "printstraat" };

export type EdgeCategory = "invoer" | "verrijking" | "uitvoer" | "bevestiging";

export const EDGE_CATEGORY_COLOR: Record<EdgeCategory, string> = {
  invoer: "var(--color-arch-blue)",
  verrijking: "var(--color-arch-teal)",
  uitvoer: "var(--color-arch-indigo)",
  bevestiging: "var(--color-arch-violet)",
};

export interface FlowEdge {
  source: string;
  target: string;
  category: EdgeCategory;
}

// The static diagram — always the same shape, regardless of the selected flow. Which edges are
// "live" (vs. dimmed) is derived per flow in tracePath.ts.
//
// Everything runs through OMC: inputs feed it, it calls each register and gets a response back
// before continuing (a round trip, drawn downward to the register row), it runs the flow's
// checks (blocks inside the OMC block — see CHECK_NODES), and hands what passes to an output,
// which reports back as a contactmoment.
export const EDGES: FlowEdge[] = [
  ...INPUT_NODES.map((n) => ({ source: n.key, target: PATTERN_ENGINE_KEY, category: "invoer" as const })),
  ...REGISTER_NODES.map((n) => ({ source: PATTERN_ENGINE_KEY, target: n.key, category: "verrijking" as const })),
  ...CHANNEL_NODES.map((n) => ({ source: PATTERN_ENGINE_KEY, target: n.key, category: "uitvoer" as const })),

  { source: "notify-email", target: "contactmoment", category: "bevestiging" },
  { source: "notify-sms", target: "contactmoment", category: "bevestiging" },
  { source: "notify-post", target: "contactmoment", category: "bevestiging" },
  { source: "postguard", target: "contactmoment", category: "bevestiging" },
  { source: "berichtenbox", target: "contactmoment", category: "bevestiging" },
  { source: "printstraat", target: "contactmoment", category: "bevestiging" },
  { source: "lokale-berichtenbox", target: "contactherstel", category: "bevestiging" },
];

export interface FlowOption {
  /** Matches a ScenarioFlow.key from the API, or "all" for the overview state. */
  key: string;
  name: string;
  nl: string;
  /** Which INPUT_NODES entry point(s) this flow is reachable from — most flows have exactly
   * one; without this, scenarioKeys() can't tell Open Notificaties and Oneground apart and
   * both would show as "live" for every flow. */
  inputs: string[];
  registers: string[];
  /** CHECK_NODES keys, in the order this flow runs them. */
  filters: string[];
  channels: string[];
  confirmations: string[];
}

export const FLOW_OPTIONS: FlowOption[] = [
  {
    key: "all",
    name: "Alle flows",
    nl: "Volledig overzicht",
    inputs: INPUT_NODES.map((n) => n.key),
    registers: REGISTER_NODES.map((n) => n.key),
    filters: CHECK_NODES.map((n) => n.key),
    channels: CHANNEL_NODES.map((n) => n.key),
    confirmations: CONFIRMATION_NODES.map((n) => n.key),
  },
  {
    key: "case-created",
    name: "Case Created",
    nl: "Zaak aangemaakt",
    inputs: ["opennotificaties"],
    registers: ["openzaak", "openklant"],
    filters: ["zaaktypewhitelist", "informerencheck", "kanaalresolutie"],
    channels: ["notify-email", "notify-sms"],
    confirmations: ["contactmoment"],
  },
  {
    key: "case-updated",
    name: "Case Status Updated",
    nl: "Zaakstatus bijgewerkt",
    inputs: ["opennotificaties"],
    registers: ["openzaak", "openklant"],
    filters: ["zaaktypewhitelist", "informerencheck", "kanaalresolutie"],
    channels: ["notify-email", "notify-sms"],
    confirmations: ["contactmoment"],
  },
  {
    key: "case-closed",
    name: "Case Closed",
    nl: "Zaak afgesloten",
    inputs: ["opennotificaties"],
    registers: ["openzaak", "openklant"],
    filters: ["zaaktypewhitelist", "informerencheck", "kanaalresolutie"],
    channels: ["notify-email", "notify-sms"],
    confirmations: ["contactmoment"],
  },
  {
    key: "task-assigned",
    name: "Task Assigned",
    nl: "Taak toegewezen",
    inputs: ["opennotificaties"],
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
    inputs: ["opennotificaties"],
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
    inputs: ["opennotificaties"],
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
    key: "product-created",
    name: "Product Created",
    nl: "Product aangemaakt",
    inputs: ["opennotificaties"],
    // Real order in ProductScenarioImplementation.ProcessProductAsync: fetch the product from
    // Open Product, the producttype whitelist, the gepubliceerd check, then one OpenKlant
    // lookup per eigenaar, then the delivery fan-out ("productbezorging"). Always e-mail, so no
    // kanaalresolutie.
    registers: ["openproduct", "openklant"],
    filters: ["producttypewhitelist", "productgepubliceerd", "productbezorging"],
    channels: ["notify-email"],
    confirmations: ["contactmoment"],
  },
  {
    key: "print-requested",
    name: "Print Requested",
    nl: "Printstraat",
    inputs: ["opennotificaties"],
    // Real order in PrintScenarioImplementation.ProcessPrintAsync: printschakelaar, read the print
    // object, the pdfurl check, the BSN in the betrokkene-URN, the partij, the pdf itself, then
    // the precompiled letter to Notify NL.
    registers: ["objecten", "openklant", "documenten"],
    filters: ["printschakelaar", "documentcheck", "betrokkeneurn"],
    channels: ["printstraat"],
    confirmations: ["contactmoment"],
  },
  {
    // Open VTB's "bericht gepubliceerd" CloudEvent, delivered via Open Notificaties like every other
    // event. The longest flow: Berichtenbox, then digitale post (e-mail), then a letter to the BRP
    // address — each traced as it happens.
    key: "message-box",
    name: "Berichtenbox (MOBB)",
    nl: "Bericht in Berichtenbox",
    inputs: ["opennotificaties"],
    registers: ["vtb", "openklant", "documenten", "brp"],
    filters: [
      "vtbcloudeventtype",
      "vtbberichtid",
      "vtbberichttekst",
      "vtbontvanger",
      "vtbberichttype",
      "mobbgeschiktheid",
      "mobbemailadres",
      "brpadres",
    ],
    channels: ["berichtenbox", "notify-email", "notify-post"],
    confirmations: ["contactmoment"],
  },
  {
    key: "kto",
    name: "Customer Satisfaction (KTO)",
    nl: "Klanttevredenheidsonderzoek",
    inputs: ["opennotificaties"],
    registers: ["objecten"],
    filters: [],
    channels: ["kto"],
    confirmations: [],
  },
  {
    key: "mijnzaken-gemuteerd",
    name: "MijnZaken - Case Mutated",
    nl: "MijnZaken - Zaak gemuteerd",
    inputs: ["oneground"],
    registers: ["openzaak"],
    // Real order in MijnOverheidForwarder.HandleMutatedAsync: fetch case, natural-person
    // check, fetch status + status type, then the shared zaaktype whitelist + informeren-check
    // (same fields/env vars the main pipeline's case scenarios use, re-checked independently
    // here), then its own staleness check against Case.LatestMutationDate ("laatstGemuteerd").
    filters: ["naturalpersoncheck", "zaaktypewhitelist", "informerencheck", "mijnzaken-staleness"],
    channels: ["logius-mijnzaken"],
    confirmations: [],
  },
  {
    key: "mijnzaken-geopend",
    name: "MijnZaken - Case Opened",
    nl: "MijnZaken - Zaak geopend",
    inputs: ["oneground"],
    registers: ["openzaak"],
    // No whitelist/informeren check for this type. HandleOpenedAsync has a first-open
    // shortcut (no prior laatstGeopend on the case → forward unconditionally, skipping both
    // filters below) — the direct Output Patronen → Verouderd-check edge covers that path.
    filters: ["naturalpersoncheck", "mijnzaken-staleness"],
    channels: ["logius-mijnzaken"],
    confirmations: [],
  },
  {
    key: "mijnzaken-verwijderd",
    name: "MijnZaken - Case Deleted",
    nl: "MijnZaken - Zaak verwijderd",
    inputs: ["oneground"],
    // Unconditional forward — HandleDeletedAsync never fetches case data (a deleted case
    // can't be fetched), so there's nothing to filter on at all.
    registers: [],
    filters: [],
    channels: ["logius-mijnzaken"],
    confirmations: [],
  },
];

// Node positions live in status/flow/page.tsx (computeLayout): a fixed grid, not auto-placed.
