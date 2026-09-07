"use client";

import { useEffect } from "react";
import { createPortal } from "react-dom";
import { ArchitectureNode } from "@/lib/architecture";
import { TraceLogLine } from "@/hooks/useOmcTelemetry";
import { isError, STATUS_LABEL } from "./LiveLogPanel";

/** All log lines for one node, newest first — same filter a user would get by eye from the
 * shared live log, just scoped to this card so a busy trace doesn't bury what it did. */
export function NodeLogModal({
  node,
  log,
  onClose,
}: {
  node: ArchitectureNode | null;
  log: TraceLogLine[];
  onClose: () => void;
}) {
  useEffect(() => {
    if (!node) return;
    function onKeyDown(e: KeyboardEvent) {
      if (e.key === "Escape") onClose();
    }
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [node, onClose]);

  if (!node || typeof document === "undefined") return null;

  const lines = log.filter((line) => line.stage === node.key);

  return createPortal(
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-arch-ink/60 p-6" onClick={onClose}>
      <div
        className="flex max-h-[80vh] w-full max-w-2xl flex-col overflow-hidden rounded-xl border border-arch-border bg-arch-surface shadow-[0_12px_40px_rgb(15_23_42_/_0.25)]"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between border-b border-arch-border px-5 py-3">
          <div>
            <div className="text-[0.85rem] font-bold text-arch-ink">{node.name}</div>
            <div className="text-[0.7rem] text-arch-muted">
              {lines.length} {lines.length === 1 ? "gebeurtenis" : "gebeurtenissen"} sinds deze pagina is geopend
            </div>
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

        <div className="flex-1 overflow-y-auto px-5 py-3 font-mono text-[0.68rem] leading-relaxed">
          {lines.length === 0 ? (
            <p className="py-6 text-center text-arch-faint">
              Nog geen gebeurtenissen voor dit onderdeel — start een trace en volg een notificatie.
            </p>
          ) : (
            lines.map((line) => (
              <div
                key={line.id}
                className={`flex flex-wrap gap-2 border-b border-arch-border/60 py-1.5 last:border-0 ${
                  isError(line.status) ? "text-red" : "text-arch-ink"
                }`}
              >
                <span className="shrink-0 text-arch-faint">{line.time}</span>
                <span className="shrink-0 text-arch-muted">#{line.traceId.slice(0, 8)}</span>
                {line.scenario && <span className="shrink-0 text-arch-teal">{line.scenario}</span>}
                <span className={isError(line.status) ? "font-semibold" : "text-arch-muted"}>
                  {STATUS_LABEL[line.status]}
                </span>
                {line.detail && (
                  <span className={isError(line.status) ? "font-semibold text-red" : "text-arch-faint"}>
                    ({line.detail})
                  </span>
                )}
              </div>
            ))
          )}
        </div>
      </div>
    </div>,
    document.body,
  );
}
