import { EDGES } from "./architecture";

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

/** Shortest hop-by-hop path between two node keys. Returns null if either key is unknown to
 * the architecture graph or no path exists (shouldn't happen — the graph is connected). */
export function findTracePath(from: string, to: string): string[] | null {
  if (from === to) return [from];
  if (!ADJACENCY.has(from) || !ADJACENCY.has(to)) return null;

  const queue: string[][] = [[from]];
  const visited = new Set([from]);

  while (queue.length > 0) {
    const path = queue.shift()!;
    const node = path[path.length - 1];

    for (const next of ADJACENCY.get(node) ?? []) {
      if (visited.has(next)) continue;
      const nextPath = [...path, next];
      if (next === to) return nextPath;
      visited.add(next);
      queue.push(nextPath);
    }
  }
  return null;
}
