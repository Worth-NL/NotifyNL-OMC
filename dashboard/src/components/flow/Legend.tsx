const ITEMS: { color: string; label: string }[] = [
  { color: "#FF7A59", label: "Webhook / entry" },
  { color: "#1E2238", label: "Scenario node" },
  { color: "#16a34a", label: "Success" },
  { color: "#dc2626", label: "Aborted (silent)" },
  { color: "#f59e0b", label: "Failure (retry)" },
  { color: "#6b7280", label: "Skipped" },
];

export function Legend() {
  return (
    <div className="flex flex-shrink-0 flex-wrap gap-x-4 gap-y-1.5 border-b border-border bg-surface px-[1.4rem] py-2.5 text-[0.7rem]">
      {ITEMS.map((item) => (
        <span key={item.label} className="flex items-center gap-1.5 text-muted">
          <span
            className="h-[9px] w-[9px] shrink-0 rounded-full"
            style={{ background: item.color }}
          />
          {item.label}
        </span>
      ))}
      <span className="flex items-center gap-1.5 text-muted">
        <span className="h-[9px] w-[9px] shrink-0 rounded-[2px] bg-dark outline-dashed outline-2 outline-offset-1 outline-orange" />
        Click to navigate →
      </span>
    </div>
  );
}
