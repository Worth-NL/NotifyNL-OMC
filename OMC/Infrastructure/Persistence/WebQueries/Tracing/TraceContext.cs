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
        public static void EmitPendingFailure()
        {
            if (s_current.Value is { LastStatus: "start", LastStage: { } stage })
            {
                Emit(stage, "fail");
            }
        }

        /// <summary>
        /// Ends the current trace. Call in a <see langword="finally"/> block around processing
        /// so the ambient context never leaks into unrelated work on a reused async context.
        /// </summary>
        public static void Clear()
            => s_current.Value = null;
    }
}
