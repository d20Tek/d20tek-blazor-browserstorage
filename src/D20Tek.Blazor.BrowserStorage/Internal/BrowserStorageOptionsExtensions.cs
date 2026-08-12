namespace D20Tek.Blazor.BrowserStorage.Internal;

internal static class BrowserStorageOptionsExtensions
{
    public static string PrefixKey(this BrowserStorageOptions options, string key) =>
        $"{options.KeyPrefix}{key}";

    public static string StripPrefix(this BrowserStorageOptions options, string key) =>
        options.KeyPrefix.Length > 0 && key.StartsWith(options.KeyPrefix, StringComparison.Ordinal)
            ? key[options.KeyPrefix.Length..]
            : key;
}
