namespace D20Tek.Blazor.BrowserStorage.Tests;

[TestClass]
[DoNotParallelize]
public class StorageResultLogIfFailureTests
{
    private static string CaptureConsoleOutput(Action action)
    {
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            action();
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        return writer.ToString();
    }

    // ---------- StorageResult<T>.LogIfFailure ----------

    [TestMethod]
    public void Generic_LogIfFailure_Success_WithLogger_DoesNotLog()
    {
        // Arrange
        var logger = new RecordingLogger();
        var result = StorageResult<int>.Success(42);

        // Act
        result.LogIfFailure(logger);

        // Assert
        Assert.IsEmpty(logger.Entries);
    }

    [TestMethod]
    public void Generic_LogIfFailure_Success_WithoutLogger_DoesNotWriteToConsole()
    {
        // Arrange
        var result = StorageResult<string>.Success("value");

        // Act
        var output = CaptureConsoleOutput(() => result.LogIfFailure());

        // Assert
        Assert.AreEqual(string.Empty, output);
    }

    [TestMethod]
    public void Generic_LogIfFailure_Failure_WithLogger_LogsError()
    {
        // Arrange
        var logger = new RecordingLogger();
        var result = StorageResult<int>.Failure("something went wrong");

        // Act
        result.LogIfFailure(logger);

        // Assert
        Assert.HasCount(1, logger.Entries);
        Assert.AreEqual(LogLevel.Error, logger.Entries[0].Level);
        Assert.AreEqual("something went wrong", logger.Entries[0].Message);
    }

    [TestMethod]
    public void Generic_LogIfFailure_Failure_WithoutLogger_WritesErrorToConsole()
    {
        // Arrange
        var result = StorageResult<int>.Failure("boom");

        // Act
        var output = CaptureConsoleOutput(() => result.LogIfFailure());

        // Assert
        Assert.Contains("boom", output);
    }

    [TestMethod]
    public void Generic_LogIfFailure_Failure_WithoutLogger_NullMessage_WritesBlankLineToConsole()
    {
        // Arrange
        var result = new StorageResult<int>(false, default, null);

        // Act
        var output = CaptureConsoleOutput(() => result.LogIfFailure());

        // Assert
        Assert.AreEqual(Environment.NewLine, output);
    }

    // ---------- StorageResult.LogIfFailure ----------

    [TestMethod]
    public void NonGeneric_LogIfFailure_Success_WithLogger_DoesNotLog()
    {
        // Arrange
        var logger = new RecordingLogger();
        var result = StorageResult.Success();

        // Act
        result.LogIfFailure(logger);

        // Assert
        Assert.IsEmpty(logger.Entries);
    }

    [TestMethod]
    public void NonGeneric_LogIfFailure_Success_WithoutLogger_DoesNotWriteToConsole()
    {
        // Arrange
        var result = StorageResult.Success();

        // Act
        var output = CaptureConsoleOutput(() => result.LogIfFailure());

        // Assert
        Assert.AreEqual(string.Empty, output);
    }

    [TestMethod]
    public void NonGeneric_LogIfFailure_Failure_WithLogger_LogsError()
    {
        // Arrange
        var logger = new RecordingLogger();
        var result = StorageResult.Failure("write failed");

        // Act
        result.LogIfFailure(logger);

        // Assert
        Assert.HasCount(1, logger.Entries);
        Assert.AreEqual(LogLevel.Error, logger.Entries[0].Level);
        Assert.AreEqual("write failed", logger.Entries[0].Message);
    }

    [TestMethod]
    public void NonGeneric_LogIfFailure_Failure_WithoutLogger_WritesErrorToConsole()
    {
        // Arrange
        var result = StorageResult.Failure("write failed");

        // Act
        var output = CaptureConsoleOutput(() => result.LogIfFailure());

        // Assert
        Assert.Contains("write failed", output);
    }

    [TestMethod]
    public void NonGeneric_LogIfFailure_Failure_WithoutLogger_NullMessage_WritesBlankLineToConsole()
    {
        // Arrange
        var result = new StorageResult(false, null);

        // Act
        var output = CaptureConsoleOutput(() => result.LogIfFailure());

        // Assert
        Assert.AreEqual(Environment.NewLine, output);
    }
}
