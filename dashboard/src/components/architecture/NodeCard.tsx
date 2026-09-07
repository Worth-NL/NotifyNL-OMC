import { ArchitectureNode } from "@/lib/architecture";

export type NodeState = "highlighted" | "dimmed" | "inactive";

function formatCount(n: number): string {
  return new Intl.NumberFormat("nl-NL").format(n);
}

export function NodeCard({
  node,
  state,
  throughput,
  onClick,
}: {
  node: ArchitectureNode;
  state: NodeState;
  throughput?: number;
  onClick?: () => void;
}) {
  const isInactive = state === "inactive";
  const isDimmed = state === "dimmed";

  return (
    <button
      type="button"
      onClick={onClick}
      className={`w-full rounded-lg border bg-arch-surface p-3 text-left transition-all duration-200 ${
        onClick ? "cursor-pointer hover:border-arch-teal/60" : ""
      } ${
        isInactive
          ? "border-dashed border-arch-border opacity-70"
          : isDimmed
            ? "border-arch-border opacity-40 grayscale-[35%]"
            : "border-arch-teal shadow-[0_1px_6px_rgb(13_148_136_/_0.15)] ring-1 ring-arch-teal/20"
      }`}
    >
      <div className="flex items-start justify-between gap-2">
        <span className={`text-[0.8rem] font-semibold ${isInactive ? "text-arch-faint" : "text-arch-ink"}`}>
          {node.name}
        </span>
        {node.version && (
          <span className="shrink-0 rounded bg-arch-bg px-1.5 py-0.5 text-[0.62rem] font-medium text-arch-muted">
            {node.version}
          </span>
        )}
      </div>
      <p className="mt-0.5 text-[0.68rem] leading-snug text-arch-muted">{node.subtitle}</p>
      <div className="mt-2 flex items-center justify-between">
        <span className="flex items-center gap-1.5 text-[0.68rem] font-medium">
          <span
            className={`h-1.5 w-1.5 rounded-full ${isInactive ? "bg-arch-faint" : "bg-arch-green"}`}
          />
          <span className={isInactive ? "text-arch-faint" : "text-arch-muted"}>
            {isInactive ? "Inactief" : "Actief"}
          </span>
        </span>
        {!isInactive && throughput !== undefined && (
          <span className="rounded-full bg-arch-bg px-1.5 py-0.5 text-[0.62rem] font-medium text-arch-muted">
            {formatCount(throughput)}
          </span>
        )}
      </div>
    </button>
  );
}
