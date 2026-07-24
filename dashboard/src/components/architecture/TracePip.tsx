"use client";

import { getBezierPath, Position } from "@xyflow/react";
import { LayoutResult } from "@/lib/layout";
import { TraceHop } from "@/hooks/useTraceStream";

export interface NodeRect {
  x: number;
  y: number;
  width: number;
  height: number;
}

type HandleSide = "left" | "right" | "top" | "bottom";

const HANDLE_POSITION: Record<HandleSide, Position> = {
  left: Position.Left,
  right: Position.Right,
  top: Position.Top,
  bottom: Position.Bottom,
};

function handlePoint(rect: NodeRect, side: HandleSide): { x: number; y: number } {
  switch (side) {
    case "left":
      return { x: rect.x, y: rect.y + rect.height / 2 };
    case "right":
      return { x: rect.x + rect.width, y: rect.y + rect.height / 2 };
    case "top":
      return { x: rect.x + rect.width / 2, y: rect.y };
    case "bottom":
      return { x: rect.x + rect.width / 2, y: rect.y + rect.height };
  }
}

// A hop's direction can run either way along an edge defined in EDGES (see tracePath.ts) — a
// register round-trip uses the same visual line whether the pip is "arriving" or "returning".
// Travelling the reverse direction just swaps which side is the exit vs. the entry.
function resolveHandles(from: string, to: string, edgeHandles: LayoutResult["edgeHandles"]) {
  const forward = edgeHandles[`${from}->${to}`];
  if (forward) return forward;

  const reverse = edgeHandles[`${to}->${from}`];
  if (reverse) return { sourceHandle: reverse.targetHandle, targetHandle: reverse.sourceHandle };

  return { sourceHandle: "bottom" as const, targetHandle: "top" as const };
}

/**
 * Renders inside a <ViewportPortal> so it shares the same coordinate space (and, if panning
 * were ever re-enabled, the same pan/zoom transform) as the architecture graph's nodes/edges.
 * `hop.seq` (owned by useTraceStream, incremented once per hop it plays) is used as part of
 * the <animateMotion> key so the SMIL animation restarts fresh even when two different hops
 * happen to share the same (from, to) pair — no local state needed here at all.
 */
export function TracePip({
  hop,
  nodeRects,
  edgeHandles,
}: {
  hop: TraceHop | null;
  nodeRects: Record<string, NodeRect>;
  edgeHandles: LayoutResult["edgeHandles"];
}) {
  if (!hop) return null;

  const fromRect = nodeRects[hop.from];
  const toRect = nodeRects[hop.to];
  if (!fromRect || !toRect) return null;

  const handles = resolveHandles(hop.from, hop.to, edgeHandles);
  const source = handlePoint(fromRect, handles.sourceHandle);
  const target = handlePoint(toRect, handles.targetHandle);

  const [path] = getBezierPath({
    sourceX: source.x,
    sourceY: source.y,
    sourcePosition: HANDLE_POSITION[handles.sourceHandle],
    targetX: target.x,
    targetY: target.y,
    targetPosition: HANDLE_POSITION[handles.targetHandle],
    curvature: 0.42,
  });

  return (
    <svg style={{ position: "absolute", overflow: "visible", pointerEvents: "none" }}>
      <circle r={5.5} fill="var(--color-arch-teal)" stroke="white" strokeWidth={1.5}>
        <animateMotion key={`${hop.from}-${hop.to}-${hop.seq}`} dur="0.5s" fill="freeze" path={path} />
      </circle>
    </svg>
  );
}
