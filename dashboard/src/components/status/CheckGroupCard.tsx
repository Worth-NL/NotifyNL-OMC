"use client";

import { useState } from "react";
import { CheckGroup } from "@/hooks/useConfigChecks";

export function CheckGroupCard({ group }: { group: CheckGroup }) {
  const [open, setOpen] = useState(true);
  const total = group.ok + group.fail;
  const badgeClass =
    group.fail > 0
      ? "bg-red-bg text-red border border-red-border"
      : "bg-green-bg text-green border border-green-border";

  return (
    <div className="mb-3 overflow-hidden rounded-xl border border-border bg-surface shadow-[0_1px_4px_rgb(30_34_56_/_0.05)]">
      <button
        type="button"
        onClick={() => setOpen((o) => !o)}
        className="flex w-full items-center gap-2.5 px-5 py-3.5 text-left transition hover:bg-bg"
      >
        <span className="text-base">{group.icon}</span>
        <span className="flex-1 text-[0.875rem] font-semibold text-dark">{group.name}</span>
        <span className={`rounded-full px-2.5 py-0.5 text-[0.7rem] font-semibold whitespace-nowrap ${badgeClass}`}>
          {group.fail > 0 ? `${group.ok}/${total} — ${group.fail} missing` : `${total}/${total} ✓`}
        </span>
      </button>
      {open && (
        <div>
          {group.checks.map((check, i) => (
            <div
              key={`${check.name}-${i}`}
              className="flex items-start border-t border-border px-5 py-2.5 text-[0.825rem] [animation:fade-in_0.15s_ease]"
            >
              <i
                className={`w-5 shrink-0 pt-px text-base leading-none font-bold not-italic ${
                  check.ok ? "text-green" : "text-red"
                }`}
              >
                {check.ok ? "✓" : "✗"}
              </i>
              <div className="min-w-0 flex-1">
                <div className="flex items-baseline">
                  <span className="flex-1 pr-3 font-medium whitespace-nowrap text-charcoal">
                    {check.name}
                  </span>
                  {check.ok && check.detail && (
                    <span className="max-w-[380px] overflow-hidden font-mono text-[0.75rem] text-ellipsis whitespace-nowrap text-green font-medium">
                      {check.detail}
                    </span>
                  )}
                </div>
                {!check.ok && check.detail && (
                  <span className="mt-1 block max-h-32 overflow-y-auto rounded-md border border-red-border bg-red-bg px-2.5 py-1.5 font-mono text-[0.75rem] break-all whitespace-pre-wrap text-red">
                    {check.detail}
                  </span>
                )}
                {!check.ok && check.hint && (
                  <div className="mt-1 font-mono text-[0.7rem] font-medium text-orange before:text-muted before:font-normal before:content-['→_set:_']">
                    {check.hint}
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
