namespace D20Tek.Blazor.BrowserStorage.Internal;

internal static class TrimmingMessages
{
    public const string RequiresUnreferencedCode =
        "JSON (de)serialization uses reflection to walk the properties of T for types that are not a supported primitive. " +
        "Members can be trimmed. Configure BrowserStorageOptions.JsonOptions.TypeInfoResolver with a source-generated " +
        "JsonSerializerContext (or a JsonTypeInfoResolver) to preserve types under trimming.";

    public const string RequiresDynamicCode =
        "JSON (de)serialization may require runtime code generation for types not covered by the configured JsonTypeInfoResolver. " +
        "Configure BrowserStorageOptions.JsonOptions.TypeInfoResolver with a source-generated JsonSerializerContext to support AOT.";
}
