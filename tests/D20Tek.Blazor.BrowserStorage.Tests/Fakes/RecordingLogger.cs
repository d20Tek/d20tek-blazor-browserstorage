namespace D20Tek.Blazor.BrowserStorage.Tests.Fakes;

internal sealed class RecordingLogger : ILogger
{
    public List<(LogLevel Level, string Message)> Entries { get; } = [];

    [ExcludeFromCodeCoverage]
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    [ExcludeFromCodeCoverage]
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Entries.Add((logLevel, formatter(state, exception)));
    }
}
