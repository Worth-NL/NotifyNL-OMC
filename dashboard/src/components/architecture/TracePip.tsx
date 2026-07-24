"use client";

import { getBezierPath, Position } from "@xyflow/react";
import { TraceHop } from "@/hooks/useOmcTelemetry";

export interface NodeRect {
  x: number;
  y: number;
  width: number;
  height: number;
}

function centerOf(rect: NodeRect): { x: number; y: number } {
  return { x: rect.x + rect.width / 2, y: rect.y + rect.height / 2 };
}

// Which side a bezier "leaves"/"enters" from, purely for a natural-looking curve shape — this
// has no bearing on jump-continuity (that comes from always using node centers as endpoints,
// so it never matters which side the curve bulges toward).
function directionPosition(dx: number, dy: number, isSource: boolean): Position {
  if (Math.abs(dx) > Math.abs(dy)) {
    const leaving = dx > 0;
    return isSource ? (leaving ? Position.Right : Position.Left) : leaving ? Position.Left : Position.Right;
  }
  const leaving = dy > 0;
  return isSource ? (leaving ? Position.Bottom : Position.Top) : leaving ? Position.Top : Position.Bottom;
}

/**
 * Renders inside a <ViewportPortal> so it shares the same coordinate space (and, if panning
 * were ever re-enabled, the same pan/zoom transform) as the architecture graph's nodes/edges.
 *
 * Always travels node-center to node-center — deliberately not the same handle points the
 * static edges use. Two different edges through the same hub node (e.g. Output Patronen) can
 * legitimately use different sides (one arrives from below, another departs to the right), so
 * chaining hops via handle points made the pip visibly jump across the card between them. A
 * node's center is a single canonical point regardless of which edge is involved, so any
 * sequence of hops through that node lines up exactly — the pip glides continuously with no
 * jump, no matter how many different edges it passes through in a row.
 *
 * `hop.seq` (owned by useOmcTelemetry, incremented once per hop it plays) is used as part of
 * the <animateMotion> key so the SMIL animation restarts fresh even when two different hops
 * happen to share the same (from, to) pair — no local state needed here at all.
 */
export function TracePip({ hop, nodeRects }: { hop: TraceHop | null; nodeRects: Record<string, NodeRect> }) {
  if (!hop) return null;

  const fromRect = nodeRects[hop.from];
  const toRect = nodeRects[hop.to];
  if (!fromRect || !toRect) return null;

  const source = centerOf(fromRect);
  const target = centerOf(toRect);
  const dx = target.x - source.x;
  const dy = target.y - source.y;

  const [path] = getBezierPath({
    sourceX: source.x,
    sourceY: source.y,
    sourcePosition: directionPosition(dx, dy, true),
    targetX: target.x,
    targetY: target.y,
    targetPosition: directionPosition(dx, dy, false),
    curvature: 0.3,
  });

  return (
    <svg style={{ position: "absolute", overflow: "visible", pointerEvents: "none" }}>
      <circle r={5.5} fill="var(--color-arch-teal)" stroke="white" strokeWidth={1.5}>
        <animateMotion key={`${hop.from}-${hop.to}-${hop.seq}`} dur="0.6s" fill="freeze" path={path} />
      </circle>
    </svg>
  );
}
