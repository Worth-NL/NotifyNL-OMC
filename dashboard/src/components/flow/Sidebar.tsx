"use client";

import { ScenarioFlow } from "@/lib/api";

const CHANNEL_CLASS: Record<string, string> = {
  zaken: "bg-blue-100 text-blue-700",
  objecten: "bg-green-100 text-green-800",
  besluiten: "bg-violet-100 text-violet-700",
  overview: "bg-border text-muted",
};

export function Sidebar({
  scenarios,
  currentKey,
  onSelect,
}: {
  scenarios: ScenarioFlow[];
  currentKey: string | null;
  onSelect: (key: string) => void;
}) {
  return (
    <aside className="flex w-[248px] shrink-0 flex-col overflow-y-auto border-r border-border bg-surface">
      <div className="border-b border-border p-[1.1rem] pb-2.5">
        <div className="mb-0.5 text-[0.62rem] font-bold tracking-[0.12em] text-orange uppercase">
          Scenario flows
        </div>
        <div className="text-[0.85rem] font-bold text-dark">Decision diagrams</div>
        <div className="mt-1 text-[0.68rem] leading-relaxed text-muted">
          Click a scenario — or click a dark node inside any diagram — to navigate between flows.
        </div>
      </div>
      <div className="py-1.5">
        {scenarios.map((s) => (
          <button
            key={s.key}
            type="button"
            onClick={() => onSelect(s.key)}
            className={`flex w-full items-start gap-2 border-l-[3px] px-[1.1rem] py-2.5 text-left transition hover:bg-bg ${
              s.key === currentKey ? "border-orange bg-orange-light" : "border-transparent"
            }`}
          >
            <span>
              <span className="block text-[0.78rem] leading-tight font-semibold text-dark">
                {s.name}
              </span>
              <span className="block text-[0.68rem] text-muted">{s.nl}</span>
              {s.channel !== "overview" && (
                <span
                  className={`mt-1 inline-block rounded px-[0.35rem] py-[0.1rem] text-[0.58rem] font-bold tracking-[0.04em] uppercase ${CHANNEL_CLASS[s.channel] ?? ""}`}
                >
                  {s.channel}
                </span>
              )}
            </span>
          </button>
        ))}
      </div>
    </aside>
  );
}
