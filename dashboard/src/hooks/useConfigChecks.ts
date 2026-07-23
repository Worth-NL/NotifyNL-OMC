"use client";

import { useEffect, useRef, useState } from "react";
import { CheckResult, OMC_API_URL, StreamComplete } from "@/lib/api";

export interface CheckGroup {
  name: string;
  icon: string;
  checks: CheckResult[];
  ok: number;
  fail: number;
}

export type StreamStatus = "connecting" | "streaming" | "complete" | "error";

export interface ConfigChecksState {
  groups: CheckGroup[];
  received: number;
  expectedTotal: number | null;
  summary: StreamComplete | null;
  status: StreamStatus;
}

export function useConfigChecks(): ConfigChecksState {
  const [state, setState] = useState<ConfigChecksState>({
    groups: [],
    received: 0,
    expectedTotal: null,
    summary: null,
    status: "connecting",
  });

  const groupsRef = useRef<CheckGroup[]>([]);

  useEffect(() => {
    const source = new EventSource(`${OMC_API_URL}/status/stream`);

    source.addEventListener("start", (event) => {
      const data = JSON.parse((event as MessageEvent).data) as { total: number };
      setState((prev) => ({ ...prev, expectedTotal: data.total, status: "streaming" }));
    });

    source.onmessage = (event) => {
      const check = JSON.parse(event.data) as CheckResult;

      const groups = groupsRef.current;
      let group = groups.find((g) => g.name === check.group);
      if (!group) {
        group = { name: check.group, icon: check.icon, checks: [], ok: 0, fail: 0 };
        groups.push(group);
      }
      group.checks.push(check);
      if (check.ok) group.ok++;
      else group.fail++;

      setState((prev) => ({
        ...prev,
        groups: [...groups],
        received: prev.received + 1,
        status: "streaming",
      }));
    };

    source.addEventListener("complete", (event) => {
      const summary = JSON.parse((event as MessageEvent).data) as StreamComplete;
      setState((prev) => ({ ...prev, summary, status: "complete" }));
      source.close();
    });

    source.onerror = () => {
      setState((prev) => (prev.status === "complete" ? prev : { ...prev, status: "error" }));
      source.close();
    };

    return () => source.close();
  }, []);

  return state;
}
