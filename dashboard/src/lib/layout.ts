import dagre from "@dagrejs/dagre";

export interface LayoutInputNode {
  id: string;
  width: number;
  height: number;
}

export interface LayoutInputEdge {
  source: string;
  target: string;
}

export interface LayoutResult {
  positions: Record<string, { x: number; y: number }>;
  /** Handle pair to use per edge, chosen from the actual computed geometry so a mostly-vertical
   * connection (e.g. two nodes stacked in the same column) exits/enters top/bottom instead of
   * looping out the side, and a mostly-horizontal one uses left/right as usual. */
  edgeHandles: Record<string, { sourceHandle: "right" | "bottom"; targetHandle: "left" | "top" }>;
}

/**
 * Picks a handle pair per edge from real node geometry — exported separately from
 * `layoutGraph` so callers who hand-adjust a few Dagre-computed positions afterward (see the
 * filter-chain override in status/flow/page.tsx) can re-derive handles matching the *final*
 * layout instead of the pre-adjustment one.
 */
export function computeEdgeHandles(
  positions: LayoutResult["positions"],
  nodes: LayoutInputNode[],
  edges: LayoutInputEdge[],
): LayoutResult["edgeHandles"] {
  const edgeHandles: LayoutResult["edgeHandles"] = {};
  for (const e of edges) {
    const sourcePos = positions[e.source];
    const targetPos = positions[e.target];
    const sourceNode = nodes.find((n) => n.id === e.source)!;
    const dx = targetPos.x - sourcePos.x;
    const dy = targetPos.y - sourcePos.y;
    const mostlyVertical = Math.abs(dy) > Math.abs(dx) + sourceNode.width * 0.5;

    edgeHandles[`${e.source}->${e.target}`] = mostlyVertical
      ? { sourceHandle: "bottom", targetHandle: "top" }
      : { sourceHandle: "right", targetHandle: "left" };
  }
  return edgeHandles;
}

/**
 * Runs Dagre's layered graph layout (left-to-right) to get crossing-minimized node positions
 * from the actual edge topology, instead of hand-placed coordinates that inevitably cross once
 * a column has more than a couple of fan-out connections.
 */
export function layoutGraph(nodes: LayoutInputNode[], edges: LayoutInputEdge[]): LayoutResult {
  const g = new dagre.graphlib.Graph();
  g.setGraph({ rankdir: "LR", nodesep: 28, ranksep: 110, marginx: 20, marginy: 20 });
  g.setDefaultEdgeLabel(() => ({}));

  for (const n of nodes) {
    g.setNode(n.id, { width: n.width, height: n.height });
  }
  for (const e of edges) {
    g.setEdge(e.source, e.target);
  }

  dagre.layout(g);

  const positions: LayoutResult["positions"] = {};
  for (const n of nodes) {
    const { x, y } = g.node(n.id);
    // Dagre returns the node's center — React Flow positions by top-left corner.
    positions[n.id] = { x: x - n.width / 2, y: y - n.height / 2 };
  }

  return { positions, edgeHandles: computeEdgeHandles(positions, nodes, edges) };
}
