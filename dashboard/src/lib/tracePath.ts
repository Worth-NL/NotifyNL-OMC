import { EDGES, FLOW_OPTIONS, PATTERN_ENGINE_KEY } from "./architecture";

// The architecture graph's edges are directional for rendering (arrows point the way data
// conceptually flows), but a real notification's journey is a sequence of round trips through
// Output Patronen — treating the graph as undirected for pathfinding purposes lets the pip
// travel back through Output Patronen between unrelated stages (e.g. a whitelist check and a
// register call), which is exactly what really happens: OMC is the sole hub, nothing calls
// anything else directly.
function buildAdjacency(): Map<string, Set<string>> {
  const adjacency = new Map<string, Set<string>>();
  const link = (a: string, b: string) => {
    if (!adjacency.has(a)) adjacency.set(a, new Set());
    adjacency.get(a)!.add(b);
  };
  for (const e of EDGES) {
    link(e.source, e.target);
    link(e.target, e.source);
  }
  return adjacency;
}

const ADJACENCY = buildAdjacency();

// Plain shortest-path BFS over the full graph can "shortcut" through a gate that belongs to a
// completely different scenario — e.g. Case Created's last register call (Open Klant) has only
// one edge, to Output Patronen, and from there the *graph-theoretically* shortest route to
// Kanaalresolutie is via Berichten-schakelaar (2 hops) rather than the real chain of
// Zaaktype whitelist → Informeren-check (3 hops), even though Case Created never touches
// Berichten-schakelaar at all — that gate is Message Received's own. Restricting BFS to the
// set of nodes a given scenario actually uses (mirroring the same FLOW_OPTIONS data the page
// already uses to dim unused cards) makes it pick the real chain instead of a topological
// shortcut through an unrelated scenario's gate.
const SCENARIO_KEYS: Map<string, Set<string>> = new Map(
  FLOW_OPTIONS.map((option) => [
    option.key,
    new Set([
      ...option.inputs,
      PATTERN_ENGINE_KEY,
      ...option.registers,
      ...option.filters,
      ...option.channels,
      ...option.confirmations,
    ]),
  ]),
);

/** The set of node keys a given scenario actually uses, for constraining trace playback to the
 * real chain — `null` for an unrecognized/not-yet-determined scenario (no restriction). */
export function scenarioKeys(scenario: string | null): Set<string> | null {
  return scenario ? (SCENARIO_KEYS.get(scenario) ?? null) : null;
}

const REAL_EDGE_KEYS = new Set(EDGES.map((e) => `${e.source}->${e.target}`));
const ALL_EDGE_KEYS = new Set(REAL_EDGE_KEYS);

// Which *edges* (not just nodes) are actually walked by a given scenario — used to decide which
// static lines the diagram lights up. Node membership alone isn't enough: several scenarios
// share the same node (e.g. Zaaktype whitelist, or Verouderd-check) via a *different* edge into
// it — a scenario-family's own shortcut (Output Patronen → Zaaktype whitelist for the 4 main
// case-family flows, Natuurlijk persoon-check → Verouderd-check for MijnZaken - Case Opened,
// Output Patronen → Logius MijnZaken for MijnZaken - Case Deleted). Highlighting every edge
// between two "used" nodes lit up all of those siblings' shortcuts at once alongside the real
// chain, drawing several crossing lines into the same card instead of one strand. Reconstructing
// the real chain from each scenario's *ordered* filters list (hub → filters[0] → filters[1] →
// … → filters[last] → each channel → each confirmation) picks out only the edges that scenario
// actually traverses.
const SCENARIO_EDGE_KEYS: Map<string, Set<string>> = new Map(
  FLOW_OPTIONS.map((option) => {
    if (option.key === "all") return [option.key, ALL_EDGE_KEYS];

    const live = new Set<string>();
    const addIfReal = (source: string, target: string) => {
      const key = `${source}->${target}`;
      if (REAL_EDGE_KEYS.has(key)) live.add(key);
    };

    for (const input of option.inputs) addIfReal(input, PATTERN_ENGINE_KEY);
    for (const register of option.registers) addIfReal(PATTERN_ENGINE_KEY, register);

    if (option.filters.length > 0) {
      addIfReal(PATTERN_ENGINE_KEY, option.filters[0]);
      for (let i = 0; i < option.filters.length - 1; i++) {
        addIfReal(option.filters[i], option.filters[i + 1]);
      }
      const lastFilter = option.filters[option.filters.length - 1];
      for (const channel of option.channels) addIfReal(lastFilter, channel);
    } else {
      for (const channel of option.channels) addIfReal(PATTERN_ENGINE_KEY, channel);
    }

    for (const channel of option.channels) {
      for (const confirmation of option.confirmations) addIfReal(channel, confirmation);
    }

    return [option.key, live];
  }),
);

/** The set of edges (as `"source->target"` keys matching each EDGES entry) a given scenario
 * actually traverses, for deciding which static diagram lines to highlight — `null` for an
 * unrecognized/not-yet-determined scenario (no restriction, nothing lights up). */
export function scenarioEdgeKeys(scenario: string | null): Set<string> | null {
  return scenario ? (SCENARIO_EDGE_KEYS.get(scenario) ?? null) : null;
}

/** Shortest hop-by-hop path between two node keys. Returns null if either key is unknown to
 * the architecture graph or no path exists (shouldn't happen — the graph is connected).
 * When `allowedKeys` is given, only travels through nodes in that set (plus the endpoints
 * themselves) — see the module comment above for why. */
export function findTracePath(from: string, to: string, allowedKeys?: Set<string> | null): string[] | null {
  if (from === to) return [from];
  if (!ADJACENCY.has(from) || !ADJACENCY.has(to)) return null;

  const isAllowed = (key: string) => !allowedKeys || key === from || key === to || allowedKeys.has(key);

  const queue: string[][] = [[from]];
  const visited = new Set([from]);

  while (queue.length > 0) {
    const path = queue.shift()!;
    const node = path[path.length - 1];

    for (const next of ADJACENCY.get(node) ?? []) {
      if (visited.has(next) || !isAllowed(next)) continue;
      const nextPath = [...path, next];
      if (next === to) return nextPath;
      visited.add(next);
      queue.push(nextPath);
    }
  }
  return null;
}
