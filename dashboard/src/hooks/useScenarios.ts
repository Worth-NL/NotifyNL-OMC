"use client";

import { useEffect, useState } from "react";
import { fetchScenarios, ScenarioFlow } from "@/lib/api";

interface State {
  scenarios: ScenarioFlow[];
  loading: boolean;
  error: string | null;
}

export function useScenarios(): State {
  const [state, setState] = useState<State>({ scenarios: [], loading: true, error: null });

  useEffect(() => {
    let cancelled = false;

    fetchScenarios()
      .then((scenarios) => {
        if (!cancelled) setState({ scenarios, loading: false, error: null });
      })
      .catch((err) => {
        if (!cancelled) {
          setState({ scenarios: [], loading: false, error: (err as Error).message });
        }
      });

    return () => {
      cancelled = true;
    };
  }, []);

  return state;
}
