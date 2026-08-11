namespace D20Tek.Blazor.BrowserStorage;

/// <summary>
/// Represents the result of a storage read operation.
/// </summary>
/// <typeparam name="T">The type of the stored value.</typeparam>
/// <param name="Success">Indicates whether the key was found and deserialized successfully.</param>
/// <param name="Value">The deserialized value, or default if not found.</param>
public readonly record struct StorageResult<T>(bool Success, T? Value);
