# OMC Dashboard

Next.js app serving the `/status` (configuration health check) and `/status/flow` (scenario
flow viewer) pages. Both pages are client-only — no server-side rendering or API routes — and
talk to the OMC .NET API over `fetch`/`EventSource`.

In production this app is **not** run as its own server. It's statically exported
(`output: 'export'` in `next.config.ts`) and the OMC Dockerfile bakes the exported files
straight into the API's own image (`wwwroot/`), which serves them via `UseStaticFiles()`. One
container, one deployment — exactly what's already shipped today, just with a richer dashboard.

## Running via `dotnet build` / `dotnet run` / Visual Studio F5

**Just works, first try, no setup.** Building `OMC.EventsHandler` (a plain `dotnet build`,
`dotnet run`, or pressing F5 in Visual Studio) automatically runs `npm run build:omc` the first
time `wwwroot/` doesn't exist, and copies the result in — see the `BuildDashboardIfMissing`
target in `EventsHandler.csproj`. After that first build, `/status` and `/status/flow` serve the
real dashboard, same as production.

This only runs **once** — subsequent builds skip it (wwwroot already exists), so it doesn't slow
down day-to-day API development. If you never touch the dashboard, you'll never notice this step
beyond a few extra seconds on your very first build after pulling this change.

If Node/npm isn't installed, or the npm build fails for any reason, the .NET build still
succeeds (just a warning) — `/status` and `/status/flow` then redirect to `/swagger` instead of
erroring, so nothing about API development is ever blocked by this.

To skip the auto-build deliberately (e.g. you know you don't want it and don't want the wait):

```bash
dotnet build -p:SkipDashboardBuild=true
```

To force a rebuild after changing dashboard source (delete `wwwroot/` and build again triggers
it), or just run the npm script directly:

```bash
cd dashboard && npm run build:omc
```

CI never runs this — the Docker build handles the dashboard in its own Node stage instead (see
the Dockerfile), and the auto-build target is skipped whenever `CI=true` (set automatically by
GitHub Actions).

## Running for frontend development (CLI, with hot reload)

For actually iterating on the dashboard UI, run it as a normal separate dev server instead —
the static export above is a snapshot, not something F5 rebuilds live.

```bash
# Terminal 1 — from OMC/Infrastructure/WebApi/EventsHandler
dotnet run --urls http://localhost:5270

# Terminal 2 — from dashboard/
cp .env.local.example .env.local   # first time only
npm install                        # first time only
npm run dev
```

Open `http://localhost:3000/status`. Hot reload works as usual. `.env.local` points the
dashboard at the API on a different port — CORS for this is already configured on the API side
(`DASHBOARD_ORIGINS`, defaults to `http://localhost:3000`).
