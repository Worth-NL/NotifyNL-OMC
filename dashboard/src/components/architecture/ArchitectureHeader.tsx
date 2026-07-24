export function ArchitectureHeader({
  isTracing,
  onToggleTrace,
  onReset,
}: {
  isTracing: boolean;
  onToggleTrace: () => void;
  onReset: () => void;
}) {
  return (
    <div className="flex flex-wrap items-start justify-between gap-4">
      <div className="max-w-2xl">
        <div className="mb-1 text-[0.62rem] font-bold tracking-[0.12em] text-arch-teal uppercase">
          Output Management Component · Runtime
        </div>
        <h1 className="text-[1.4rem] font-bold text-arch-ink">
          Patroongestuurde notificatie- &amp; berichtenstroom
        </h1>
        <p className="mt-1 text-[0.78rem] leading-relaxed text-arch-muted">
          Het OMC handelt <strong className="text-arch-ink">single-shot, direct af te handelen</strong>{" "}
          berichtstromen af — geen langlopende workflow, maar één gebeurtenis → één bericht —
          binnen een <strong className="text-arch-ink">event-gedreven</strong> landschap. Zo richt
          je <strong className="text-arch-ink">proactieve, gepersonaliseerde</strong>{" "}
          communicatiepatronen in binnen een overheidsorganisatie: van invoerbron via de
          patroon-engine, verrijkt uit registers, langs kanaalkeuze en filters naar uitvoer en
          afleverbevestiging.
        </p>
      </div>

      <div className="flex shrink-0 gap-2">
        <button
          type="button"
          onClick={onToggleTrace}
          className={`rounded-md px-3 py-1.5 text-[0.72rem] font-semibold transition-colors ${
            isTracing
              ? "bg-arch-teal text-white"
              : "border border-arch-teal text-arch-teal hover:bg-arch-teal-light"
          }`}
        >
          {isTracing ? "Pauzeer trace" : "Trace starten"}
        </button>
        <button
          type="button"
          onClick={onReset}
          className="rounded-md border border-arch-border px-3 py-1.5 text-[0.72rem] font-semibold text-arch-muted transition-colors hover:bg-arch-bg"
        >
          Herstel
        </button>
      </div>
    </div>
  );
}
