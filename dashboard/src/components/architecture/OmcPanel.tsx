import { ArchitectureNode, FlowOption } from "@/lib/architecture";

function formatCount(n: number): string {
  return new Intl.NumberFormat("nl-NL").format(n);
}

function DiagramIcon() {
  return (
    <svg viewBox="0 0 16 16" fill="none" className="h-3.5 w-3.5">
      <rect x="1" y="1.5" width="5" height="4" rx="1" stroke="currentColor" strokeWidth="1.2" />
      <rect x="10" y="1.5" width="5" height="4" rx="1" stroke="currentColor" strokeWidth="1.2" />
      <rect x="5.5" y="10.5" width="5" height="4" rx="1" stroke="currentColor" strokeWidth="1.2" />
      <path
        d="M3.5 5.5V8a1 1 0 0 0 1 1H8m4-3.5V8a1 1 0 0 1-1 1H8m0 0v1.5"
        stroke="currentColor"
        strokeWidth="1.2"
        strokeLinecap="round"
      />
    </svg>
  );
}

function Arrow() {
  return (
    <svg viewBox="0 0 12 8" className="h-2 w-3 shrink-0 text-arch-faint" aria-hidden="true">
      <path d="M0 4h10m-3-3 3 3-3 3" fill="none" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

/** One row of checks: a check chain, and every flow that runs exactly that chain. */
export interface CheckRow {
  label: string;
  flowKeys: string[];
  checks: ArchitectureNode[];
}

export const OMC_PANEL_WIDTH = 600;
const HEADER_HEIGHT = 176;
const ROW_HEIGHT = 56;
// A long chain wraps onto more lines of blocks (the Berichtenbox has eight checks). The row's
// height is reserved up front, so the line breaks are estimated from the labels: a block is
// roughly its text plus padding and a counter, and an arrow sits between blocks.
const LINE_WIDTH = OMC_PANEL_WIDTH - 32;
const CHAR_WIDTH = 5.6;
const BLOCK_PADDING = 36;
const ARROW_WIDTH = 20;
const LINE_HEIGHT = 30;
const BOTTOM_PADDING = 12;

function lineCount(row: CheckRow): number {
  let lines = 1;
  let used = 0;
  for (const check of row.checks) {
    const width = (check.short ?? check.name).length * CHAR_WIDTH + BLOCK_PADDING;
    const needed = used === 0 ? width : used + ARROW_WIDTH + width;
    if (needed > LINE_WIDTH && used > 0) {
      lines++;
      used = width;
    } else {
      used = needed;
    }
  }
  return lines;
}

function rowHeight(row: CheckRow): number {
  return ROW_HEIGHT + (lineCount(row) - 1) * LINE_HEIGHT;
}

/** The block's height is fixed by its rows, so the layout can place everything around it. */
export function omcPanelHeight(rows: CheckRow[]): number {
  return HEADER_HEIGHT + rows.reduce((sum, row) => sum + rowHeight(row), 0) + BOTTOM_PADDING;
}

/**
 * OMC as one block: the scenario engine (which flow, how much has passed through) on top, and
 * every flow's checks underneath, in the order that flow runs them. Picking a flow highlights
 * its row and dims the rest; a live trace lights up the check it is at; clicking a check opens
 * what it checks, with its log.
 */
export function OmcPanel({
  flows,
  selectedKey,
  onSelect,
  totalProcessed,
  onViewDiagram,
  diagramAvailable,
  rows,
  activeChecks,
  throughput,
  onCheckClick,
}: {
  flows: FlowOption[];
  selectedKey: string;
  onSelect: (key: string) => void;
  totalProcessed: number;
  onViewDiagram: () => void;
  diagramAvailable: boolean;
  rows: CheckRow[];
  activeChecks: string[];
  throughput: Record<string, number>;
  onCheckClick: (key: string) => void;
}) {
  const selected = flows.find((f) => f.key === selectedKey) ?? flows[0];
  const live = new Set(activeChecks);

  return (
    <div
      className="flex flex-col rounded-lg border border-arch-teal bg-arch-surface shadow-[0_1px_8px_rgb(13_148_136_/_0.12)] ring-1 ring-arch-teal/20"
      style={{ width: OMC_PANEL_WIDTH, minHeight: omcPanelHeight(rows) }}
    >
      <div className="border-b border-arch-border px-4 pt-3 pb-3">
        <div className="flex items-start justify-between gap-3">
          <div>
            <div className="text-[1rem] font-bold text-arch-ink">OMC</div>
            <div className="text-[0.68rem] text-arch-muted">Output Management Component</div>
          </div>
          <div className="flex items-start gap-3">
            <div className="text-right">
              <div className="text-[1.1rem] leading-none font-bold text-arch-teal">{formatCount(totalProcessed)}</div>
              <div className="mt-0.5 text-[0.6rem] whitespace-nowrap text-arch-muted">verwerkt sinds laden</div>
            </div>
            <button
              type="button"
              onClick={onViewDiagram}
              disabled={!diagramAvailable}
              title={`Bekijk diagram — ${selected.name}`}
              aria-label={`Bekijk diagram voor ${selected.name}`}
              className="nodrag flex h-6 w-6 shrink-0 items-center justify-center rounded-md border border-arch-border text-arch-muted transition hover:border-arch-teal hover:text-arch-teal disabled:cursor-not-allowed disabled:opacity-40"
            >
              <DiagramIcon />
            </button>
          </div>
        </div>

        <label className="nodrag mt-3 block">
          <span className="text-[0.62rem] font-semibold tracking-wide text-arch-muted uppercase">Flow</span>
          <select
            value={selectedKey}
            onChange={(e) => onSelect(e.target.value)}
            className="mt-1 w-full rounded-md border border-arch-border bg-arch-bg px-2 py-1.5 text-[0.78rem] font-medium text-arch-ink focus:border-arch-teal focus:ring-1 focus:ring-arch-teal focus:outline-none"
          >
            {flows.map((flow) => (
              <option key={flow.key} value={flow.key}>
                {flow.name} — {flow.nl}
              </option>
            ))}
          </select>
        </label>

        <div className="mt-4 flex items-baseline justify-between">
          <span className="text-[0.62rem] font-semibold tracking-wide text-arch-muted uppercase">
            Controles per flow
          </span>
          <span className="text-[0.62rem] text-arch-faint">op volgorde — klik een controle voor uitleg en log</span>
        </div>
      </div>

      <ol className="nodrag flex flex-col px-4">
        {rows.map((row, index) => {
          const isSelected = row.flowKeys.includes(selectedKey);
          const isDimmed = selectedKey !== "all" && !isSelected;

          return (
            <li
              key={row.label}
              className={`flex flex-col justify-center gap-1 transition-opacity ${index > 0 ? "border-t border-arch-border" : ""} ${
                isDimmed ? "opacity-35" : ""
              }`}
              style={{ height: rowHeight(row) }}
            >
              <span
                className={`truncate text-[0.62rem] font-semibold tracking-wide uppercase ${
                  isSelected ? "text-arch-teal" : "text-arch-muted"
                }`}
              >
                {row.label}
              </span>

              <span className="flex flex-wrap items-center gap-x-1 gap-y-1.5">
                {row.checks.length === 0 && (
                  <span className="text-[0.68rem] text-arch-faint italic">geen controles — direct door naar de uitvoer</span>
                )}
                {row.checks.map((check, i) => {
                  const isLive = live.has(check.key);
                  const count = throughput[check.key] ?? 0;

                  return (
                    <span key={check.key} className="flex items-center gap-1">
                      {i > 0 && <Arrow />}
                      <button
                        type="button"
                        onClick={() => onCheckClick(check.key)}
                        title={check.details?.what ?? check.subtitle}
                        className={`flex items-center gap-1 rounded-md border px-2 py-1 text-[0.68rem] font-medium whitespace-nowrap transition ${
                          isLive
                            ? "border-arch-teal bg-arch-teal-light text-arch-ink"
                            : isSelected
                              ? "border-arch-teal/60 bg-arch-bg text-arch-ink hover:border-arch-teal"
                              : "border-arch-border bg-arch-bg text-arch-ink hover:border-arch-teal/60"
                        }`}
                      >
                        {check.short ?? check.name}
                        {count > 0 && (
                          <span className="rounded-full bg-arch-surface px-1 text-[0.58rem] text-arch-muted">
                            {formatCount(count)}
                          </span>
                        )}
                      </button>
                    </span>
                  );
                })}
              </span>
            </li>
          );
        })}
      </ol>
    </div>
  );
}
