namespace D20Tek.Blazor.BrowserStorage.Tests.Fakes;

[ExcludeFromCodeCoverage]
internal sealed class FakeJSRuntime : IJSRuntime
{
    private readonly List<(string Identifier, object?[] Args)> _invocations = [];

    public IReadOnlyList<(string Identifier, object?[] Args)> Invocations => _invocations;

    public Dictionary<string, object?> Results { get; } = [];

    public Dictionary<string, Exception> ExceptionForIdentifier { get; } = [];

    public FakeJSObjectReference? ModuleReference { get; set; }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        _invocations.Add((identifier, args ?? []));

        if (ExceptionForIdentifier.TryGetValue(identifier, out var ex))
        {
            throw ex;
        }

        if (identifier == "import" && ModuleReference is not null)
        {
            return ValueTask.FromResult((TValue)(object)ModuleReference);
        }

        // Storage availability probe: default to available unless a test overrides via Results["eval:probe"].
        if (identifier == "eval" && args is { Length: > 0 } &&
            args[0] is string script && script.Contains("__d20tek_probe__"))
        {
            if (Results.TryGetValue("eval:probe", out var probeResult))
            {
                return ValueTask.FromResult((TValue)probeResult!);
            }

            return ValueTask.FromResult((TValue)(object)true);
        }

        return Results.TryGetValue(identifier, out var result)
            ? ValueTask.FromResult((TValue)result!)
            : ValueTask.FromResult(default(TValue)!);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
        InvokeAsync<TValue>(identifier, args);
}
