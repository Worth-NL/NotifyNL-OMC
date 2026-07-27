"use client";

import { useEffect, useRef, useState } from "react";
import { TRACE_STREAM_URL, TraceEvent } from "@/lib/api";
import { findTracePath, scenarioKeys } from "@/lib/tracePath";
import { CHANNEL_NODES, PATTERN_ENGINE_KEY, REGISTER_NODES } from "@/lib/architecture";

const REGISTER_KEYS = new Set(REGISTER_NODES.map((n) => n.key));
const CHANNEL_KEYS = new Set(CHANNEL_NODES.map((n) => n.key));

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
  traceId: string;
  from: string;
  to: string;
  /** Monotonic counter shared across every trace, exposed so consumers can force a restart
   * even when two different hops happen to share the same (from, to) pair. */
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
// generous — kept well longer than TrafficEdge's 0.7s glide so each hop finishes with a brief
// pause before the next one starts, rather than one glide cutting the previous off.
const HOP_DELAY_MS = 1000;
const MAX_LOG_LINES = 150;
// A channel-send ball waits at notify-email/sms/post for the real "Notify NL" delivery
// confirmation (see BaseScenario.ProcessDataAsync's "pending" status + NotifyCallbackResponder)
// instead of auto-expiring like a normal hop — "Notify NL" itself only checks delivery status
// against AWS roughly every 5 minutes, so a genuine confirmation can take a while. If nothing
// comes back in this long, give up rather than leave that trace's state open forever.
const CONFIRM_TIMEOUT_MS = 15 * 60 * 1000;

// "notify-*" only ever reaches "ok"/"fail" via the async /Confirm callback now (see
// BaseScenario.ProcessDataAsync — the synchronous send only ever emits "pending"), and
// "contactmoment" only exists as that confirmation's follow-up — so both are keyed off real
// wall-clock delivery time (seconds to several minutes), not OMC's own processing time. Kept
// out of "Gem. afhandeltijd" so that figure keeps meaning "how fast OMC's own pipeline ran"
// rather than jumping to minutes for any trace that actually sends a channel message.
function isAsyncConfirmationEvent(event: TraceEvent): boolean {
  return (
    event.stage === "contactmoment" ||
    (CHANNEL_KEYS.has(event.stage) && (event.status === "ok" || event.status === "fail"))
  );
}

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
  /** One entry per trace currently mid-playback — several notifications arriving close
   * together animate independently and simultaneously, each on its own pace. */
  activeHops: TraceHop[];
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
  const [activeHops, setActiveHops] = useState<TraceHop[]>([]);
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

  // Each trace gets its own queue and its own play loop, so several notifications arriving
  // close together each pace out on their own timeline instead of all serializing onto one
  // shared queue (which used to make simultaneous traces play back-to-back instead of at once).
  const hopQueuesByTraceRef = useRef<Map<string, PlannedHop[]>>(new Map());
  const playingTracesRef = useRef<Set<string>>(new Set());
  // The one active hop per trace that's currently live — surfaced as an array for rendering.
  const activeHopsByTraceRef = useRef<Map<string, TraceHop>>(new Map());
  const lastStageByTraceRef = useRef<Map<string, string>>(new Map());
  const logCounterRef = useRef(0);
  const hopSeqRef = useRef(0);

  // Traces currently parked at a channel node awaiting a real delivery confirmation, and the
  // give-up timer for each — see CONFIRM_TIMEOUT_MS above.
  const pendingConfirmTracesRef = useRef<Set<string>>(new Set());
  const confirmTimeoutsRef = useRef<Map<string, ReturnType<typeof setTimeout>>>(new Map());

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

    function publishActiveHops() {
      setActiveHops(Array.from(activeHopsByTraceRef.current.values()));
    }

    function playQueueForTrace(traceId: string) {
      if (playingTracesRef.current.has(traceId)) return;
      playingTracesRef.current.add(traceId);

      const step = () => {
        const queue = hopQueuesByTraceRef.current.get(traceId);
        const hop = queue?.shift();

        if (!hop) {
          playingTracesRef.current.delete(traceId);

          // Parked at a channel node awaiting a real delivery confirmation — leave its ball
          // exactly where it is (no auto-expiry) until the confirmation arrives and resumes
          // this same queue, or CONFIRM_TIMEOUT_MS gives up on it (see giveUpOnConfirmation).
          if (pendingConfirmTracesRef.current.has(traceId)) return;

          // Let the trace's last hop stay visible through its own glide + a brief pause
          // (matching how long a mid-trace hop lingers before the next one takes over), then
          // remove it so a finished trace's ball doesn't sit frozen on the diagram forever.
          setTimeout(() => {
            // A new real event can resume this same trace (playQueueForTrace re-adding it to
            // playingTracesRef) before this fires — if so, its active hop is genuinely live
            // again, so back off instead of deleting it out from under that new animation.
            if (playingTracesRef.current.has(traceId)) return;
            activeHopsByTraceRef.current.delete(traceId);
            publishActiveHops();
          }, HOP_DELAY_MS);
          return;
        }

        // A same-stage status change (e.g. "start" -> "ok", or -> "pending") has nowhere new
        // to animate to — it's a self-hop (from === to), which matches no real edge. If the
        // trace's dot is already resting exactly there from the hop that arrived at this node,
        // leave that entry alone rather than replacing it with this edge-less one: every edge
        // would find zero hops for this trace and the dot would simply vanish until the next
        // real, edge-matching hop comes along (or, while waiting on a delivery confirmation,
        // never — see CONFIRM_TIMEOUT_MS above).
        const existing = activeHopsByTraceRef.current.get(traceId);
        const isRedundantSelfHop = hop.from === hop.to && existing?.to === hop.to;
        if (!isRedundantSelfHop) {
          hopSeqRef.current += 1;
          activeHopsByTraceRef.current.set(traceId, { traceId, from: hop.from, to: hop.to, seq: hopSeqRef.current });
          publishActiveHops();
        }

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

    // No real confirmation showed up within CONFIRM_TIMEOUT_MS of the channel send — rather
    // than leaving the ball sitting there forever (or silently vanishing, which would look like
    // a bug), say so plainly in the log and let the trace's state close out.
    function giveUpOnConfirmation(traceId: string, stage: string) {
      if (!pendingConfirmTracesRef.current.has(traceId)) return; // already resolved for real
      pendingConfirmTracesRef.current.delete(traceId);
      confirmTimeoutsRef.current.delete(traceId);

      const line: TraceLogLine = {
        id: `${traceId}-${logCounterRef.current++}`,
        time: new Date().toLocaleTimeString("nl-NL"),
        traceId,
        stage,
        status: "fail",
        scenario: null,
        detail: "Geen bevestiging ontvangen van Notify NL binnen 15 minuten",
      };
      setLog((prev) => [line, ...prev].slice(0, MAX_LOG_LINES));

      activeHopsByTraceRef.current.delete(traceId);
      publishActiveHops();
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
      if (!isAsyncConfirmationEvent(event)) {
        currentTraceLastElapsedRef.current = event.elapsedMs;
      }

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

      // A channel node's real delivery confirmation, arriving separately (possibly minutes
      // later — see NotifyCallbackResponder) — stop waiting on it and let its queue continue
      // on to "contactmoment" normally. Checked before routing since it's independent of it.
      if (CHANNEL_KEYS.has(event.stage) && (event.status === "ok" || event.status === "fail")) {
        pendingConfirmTracesRef.current.delete(event.traceId);
        const timeout = confirmTimeoutsRef.current.get(event.traceId);
        if (timeout) {
          clearTimeout(timeout);
          confirmTimeoutsRef.current.delete(event.traceId);
        }
      }

      // Handed off to Notify NL — start waiting for the real confirmation instead of letting
      // this hop auto-expire like a normal one (see the "queue empty" branch in playQueueForTrace).
      if (event.status === "pending" && CHANNEL_KEYS.has(event.stage)) {
        pendingConfirmTracesRef.current.add(event.traceId);
        const existing = confirmTimeoutsRef.current.get(event.traceId);
        if (existing) clearTimeout(existing);
        confirmTimeoutsRef.current.set(
          event.traceId,
          setTimeout(() => giveUpOnConfirmation(event.traceId, event.stage), CONFIRM_TIMEOUT_MS),
        );
      }

      const queue = hopQueuesByTraceRef.current.get(event.traceId) ?? [];
      hopQueuesByTraceRef.current.set(event.traceId, queue);

      const lastStage = lastStageByTraceRef.current.get(event.traceId);

      if (!lastStage) {
        // First step of a new trace — nothing to animate a hop from yet, just record it.
        lastStageByTraceRef.current.set(event.traceId, event.stage);
        queue.push({ from: event.stage, to: event.stage, commit: logLine });
        playQueueForTrace(event.traceId);
        return;
      }

      // Restrict pathfinding to the current scenario's own nodes once it's known — otherwise a
      // register with only one edge (a straight round trip to Output Patronen) can BFS its way
      // to the next stage via a topologically shorter route through an unrelated scenario's
      // gate (see tracePath.ts's module comment for the concrete Berichten-schakelaar case).
      const allowedKeys = scenarioKeys(event.scenario);
      const path =
        lastStage === event.stage ? [lastStage, event.stage] : findTracePath(lastStage, event.stage, allowedKeys);
      if (!path || path.length < 2) {
        lastStageByTraceRef.current.set(event.traceId, event.stage);
        queue.push({ from: event.stage, to: event.stage, commit: logLine });
        playQueueForTrace(event.traceId);
        return;
      }

      for (let i = 0; i < path.length - 1; i++) {
        const isLastLeg = i === path.length - 2;
        queue.push({
          from: path[i],
          to: path[i + 1],
          commit: isLastLeg ? logLine : undefined,
        });
      }

      // Registers are never a forward stage of their own — OMC calls out and comes straight
      // back to Output Patronen before doing anything else (see lib/architecture.ts's EDGES
      // comment). Once a register call reaches a terminal status, queue that return leg right
      // away instead of waiting for whatever stage happens to fire next — otherwise two calls
      // to the same register in a row (or just a gap before the next real event) would leave
      // the pip sitting at the register instead of visibly heading back to the hub.
      const isRegisterRoundTrip = event.status !== "start" && REGISTER_KEYS.has(event.stage);
      if (isRegisterRoundTrip) {
        queue.push({ from: event.stage, to: PATTERN_ENGINE_KEY });
        lastStageByTraceRef.current.set(event.traceId, PATTERN_ENGINE_KEY);
      } else {
        lastStageByTraceRef.current.set(event.traceId, event.stage);
      }

      playQueueForTrace(event.traceId);
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
      confirmTimeoutsRef.current.forEach((timeout) => clearTimeout(timeout));
      confirmTimeoutsRef.current.clear();
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
      hopQueuesByTraceRef.current = new Map();
      playingTracesRef.current = new Set();
      activeHopsByTraceRef.current = new Map();
      lastStageByTraceRef.current = new Map();
      pendingConfirmTracesRef.current = new Set();
      confirmTimeoutsRef.current.forEach((timeout) => clearTimeout(timeout));
      confirmTimeoutsRef.current = new Map();
      setActiveHops([]);
      setLog([]);
      setVisitedKeys(new Set());
    };
  }, [tracingActive]);

  return {
    connected,
    activeHops,
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
