"use client";

import { TopBar } from "@/components/TopBar";
import { CheckGroupCard } from "@/components/status/CheckGroupCard";
import { useConfigChecks } from "@/hooks/useConfigChecks";
import { OMC_API_URL } from "@/lib/api";

export default function StatusPage() {
  const { groups, received, expectedTotal, summary, status } = useConfigChecks();

  const percent = expectedTotal ? Math.min(100, (received / expectedTotal) * 100) : 0;

  const progressLabel =
    status === "connecting"
      ? "Connecting…"
      : status === "error"
        ? "Connection lost — refresh to retry"
        : summary
          ? `All done — ${summary.passed} passed, ${summary.failed} failed`
          : `${received} checks complete…`;

  return (
    <>
      <TopBar
        links={[
          { href: "/status/flow", label: "Scenario flow →" },
          { href: `${OMC_API_URL}/swagger`, label: "API docs ↗", external: true },
        ]}
      />

      <div className="mx-auto max-w-[880px] px-6 pt-10 pb-16">
        <div className="mb-8">
          <div className="mb-1 text-[0.7rem] font-semibold tracking-[0.1em] text-orange uppercase">
            Configuration status
          </div>
          <h1 className="text-[1.75rem] leading-tight font-bold text-dark">
            OMC Configuration Check
          </h1>
          <p className="mt-1.5 text-[0.875rem] text-muted">
            Live status of all required settings and service connections. Secret values are
            never shown.
          </p>

          <div className="mt-6">
            <div className="h-[5px] overflow-hidden rounded-full bg-border-strong">
              <div
                className="h-full rounded-full bg-orange transition-[width] duration-300 ease-out"
                style={{ width: `${percent}%` }}
              />
            </div>
            <div className="mt-1.5 text-[0.78rem] font-medium text-muted">{progressLabel}</div>
          </div>
        </div>

        <div>
          {groups.map((group) => (
            <CheckGroupCard key={group.name} group={group} />
          ))}
        </div>

        {summary && (
          <div className="mt-6 rounded-xl border border-border bg-surface p-7 text-center shadow-[0_1px_4px_rgb(30_34_56_/_0.05)]">
            {summary.failed === 0 ? (
              <>
                <div className="mb-1.5 text-[1.1rem] font-bold text-green">
                  Everything looks good
                </div>
                <div className="text-[0.85rem] text-muted">
                  All required settings are present and services are reachable.
                </div>
              </>
            ) : (
              <>
                <div className="mb-1.5 text-[1.1rem] font-bold text-orange">
                  ⚠️ {summary.failed} issue{summary.failed !== 1 ? "s" : ""} found
                </div>
                <div className="text-[0.85rem] text-muted">
                  {summary.passed} of {summary.total} checks passed. Review the items marked ✗
                  above.
                </div>
              </>
            )}
          </div>
        )}
      </div>
    </>
  );
}
