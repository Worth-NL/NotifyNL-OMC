import { BaseEdge, EdgeProps, getBezierPath } from "@xyflow/react";

export interface TrafficEdgeData extends Record<string, unknown> {
  /** True while a real trace hop is currently traversing this exact edge. */
  live?: boolean;
  /** True when the hop is walking the edge backward (e.g. a register's return trip to
   * Output Patronen) — flips which way the flowing dots travel. */
  reverse?: boolean;
  /** Category color for the flowing-dots overlay; falls back to the edge's own stroke. */
  liveColor?: string;
}

// Curved edge matching the reference design's style. The flowing-dots overlay below uses the
// same stroke-dasharray/stroke-dashoffset technique as the reference's always-on ambient
// animation, but — unlike that version — is only ever rendered on the specific edge a real
// trace hop is currently traversing, for as long as it's live. Nothing animates when no real
// notification is moving through the system.
export function TrafficEdge({
  id,
  sourceX,
  sourceY,
  targetX,
  targetY,
  sourcePosition,
  targetPosition,
  style,
  data,
}: EdgeProps) {
  const [path] = getBezierPath({
    sourceX,
    sourceY,
    sourcePosition,
    targetX,
    targetY,
    targetPosition,
    curvature: 0.42,
  });

  const { live, reverse, liveColor } = (data as TrafficEdgeData) ?? {};

  return (
    <>
      <BaseEdge id={id} path={path} style={style} />
      {live && (
        <path
          d={path}
          fill="none"
          stroke={liveColor ?? style?.stroke ?? "var(--color-arch-teal)"}
          strokeWidth={2.75}
          strokeLinecap="round"
          strokeDasharray="2 15"
          style={{
            animation: "arch-flow-dash 0.5s linear infinite",
            animationDirection: reverse ? "reverse" : "normal",
          }}
        />
      )}
    </>
  );
}
