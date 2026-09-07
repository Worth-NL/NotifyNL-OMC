import { Handle, Position } from "@xyflow/react";
import { ArchitectureNode, FlowOption } from "@/lib/architecture";
import { NodeCard, NodeState } from "./NodeCard";
import { PatternEnginePanel } from "./PatternEnginePanel";

const HANDLE_CLASS = "!h-2 !w-2 !border-0 !bg-arch-border";

/** Every node exposes all four sides as both a possible target and source — which pair an
 * edge actually uses is decided per-edge in layout.ts based on the real computed geometry,
 * so a connection between vertically stacked nodes exits top/bottom instead of looping
 * sideways, matching the reference design. */
function AllHandles() {
  return (
    <>
      <Handle type="target" id="left" position={Position.Left} className={HANDLE_CLASS} />
      <Handle type="target" id="top" position={Position.Top} className={HANDLE_CLASS} />
      <Handle type="source" id="right" position={Position.Right} className={HANDLE_CLASS} />
      <Handle type="source" id="bottom" position={Position.Bottom} className={HANDLE_CLASS} />
    </>
  );
}

export interface FlowNodeData extends Record<string, unknown> {
  node: ArchitectureNode;
  state: NodeState;
  throughput?: number;
  onClick?: () => void;
}

// @xyflow/react sets `pointer-events: none` inline on `.react-flow__node` whenever the node
// isn't selectable/draggable and has no onNodeClick/mouse handlers registered on <ReactFlow>
// (see NodeWrapper's `hasPointerEvents` check in its source) — which is our case, since we
// use nodesDraggable={false}/elementsSelectable={false} and drive interaction entirely through
// our own component content. That inline style inherits down to every descendant, silently
// disabling the select/buttons inside. `pointer-events-auto` here overrides it back on.
export function FlowNode({ data }: { data: FlowNodeData }) {
  return (
    <div className="pointer-events-auto" style={{ width: 220 }}>
      <AllHandles />
      <NodeCard node={data.node} state={data.state} throughput={data.throughput} onClick={data.onClick} />
    </div>
  );
}

export interface PatternEngineNodeData extends Record<string, unknown> {
  flows: FlowOption[];
  selectedKey: string;
  onSelect: (key: string) => void;
  totalProcessed: number;
  onViewDiagram: () => void;
  diagramAvailable: boolean;
}

export function PatternEngineFlowNode({ data }: { data: PatternEngineNodeData }) {
  return (
    <div className="pointer-events-auto">
      <AllHandles />
      <PatternEnginePanel
        flows={data.flows}
        selectedKey={data.selectedKey}
        onSelect={data.onSelect}
        totalProcessed={data.totalProcessed}
        onViewDiagram={data.onViewDiagram}
        diagramAvailable={data.diagramAvailable}
      />
    </div>
  );
}

export function ColumnLabelNode({ data }: { data: { label: string } }) {
  return (
    <div className="w-[220px] px-0.5 text-[0.62rem] font-bold tracking-[0.1em] text-arch-muted uppercase">
      {data.label}
    </div>
  );
}
