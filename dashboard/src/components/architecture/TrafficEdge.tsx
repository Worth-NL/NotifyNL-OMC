import { BaseEdge, Edge, EdgeProps, getBezierPath } from "@xyflow/react";

export interface TrafficEdgeData extends Record<string, unknown> {
  live: boolean;
  /** Simulated messages/sec for the edge's source node — drives bubble speed and count. */
  throughput: number;
}

type TrafficEdgeType = Edge<TrafficEdgeData, "traffic">;

/** How many traffic bubbles to show and how fast, scaled to (fake) message volume. */
function bubbleProfile(throughput: number): { count: number; durationSec: number } {
  if (throughput < 150) return { count: 1, durationSec: 4.5 };
  if (throughput < 500) return { count: 2, durationSec: 3 };
  return { count: 3, durationSec: 1.8 };
}

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
}: EdgeProps<TrafficEdgeType>) {
  const [path] = getBezierPath({
    sourceX,
    sourceY,
    sourcePosition,
    targetX,
    targetY,
    targetPosition,
    curvature: 0.42,
  });

  const live = data?.live ?? false;
  const { count, durationSec } = bubbleProfile(data?.throughput ?? 0);
  const color = (style?.stroke as string) ?? "var(--color-arch-teal)";

  return (
    <>
      <BaseEdge id={id} path={path} style={style} />
      {live &&
        Array.from({ length: count }).map((_, i) => (
          <circle key={i} r={2.75} fill={color}>
            <animateMotion
              dur={`${durationSec}s`}
              begin={`${(i * durationSec) / count}s`}
              repeatCount="indefinite"
              path={path}
            />
          </circle>
        ))}
    </>
  );
}
