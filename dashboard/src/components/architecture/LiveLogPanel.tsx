import { TraceLogLine } from "@/hooks/useTraceStream";

const STATUS_LABEL: Record<TraceLogLine["status"], string> = {
  start: "start",
  ok: "ok",
  fail: "fout",
  abort: "afgebroken",
};

function isError(status: TraceLogLine["status"]): boolean {
  return status === "fail" || status === "abort";
}

export function LiveLogPanel({ log, connected }: { log: TraceLogLine[]; connected: boolean }) {
  return (
    <div className="rounded-lg border border-arch-border bg-arch-surface">
      <div className="flex items-center justify-between border-b border-arch-border px-4 py-2">
        <span className="text-[0.72rem] font-bold text-arch-ink">Live trace-log</span>
        <span className="flex items-center gap-1.5 text-[0.65rem] font-medium text-arch-muted">
          <span className={`h-1.5 w-1.5 rounded-full ${connected ? "bg-arch-green" : "bg-arch-faint"}`} />
          {connected ? "Verbonden" : "Niet verbonden"}
        </span>
      </div>

      <div className="max-h-56 overflow-y-auto px-4 py-2 font-mono text-[0.68rem] leading-relaxed">
        {log.length === 0 ? (
          <p className="py-4 text-center text-arch-faint">
            {connected
              ? "Verbonden — wachtend op een binnenkomende notificatie…"
              : "Start een trace om live verkeer te volgen."}
          </p>
        ) : (
          log.map((line) => (
            <div key={line.id} className={`flex flex-wrap gap-2 ${isError(line.status) ? "text-red" : "text-arch-ink"}`}>
              <span className="shrink-0 text-arch-faint">{line.time}</span>
              <span className="shrink-0 text-arch-muted">#{line.traceId.slice(0, 8)}</span>
              {line.scenario && <span className="shrink-0 text-arch-teal">{line.scenario}</span>}
              <span className="shrink-0 font-semibold">{line.stage}</span>
              <span className={isError(line.status) ? "font-semibold" : "text-arch-muted"}>{STATUS_LABEL[line.status]}</span>
              {line.detail && <span className="text-arch-faint">({line.detail})</span>}
            </div>
          ))
        )}
      </div>
    </div>
  );
}
