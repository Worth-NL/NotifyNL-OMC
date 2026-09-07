# OMC Dashboard

Next.js app serving the `/status` (configuration health check) and `/status/flow` (scenario
flow viewer) pages. Both pages are client-only — no server-side rendering or API routes — and
talk to the OMC .NET API over `fetch`/`EventSource`.

In production this app is **not** run as its own server. It's statically exported
(`output: 'export'` in `next.config.ts`) and the OMC Dockerfile bakes the exported files
straight into the API's own image (`wwwroot/`), which serves them via `UseStaticFiles()`. One
container, one deployment — exactly what's already shipped today, just with a richer dashboard.

## Node.js/npm — only required if you want the status page to work locally

**Not required for OMC/API development in general.** The API builds, runs, and debugs completely
normally without Node.js installed — `/status` and `/status/flow` just redirect to `/swagger`
instead of showing the dashboard (see below). Only install this if you specifically want to see
or work on the dashboard itself.

If a build shows `'npm' is not recognized`, that's expected until you install Node.js:

1. Download the **LTS** installer from [nodejs.org](https://nodejs.org) and run it (default
   options are fine — it installs `npm` and adds both to your `PATH` automatically).
2. **Restart Visual Studio / any open terminals.** This is the step people miss — apps already
   running when Node was installed don't see the updated `PATH` until restarted.
3. Verify in a *new* terminal: `node --version` and `npm --version` should both print a version.

That's it — no separate npm install, no global packages. The next build/F5 will pick it up.

## Running via `dotnet build` / `dotnet run` / Visual Studio F5

**Just works, first try, no setup.** Building `OMC.EventsHandler` (a plain `dotnet build`,
`dotnet run`, or pressing F5 in Visual Studio) automatically runs `npm run build:omc` the first
time `wwwroot/` doesn't exist, and copies the result in — see the `BuildDashboardIfMissing`
target in `EventsHandler.csproj`. After that first build, `/status` and `/status/flow` serve the
real dashboard, same as production.

This only runs **once** — subsequent builds skip it (wwwroot already exists), so it doesn't slow
down day-to-day API development. If you never touch the dashboard, you'll never notice this step
beyond a few extra seconds on your very first build after pulling this change.

**Visual Studio note:** if `wwwroot/` is missing but F5 shows an empty Output window and no
dashboard, that's Visual Studio's Fast Up-to-Date Check skipping MSBuild entirely (it doesn't
know this target's `!Exists('wwwroot')` condition, so if your last build's outputs still look
current to it, it launches the app directly without building). Use **Rebuild** instead of F5 —
that always forces a real MSBuild pass. This mostly won't affect a genuinely fresh clone, since
there's no prior build output for the check to compare against, so the first F5 there should
trigger a real build regardless.

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
