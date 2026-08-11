namespace D20Tek.Blazor.BrowserStorage;

/// <summary>
/// Event arguments for storage change notifications.
/// </summary>
/// <param name="key">The storage key that changed.</param>
/// <param name="oldValue">The previous value, or null if the key was new.</param>
/// <param name="newValue">The new value, or null if the key was removed.</param>
public sealed class StorageChangedEventArgs(string key, object? oldValue, object? newValue) : EventArgs
{
    /// <summary>
    /// Gets the storage key that changed.
    /// </summary>
    public string Key { get; } = key;

    /// <summary>
    /// Gets the previous value, or null if the key was new.
    /// </summary>
    public object? OldValue { get; } = oldValue;

    /// <summary>
    /// Gets the new value, or null if the key was removed.
    /// </summary>
    public object? NewValue { get; } = newValue;
}
