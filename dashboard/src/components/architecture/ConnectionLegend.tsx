import { EDGE_CATEGORY_COLOR, EdgeCategory } from "@/lib/architecture";

const CONNECTION_TYPES: { label: string; category: EdgeCategory }[] = [
  { label: "invoer", category: "invoer" },
  { label: "registers (heen en terug)", category: "verrijking" },
  { label: "uitvoer", category: "uitvoer" },
  { label: "bevestiging", category: "bevestiging" },
];

export function ConnectionLegend() {
  return (
    <div className="flex flex-wrap items-center gap-x-4 gap-y-1.5 text-[0.68rem] text-arch-muted">
      {CONNECTION_TYPES.map((c) => (
        <span key={c.label} className="flex items-center gap-1.5">
          <span className="h-0.5 w-4 rounded-full" style={{ background: EDGE_CATEGORY_COLOR[c.category] }} />
          {c.label}
        </span>
      ))}
      <span className="flex items-center gap-1.5">
        <span className="h-1.5 w-1.5 rounded-full bg-arch-green" />
        actief
      </span>
      <span className="flex items-center gap-1.5">
        <span className="h-3 w-4 rounded-sm border border-dashed border-arch-faint" />
        nog geen client
      </span>
      <span className="flex items-center gap-1.5">
        <span className="h-3 w-4 rounded-sm border border-arch-border opacity-50" />
        gedimd: deze flow gebruikt het niet
      </span>
    </div>
  );
}
