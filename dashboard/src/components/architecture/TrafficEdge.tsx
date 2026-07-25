import { BaseEdge, EdgeProps, getBezierPath } from "@xyflow/react";

export interface TrafficEdgeData extends Record<string, unknown> {
  /** True while a real trace hop is currently traversing this exact edge. */
  live?: boolean;
  /** True when the hop is walking the edge backward (e.g. a register's return trip to
   * Output Patronen) — the dot rides the identical curve, just sampled end-to-start. */
  reverse?: boolean;
  /** Monotonic hop counter from useOmcTelemetry — used as the dot's React key so its SMIL
   * animation restarts even when this same edge carries two separate hops in a row (e.g. the
   * scenario resolver and the scenario itself each calling OpenZaak). */
  seq?: number;
  /** Category color for the traveling dot; falls back to the edge's own stroke. */
  liveColor?: string;
}

// Curved edge matching the reference design's style. When `live`, a single dot rides this
// edge's own already-correct bezier path once, over the real duration of that one hop — no
// separate cross-node pip/portal needed, and no risk of it disagreeing with the line drawn
// underneath, since it's the exact same `path` value. Nothing animates when no real
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

  const { live, reverse, seq, liveColor } = (data as TrafficEdgeData) ?? {};

  // For a reverse hop (a register's return trip to Output Patronen), get the identical curve
  // walked backward by asking getBezierPath for it directly — swap which end is "source" and
  // "target" — rather than playing the forward path with SMIL's keyPoints/keyTimes reversal:
  // Chromium doesn't actually animate keyPoints against an inline `path` attribute (verified —
  // calcMode="linear" is correctly applied, but the dot still renders as a static, unanimated
  // snap to its end position instead of playing), so this avoids that engine limitation
  // entirely by only ever using plain, always-supported forward playback.
  const [reversePath] = reverse
    ? getBezierPath({
        sourceX: targetX,
        sourceY: targetY,
        sourcePosition: targetPosition,
        targetX: sourceX,
        targetY: sourceY,
        targetPosition: sourcePosition,
        curvature: 0.42,
      })
    : [undefined];

  return (
    <>
      <BaseEdge id={id} path={path} style={style} />
      {live && (
        <circle r={5.5} fill={liveColor ?? style?.stroke ?? "var(--color-arch-teal)"} stroke="white" strokeWidth={1.5}>
          <animateMotion
            key={seq}
            // `begin="indefinite"` plus an explicit beginElement() call on mount, rather than
            // relying on the default implicit "begin as soon as inserted" behavior — the first
            // dot of a page session plays fine either way, but every subsequent one silently
            // never started (verified: present in the DOM with correct attributes, but frozen
            // at its end position for its entire duration). Whatever timing ambiguity causes
            // that, explicitly driving begin() sidesteps it.
            begin="indefinite"
            ref={(el) => (el as SVGAnimateMotionElement | null)?.beginElement()}
            dur="0.7s"
            fill="freeze"
            path={reverse ? reversePath : path}
          />
        </circle>
      )}
    </>
  );
}
