"use client";

import { useEffect, useRef, useState } from "react";
import { TRACE_STREAM_URL, TraceEvent } from "@/lib/api";
import { findTracePath } from "@/lib/tracePath";
import { PATTERN_ENGINE_KEY, REGISTER_NODES } from "@/lib/architecture";

const REGISTER_KEYS = new Set(REGISTER_NODES.map((n) => n.key));

export interface TraceLogLine {
  id: string;
  time: string;
  traceId: string;
  stage: string;
  status: TraceEvent["status"];
  scenario: string | null;
  detail: string | null;
}

export interface TraceHop {
  from: string;
  to: string;
  /** Monotonic per-hop counter, exposed so consumers can force a restart even when two
   * different hops happen to share the same (from, to) pair. */
  seq: number;
}

interface PlannedHop {
  from: string;
  to: string;
  /** Attached to a hop's final leg — committed to the log/visited-set on arrival. */
  commit?: TraceLogLine;
}

// Real steps arrive within milliseconds of each other — far too fast for a human to see move.
// This is the artificial per-hop pace for the edge-flow/log replay, independent of how fast
// the real pipeline actually ran (each event still carries its own real elapsedMs). Deliberately
// generous — kept well longer than TrafficEdge's 0.5s flow-dash cycle so each hop gets a couple
// of clearly visible cycles before the next one starts, rather than a blink-and-you-miss-it flash.
const HOP_DELAY_MS = 1000;
const MAX_LOG_LINES = 150;

// How the numeric metrics are derived from real events — no simulation, no database:
const THROUGHPUT_WINDOW_MS = 10_000; // "messages/sec" = events seen in the last 10s / 10
const TICK_MS = 2500; // how often throughput/load/sparkline recompute
const SPARKLINE_LENGTH = 24; // 24 * 2.5s = the last minute
const MAX_DURATION_SAMPLES = 20; // rolling window for the average-handling-time figure
// "Load" has no real backing metric (no CPU/queue-depth signal is exposed) — it's a proxy
// scaled against an assumed ceiling of events/sec, not a literal server load measurement.
const LOAD_CEILING_EVENTS_PER_SEC = 5;

export interface OmcTelemetry {
  connected: boolean;
  activeHop: TraceHop | null;
  visitedKeys: Set<string>;
  log: TraceLogLine[];
  /** Distinct notifications seen since this page was opened. */
  totalProcessed: number;
  /** Per-node-key count of real events touching it, since this page was opened. */
  nodeThroughput: Record<string, number>;
  throughputPerSec: number;
  avgHandlingMs: number;
  load: number;
  sparkline: number[];
}

/**
 * The single real-time data source for the architecture page. Connects to
 * /status/trace/stream once, for as long as this hook stays mounted, and derives every number
 * it returns from genuine events — nothing here is simulated or backed by a database.
 * `tracingActive` only toggles whether the edge-flow/log replay is shown; the underlying
 * connection and counters (throughput, load, totals) keep running regardless, so they reflect
 * activity "since this page was opened" even while the visual trace is paused.
 */
export function useOmcTelemetry(tracingActive: boolean): OmcTelemetry {
  const [connected, setConnected] = useState(false);
  const [activeHop, setActiveHop] = useState<TraceHop | null>(null);
  const [visitedKeys, setVisitedKeys] = useState<Set<string>>(new Set());
  const [log, setLog] = useState<TraceLogLine[]>([]);
  const [totalProcessed, setTotalProcessed] = useState(0);
  const [nodeThroughput, setNodeThroughput] = useState<Record<string, number>>({});
  const [throughputPerSec, setThroughputPerSec] = useState(0);
  const [avgHandlingMs, setAvgHandlingMs] = useState(0);
  const [load, setLoad] = useState(0);
  const [sparkline, setSparkline] = useState<number[]>([]);

  const tracingActiveRef = useRef(tracingActive);
  useEffect(() => {
    tracingActiveRef.current = tracingActive;
  });

  const hopQueueRef = useRef<PlannedHop[]>([]);
  const playingRef = useRef(false);
  const lastStageByTraceRef = useRef<Map<string, string>>(new Map());
  const logCounterRef = useRef(0);
  const hopSeqRef = useRef(0);

  const seenTraceIdsRef = useRef<Set<string>>(new Set());
  const nodeThroughputRef = useRef<Record<string, number>>({});
  const eventTimestampsRef = useRef<number[]>([]);
  const durationSamplesRef = useRef<number[]>([]);
  const currentTraceIdRef = useRef<string | null>(null);
  const currentTraceLastElapsedRef = useRef(0);

  useEffect(() => {
    let cancelled = false;
    const source = new EventSource(TRACE_STREAM_URL);

    source.addEventListener("ready", () => {
      if (!cancelled) setConnected(true);
    });
    source.onerror = () => {
      if (!cancelled) setConnected(false);
    };

    function playQueue() {
      if (playingRef.current) return;
      playingRef.current = true;

      const step = () => {
        const hop = hopQueueRef.current.shift();
        if (!hop) {
          playingRef.current = false;
          return;
        }

        hopSeqRef.current += 1;
        setActiveHop({ from: hop.from, to: hop.to, seq: hopSeqRef.current });
        setVisitedKeys((prev) => {
          const next = new Set(prev);
          next.add(hop.to);
          return next;
        });
        if (hop.commit) {
          const line = hop.commit;
          setLog((prev) => [line, ...prev].slice(0, MAX_LOG_LINES));
        }

        setTimeout(step, HOP_DELAY_MS);
      };

      step();
    }

    source.onmessage = (message) => {
      let event: TraceEvent;
      try {
        event = JSON.parse(message.data);
      } catch {
        return;
      }

      // Real counters — always updated, regardless of whether the visual trace is switched on.
      if (!seenTraceIdsRef.current.has(event.traceId)) {
        seenTraceIdsRef.current.add(event.traceId);
        setTotalProcessed(seenTraceIdsRef.current.size);

        if (currentTraceIdRef.current && currentTraceIdRef.current !== event.traceId) {
          // A different trace just started — the previous one is presumed finished; bank its
          // last-known elapsed time as one real "handling time" sample.
          durationSamplesRef.current = [...durationSamplesRef.current, currentTraceLastElapsedRef.current].slice(
            -MAX_DURATION_SAMPLES,
          );
          const avg =
            durationSamplesRef.current.reduce((sum, ms) => sum + ms, 0) / durationSamplesRef.current.length;
          setAvgHandlingMs(Math.round(avg));
        }
        currentTraceIdRef.current = event.traceId;
      }
      currentTraceLastElapsedRef.current = event.elapsedMs;

      nodeThroughputRef.current = {
        ...nodeThroughputRef.current,
        [event.stage]: (nodeThroughputRef.current[event.stage] ?? 0) + 1,
      };
      setNodeThroughput(nodeThroughputRef.current);

      eventTimestampsRef.current.push(Date.now());

      if (!tracingActiveRef.current) return; // counted above; skip the visual replay work

      const logLine: TraceLogLine = {
        id: `${event.traceId}-${logCounterRef.current++}`,
        time: new Date().toLocaleTimeString("nl-NL"),
        traceId: event.traceId,
        stage: event.stage,
        status: event.status,
        scenario: event.scenario,
        detail: event.detail,
      };

      const lastStage = lastStageByTraceRef.current.get(event.traceId);

      if (!lastStage) {
        // First step of a new trace — nothing to animate a hop from yet, just record it.
        lastStageByTraceRef.current.set(event.traceId, event.stage);
        hopQueueRef.current.push({ from: event.stage, to: event.stage, commit: logLine });
        playQueue();
        return;
      }

      const path = lastStage === event.stage ? [lastStage, event.stage] : findTracePath(lastStage, event.stage);
      if (!path || path.length < 2) {
        lastStageByTraceRef.current.set(event.traceId, event.stage);
        hopQueueRef.current.push({ from: event.stage, to: event.stage, commit: logLine });
        playQueue();
        return;
      }

      for (let i = 0; i < path.length - 1; i++) {
        const isLastLeg = i === path.length - 2;
        hopQueueRef.current.push({
          from: path[i],
          to: path[i + 1],
          commit: isLastLeg ? logLine : undefined,
        });
      }

      // Registers are never a forward stage of their own — OMC calls out and comes straight
      // back to Output Patronen before doing anything else (see lib/architecture.ts's EDGES
      // comment). Once a register call reaches a terminal status, queue that return leg right
      // away instead of waiting for whatever stage happens to fire next: without this, two
      // calls to the same register in a row (or just a gap before the next real event) would
      // leave the pip sitting at the register instead of visibly heading back to the hub.
      const isRegisterRoundTrip = event.status !== "start" && REGISTER_KEYS.has(event.stage);
      if (isRegisterRoundTrip) {
        hopQueueRef.current.push({ from: event.stage, to: PATTERN_ENGINE_KEY });
        lastStageByTraceRef.current.set(event.traceId, PATTERN_ENGINE_KEY);
      } else {
        lastStageByTraceRef.current.set(event.traceId, event.stage);
      }

      playQueue();
    };

    const tick = setInterval(() => {
      const now = Date.now();
      eventTimestampsRef.current = eventTimestampsRef.current.filter((t) => now - t < THROUGHPUT_WINDOW_MS);
      const rate = eventTimestampsRef.current.length / (THROUGHPUT_WINDOW_MS / 1000);
      setThroughputPerSec(Math.round(rate * 10) / 10);

      const loadPct = Math.min(100, Math.round((rate / LOAD_CEILING_EVENTS_PER_SEC) * 100));
      setLoad(loadPct);
      setSparkline((prev) => [...prev, loadPct].slice(-SPARKLINE_LENGTH));
    }, TICK_MS);

    return () => {
      cancelled = true;
      clearInterval(tick);
      source.close();
    };
    // Connects once for the page's lifetime — intentionally not re-run when `tracingActive`
    // changes; that flag is only read (via the ref above) inside the already-open connection.
  }, []);

  // Clears only the *visual* replay state when tracing is switched off — the real counters
  // above (totalProcessed, nodeThroughput, throughputPerSec, load, sparkline) are untouched,
  // since they represent activity since the page loaded, not since tracing was turned on.
  useEffect(() => {
    if (!tracingActive) return;
    return () => {
      hopQueueRef.current = [];
      playingRef.current = false;
      lastStageByTraceRef.current = new Map();
      setActiveHop(null);
      setLog([]);
      setVisitedKeys(new Set());
    };
  }, [tracingActive]);

  return {
    connected,
    activeHop,
    visitedKeys,
    log,
    totalProcessed,
    nodeThroughput,
    throughputPerSec,
    avgHandlingMs,
    load,
    sparkline,
  };
}
