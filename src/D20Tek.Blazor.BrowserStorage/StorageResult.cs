namespace D20Tek.Blazor.BrowserStorage;

/// <summary>
/// Represents the result of a storage read operation.
/// </summary>
/// <typeparam name="T">The type of the stored value.</typeparam>
/// <param name="IsSuccess">Indicates whether the key was found and deserialized successfully.</param>
/// <param name="Value">The deserialized value, or default if not found.</param>
/// <param name="ErrorMessage">An optional description of the failure, when <paramref name="IsSuccess"/> is false.</param>
public readonly record struct StorageResult<T>(bool IsSuccess, T? Value, string? ErrorMessage = null)
{
    /// <summary>Creates a successful result with no error message.</summary>
    public static StorageResult<T> Success(T? value) => new(true, value);

    /// <summary>Creates a failure result with the specified error message.</summary>
    public static StorageResult<T> Failure(string errorMessage) => new(false, default, errorMessage);
}

/// <summary>
/// Represents the result of a storage write, remove, or clear operation.
/// </summary>
/// <param name="IsSuccess">Indicates whether the operation succeeded.</param>
/// <param name="ErrorMessage">An optional description of the failure, when <paramref name="IsSuccess"/> is false.</param>
public readonly record struct StorageResult(bool IsSuccess, string? ErrorMessage = null)
{
    /// <summary>Creates a successful result with no error message.</summary>
    public static StorageResult Success() => new(true);

    /// <summary>Creates a failure result with the specified error message.</summary>
    public static StorageResult Failure(string errorMessage) => new(false, errorMessage);
}
