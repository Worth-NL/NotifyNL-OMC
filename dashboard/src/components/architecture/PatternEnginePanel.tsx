import { FlowOption } from "@/lib/architecture";

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

export function PatternEnginePanel({
  flows,
  selectedKey,
  onSelect,
  totalProcessed,
  onViewDiagram,
  diagramAvailable,
}: {
  flows: FlowOption[];
  selectedKey: string;
  onSelect: (key: string) => void;
  totalProcessed: number;
  onViewDiagram: () => void;
  diagramAvailable: boolean;
}) {
  const selected = flows.find((f) => f.key === selectedKey) ?? flows[0];

  return (
    <div className="w-[260px] rounded-lg border border-arch-teal bg-arch-surface p-4 shadow-[0_1px_8px_rgb(13_148_136_/_0.12)] ring-1 ring-arch-teal/20">
      <div className="flex items-start justify-between gap-2">
        <div>
          <div className="text-[0.85rem] font-bold text-arch-ink">Output Patronen</div>
          <div className="text-[0.68rem] text-arch-muted">Patroon-engine</div>
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

      <p className="mt-1.5 text-[0.7rem] leading-snug text-arch-muted">
        Regelgebaseerde patronen bepalen kanaalkeuze, opmaak, verrijking en timing per uitgaand
        bericht.
      </p>

      <label className="nodrag mt-3 block">
        <span className="text-[0.62rem] font-semibold tracking-wide text-arch-muted uppercase">
          Flow
        </span>
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

      <div className="mt-3 text-[1.4rem] font-bold text-arch-teal">{formatCount(totalProcessed)}</div>
      <div className="text-[0.65rem] text-arch-muted">berichten verwerkt sinds laden</div>

      <div className="mt-3 flex items-center gap-1.5 text-[0.68rem] font-medium">
        <span className="h-1.5 w-1.5 rounded-full bg-arch-green" />
        <span className="text-arch-muted">Actief</span>
      </div>

      <p className="mt-2 border-t border-arch-border pt-2 text-[0.62rem] leading-snug text-arch-faint">
        {selected.key === "all"
          ? "Selecteer een flow om te zien welke onderdelen die daadwerkelijk gebruikt."
          : `${selected.registers.length} register(s), ${selected.channels.length} kanaal/kanalen actief.`}
      </p>
    </div>
  );
}
