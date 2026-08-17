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
