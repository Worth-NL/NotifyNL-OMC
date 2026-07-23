"use client";

import { useState } from "react";
import { TopBar } from "@/components/TopBar";
import { Sidebar } from "@/components/flow/Sidebar";
import { Legend } from "@/components/flow/Legend";
import { DiagramViewer } from "@/components/flow/DiagramViewer";
import { useScenarios } from "@/hooks/useScenarios";

export default function FlowPage() {
  const { scenarios, loading, error } = useScenarios();
  const [selectedKey, setSelectedKey] = useState<string | null>(null);

  // Default to the first scenario once loaded, without an extra render pass.
  const currentKey = selectedKey ?? scenarios[0]?.key ?? null;
  const current = scenarios.find((s) => s.key === currentKey) ?? null;

  return (
    <div className="flex h-screen flex-col">
      <TopBar links={[{ href: "/status", label: "← Configuration" }]} />

      <div className="flex h-[calc(100vh-56px)] flex-1 overflow-hidden">
        <Sidebar scenarios={scenarios} currentKey={currentKey} onSelect={setSelectedKey} />

        <div className="flex flex-1 flex-col overflow-hidden">
          <Legend />

          <div className="flex-shrink-0 border-b border-border bg-surface px-[1.4rem] pt-3.5 pb-2.5">
            <div className="mb-0.5 text-[0.62rem] font-bold tracking-[0.12em] text-orange uppercase">
              {current ? (current.channel === "overview" ? "overview" : `${current.channel} channel`) : "overview"}
            </div>
            <div className="text-[1.15rem] font-bold text-dark">{current?.name ?? "—"}</div>
            <div className="mt-0.5 text-[0.75rem] text-muted">
              {current?.desc ?? "Select a scenario from the list."}
            </div>
          </div>

          {loading && (
            <div className="flex items-center gap-2.5 p-12 text-[0.82rem] text-muted">
              <div className="h-4 w-4 animate-[spin_0.6s_linear_infinite] rounded-full border-2 border-border border-t-orange" />
              Initialising…
            </div>
          )}
          {error && <div className="p-12 text-[0.82rem] text-red">Failed to load scenarios: {error}</div>}
          {current && (
            <DiagramViewer key={current.key} scenario={current} onNavigate={setSelectedKey} />
          )}
        </div>
      </div>
    </div>
  );
}
