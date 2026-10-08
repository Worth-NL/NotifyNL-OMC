"use client";

import { useEffect, useRef, useState } from "react";
import { TRACE_STREAM_URL, TraceEvent } from "@/lib/api";
import { findTracePath, scenarioKeys } from "@/lib/tracePath";
import { CHANNEL_NODES, CHECK_KEYS, PATTERN_ENGINE_KEY, REGISTER_NODES, STAGE_ALIASES } from "@/lib/architecture";

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
  /** The check this hop arrives at, when its event was a check rather than a node. */
  step?: string;
}

// Real steps arrive within milliseconds of each other — far too fast for a human to see move.
// This is the artificial per-hop pace for the edge-flow/log replay, independent of how fast
// the real pipeline actually ran (each event still carries its own real elapsedMs). A hop along
// a line is kept a little longer than TrafficEdge's 0.7s glide, so each glide finishes before
// the next one starts.
const HOP_DELAY_MS = 900;
// A step that doesn't move the dot (a check lighting up, a status change on the same node) only
// needs to be readable, not travelled. Pacing those like a full hop made one Bericht take 20s+
// to replay, so the next one started before it finished and a single message looked like several
// dots at once.
const STEP_DELAY_MS = 350;
const MAX_LOG_LINES = 150;
// A channel-send ball waits at notify-email/sms/post for the real "Notify NL" delivery
// confirmation (see BaseScenario.ProcessDataAsync's "pending" status + NotifyCallbackResponder)
// instead of auto-expiring like a normal hop — "Notify NL" itself only checks delivery status
// against AWS roughly every 5 minutes, so a genuine confirmation can take a while. If nothing
// comes back in this long, give up rather than leave that trace's state open forever.
const CONFIRM_TIMEOUT_MS = 15 * 60 * 1000;

export interface OmcTelemetry {
  connected: boolean;
  /** One entry per trace currently mid-playback — several notifications arriving close
   * together animate independently and simultaneously, each on its own pace. */
  activeHops: TraceHop[];
  /** The check (CHECK_NODES key) each in-flight trace is currently at, if any — checks are
   * blocks inside the OMC block rather than nodes, so they light up there instead. */
  activeSteps: string[];
  visitedKeys: Set<string>;
  log: TraceLogLine[];
  /** Distinct notifications seen since this page was opened. */
  totalProcessed: number;
  /** Per-node-key count of real events touching it, since this page was opened. */
  nodeThroughput: Record<string, number>;
}

/**
 * The single real-time data source for the flow page. Connects to /status/trace/stream once,
 * for as long as this hook stays mounted, and replays every real event on the diagram and in
 * the log — nothing here is simulated or backed by a database.
 */
export function useOmcTelemetry(): OmcTelemetry {
  const [connected, setConnected] = useState(false);
  const [activeHops, setActiveHops] = useState<TraceHop[]>([]);
  const [activeSteps, setActiveSteps] = useState<string[]>([]);
  const [visitedKeys, setVisitedKeys] = useState<Set<string>>(new Set());
  const [log, setLog] = useState<TraceLogLine[]>([]);
  const [totalProcessed, setTotalProcessed] = useState(0);
  const [nodeThroughput, setNodeThroughput] = useState<Record<string, number>>({});

  // Each trace gets its own queue and its own play loop, so several notifications arriving
  // close together each pace out on their own timeline instead of all serializing onto one
  // shared queue (which used to make simultaneous traces play back-to-back instead of at once).
  const hopQueuesByTraceRef = useRef<Map<string, PlannedHop[]>>(new Map());
  const playingTracesRef = useRef<Set<string>>(new Set());
  // The one active hop per trace that's currently live — surfaced as an array for rendering.
  const activeHopsByTraceRef = useRef<Map<string, TraceHop>>(new Map());
  const activeStepByTraceRef = useRef<Map<string, string>>(new Map());
  const lastStageByTraceRef = useRef<Map<string, string>>(new Map());
  // Per trace: the stage it was conceptually at right before its current run of back-to-back
  // register calls started — register round-trips retrace all the way back here instead of
  // unconditionally to the hub, so e.g. naturalpersoncheck -> openzaak -> naturalpersoncheck
  // draws the same line it arrived by, and the next real hop continues from naturalpersoncheck
  // rather than the hub picking a different, shorter edge (see the mijnzaken-gemuteerd flow's
  // naturalpersoncheck -> zaaktypewhitelist edge for the concrete case this fixes).
  const preRegisterStageByTraceRef = useRef<Map<string, string>>(new Map());
  const logCounterRef = useRef(0);
  const hopSeqRef = useRef(0);

  // Traces currently parked at a channel node awaiting a real delivery confirmation, and the
  // give-up timer for each — see CONFIRM_TIMEOUT_MS above.
  const pendingConfirmTracesRef = useRef<Set<string>>(new Set());
  const confirmTimeoutsRef = useRef<Map<string, ReturnType<typeof setTimeout>>>(new Map());

  const seenTraceIdsRef = useRef<Set<string>>(new Set());
  const nodeThroughputRef = useRef<Record<string, number>>({});

  useEffect(() => {
    let cancelled = false;
    const confirmTimeouts = confirmTimeoutsRef.current;
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

    function publishActiveSteps() {
      setActiveSteps(Array.from(new Set(activeStepByTraceRef.current.values())));
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
            activeStepByTraceRef.current.delete(traceId);
            publishActiveSteps();

            // This trace's per-trace working state (which stage it last visited, its now-empty
            // hop queue) is done being useful — free it. Without this, a dashboard tab left open
            // for hours/days accumulates one never-freed entry per trace forever; unlike
            // seenTraceIdsRef (a permanent dedup set backing the monotonic "total processed"
            // counter, which genuinely needs to remember every trace ID), nothing later reads a
            // finished trace's last stage or queue.
            lastStageByTraceRef.current.delete(traceId);
            hopQueuesByTraceRef.current.delete(traceId);
            preRegisterStageByTraceRef.current.delete(traceId);
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

        // A check lights up its block in the OMC block for as long as the trace stays there;
        // moving on to any other node (a register, an output) leaves it.
        if (hop.step) {
          activeStepByTraceRef.current.set(traceId, hop.step);
          publishActiveSteps();
        } else if (hop.to !== PATTERN_ENGINE_KEY && activeStepByTraceRef.current.delete(traceId)) {
          publishActiveSteps();
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

        setTimeout(step, hop.from === hop.to ? STEP_DELAY_MS : HOP_DELAY_MS);
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
      activeStepByTraceRef.current.delete(traceId);
      publishActiveSteps();
    }

    source.onmessage = (message) => {
      let event: TraceEvent;
      try {
        const raw: TraceEvent = JSON.parse(message.data);
        // Stages the backend names differently from their card (e.g. the print flow's "notifynl"
        // is the Printstraat card) are mapped once here, so counts, log and dot all agree.
        event = { ...raw, stage: STAGE_ALIASES[raw.stage] ?? raw.stage };
      } catch {
        return;
      }

      // Real counters.
      if (!seenTraceIdsRef.current.has(event.traceId)) {
        seenTraceIdsRef.current.add(event.traceId);
        setTotalProcessed(seenTraceIdsRef.current.size);
      }

      nodeThroughputRef.current = {
        ...nodeThroughputRef.current,
        [event.stage]: (nodeThroughputRef.current[event.stage] ?? 0) + 1,
      };
      setNodeThroughput(nodeThroughputRef.current);

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

      // A check is a block inside the OMC block, not a node of its own: the dot goes to (or
      // stays on) OMC, and the block itself lights up. Its log line keeps the real stage.
      const step = CHECK_KEYS.has(event.stage) ? event.stage : undefined;
      const graphStage = step ? PATTERN_ENGINE_KEY : event.stage;

      const lastStage = lastStageByTraceRef.current.get(event.traceId);

      if (!lastStage) {
        // First step of a new trace — nothing to animate a hop from yet, just record it.
        lastStageByTraceRef.current.set(event.traceId, graphStage);
        queue.push({ from: graphStage, to: graphStage, commit: logLine, step });
        playQueueForTrace(event.traceId);
        return;
      }

      // Restrict pathfinding to the current scenario's own nodes once it's known — otherwise a
      // register with only one edge (a straight round trip to Output Patronen) can BFS its way
      // to the next stage via a topologically shorter route through an unrelated scenario's
      // gate (see tracePath.ts's module comment for the concrete Berichten-schakelaar case).
      const allowedKeys = scenarioKeys(event.scenario);

      // Entering a register call that isn't itself a continuation of an already-in-progress run
      // of back-to-back register calls — remember where to retrace back to once every register
      // round-trip in this run has resolved.
      if (event.status === "start" && REGISTER_KEYS.has(event.stage) && !REGISTER_KEYS.has(lastStage)) {
        preRegisterStageByTraceRef.current.set(event.traceId, lastStage);
      }

      const path =
        lastStage === graphStage ? [lastStage, graphStage] : findTracePath(lastStage, graphStage, allowedKeys);
      if (!path || path.length < 2) {
        lastStageByTraceRef.current.set(event.traceId, graphStage);
        queue.push({ from: graphStage, to: graphStage, commit: logLine, step });
        playQueueForTrace(event.traceId);
        return;
      }

      for (let i = 0; i < path.length - 1; i++) {
        const isLastLeg = i === path.length - 2;
        queue.push({
          from: path[i],
          to: path[i + 1],
          commit: isLastLeg ? logLine : undefined,
          step: isLastLeg ? step : undefined,
        });
      }

      // Registers are never a forward stage of their own — OMC calls out and comes straight
      // back before doing anything else (see lib/architecture.ts's EDGES comment). Once a
      // register call reaches a terminal status, queue that return leg right away instead of
      // waiting for whatever stage happens to fire next — otherwise two calls to the same
      // register in a row (or just a gap before the next real event) would leave the pip sitting
      // at the register instead of visibly heading back. The return retraces every line back to
      // wherever the trace truly was before this run of register detours started (normally the
      // hub, but e.g. naturalpersoncheck for mijnzaken-gemuteerd) rather than always stopping at
      // the hub — so a later hop continuing on from there uses the same edge it arrived by,
      // instead of the hub picking a different, shorter one.
      const isRegisterRoundTrip = event.status !== "start" && REGISTER_KEYS.has(event.stage);
      if (isRegisterRoundTrip) {
        const returnTo = preRegisterStageByTraceRef.current.get(event.traceId) ?? PATTERN_ENGINE_KEY;
        const returnPath = findTracePath(event.stage, returnTo, allowedKeys) ?? [event.stage, PATTERN_ENGINE_KEY];
        for (let i = 0; i < returnPath.length - 1; i++) {
          queue.push({ from: returnPath[i], to: returnPath[i + 1] });
        }
        lastStageByTraceRef.current.set(event.traceId, returnPath[returnPath.length - 1]);
      } else {
        lastStageByTraceRef.current.set(event.traceId, graphStage);
      }

      playQueueForTrace(event.traceId);
    };

    return () => {
      cancelled = true;
      source.close();
      confirmTimeouts.forEach((timeout) => clearTimeout(timeout));
      confirmTimeouts.clear();
    };
  }, []);

  return {
    connected,
    activeHops,
    activeSteps,
    visitedKeys,
    log,
    totalProcessed,
    nodeThroughput,
  };
}
