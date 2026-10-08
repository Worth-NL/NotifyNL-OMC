"use client";

import { useEffect, useMemo, useState } from "react";
import Link from "next/link";
import {
  Background,
  BackgroundVariant,
  Controls,
  PanOnScrollMode,
  ReactFlow,
  ReactFlowProvider,
  Edge,
  Node,
  useNodesState,
  useReactFlow,
} from "@xyflow/react";
import "@xyflow/react/dist/style.css";
import {
  ArchitectureNode,
  CHANNEL_NODES,
  CHECK_NODES,
  CONFIRMATION_NODES,
  EDGES,
  EDGE_CATEGORY_COLOR,
  FLOW_OPTIONS,
  INPUT_NODES,
  PATTERN_ENGINE_KEY,
  REGISTER_NODES,
} from "@/lib/architecture";
import { scenarioEdgeKeys, scenarioKeys } from "@/lib/tracePath";
import { useOmcTelemetry } from "@/hooks/useOmcTelemetry";
import { fetchScenarios, ScenarioFlow } from "@/lib/api";
import { ConnectionLegend } from "@/components/architecture/ConnectionLegend";
import { NodeState } from "@/components/architecture/NodeCard";
import { ColumnLabelNode, FlowNode, OmcFlowNode } from "@/components/architecture/FlowNode";
import { CheckRow, OMC_PANEL_WIDTH, omcPanelHeight } from "@/components/architecture/OmcPanel";
import { TrafficEdge, TrafficEdgeData } from "@/components/architecture/TrafficEdge";
import { DiagramModal } from "@/components/architecture/DiagramModal";
import { LiveLogPanel } from "@/components/architecture/LiveLogPanel";
import { NodeLogModal } from "@/components/architecture/NodeLogModal";

// The "all" flow option has no single scenario of its own — it maps to the backend's
// top-level routing-overview diagram instead.
const OVERVIEW_DIAGRAM_KEY = "routing";

// Nodes drawn as cards. Individual checks are not among them: they are blocks inside the OMC
// block (see CHECK_NODES), but they keep their explanation and log, so the modal opens either.
const GRAPH_NODES = [...INPUT_NODES, ...REGISTER_NODES, ...CHANNEL_NODES, ...CONFIRMATION_NODES];
const LOGGABLE_NODES = [...GRAPH_NODES, ...CHECK_NODES];

const NODE_TYPES = { flowNode: FlowNode, omc: OmcFlowNode, columnLabel: ColumnLabelNode };
const EDGE_TYPES = { traffic: TrafficEdge };

// One row per distinct check chain, in FLOW_OPTIONS order. Flows that run exactly the same
// checks (Zaak aangemaakt / bijgewerkt / afgesloten) share a row rather than repeating it.
const CHECK_ROWS: CheckRow[] = (() => {
  const rows = new Map<string, CheckRow>();
  for (const flow of FLOW_OPTIONS) {
    if (flow.key === "all") continue;
    const signature = flow.filters.join(">");
    const existing = rows.get(signature);
    if (existing) {
      existing.flowKeys.push(flow.key);
      existing.label = `${existing.label} · ${flow.nl}`;
      continue;
    }
    rows.set(signature, {
      label: flow.nl,
      flowKeys: [flow.key],
      checks: flow.filters
        .map((key) => CHECK_NODES.find((c) => c.key === key))
        .filter((c): c is ArchitectureNode => c !== undefined),
    });
  }
  return [...rows.values()];
})();

// ── Layout ───────────────────────────────────────────────────────────────────────────────
// A strict grid, like the OMC3 "Stroom" view: fixed columns left to right (Invoer → OMC →
// Uitvoer → Afleverbevestiging), every column centred on the same horizontal line, and the
// registers in a row underneath, each a round trip from OMC's bottom edge. Nothing is
// auto-placed, so adding a scenario only adds a row of checks to the OMC block.

const CARD_WIDTH = 220;
const CARD_HEIGHT = 108;
const ROW_GAP = 16;
const COLUMN_GAP = 110;
const REGISTER_GAP_X = 20;
const REGISTER_TOP_GAP = 110;
const REGISTER_MAX_PER_ROW = 10;
// Every register card is this tall, whatever its text, so the row lines up (see NodeCard).
const REGISTER_CARD_HEIGHT = 124;
const LABEL_OFFSET_Y = 34;

const OMC_PANEL_HEIGHT = omcPanelHeight(CHECK_ROWS);

const X_INPUT = 0;
const X_OMC = X_INPUT + CARD_WIDTH + COLUMN_GAP;
const X_OUTPUT = X_OMC + OMC_PANEL_WIDTH + COLUMN_GAP;
const X_CONFIRMATION = X_OUTPUT + CARD_WIDTH + COLUMN_GAP;
// The register row spans the whole pipeline, from the Invoer column to Afleverbevestiging.
const GRID_WIDTH = X_CONFIRMATION + CARD_WIDTH - X_INPUT;

type Position = { x: number; y: number };

/** Top of each card in a column of `count`, centred on the line every column shares. */
function stackedYs(count: number, centerY: number): number[] {
  const height = count * CARD_HEIGHT + (count - 1) * ROW_GAP;
  const top = centerY - height / 2;
  return Array.from({ length: count }, (_, i) => top + i * (CARD_HEIGHT + ROW_GAP));
}

interface Layout {
  positions: Record<string, Position>;
  /** Card widths that differ from CARD_WIDTH (the register row). */
  widths: Record<string, number>;
  /** Fixed card heights (the register row). */
  heights: Record<string, number>;
  labels: { label: string; x: number; y: number }[];
}

function computeLayout(visible: (n: ArchitectureNode) => boolean): Layout {
  const centerY = OMC_PANEL_HEIGHT / 2;
  const positions: Record<string, Position> = { [PATTERN_ENGINE_KEY]: { x: X_OMC, y: 0 } };
  const widths: Record<string, number> = {};
  const heights: Record<string, number> = {};

  const inputs = INPUT_NODES.filter(visible);
  stackedYs(inputs.length, centerY).forEach((y, i) => (positions[inputs[i].key] = { x: X_INPUT, y }));

  const outputs = CHANNEL_NODES.filter(visible);
  stackedYs(outputs.length, centerY).forEach((y, i) => (positions[outputs[i].key] = { x: X_OUTPUT, y }));

  // Each confirmation sits level with the middle of what feeds it, so its edges fan in evenly.
  const feeders: Record<string, string[]> = {
    contactmoment: ["notify-email", "notify-sms", "notify-post", "printstraat", "berichtenbox"],
    contactherstel: ["lokale-berichtenbox"],
  };
  for (const confirmation of CONFIRMATION_NODES.filter(visible)) {
    const ys = (feeders[confirmation.key] ?? []).map((k) => positions[k]?.y).filter((y) => y !== undefined);
    const y = ys.length > 0 ? ys[Math.floor(ys.length / 2)] : centerY - CARD_HEIGHT / 2;
    positions[confirmation.key] = { x: X_CONFIRMATION, y };
  }

  const columnBottom = Math.max(
    OMC_PANEL_HEIGHT,
    ...[...inputs, ...outputs].map((n) => positions[n.key].y + CARD_HEIGHT),
  );
  const registerTop = columnBottom + REGISTER_TOP_GAP;

  // One row across the full width of the pipeline (like OMC3's invoerclients row), so every
  // round trip drops straight down from the engine without weaving between cards. Only with
  // the planned integrations shown does it need a second, equally wide row.
  const registers = REGISTER_NODES.filter(visible);
  const rowCount = Math.max(1, Math.ceil(registers.length / REGISTER_MAX_PER_ROW));
  const perRow = Math.ceil(registers.length / rowCount);
  const registerWidth = (GRID_WIDTH - (perRow - 1) * REGISTER_GAP_X) / perRow;
  for (let row = 0; row < rowCount; row++) {
    registers.slice(row * perRow, (row + 1) * perRow).forEach((n, i) => {
      positions[n.key] = {
        x: X_INPUT + i * (registerWidth + REGISTER_GAP_X),
        y: registerTop + row * (REGISTER_CARD_HEIGHT + 2 * ROW_GAP),
      };
      widths[n.key] = registerWidth;
      heights[n.key] = REGISTER_CARD_HEIGHT;
    });
  }

  const columnTop = (nodes: ArchitectureNode[]) => Math.min(...nodes.map((n) => positions[n.key].y));
  const confirmations = CONFIRMATION_NODES.filter(visible);

  const labels = [
    { label: "Invoer", x: X_INPUT, y: columnTop(inputs) - LABEL_OFFSET_Y },
    { label: "Verwerking", x: X_OMC, y: -LABEL_OFFSET_Y },
    { label: "Uitvoer", x: X_OUTPUT, y: columnTop(outputs) - LABEL_OFFSET_Y },
    { label: "Afleverbevestiging", x: X_CONFIRMATION, y: columnTop(confirmations) - LABEL_OFFSET_Y },
    { label: "Registers — heen en terug", x: X_INPUT, y: registerTop - LABEL_OFFSET_Y },
  ];

  return { positions, widths, heights, labels };
}

/** Pipeline edges run left to right; OMC's register calls drop out of its bottom edge. */
function edgeHandles(source: string, target: string) {
  return source === PATTERN_ENGINE_KEY && REGISTER_NODES.some((n) => n.key === target)
    ? { sourceHandle: "bottom", targetHandle: "top" }
    : { sourceHandle: "right", targetHandle: "left" };
}

// `fitView` (the boolean prop) only ever runs once, synchronously at mount — if the
// container hasn't settled into its final size yet (e.g. still animating in, or a class
// hasn't been applied by the time React Flow first measures), the fit is wrong and never
// recalculated. Re-running it imperatively next frame is the standard fix. Remounted (via its
// `key`) whenever the visible set changes, so the diagram re-fits after the toggle.
function FitViewOnReady() {
  const { fitView } = useReactFlow();
  useEffect(() => {
    const id = requestAnimationFrame(() => fitView({ padding: 0.06 }));
    return () => cancelAnimationFrame(id);
  }, [fitView]);
  return null;
}

export default function FlowPage() {
  const [selectedFlowKey, setSelectedFlowKey] = useState("all");
  const [scenarios, setScenarios] = useState<ScenarioFlow[]>([]);
  const [diagramOpen, setDiagramOpen] = useState(false);
  const [logModalNodeKey, setLogModalNodeKey] = useState<string | null>(null);
  // Integrations that don't exist yet ("nog geen client") are hidden by default — they are
  // design placeholders, and showing them doubled the diagram without saying anything real.
  const [showPlanned, setShowPlanned] = useState(false);

  const isVisible = useMemo(
    () => (n: ArchitectureNode) => n.active || showPlanned,
    [showPlanned],
  );
  const layout = useMemo(() => computeLayout(isVisible), [isVisible]);

  const telemetry = useOmcTelemetry();

  useEffect(() => {
    let cancelled = false;
    fetchScenarios()
      .then((data) => {
        if (!cancelled) setScenarios(data);
      })
      .catch((err) => console.error("Failed to load scenario diagrams", err));
    return () => {
      cancelled = true;
    };
  }, []);

  const activeDiagram = useMemo(() => {
    const diagramKey = selectedFlowKey === "all" ? OVERVIEW_DIAGRAM_KEY : selectedFlowKey;
    return scenarios.find((s) => s.key === diagramKey) ?? null;
  }, [scenarios, selectedFlowKey]);

  const logModalNode = useMemo(
    () => LOGGABLE_NODES.find((n) => n.key === logModalNodeKey) ?? null,
    [logModalNodeKey],
  );

  const usedKeys = useMemo(() => scenarioKeys(selectedFlowKey) ?? new Set<string>(), [selectedFlowKey]);
  const usedEdgeKeys = useMemo(
    () => scenarioEdgeKeys(selectedFlowKey) ?? new Set<string>(),
    [selectedFlowKey],
  );

  function nodeState(key: string, active: boolean): NodeState {
    if (!active) return "inactive";
    return usedKeys.has(key) ? "highlighted" : "dimmed";
  }

  const nodes: Node[] = useMemo(() => {
    const cardNodes: Node[] = GRAPH_NODES.filter(isVisible).map((n) => ({
      id: n.key,
      type: "flowNode",
      position: layout.positions[n.key],
      data: {
        node: n,
        width: layout.widths[n.key],
        height: layout.heights[n.key],
        state: nodeState(n.key, n.active),
        throughput: telemetry.nodeThroughput[n.key],
        onClick: () => setLogModalNodeKey(n.key),
      },
      draggable: false,
      selectable: false,
    }));

    const omcNode: Node = {
      id: PATTERN_ENGINE_KEY,
      type: "omc",
      position: layout.positions[PATTERN_ENGINE_KEY],
      data: {
        flows: FLOW_OPTIONS,
        selectedKey: selectedFlowKey,
        onSelect: setSelectedFlowKey,
        totalProcessed: telemetry.totalProcessed,
        onViewDiagram: () => setDiagramOpen(true),
        diagramAvailable: activeDiagram !== null,
        rows: CHECK_ROWS,
        activeChecks: telemetry.activeSteps,
        throughput: telemetry.nodeThroughput,
        onCheckClick: (key: string) => setLogModalNodeKey(key),
      },
      draggable: false,
      selectable: false,
    };

    const labelNodes: Node[] = layout.labels.map((c) => ({
      id: `label-${c.label}`,
      type: "columnLabel",
      position: { x: c.x, y: c.y },
      data: { label: c.label },
      draggable: false,
      selectable: false,
    }));

    return [...labelNodes, ...cardNodes, omcNode];
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedFlowKey, telemetry.nodeThroughput, telemetry.totalProcessed, telemetry.activeSteps, usedKeys, activeDiagram, layout, isVisible]);

  // React Flow measures each node once and keeps that on its own copy. Handing it brand-new node
  // objects (which `nodes` above does on every trace step, as counters and lit checks change)
  // without those measurements makes it measure again, and edges aren't drawn until it has — so
  // every edge, and the dot travelling on one, blinked out and restarted from the beginning of its
  // line on each step. That read as several dots per message. Merging updates into the nodes it
  // already measured (via onNodesChange) keeps the measurements, and the dot runs once.
  const [flowNodes, setFlowNodes, onNodesChange] = useNodesState<Node>([]);
  useEffect(() => {
    setFlowNodes((previous) =>
      nodes.map((node) => {
        const measured = previous.find((p) => p.id === node.id);
        return measured ? { ...measured, ...node, measured: measured.measured } : node;
      }),
    );
  }, [nodes, setFlowNodes]);

  const activeHops = telemetry.activeHops;

  // Static per-edge definitions — deliberately independent of `activeHops`, so a hop change
  // never recreates the other ~32 edges' objects. React Flow re-renders a TrafficEdge whenever
  // its object reference changes; recomputing the whole array every ~second (once per hop) was
  // making 32 unrelated edges re-render alongside the one that actually mattered, which was
  // enough main-thread churn to visibly disrupt the traveling dot's SMIL timing.
  const baseEdges: Edge[] = useMemo(
    () =>
      EDGES.filter((e) => {
        const source = GRAPH_NODES.find((n) => n.key === e.source);
        const target = GRAPH_NODES.find((n) => n.key === e.target);
        return (!source || isVisible(source)) && (!target || isVisible(target));
      }).map((e, i) => {
        const sourceActive = GRAPH_NODES.find((n) => n.key === e.source)?.active ?? true;
        const targetActive = GRAPH_NODES.find((n) => n.key === e.target)?.active ?? true;
        const live = sourceActive && targetActive && usedEdgeKeys.has(`${e.source}->${e.target}`);
        const color = EDGE_CATEGORY_COLOR[e.category];
        const handles = edgeHandles(e.source, e.target);

        const data: TrafficEdgeData = { liveColor: color };

        return {
          id: `e-${i}-${e.source}-${e.target}`,
          source: e.source,
          target: e.target,
          sourceHandle: handles.sourceHandle,
          targetHandle: handles.targetHandle,
          type: "traffic",
          data,
          style: {
            stroke: color,
            strokeWidth: live ? 1.75 : 1,
            opacity: sourceActive && targetActive ? (live ? 0.9 : 0.35) : 0.15,
            strokeDasharray: sourceActive && targetActive ? undefined : "4 3",
          },
        };
      }),
    [usedEdgeKeys, isVisible],
  );

  // Patches in the list of hops (if any — several simultaneous traces can each be traversing
  // this same edge at once) for only the edges actually involved — every other edge keeps the
  // exact same object reference from `baseEdges`.
  const edges: Edge[] = useMemo(() => {
    if (activeHops.length === 0) return baseEdges;
    return baseEdges.map((edge) => {
      const hops = activeHops
        .map((hop) => {
          const traceForward = hop.from === edge.source && hop.to === edge.target;
          const traceReverse = hop.from === edge.target && hop.to === edge.source;
          if (!traceForward && !traceReverse) return null;
          return { seq: hop.seq, reverse: traceReverse };
        })
        .filter((hop): hop is { seq: number; reverse: boolean } => hop !== null);

      if (hops.length === 0) return edge;

      return {
        ...edge,
        data: { ...edge.data, hops } satisfies TrafficEdgeData,
      };
    });
  }, [baseEdges, activeHops]);

  return (
    <div className="flex min-h-screen flex-col bg-arch-bg lg:h-screen">
      <nav className="flex h-12 items-center justify-between border-b border-arch-border bg-arch-surface px-6">
        <Link href="/status" className="text-[0.72rem] font-semibold text-arch-muted hover:text-arch-ink">
          ← Configuratiestatus
        </Link>
        <span className="text-[0.68rem] font-bold tracking-[0.1em] text-arch-teal uppercase">
          Notificatie.nl — OMC
        </span>
      </nav>

      <div className="flex flex-1 flex-col gap-4 p-4 lg:min-h-0 lg:flex-row">
        <div className="flex h-[75vh] flex-col overflow-hidden rounded-lg border border-arch-border bg-arch-surface lg:h-auto lg:min-w-0 lg:flex-1">
          <div className="flex flex-wrap items-center justify-between gap-3 border-b border-arch-border px-4 py-2">
            <ConnectionLegend />
            <label className="flex cursor-pointer items-center gap-2 text-[0.68rem] text-arch-muted">
              <input
                type="checkbox"
                checked={showPlanned}
                onChange={(e) => setShowPlanned(e.target.checked)}
                className="accent-arch-teal"
              />
              Toon geplande koppelingen
            </label>
          </div>
          <div className="min-h-0 flex-1">
            <ReactFlowProvider>
              <ReactFlow
                nodes={flowNodes}
                onNodesChange={onNodesChange}
                edges={edges}
                nodeTypes={NODE_TYPES}
                edgeTypes={EDGE_TYPES}
                nodesDraggable={false}
                nodesConnectable={false}
                elementsSelectable={false}
                panOnDrag
                panOnScroll
                panOnScrollMode={PanOnScrollMode.Free}
                zoomOnScroll={false}
                zoomOnPinch
                zoomOnDoubleClick
                minZoom={0.15}
                maxZoom={1.5}
                proOptions={{ hideAttribution: true }}
              >
                <Background variant={BackgroundVariant.Dots} gap={16} size={1} color="var(--color-arch-border)" />
                <Controls showInteractive={false} position="top-right" />
                <FitViewOnReady key={showPlanned ? "planned" : "live"} />
              </ReactFlow>
            </ReactFlowProvider>
          </div>
        </div>

        <LiveLogPanel log={telemetry.log} connected={telemetry.connected} />
      </div>

      <DiagramModal scenario={diagramOpen ? activeDiagram : null} onClose={() => setDiagramOpen(false)} />
      <NodeLogModal node={logModalNode} log={telemetry.log} onClose={() => setLogModalNodeKey(null)} />
    </div>
  );
}
