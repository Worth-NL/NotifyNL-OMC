"use client";

import { useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { Background, BackgroundVariant, ReactFlow, ReactFlowProvider, Edge, Node, useReactFlow } from "@xyflow/react";
import "@xyflow/react/dist/style.css";
import {
  CHANNEL_NODES,
  CONFIRMATION_NODES,
  EDGES,
  EDGE_CATEGORY_COLOR,
  FILTER_NODES,
  FLOW_OPTIONS,
  INPUT_NODES,
  PATTERN_ENGINE_KEY,
  REGISTER_NODES,
} from "@/lib/architecture";
import { computeEdgeHandles, layoutGraph, LayoutResult } from "@/lib/layout";
import { scenarioKeys } from "@/lib/tracePath";
import { useOmcTelemetry } from "@/hooks/useOmcTelemetry";
import { fetchScenarios, ScenarioFlow } from "@/lib/api";
import { ArchitectureHeader } from "@/components/architecture/ArchitectureHeader";
import { MetricsBar } from "@/components/architecture/MetricsBar";
import { ConnectionLegend } from "@/components/architecture/ConnectionLegend";
import { NodeState } from "@/components/architecture/NodeCard";
import { ColumnLabelNode, FlowNode, PatternEngineFlowNode } from "@/components/architecture/FlowNode";
import { TrafficEdge, TrafficEdgeData } from "@/components/architecture/TrafficEdge";
import { DiagramModal } from "@/components/architecture/DiagramModal";
import { LiveLogPanel } from "@/components/architecture/LiveLogPanel";
import { NodeLogModal } from "@/components/architecture/NodeLogModal";

// The "all" flow option has no single scenario of its own — it maps to the backend's
// top-level routing-overview diagram instead.
const OVERVIEW_DIAGRAM_KEY = "routing";

const ALL_ARCH_NODES = [...INPUT_NODES, ...REGISTER_NODES, ...FILTER_NODES, ...CHANNEL_NODES, ...CONFIRMATION_NODES];

const NODE_TYPES = { flowNode: FlowNode, patternEngine: PatternEngineFlowNode, columnLabel: ColumnLabelNode };
const EDGE_TYPES = { traffic: TrafficEdge };

const CARD_WIDTH = 220;
const CARD_HEIGHT = 108;
const PATTERN_ENGINE_WIDTH = 260;
const PATTERN_ENGINE_HEIGHT = 340;

// The main chain (Invoer -> Output Patronen -> Kanaal keuze -> filters -> Uitvoer ->
// Afleverbevestiging) is a real left-to-right pipeline, so Dagre lays it out. Registers are
// excluded from that pass entirely — OMC calls each one and gets a response back, they're
// never a forward hop — and are placed by hand in a row directly below Output Patronen, with
// edges dropping straight from its bottom edge (see EDGES comment in lib/architecture.ts).
const MAIN_CHAIN_NODES = [...INPUT_NODES, ...FILTER_NODES, ...CHANNEL_NODES, ...CONFIRMATION_NODES];
const MAIN_CHAIN_KEYS = new Set<string>([...MAIN_CHAIN_NODES.map((n) => n.key), PATTERN_ENGINE_KEY]);
const MAIN_CHAIN_EDGES = EDGES.filter((e) => MAIN_CHAIN_KEYS.has(e.source) && MAIN_CHAIN_KEYS.has(e.target));

const MAIN_CHAIN_LAYOUT_NODES = [
  ...MAIN_CHAIN_NODES.map((n) => ({ id: n.key, width: CARD_WIDTH, height: CARD_HEIGHT })),
  { id: PATTERN_ENGINE_KEY, width: PATTERN_ENGINE_WIDTH, height: PATTERN_ENGINE_HEIGHT },
];

const mainLayoutAuto = layoutGraph(MAIN_CHAIN_LAYOUT_NODES, MAIN_CHAIN_EDGES);

// Dagre's own crossing-minimized ranks are a fine starting point, but its within-rank ordering
// doesn't line up Zaaktype whitelist / Informeren-check / Kanaalresolutie into the single
// straight chain they conceptually are. Patching just these positions after Dagre runs — same
// technique as the register row below — keeps everything else on its normal auto-computed
// layout.
const FILTER_ROW_Y = mainLayoutAuto.positions.documentcheck.y;

// Berichten-schakelaar (Message Received's own gate, unrelated to the whitelist/informeren
// chain) sits above Documentstatus, well above Output Patronen's own top edge — genuine empty
// space its long edge to Kanaalresolutie can clear through.
const filterPositionOverrides: LayoutResult["positions"] = {
  berichtenschakelaar: { x: mainLayoutAuto.positions.documentcheck.x, y: mainLayoutAuto.positions[PATTERN_ENGINE_KEY].y - (CARD_HEIGHT + 40) },
  zaaktypewhitelist: { x: mainLayoutAuto.positions.zaaktypewhitelist.x, y: FILTER_ROW_Y },
  informerencheck: { x: mainLayoutAuto.positions.informerencheck.x, y: FILTER_ROW_Y },
  kanaalresolutie: { x: mainLayoutAuto.positions.kanaalresolutie.x, y: FILTER_ROW_Y },
};

const mainPositions = { ...mainLayoutAuto.positions, ...filterPositionOverrides };
const mainEdgeHandles = computeEdgeHandles(mainPositions, MAIN_CHAIN_LAYOUT_NODES, MAIN_CHAIN_EDGES);
// Entering Kanaalresolutie from directly above — rather than the auto-computed left/right,
// which routes this long cross-diagram edge straight through Zaaktype whitelist and
// Informeren-check's row — keeps the curve elevated above the whole filter row for nearly its
// entire span, only dropping in the short final stretch directly over Kanaalresolutie itself.
mainEdgeHandles["berichtenschakelaar->kanaalresolutie"] = { sourceHandle: "right", targetHandle: "top" };

const mainLayout: LayoutResult = {
  positions: mainPositions,
  edgeHandles: mainEdgeHandles,
};

const REGISTER_GAP_X = 20;
const REGISTER_ROW_GAP_Y = 100;
const REGISTER_ROW_SPACING_Y = 24;
const REGISTER_MAX_PER_ROW = 5;
const patternPos = mainLayout.positions[PATTERN_ENGINE_KEY];
const registerFirstRowY = patternPos.y + PATTERN_ENGINE_HEIGHT + REGISTER_ROW_GAP_Y;

const registerRows: (typeof REGISTER_NODES)[number][][] = [];
for (let i = 0; i < REGISTER_NODES.length; i += REGISTER_MAX_PER_ROW) {
  registerRows.push(REGISTER_NODES.slice(i, i + REGISTER_MAX_PER_ROW));
}

const registerPositions: LayoutResult["positions"] = {};
const registerEdgeHandles: LayoutResult["edgeHandles"] = {};
let registerLabelX = Infinity;
registerRows.forEach((row, rowIndex) => {
  const rowWidth = row.length * CARD_WIDTH + (row.length - 1) * REGISTER_GAP_X;
  const rowStartX = patternPos.x + PATTERN_ENGINE_WIDTH / 2 - rowWidth / 2;
  const rowY = registerFirstRowY + rowIndex * (CARD_HEIGHT + REGISTER_ROW_SPACING_Y);
  registerLabelX = Math.min(registerLabelX, rowStartX);
  row.forEach((n, i) => {
    registerPositions[n.key] = { x: rowStartX + i * (CARD_WIDTH + REGISTER_GAP_X), y: rowY };
    registerEdgeHandles[`${PATTERN_ENGINE_KEY}->${n.key}`] = { sourceHandle: "bottom", targetHandle: "top" };
  });
});

const LAYOUT: LayoutResult = {
  positions: { ...mainLayout.positions, ...registerPositions },
  edgeHandles: { ...mainLayout.edgeHandles, ...registerEdgeHandles },
};

const COLUMN_GROUPS: { label: string; keys: string[] }[] = [
  { label: "Invoer", keys: INPUT_NODES.map((n) => n.key) },
  { label: "Verwerking", keys: [PATTERN_ENGINE_KEY] },
  { label: "Kanaal & filters", keys: FILTER_NODES.map((n) => n.key) },
  { label: "Uitvoer", keys: CHANNEL_NODES.map((n) => n.key) },
  { label: "Afleverbevestiging", keys: CONFIRMATION_NODES.map((n) => n.key) },
];

const COLUMN_LABELS = [
  ...COLUMN_GROUPS.map((g) => ({
    label: g.label,
    x: Math.min(...g.keys.map((k) => LAYOUT.positions[k].x)),
    y: Math.min(...g.keys.map((k) => LAYOUT.positions[k].y)) - 40,
  })),
  { label: "Registers", x: registerLabelX, y: registerFirstRowY - 40 },
];

// `fitView` (the boolean prop) only ever runs once, synchronously at mount — if the
// container hasn't settled into its final size yet (e.g. still animating in, or a class
// hasn't been applied by the time React Flow first measures), the fit is wrong and never
// recalculated. Re-running it imperatively next frame is the standard fix.
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
  const [environment, setEnvironment] = useState<"productie" | "test">("productie");
  const [isTracing, setIsTracing] = useState(false);
  const [scenarios, setScenarios] = useState<ScenarioFlow[]>([]);
  const [diagramOpen, setDiagramOpen] = useState(false);
  const [logModalNodeKey, setLogModalNodeKey] = useState<string | null>(null);

  const telemetry = useOmcTelemetry(isTracing);

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
    () => ALL_ARCH_NODES.find((n) => n.key === logModalNodeKey) ?? null,
    [logModalNodeKey],
  );

  const usedKeys = useMemo(() => scenarioKeys(selectedFlowKey) ?? new Set<string>(), [selectedFlowKey]);

  function nodeState(key: string, active: boolean): NodeState {
    if (!active) return "inactive";
    return usedKeys.has(key) ? "highlighted" : "dimmed";
  }

  function reset() {
    setSelectedFlowKey("all");
    setIsTracing(false);
  }

  const nodes: Node[] = useMemo(() => {
    const cardNodes: Node[] = ALL_ARCH_NODES.map((n) => ({
      id: n.key,
      type: "flowNode",
      position: LAYOUT.positions[n.key],
      data: {
        node: n,
        state: nodeState(n.key, n.active),
        throughput: telemetry.nodeThroughput[n.key],
        onClick: () => setLogModalNodeKey(n.key),
      },
      draggable: false,
      selectable: false,
    }));

    const patternEngineNode: Node = {
      id: PATTERN_ENGINE_KEY,
      type: "patternEngine",
      position: LAYOUT.positions[PATTERN_ENGINE_KEY],
      data: {
        flows: FLOW_OPTIONS,
        selectedKey: selectedFlowKey,
        onSelect: setSelectedFlowKey,
        totalProcessed: telemetry.totalProcessed,
        onViewDiagram: () => setDiagramOpen(true),
        diagramAvailable: activeDiagram !== null,
      },
      draggable: false,
      selectable: false,
    };

    const labelNodes: Node[] = COLUMN_LABELS.map((c) => ({
      id: `label-${c.label}`,
      type: "columnLabel",
      position: { x: c.x, y: c.y },
      data: { label: c.label },
      draggable: false,
      selectable: false,
    }));

    return [...labelNodes, ...cardNodes, patternEngineNode];
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedFlowKey, telemetry.nodeThroughput, telemetry.totalProcessed, usedKeys, activeDiagram]);

  const activeHops = telemetry.activeHops;

  // Static per-edge definitions — deliberately independent of `activeHops`, so a hop change
  // never recreates the other ~32 edges' objects. React Flow re-renders a TrafficEdge whenever
  // its object reference changes; recomputing the whole array every ~second (once per hop) was
  // making 32 unrelated edges re-render alongside the one that actually mattered, which was
  // enough main-thread churn to visibly disrupt the traveling dot's SMIL timing.
  const baseEdges: Edge[] = useMemo(
    () =>
      EDGES.map((e, i) => {
        const sourceActive = ALL_ARCH_NODES.find((n) => n.key === e.source)?.active ?? true;
        const targetActive = ALL_ARCH_NODES.find((n) => n.key === e.target)?.active ?? true;
        const live = sourceActive && targetActive && usedKeys.has(e.source) && usedKeys.has(e.target);
        const color = EDGE_CATEGORY_COLOR[e.category];
        const handles = LAYOUT.edgeHandles[`${e.source}->${e.target}`];

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
    [usedKeys],
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
    <div className="min-h-screen bg-arch-bg">
      <nav className="flex h-12 items-center justify-between border-b border-arch-border bg-arch-surface px-6">
        <Link href="/status" className="text-[0.72rem] font-semibold text-arch-muted hover:text-arch-ink">
          ← Configuratiestatus
        </Link>
        <span className="text-[0.68rem] font-bold tracking-[0.1em] text-arch-teal uppercase">
          Notificatie.nl — OMC
        </span>
      </nav>

      <div className="flex flex-col gap-4 px-6 py-6">
        <ArchitectureHeader
          isTracing={isTracing}
          onToggleTrace={() => setIsTracing((t) => !t)}
          onReset={reset}
        />

        <MetricsBar
          environment={environment}
          onEnvironmentChange={setEnvironment}
          load={telemetry.load}
          avgHandlingMs={telemetry.avgHandlingMs}
          throughputPerSec={telemetry.throughputPerSec}
          sparkline={telemetry.sparkline}
        />

        <ConnectionLegend />

        <div
          className="w-full overflow-hidden rounded-lg border border-arch-border bg-arch-surface"
          style={{ height: 900 }}
        >
          <ReactFlowProvider>
            <ReactFlow
              nodes={nodes}
              edges={edges}
              nodeTypes={NODE_TYPES}
              edgeTypes={EDGE_TYPES}
              nodesDraggable={false}
              nodesConnectable={false}
              elementsSelectable={false}
              panOnDrag={false}
              panOnScroll={false}
              zoomOnScroll={false}
              zoomOnPinch={false}
              zoomOnDoubleClick={false}
              minZoom={0.15}
              maxZoom={1.1}
              proOptions={{ hideAttribution: true }}
            >
              <Background variant={BackgroundVariant.Dots} gap={16} size={1} color="var(--color-arch-border)" />
              <FitViewOnReady />
            </ReactFlow>
          </ReactFlowProvider>
        </div>

        <LiveLogPanel log={telemetry.log} connected={telemetry.connected} />

        <p className="text-[0.68rem] text-arch-faint">
          Selecteer een flow in het paneel &ldquo;Output Patronen&rdquo; om te zien welke
          registers, kanalen en bevestigingen die flow daadwerkelijk gebruikt — niet-gebruikte
          onderdelen dimmen. Grijze, gestippelde kaarten zijn nog niet aangesloten op een echte
          OMC-integratie. Klik het diagram-icoon om de beslisboom van de geselecteerde flow te
          bekijken. Klik &ldquo;Trace starten&rdquo; om een echte binnenkomende notificatie live
          door het systeem te volgen.
        </p>
      </div>

      <DiagramModal scenario={diagramOpen ? activeDiagram : null} onClose={() => setDiagramOpen(false)} />
      <NodeLogModal node={logModalNode} log={telemetry.log} onClose={() => setLogModalNodeKey(null)} />
    </div>
  );
}
