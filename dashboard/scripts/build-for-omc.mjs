// Builds the static export and copies it into the OMC API project's wwwroot, so
// `dotnet run` / Visual Studio F5 serves the real dashboard at /status instead of
// falling back to /swagger. Run via `npm run build:omc`.

import { execFileSync } from "node:child_process";
import { existsSync, rmSync, mkdirSync, cpSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, resolve } from "node:path";

const __dirname = dirname(fileURLToPath(import.meta.url));
const dashboardDir = resolve(__dirname, "..");
const outDir = resolve(dashboardDir, "out");
const wwwroot = resolve(
  dashboardDir,
  "..",
  "OMC",
  "Infrastructure",
  "WebApi",
  "EventsHandler",
  "wwwroot",
);

// This build must produce same-origin (relative) API URLs — see src/lib/api.ts — because
// it's served by the OMC API itself in production. Explicitly clear the var here so a
// developer's dashboard/.env.local (used for `npm run dev` against a separately running
// API on another port) can never leak an absolute URL into this build.
execFileSync("npx", ["next", "build"], {
  cwd: dashboardDir,
  stdio: "inherit",
  shell: true,
  env: { ...process.env, NEXT_PUBLIC_OMC_API_URL: "" },
});

if (!existsSync(outDir)) {
  console.error("dashboard/out not found — did `next build` fail above?");
  process.exit(1);
}

rmSync(wwwroot, { recursive: true, force: true });
mkdirSync(wwwroot, { recursive: true });
cpSync(outDir, wwwroot, { recursive: true });

console.log(`Copied static export into ${wwwroot}`);
console.log("Run the OMC API (dotnet run, or F5 in Visual Studio) and visit /status.");
