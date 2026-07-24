"use client";

import { useEffect, useRef, useState } from "react";

// Client-side simulated telemetry — there is no database backing this, by design (OMC
// stays stateless). This stands in for a future real-time source, most likely an SSE
// endpoint similar to /status/stream (e.g. /status/metrics/stream) that reports actual
// in-flight request counts from the running process. Swapping the simulation below for a
// real EventSource subscription should be a self-contained change inside this hook —
// consuming components only see the LiveMetrics shape, never how it's produced.

export interface LiveMetrics {
  load: number;
  avgHandlingMs: number;
  throughputPerSec: number;
  totalProcessed: number;
  sparkline: number[];
  nodeThroughput: Record<string, number>;
}

const SPARKLINE_LENGTH = 24;

function randomWalk(value: number, min: number, max: number, maxStep: number): number {
  const next = value + (Math.random() - 0.5) * maxStep;
  return Math.min(max, Math.max(min, next));
}

// Deterministic per-key starting value so server-rendered and client-hydrated markup
// match exactly — actual randomness only ever happens inside the interval below, which
// never runs during server rendering, so it can't cause a hydration mismatch.
function seedThroughput(key: string): number {
  let hash = 0;
  for (let i = 0; i < key.length; i++) {
    hash = (hash * 31 + key.charCodeAt(i)) >>> 0;
  }
  return hash % 50_000;
}

export function useLiveMetrics(nodeKeys: string[], intervalMs = 1500): LiveMetrics {
  const [state, setState] = useState<LiveMetrics>(() => ({
    load: 60,
    avgHandlingMs: 200,
    throughputPerSec: 750,
    totalProcessed: 150_000,
    sparkline: Array.from({ length: SPARKLINE_LENGTH }, () => 60),
    nodeThroughput: Object.fromEntries(nodeKeys.map((k) => [k, seedThroughput(k)])),
  }));

  // nodeKeys is derived from static data, but keep a ref so the interval closure always
  // sees the latest list without needing to restart the interval on every render.
  const nodeKeysRef = useRef(nodeKeys);
  useEffect(() => {
    nodeKeysRef.current = nodeKeys;
  });

  useEffect(() => {
    const id = setInterval(() => {
      setState((prev) => {
        const load = randomWalk(prev.load, 30, 95, 12);
        const avgHandlingMs = Math.round(randomWalk(prev.avgHandlingMs, 120, 320, 30));
        const throughputPerSec = Math.round(randomWalk(prev.throughputPerSec, 400, 950, 80));
        const totalProcessed = prev.totalProcessed + Math.round(throughputPerSec * (intervalMs / 1000));

        const nodeThroughput = { ...prev.nodeThroughput };
        for (const key of nodeKeysRef.current) {
          nodeThroughput[key] = (nodeThroughput[key] ?? 0) + Math.round(Math.random() * 40);
        }

        return {
          load,
          avgHandlingMs,
          throughputPerSec,
          totalProcessed,
          sparkline: [...prev.sparkline.slice(1), load],
          nodeThroughput,
        };
      });
    }, intervalMs);

    return () => clearInterval(id);
  }, [intervalMs]);

  return state;
}
