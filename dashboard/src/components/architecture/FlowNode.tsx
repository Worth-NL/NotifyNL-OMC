import { Handle, Position } from "@xyflow/react";
import { ArchitectureNode, FlowOption } from "@/lib/architecture";
import { NodeCard, NodeState } from "./NodeCard";
import { CheckRow, OmcPanel } from "./OmcPanel";

const HANDLE_CLASS = "!h-2 !w-2 !border-0 !bg-arch-border";

/** Every node exposes all four sides as both a possible target and source — which pair an edge
 * uses is fixed per column in status/flow/page.tsx: left/right along the pipeline, bottom/top
 * for the engine's round trips to the register row below it. */
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
  /** Card width in px; the register row uses narrower cards so it fits under the pipeline. */
  width?: number;
  /** Fixed card height in px, so every card in the register row is the same size. */
  height?: number;
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
    <div className="pointer-events-auto" style={{ width: data.width ?? 220 }}>
      <AllHandles />
      <NodeCard
        node={data.node}
        state={data.state}
        throughput={data.throughput}
        onClick={data.onClick}
        height={data.height}
      />
    </div>
  );
}

export interface OmcNodeData extends Record<string, unknown> {
  flows: FlowOption[];
  selectedKey: string;
  onSelect: (key: string) => void;
  totalProcessed: number;
  onViewDiagram: () => void;
  diagramAvailable: boolean;
  rows: CheckRow[];
  activeChecks: string[];
  throughput: Record<string, number>;
  onCheckClick: (key: string) => void;
}

export function OmcFlowNode({ data }: { data: OmcNodeData }) {
  return (
    <div className="pointer-events-auto">
      <AllHandles />
      <OmcPanel
        flows={data.flows}
        selectedKey={data.selectedKey}
        onSelect={data.onSelect}
        totalProcessed={data.totalProcessed}
        onViewDiagram={data.onViewDiagram}
        diagramAvailable={data.diagramAvailable}
        rows={data.rows}
        activeChecks={data.activeChecks}
        throughput={data.throughput}
        onCheckClick={data.onCheckClick}
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
