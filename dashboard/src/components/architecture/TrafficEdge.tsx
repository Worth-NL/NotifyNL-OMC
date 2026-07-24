import { BaseEdge, EdgeProps, getBezierPath } from "@xyflow/react";

// Curved edge matching the reference design's style. Used to render animated "ambient
// traffic" bubbles driven by spoofed throughput data — removed because it read as real
// activity when nothing was actually happening. The real trace pip (TracePip.tsx) is now the
// only thing that moves along these edges, and only for genuine notifications.
export function TrafficEdge({ id, sourceX, sourceY, targetX, targetY, sourcePosition, targetPosition, style }: EdgeProps) {
  const [path] = getBezierPath({
    sourceX,
    sourceY,
    sourcePosition,
    targetX,
    targetY,
    targetPosition,
    curvature: 0.42,
  });

  return <BaseEdge id={id} path={path} style={style} />;
}
