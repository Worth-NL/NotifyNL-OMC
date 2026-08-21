"use client";

import { useEffect, useRef, useState } from "react";
import mermaid from "mermaid";
import { ScenarioFlow } from "@/lib/api";
import { SCENARIO_NODE_MAP } from "@/lib/scenarioNodeMap";

let mermaidInitialized = false;
function ensureMermaidInitialized() {
  if (mermaidInitialized) return;
  mermaid.initialize({
    startOnLoad: false,
    securityLevel: "loose",
    theme: "base",
    themeVariables: {
      primaryColor: "#dbeafe",
      primaryTextColor: "#1E2238",
      primaryBorderColor: "#93c5fd",
      lineColor: "#80818C",
      secondaryColor: "#F6F7FA",
      tertiaryColor: "#fff3cd",
      fontSize: "13px",
      fontFamily: "'Sora', ui-sans-serif, system-ui, sans-serif",
    },
  });
  mermaidInitialized = true;
}

const MIN_ZOOM = 0.3;
const MAX_ZOOM = 5;
const DEFAULT_ZOOM = 1;

let renderCounter = 0;

export function DiagramViewer({
  scenario,
  onNavigate,
}: {
  scenario: ScenarioFlow;
  onNavigate: (key: string) => void;
}) {
  const wrapRef = useRef<HTMLDivElement>(null);
  // No internal "reset on scenario change" logic needed — the parent remounts
  // this component per scenario via `key`, so these initial values are always fresh.
  const [scale, setScale] = useState(DEFAULT_ZOOM);
  const [renderState, setRenderState] = useState<"loading" | "ready" | "error">("loading");
  const [errorMessage, setErrorMessage] = useState("");
  const onNavigateRef = useRef(onNavigate);
  useEffect(() => {
    onNavigateRef.current = onNavigate;
  });

  useEffect(() => {
    ensureMermaidInitialized();
    // Global dispatcher for Mermaid's native `click <nodeId> go` directive.
    (window as unknown as { go?: (nodeId: string) => void }).go = (nodeId: string) => {
      const key = SCENARIO_NODE_MAP[nodeId];
      if (key) onNavigateRef.current(key);
    };
  }, []);

  useEffect(() => {
    let cancelled = false;

    const id = `mmd${++renderCounter}`;
    mermaid
      .render(id, scenario.diagram)
      .then(({ svg, bindFunctions }) => {
        if (cancelled || !wrapRef.current) return;
        wrapRef.current.innerHTML = svg;
        bindFunctions?.(wrapRef.current);
        wireClicks(wrapRef.current, onNavigateRef.current);
        setRenderState("ready");
      })
      .catch((err) => {
        if (cancelled) return;
        setErrorMessage(String(err));
        setRenderState("error");
      });

    return () => {
      cancelled = true;
    };
  }, [scenario]);

  function zoom(factor: number) {
    setScale((s) => Math.min(Math.max(s * factor, MIN_ZOOM), MAX_ZOOM));
  }

  function resetZoom() {
    setScale(DEFAULT_ZOOM);
  }

  return (
    <div className="relative flex flex-1 items-start justify-start overflow-auto p-5">
      {renderState === "loading" && (
        <div className="flex items-center gap-2.5 rounded-xl border border-border bg-surface p-12 text-[0.82rem] text-muted shadow-[0_1px_8px_rgb(30_34_56_/_0.06)]">
          <div className="h-4 w-4 animate-[spin_0.6s_linear_infinite] rounded-full border-2 border-border border-t-orange" />
          Rendering…
        </div>
      )}
      {renderState === "error" && (
        <pre className="max-w-full rounded-xl border border-border bg-surface p-6 text-[0.72rem] whitespace-pre-wrap text-red shadow-[0_1px_8px_rgb(30_34_56_/_0.06)]">
          {errorMessage}
        </pre>
      )}
      {/* Mermaid target: React never renders children here — content is written imperatively so it can't conflict with React's own reconciliation. */}
      <div
        ref={wrapRef}
        style={{ transform: `scale(${scale})`, display: renderState === "ready" ? "block" : "none" }}
        className="w-max min-w-[calc(100%-2.5rem)] origin-top-left rounded-xl border border-border bg-surface p-7 shadow-[0_1px_8px_rgb(30_34_56_/_0.06)] transition-transform duration-150 ease-out [&_svg]:block [&_g.node.clickable]:cursor-pointer"
      />

      <div className="fixed right-[1.1rem] bottom-[1.1rem] z-20 flex gap-1 rounded-lg border border-border bg-surface p-1 shadow-[0_2px_8px_rgb(0_0_0_/_0.08)]">
        <button
          type="button"
          onClick={() => zoom(1.2)}
          className="flex h-[26px] w-[26px] items-center justify-center rounded-md text-[0.9rem] font-bold text-charcoal transition hover:bg-bg"
        >
          +
        </button>
        <button
          type="button"
          onClick={() => zoom(1 / 1.2)}
          className="flex h-[26px] w-[26px] items-center justify-center rounded-md text-[0.9rem] font-bold text-charcoal transition hover:bg-bg"
        >
          −
        </button>
        <button
          type="button"
          onClick={resetZoom}
          className="flex h-[26px] w-[26px] items-center justify-center rounded-md text-[0.6rem] font-black text-charcoal transition hover:bg-bg"
        >
          ⊙
        </button>
      </div>
    </div>
  );
}

/** Belt-and-suspenders DOM-level click wiring, matching the legacy dashboard's wireClicks(). */
function wireClicks(wrap: HTMLDivElement, onNavigate: (key: string) => void) {
  const svg = wrap.querySelector("svg");
  if (!svg) return;

  svg.querySelectorAll("g.node.clickable").forEach((g) => {
    // Mermaid prefixes node ids with its own render id, e.g. "mmd4-flowchart-case_updated-16".
    const match = /flowchart-(.+)-\d+$/.exec(g.id || "");
    if (!match) return;
    const key = SCENARIO_NODE_MAP[match[1]];
    if (!key) return;
    g.addEventListener("click", () => onNavigate(key));
  });
}
