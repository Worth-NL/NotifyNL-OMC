// Empty string = same-origin relative requests, which is what production
// wants (this app is statically exported and served by the OMC API itself).
// Local dev overrides this via .env.local to point at a separately running API.
export const OMC_API_URL = process.env.NEXT_PUBLIC_OMC_API_URL ?? "";

export interface CheckResult {
  group: string;
  icon: string;
  name: string;
  ok: boolean;
  detail?: string;
  hint?: string;
}

export interface StreamStart {
  total: number;
}

export interface StreamComplete {
  total: number;
  passed: number;
  failed: number;
}

export interface ScenarioFlow {
  key: string;
  icon: string;
  name: string;
  nl: string;
  channel: "zaken" | "objecten" | "besluiten" | "overview";
  desc: string;
  diagram: string;
}

export async function fetchScenarios(): Promise<ScenarioFlow[]> {
  const res = await fetch(`${OMC_API_URL}/status/scenarios`);
  if (!res.ok) {
    throw new Error(`Failed to load scenarios (${res.status})`);
  }
  return res.json();
}

// Structural only — matches WebQueries.Tracing.TraceEvent. Never carries case identifiers,
// BSNs, or other citizen data; see the comment on that record for why.
export interface TraceEvent {
  traceId: string;
  stage: string;
  status: "start" | "ok" | "fail" | "abort" | "pending";
  scenario: string | null;
  detail: string | null;
  elapsedMs: number;
}

export const TRACE_STREAM_URL = `${OMC_API_URL}/status/trace/stream`;
