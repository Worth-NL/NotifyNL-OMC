import { BaseEdge, EdgeProps, getBezierPath } from "@xyflow/react";

export interface TrafficEdgeData extends Record<string, unknown> {
  /** One entry per real trace hop currently traversing this exact edge — several notifications
   * arriving close together can each be mid-flight on the same connector simultaneously, so
   * this is a list rather than a single flag. */
  hops?: {
    /** Monotonic hop counter from useOmcTelemetry — used as this dot's React key so its SMIL
     * animation restarts even when this same edge carries two separate hops in a row (e.g. the
     * scenario resolver and the scenario itself each calling OpenZaak), and so two different
     * traces' dots on the same edge never collide. */
    seq: number;
    /** True when this hop is walking the edge backward (e.g. a register's return trip to
     * Output Patronen) — the dot rides the identical curve, just sampled end-to-start. */
    reverse: boolean;
  }[];
  /** Category color for the traveling dot(s); falls back to the edge's own stroke. */
  liveColor?: string;
}

// Stable (module-scope) ref callback — deliberately NOT an inline arrow function in the JSX
// below. React re-invokes a callback ref whenever the ref PROP's identity changes between
// renders, even for an element that stays mounted (same key). With two hops sharing one edge,
// advancing one of them re-renders the other hop's still-playing <animateMotion> too; an inline
// `(el) => el?.beginElement()` would count as a "new" ref each time and get re-fired, restarting
// that animation mid-flight — the exact glitch seen when multiple dots share a line. A stable
// reference is only called on genuine mount/unmount, so a sibling hop's re-render leaves it alone.
function beginAnimateMotion(el: SVGAnimateMotionElement | null) {
  el?.beginElement();
}

// Curved edge matching the reference design's style. Each entry in `hops` rides this edge's
// own already-correct bezier path once, over the real duration of that one hop — no separate
// cross-node pip/portal needed, and no risk of it disagreeing with the line drawn underneath,
// since it's the exact same `path` value. Nothing animates when no real notification is moving
// through the system; several dots can render at once when several traces overlap.
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

  const { hops, liveColor } = (data as TrafficEdgeData) ?? {};

  // For a reverse hop (a register's return trip to Output Patronen), get the identical curve
  // walked backward by asking getBezierPath for it directly — swap which end is "source" and
  // "target" — rather than playing the forward path with SMIL's keyPoints/keyTimes reversal:
  // Chromium doesn't actually animate keyPoints against an inline `path` attribute (verified —
  // calcMode="linear" is correctly applied, but the dot still renders as a static, unanimated
  // snap to its end position instead of playing), so this avoids that engine limitation
  // entirely by only ever using plain, always-supported forward playback. Computed unconditionally
  // (cheap) since any individual hop in the list might need it.
  const [reversePath] = getBezierPath({
    sourceX: targetX,
    sourceY: targetY,
    sourcePosition: targetPosition,
    targetX: sourceX,
    targetY: sourceY,
    targetPosition: sourcePosition,
    curvature: 0.42,
  });

  return (
    <>
      <BaseEdge id={id} path={path} style={style} />
      {hops?.map((hop) => (
        <circle key={hop.seq} r={5.5} fill={liveColor ?? style?.stroke ?? "var(--color-arch-teal)"} stroke="white" strokeWidth={1.5}>
          <animateMotion
            key={hop.seq}
            // `begin="indefinite"` plus an explicit beginElement() call on mount, rather than
            // relying on the default implicit "begin as soon as inserted" behavior — the first
            // dot of a page session plays fine either way, but every subsequent one silently
            // never started (verified: present in the DOM with correct attributes, but frozen
            // at its end position for its entire duration). Whatever timing ambiguity causes
            // that, explicitly driving begin() sidesteps it.
            begin="indefinite"
            ref={beginAnimateMotion}
            dur="0.7s"
            fill="freeze"
            path={hop.reverse ? reversePath : path}
          />
        </circle>
      ))}
    </>
  );
}
