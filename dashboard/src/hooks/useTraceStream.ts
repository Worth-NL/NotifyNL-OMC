"use client";

import { useEffect, useRef, useState } from "react";
import { TRACE_STREAM_URL, TraceEvent } from "@/lib/api";
import { findTracePath } from "@/lib/tracePath";

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
  /** Monotonic per-hop counter — lets TracePip force a fresh SMIL restart even when two
   * different hops happen to share the same (from, to) pair. */
  seq: number;
}

interface PlannedHop {
  from: string;
  to: string;
  /** Attached to a hop's final leg — committed to the log/visited-set on arrival. */
  commit?: TraceLogLine;
}

// Real steps arrive within milliseconds of each other — far too fast to see move. This is the
// artificial per-hop pace for the replay, independent of how fast the real pipeline actually ran
// (each event still carries its own real elapsedMs, shown in the log).
const HOP_DELAY_MS = 550;
const MAX_LOG_LINES = 150;

export interface TraceStreamState {
  connected: boolean;
  activeHop: TraceHop | null;
  visitedKeys: Set<string>;
  log: TraceLogLine[];
}

/**
 * Subscribes to /status/trace/stream while `enabled`, and replays the real events it receives
 * at a fixed, watchable pace — one hop across the architecture graph at a time — rather than
 * rendering them the instant they arrive. No data is stored anywhere; this hook's state is the
 * only "history" and resets whenever tracing is turned off.
 */
export function useTraceStream(enabled: boolean): TraceStreamState {
  const [connected, setConnected] = useState(false);
  const [activeHop, setActiveHop] = useState<TraceHop | null>(null);
  const [visitedKeys, setVisitedKeys] = useState<Set<string>>(new Set());
  const [log, setLog] = useState<TraceLogLine[]>([]);

  const hopQueueRef = useRef<PlannedHop[]>([]);
  const playingRef = useRef(false);
  const lastStageByTraceRef = useRef<Map<string, string>>(new Map());
  const logCounterRef = useRef(0);
  const hopSeqRef = useRef(0);

  useEffect(() => {
    if (!enabled) {
      // Nothing to connect — state is already reset, either from the initial values or from
      // the previous run's cleanup below (which fires the instant `enabled` flips to false).
      return;
    }

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
      lastStageByTraceRef.current.set(event.traceId, event.stage);

      if (!lastStage) {
        // First step of a new trace — nothing to animate a hop from yet, just record it.
        hopQueueRef.current.push({ from: event.stage, to: event.stage, commit: logLine });
        playQueue();
        return;
      }

      const path = lastStage === event.stage ? [lastStage, event.stage] : findTracePath(lastStage, event.stage);
      if (!path || path.length < 2) {
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
      playQueue();
    };

    return () => {
      cancelled = true;
      source.close();

      // Runs the instant `enabled` flips back to false (or on unmount) — resetting here,
      // rather than synchronously in the effect body, keeps this a teardown of the
      // subscription instead of a render-triggered state write.
      hopQueueRef.current = [];
      playingRef.current = false;
      lastStageByTraceRef.current = new Map();
      setActiveHop(null);
      setVisitedKeys(new Set());
      setLog([]);
      setConnected(false);
    };
  }, [enabled]);

  return { connected, activeHop, visitedKeys, log };
}
