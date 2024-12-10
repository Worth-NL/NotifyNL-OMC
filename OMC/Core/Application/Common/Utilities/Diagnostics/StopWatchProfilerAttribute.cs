// © 2023, Worth Systems.

using PostSharp.Aspects;
using PostSharp.Serialization;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Common.Utilities.Diagnostics
{
    /// <summary>
    /// The runtime profiler, used to measure how long does a specific method is executing.
    /// </summary>
    /// <seealso cref="OnMethodBoundaryAspect" />
    [PSerializable]
    [ExcludeFromCodeCoverage(Justification = "This is utility class, relying on third-party dependency.")]
    internal sealed class StopWatchProfilerAttribute : OnMethodBoundaryAspect
    {
        private Stopwatch? _stopwatch;

        /// <inheritdoc cref="OnMethodBoundaryAspect.OnEntry(MethodExecutionArgs)"/>
        public override void OnEntry(MethodExecutionArgs args)
        {
            this._stopwatch = new Stopwatch();
            this._stopwatch.Start();
        }

        /// <inheritdoc cref="OnMethodBoundaryAspect.OnExit(MethodExecutionArgs)"/>
        public override void OnExit(MethodExecutionArgs args)
        {
            this._stopwatch!.Stop();

            Console.WriteLine($@"{this._stopwatch.Elapsed.TotalMilliseconds} ms");
        }
    }
}