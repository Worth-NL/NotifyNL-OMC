const CONNECTION_TYPES: { label: string; color: string }[] = [
  { label: "producten", color: "var(--color-arch-indigo)" },
  { label: "zaken", color: "var(--color-arch-blue)" },
  { label: "taken", color: "var(--color-arch-amber)" },
  { label: "besluiten", color: "var(--color-arch-emerald)" },
  { label: "verrijking", color: "var(--color-arch-teal)" },
  { label: "bevestiging", color: "var(--color-arch-violet)" },
];

export function ConnectionLegend() {
  return (
    <div className="flex flex-wrap items-center gap-x-4 gap-y-1.5 text-[0.68rem] text-arch-muted">
      {CONNECTION_TYPES.map((c) => (
        <span key={c.label} className="flex items-center gap-1.5">
          <span className="h-0.5 w-4 rounded-full" style={{ background: c.color }} />
          {c.label}
        </span>
      ))}
      <span className="flex items-center gap-1.5">
        <span className="h-1.5 w-1.5 rounded-full bg-arch-green" />
        actief
      </span>
      <span className="flex items-center gap-1.5">
        <span className="h-1.5 w-1.5 rounded-full bg-arch-faint" />
        inactief
      </span>
    </div>
  );
}
