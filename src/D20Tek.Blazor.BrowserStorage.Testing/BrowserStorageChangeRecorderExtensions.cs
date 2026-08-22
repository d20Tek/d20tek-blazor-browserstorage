namespace D20Tek.Blazor.BrowserStorage.Testing;

/// <summary>
/// Test helpers for capturing <see cref="IBrowserStorageService.Changed"/> events raised during a
/// test's act phase, without the per-test boilerplate of hand-rolling a list plus subscribe /
/// unsubscribe calls.
/// </summary>
public static class BrowserStorageChangeRecorderExtensions
{
    /// <summary>
    /// Starts recording <see cref="IBrowserStorageService.Changed"/> events raised by
    /// <paramref name="service"/>. The captured events are exposed through <paramref name="events"/>
    /// as a thread-safe, read-only list that is populated live. Dispose the returned handle
    /// (or leave it to a <c>using</c> block) to stop recording.
    /// </summary>
    /// <param name="service">The storage service to observe.</param>
    /// <param name="events">Receives the live list of captured events.</param>
    /// <returns>An <see cref="IDisposable"/> that unsubscribes when disposed.</returns>
    public static IDisposable RecordChanges(this IBrowserStorageService service, out IReadOnlyList<StorageChangedEventArgs> events)
    {
        ArgumentNullException.ThrowIfNull(service);

        var recorder = new ChangeRecorder(service);
        events = recorder.Events;
        return recorder;
    }

    private sealed class ChangeRecorder : IDisposable
    {
        private readonly IBrowserStorageService _service;
        private readonly List<StorageChangedEventArgs> _events = [];
        private readonly Lock _sync = new();
        private int _disposed;

        public ChangeRecorder(IBrowserStorageService service)
        {
            _service = service;
            _service.Changed += OnChanged;
        }

        public IReadOnlyList<StorageChangedEventArgs> Events => new SynchronizedSnapshotList(_events, _sync);

        private void OnChanged(object? sender, StorageChangedEventArgs e)
        {
            lock (_sync) _events.Add(e);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _service.Changed -= OnChanged;
            }
        }

        private sealed class SynchronizedSnapshotList(List<StorageChangedEventArgs> source, Lock sync)
            : IReadOnlyList<StorageChangedEventArgs>
        {
            public StorageChangedEventArgs this[int index]
            {
                get { lock (sync) return source[index]; }
            }

            public int Count
            {
                get { lock (sync) return source.Count; }
            }

            public IEnumerator<StorageChangedEventArgs> GetEnumerator()
            {
                StorageChangedEventArgs[] snapshot;
                lock (sync) snapshot = [.. source];
                return ((IEnumerable<StorageChangedEventArgs>)snapshot).GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
