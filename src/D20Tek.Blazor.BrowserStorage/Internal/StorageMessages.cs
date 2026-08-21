namespace D20Tek.Blazor.BrowserStorage.Internal;

internal static class StorageMessages
{
    public static string Unavailable(string storageName) => $"Browser {storageName} is not available.";

    public static string KeyNotFound(string storageName, string key) => $"Key '{key}' not found in {storageName}.";

    public static string WriteFailed(string storageName, string key, Exception ex) =>
        $"Failed to write value to {storageName} for key '{key}': {ex.Message}";

    public static string RemoveFailed(string storageName, string key, Exception ex) =>
        $"Failed to remove value from {storageName} for key '{key}': {ex.Message}";

    public static string ClearFailed(string storageName, Exception ex) =>
        $"Failed to clear {storageName}: {ex.Message}";
}
