namespace D20Tek.Blazor.BrowserStorage.Tests.Fakes;

[ExcludeFromCodeCoverage]
internal sealed class FakeJSRuntime : IJSRuntime
{
    private readonly List<(string Identifier, object?[] Args)> _invocations = [];

    public IReadOnlyList<(string Identifier, object?[] Args)> Invocations => _invocations;

    public Dictionary<string, object?> Results { get; } = [];

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        _invocations.Add((identifier, args ?? []));

        return Results.TryGetValue(identifier, out var result)
            ? ValueTask.FromResult((TValue)result!)
            : ValueTask.FromResult(default(TValue)!);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
        InvokeAsync<TValue>(identifier, args);
}
