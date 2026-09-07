// © 2026, Worth Systems.

using System.Collections.Concurrent;
using System.Threading.Channels;

namespace WebQueries.Tracing
{
    /// <summary>
    /// In-memory, ephemeral fan-out of <see cref="TraceEvent"/>s to any dashboard clients
    /// currently subscribed via SSE. No persistence, no database — events raised while nobody
    /// is listening are simply never built (see <see cref="HasSubscribers"/>) or dropped.
    /// Registered as a singleton; every member is safe for concurrent use from any request.
    /// </summary>
    public sealed class TraceEmitter
    {
        private readonly ConcurrentDictionary<Guid, Channel<TraceEvent>> _subscribers = new();

        /// <summary>
        /// Whether at least one dashboard client is currently listening. Callers on the hot
        /// request path should check this before building a <see cref="TraceEvent"/>, so tracing
        /// costs nothing when nobody is watching.
        /// </summary>
        public bool HasSubscribers
            => !this._subscribers.IsEmpty;

        /// <summary>
        /// Registers a new subscriber and returns its identifier plus a reader for its events.
        /// </summary>
        public (Guid Id, ChannelReader<TraceEvent> Reader) Subscribe()
        {
            // Bounded + drop-oldest: a slow or stalled dashboard tab can never grow this queue
            // without bound, and never blocks the notification pipeline that's writing to it.
            var channel = Channel.CreateBounded<TraceEvent>(new BoundedChannelOptions(256)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false,
            });

            Guid id = Guid.NewGuid();
            this._subscribers[id] = channel;

            return (id, channel.Reader);
        }

        /// <summary>
        /// Removes a subscriber (e.g. once its SSE connection closes).
        /// </summary>
        public void Unsubscribe(Guid id)
        {
            if (this._subscribers.TryRemove(id, out Channel<TraceEvent>? channel))
            {
                channel.Writer.TryComplete();
            }
        }

        /// <summary>
        /// Broadcasts an event to every current subscriber. Non-blocking and best-effort: this
        /// never awaits, never throws, and never delays the caller — the write either lands
        /// immediately or the event is dropped for that subscriber.
        /// </summary>
        public void Emit(TraceEvent traceEvent)
        {
            foreach (Channel<TraceEvent> channel in this._subscribers.Values)
            {
                channel.Writer.TryWrite(traceEvent);
            }
        }
    }
}
