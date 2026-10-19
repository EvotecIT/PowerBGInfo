using System.Threading;

namespace PowerBGInfo;

public static partial class SystemInfoProvider {
    /// <summary>Caches a completed fact read, while leaving unavailable results eligible for recovery.</summary>
    private sealed class FactCache<T> where T : class {
        private readonly Func<T> _load;
        private readonly Func<T, bool> _available;
        private T? _value;

        internal FactCache(Func<T> load, Func<T, bool> available) {
            _load = load;
            _available = available;
        }

        internal T Value {
            get {
                var cached = Volatile.Read(ref _value);
                if (cached != null) return cached;
                // The shared query executor bounds provider concurrency and caller deadlines;
                // do not hold a cache lock across an external probe.
                var value = _load();
                return _available(value) ? Interlocked.CompareExchange(ref _value, value, null) ?? value : value;
            }
        }
    }
}
