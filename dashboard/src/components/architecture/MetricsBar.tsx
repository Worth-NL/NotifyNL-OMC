"use client";

function Sparkline({ values }: { values: number[] }) {
  const width = 96;
  const height = 28;
  const max = Math.max(...values, 1);
  const min = Math.min(...values, 0);
  const range = max - min || 1;

  const points = values
    .map((v, i) => {
      const x = (i / (values.length - 1)) * width;
      const y = height - ((v - min) / range) * height;
      return `${x.toFixed(1)},${y.toFixed(1)}`;
    })
    .join(" ");

  return (
    <svg width={width} height={height} viewBox={`0 0 ${width} ${height}`} className="shrink-0">
      <polyline points={points} fill="none" stroke="var(--color-arch-amber)" strokeWidth="1.5" />
    </svg>
  );
}

export function MetricsBar({
  environment,
  onEnvironmentChange,
  load,
  avgHandlingMs,
  throughputPerSec,
  sparkline,
}: {
  environment: "productie" | "test";
  onEnvironmentChange: (env: "productie" | "test") => void;
  load: number;
  avgHandlingMs: number;
  throughputPerSec: number;
  sparkline: number[];
}) {
  return (
    <div className="flex flex-wrap items-center gap-6 rounded-lg border border-arch-border bg-arch-surface px-4 py-2.5">
      <div>
        <div className="mb-1 text-[0.6rem] font-semibold tracking-wide text-arch-faint uppercase">
          Omgeving
        </div>
        <div className="flex overflow-hidden rounded-md border border-arch-border">
          {(["productie", "test"] as const).map((env) => (
            <button
              key={env}
              type="button"
              onClick={() => onEnvironmentChange(env)}
              className={`px-2.5 py-1 text-[0.68rem] font-semibold capitalize transition-colors ${
                environment === env
                  ? "bg-arch-teal text-white"
                  : "bg-arch-surface text-arch-muted hover:bg-arch-bg"
              }`}
            >
              {env}
            </button>
          ))}
        </div>
      </div>

      <div>
        <div className="mb-1 text-[0.6rem] font-semibold tracking-wide text-arch-faint uppercase">
          Belasting OMC
        </div>
        <div className="flex items-center gap-2">
          <div className="h-1.5 w-24 overflow-hidden rounded-full bg-arch-bg">
            <div
              className="h-full rounded-full bg-arch-amber transition-[width] duration-500"
              style={{ width: `${load}%` }}
            />
          </div>
          <span className="text-[0.72rem] font-semibold text-arch-ink">{Math.round(load)}%</span>
        </div>
      </div>

      <div>
        <div className="mb-1 text-[0.6rem] font-semibold tracking-wide text-arch-faint uppercase">
          Gem. afhandeltijd
        </div>
        <span className="text-[0.78rem] font-semibold text-arch-ink">{avgHandlingMs} ms</span>
      </div>

      <div>
        <div className="mb-1 text-[0.6rem] font-semibold tracking-wide text-arch-faint uppercase">
          Berichtenflow
        </div>
        <span className="text-[0.78rem] font-semibold text-arch-ink">{throughputPerSec} /s</span>
      </div>

      <div className="ml-auto flex items-center gap-2">
        <span className="text-[0.6rem] font-semibold tracking-wide text-arch-faint uppercase">
          Belasting — laatste minuut
        </span>
        <Sparkline values={sparkline} />
      </div>
    </div>
  );
}
