using System.Text.Json;

namespace D20Tek.Blazor.BrowserStorage;

/// <summary>
/// Configuration options for browser storage services.
/// </summary>
public sealed class BrowserStorageOptions
{
    /// <summary>
    /// Prefix prepended to all keys (e.g., "myapp_"). Default: empty string.
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>
    /// JSON serializer options for value serialization.
    /// Default: System.Text.Json web defaults (camelCase, case-insensitive reads).
    /// </summary>
    public JsonSerializerOptions? JsonOptions { get; set; }
}
