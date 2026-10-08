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

// The set of node keys each scenario uses (mirroring the FLOW_OPTIONS data the page uses to dim
// unused cards). Pathfinding is restricted to it so a trace's dot never travels through another
// scenario's register or output on its way somewhere. Individual checks are included for
// completeness, but they are not graph nodes: useOmcTelemetry parks the dot on the OMC block for
// them (see CHECK_NODES).
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

// Which edges a given scenario actually uses, for deciding which static lines light up. Checks
// are blocks inside the OMC block rather than nodes (see CHECK_NODES), so a flow's edges are its
// inputs into OMC, OMC's round trips to its registers, OMC's hand-off to its outputs, and each
// output's confirmation.
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
    for (const channel of option.channels) {
      addIfReal(PATTERN_ENGINE_KEY, channel);
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
