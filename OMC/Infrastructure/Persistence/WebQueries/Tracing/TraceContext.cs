// © 2026, Worth Systems.

using System.Diagnostics;

namespace WebQueries.Tracing
{
    /// <summary>
    /// Ambient per-notification trace state, flowing through the same async call chain as the
    /// notification being processed without being threaded through every method signature. Lets
    /// deeply-nested and <see langword="static"/> code (e.g. <c>BaseScenario</c>'s shared
    /// validation helpers, or a scenario's own register calls) report a step without any of
    /// those call sites taking a dependency on tracing.
    /// </summary>
    public static class TraceContext
    {
        private static readonly AsyncLocal<State?> s_current = new();

        private sealed class State
        {
            public required string TraceId { get; init; }
            public required Stopwatch Stopwatch { get; init; }
            public required TraceEmitter Emitter { get; init; }
            public string? Scenario { get; set; }
            public string? LastStage { get; set; }
            public string? LastStatus { get; set; }
            /// <summary>
            /// Distinct stages reached so far, in visiting order (consecutive repeats of the same
            /// stage collapsed to one entry) — only <see cref="ReturnAlongPath"/> reads this; every
            /// other pipeline can ignore it entirely.
            /// </summary>
            public List<string> Path { get; } = [];
        }

        /// <summary>
        /// Starts a new trace for the current async call chain. Call once per incoming
        /// notification (skip test pings), before any scenario logic runs.
        /// </summary>
        /// <returns>The generated trace identifier.</returns>
        public static string Start(TraceEmitter emitter)
        {
            string traceId = Guid.NewGuid().ToString("N")[..12];

            s_current.Value = new State
            {
                TraceId = traceId,
                Stopwatch = Stopwatch.StartNew(),
                Emitter = emitter,
            };

            return traceId;
        }

        /// <summary>
        /// Records which scenario is handling the current trace. Included on every subsequent
        /// event so the dashboard knows which flow to animate.
        /// </summary>
        public static void SetScenario(string scenarioKey)
        {
            if (s_current.Value is { } state)
            {
                state.Scenario = scenarioKey;
            }
        }

        /// <summary>
        /// The identifier of the trace currently flowing through this async call chain, or
        /// <see langword="null"/> outside <see cref="Start"/>/<see cref="Clear"/>. Lets code that
        /// hands work off to something asynchronous (e.g. a "Notify NL" send, whose delivery
        /// confirmation arrives on a later, unrelated request) stash this trace's identity so
        /// that later event can still be attributed to it.
        /// </summary>
        public static string? CurrentTraceId
            => s_current.Value?.TraceId;

        /// <summary>
        /// Emits a step for the current trace. A no-op if no trace is active (e.g. a test ping,
        /// or code running outside <see cref="Start"/>/<see cref="Clear"/>) or nobody is
        /// subscribed to the stream.
        /// </summary>
        public static void Emit(string stage, string status, string? detail = null)
        {
            State? state = s_current.Value;
            if (state is null)
            {
                return;
            }

            state.LastStage = stage;
            state.LastStatus = status;

            // Only terminal outcomes count as "reached" for path-retracing purposes — "start"
            // marks an attempt, not an arrival — and consecutive repeats of the same stage (e.g.
            // three separate "openzaak" round trips in a row) collapse to a single hop, matching
            // how the dashboard already treats a register as one visual node regardless of how
            // many individual calls land on it.
            if (status != "start" && (state.Path.Count == 0 || state.Path[^1] != stage))
            {
                state.Path.Add(stage);
            }

            if (!state.Emitter.HasSubscribers)
            {
                return;
            }

            state.Emitter.Emit(new TraceEvent(
                TraceId: state.TraceId,
                Stage: stage,
                Status: status,
                Scenario: state.Scenario,
                Detail: detail,
                ElapsedMs: state.Stopwatch.ElapsedMilliseconds));
        }

        /// <summary>
        /// If the last-reported step for the current trace was a "start" that never reached a
        /// terminal status (an exception propagated out of the register/gate call that would
        /// have completed it — most call sites don't wrap every individual call in try/catch),
        /// emits a synthetic "fail" for that same stage so the trace never just goes silent.
        /// Call from the outermost catch block around notification processing.
        /// </summary>
        /// <param name="detail">
        /// The exception message explaining why processing failed — the same text already
        /// returned to the API caller in the response body, so surfacing it here doesn't
        /// disclose anything beyond what already crosses the system boundary.
        /// </param>
        public static void EmitPendingFailure(string? detail = null)
        {
            if (s_current.Value is { LastStatus: "start", LastStage: { } stage })
            {
                Emit(stage, "fail", detail);
            }
        }

        /// <summary>
        /// Emits a synthetic return trip back through every stage this trace actually reached, in
        /// reverse order, before finally emitting <paramref name="finalStage"/> — for pipelines
        /// where a single incoming request produces a single outgoing HTTP response that
        /// conceptually flows all the way back to the original caller (MijnZaken, forwarded to
        /// Logius and responded to Oneground), rather than ending at a one-way terminal step the
        /// way a Notify NL send does. Without this, <c>findTracePath</c> would just take whatever
        /// shortcut edge happens to exist straight back to the entry point instead of retracing
        /// the real path — technically a valid route on the graph, but not what actually happened.
        /// </summary>
        public static void ReturnAlongPath(string finalStage, string finalStatus, string? finalDetail = null)
        {
            if (s_current.Value is not { } state)
            {
                return;
            }

            // Snapshot before replaying — Emit() below appends to state.Path as each hop plays
            // back, so indexing into the live list while it grows would drift.
            string[] forwardPath = [.. state.Path];

            // forwardPath[0] is the entry stage, forwardPath[^1] is wherever the trace currently
            // sits — walk everything strictly between them, in reverse, then land on finalStage
            // (normally the same stage as forwardPath[0], closing the loop).
            for (int i = forwardPath.Length - 2; i >= 1; i--)
            {
                Emit(forwardPath[i], "ok", $"Response returning via {forwardPath[i]}");
            }

            Emit(finalStage, finalStatus, finalDetail);
        }

        /// <summary>
        /// Ends the current trace. Call in a <see langword="finally"/> block around processing
        /// so the ambient context never leaks into unrelated work on a reused async context.
        /// </summary>
        public static void Clear()
            => s_current.Value = null;
    }
}
