"use client";

import { useEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";
import mermaid from "mermaid";
import { ScenarioFlow } from "@/lib/api";

// Separate init/flag from the (currently unused) legacy DiagramViewer — this modal's
// themeVariables are the resolved hex values of this page's --color-arch-* tokens (see
// globals.css), so the diagram shell (nodes without an explicit `style` override, lines,
// backgrounds) follows this app's palette instead of the old orange brand theme. Mermaid
// parses these itself for internal color math (tints/shades), so it must be literal colors —
// a "var(--color-arch-bg)" string throws "Unsupported color format" at init.
let mermaidInitialized = false;
function ensureMermaidInitialized() {
  if (mermaidInitialized) return;
  mermaid.initialize({
    startOnLoad: false,
    securityLevel: "loose",
    theme: "base",
    themeVariables: {
      primaryColor: "#ccfbf1", // --color-arch-teal-light
      primaryTextColor: "#1e293b", // --color-arch-ink
      primaryBorderColor: "#0d9488", // --color-arch-teal
      lineColor: "#64748b", // --color-arch-muted
      secondaryColor: "#eef1f7", // --color-arch-bg
      tertiaryColor: "#ffffff", // --color-arch-surface
      fontSize: "13px",
      fontFamily: "'Sora', ui-sans-serif, system-ui, sans-serif",
    },
  });
  mermaidInitialized = true;
}

const MIN_ZOOM = 0.3;
const MAX_ZOOM = 3;
const DEFAULT_ZOOM = 1;

let renderCounter = 0;

export function DiagramModal({
  scenario,
  onClose,
}: {
  scenario: ScenarioFlow | null;
  onClose: () => void;
}) {
  useEffect(() => {
    if (!scenario) return;
    function onKeyDown(e: KeyboardEvent) {
      if (e.key === "Escape") onClose();
    }
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [scenario, onClose]);

  if (!scenario || typeof document === "undefined") return null;

  return createPortal(
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-arch-ink/60 p-6"
      onClick={onClose}
    >
      {/* Keyed by scenario so scale/render-state reset fresh per scenario via remount,
          instead of a synchronous setState-in-effect reset. */}
      <DiagramModalContent key={scenario.key} scenario={scenario} onClose={onClose} />
    </div>,
    document.body,
  );
}

function DiagramModalContent({ scenario, onClose }: { scenario: ScenarioFlow; onClose: () => void }) {
  const wrapRef = useRef<HTMLDivElement>(null);
  const [scale, setScale] = useState(DEFAULT_ZOOM);
  const [renderState, setRenderState] = useState<"loading" | "ready" | "error">("loading");
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(() => {
    ensureMermaidInitialized();

    let cancelled = false;
    const id = `mmd-modal-${++renderCounter}`;
    mermaid
      .render(id, scenario.diagram)
      .then(({ svg, bindFunctions }) => {
        if (cancelled || !wrapRef.current) return;
        wrapRef.current.innerHTML = svg;
        bindFunctions?.(wrapRef.current);
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

  return (
    <div
      className="flex max-h-full w-full max-w-5xl flex-col overflow-hidden rounded-xl border border-arch-border bg-arch-surface shadow-[0_12px_40px_rgb(15_23_42_/_0.25)]"
      onClick={(e) => e.stopPropagation()}
    >
      <div className="flex items-center justify-between border-b border-arch-border px-5 py-3">
        <div>
          <div className="text-[0.85rem] font-bold text-arch-ink">{scenario.name}</div>
          <div className="text-[0.7rem] text-arch-muted">{scenario.nl}</div>
        </div>
        <button
          type="button"
          onClick={onClose}
          aria-label="Sluiten"
          className="flex h-7 w-7 items-center justify-center rounded-md text-arch-muted transition hover:bg-arch-bg hover:text-arch-ink"
        >
          ✕
        </button>
      </div>

      <div className="relative flex-1 overflow-auto p-5">
        {renderState === "loading" && (
          <div className="flex items-center gap-2 text-[0.78rem] text-arch-muted">
            <div className="h-3.5 w-3.5 animate-[spin_0.6s_linear_infinite] rounded-full border-2 border-arch-border border-t-arch-teal" />
            Diagram wordt gerenderd…
          </div>
        )}
        {renderState === "error" && (
          <pre className="max-w-full rounded-lg border border-arch-border bg-arch-bg p-4 text-[0.7rem] whitespace-pre-wrap text-red">
            {errorMessage}
          </pre>
        )}
        <div
          ref={wrapRef}
          style={{ transform: `scale(${scale})`, display: renderState === "ready" ? "block" : "none" }}
          className="w-max origin-top-left [&_svg]:block"
        />
      </div>

      <div className="flex items-center justify-end gap-1 border-t border-arch-border px-4 py-2">
        <button
          type="button"
          onClick={() => zoom(1.2)}
          className="flex h-7 w-7 items-center justify-center rounded-md text-[0.85rem] font-bold text-arch-muted transition hover:bg-arch-bg hover:text-arch-ink"
        >
          +
        </button>
        <button
          type="button"
          onClick={() => zoom(1 / 1.2)}
          className="flex h-7 w-7 items-center justify-center rounded-md text-[0.85rem] font-bold text-arch-muted transition hover:bg-arch-bg hover:text-arch-ink"
        >
          −
        </button>
        <button
          type="button"
          onClick={() => setScale(DEFAULT_ZOOM)}
          className="flex h-7 w-7 items-center justify-center rounded-md text-[0.6rem] font-black text-arch-muted transition hover:bg-arch-bg hover:text-arch-ink"
        >
          ⊙
        </button>
      </div>
    </div>
  );
}
