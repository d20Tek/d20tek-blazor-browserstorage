namespace D20Tek.Blazor.BrowserStorage.Tests;

[TestClass]
public class StorageSerializerDateTimeTests
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    // --- DateTime ---

    [TestMethod]
    public void Serialize_DateTime_ReturnsRoundTripFormat()
    {
        // Arrange
        var input = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("2025-06-15T10:30:00.0000000Z", result);
    }

    [TestMethod]
    public void Deserialize_DateTime_ParsesRoundTripFormat()
    {
        // Arrange
        var input = "2025-06-15T10:30:00.0000000Z";

        // Act
        var result = StorageSerializer.Deserialize<DateTime>(input, _jsonOptions);

        // Assert
        Assert.AreEqual(new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc), result);
        Assert.AreEqual(DateTimeKind.Utc, result.Kind);
    }

    // --- DateTimeOffset ---

    [TestMethod]
    public void Serialize_DateTimeOffset_ReturnsRoundTripFormat()
    {
        // Arrange
        var input = new DateTimeOffset(2025, 6, 15, 10, 30, 0, TimeSpan.FromHours(-5));

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("2025-06-15T10:30:00.0000000-05:00", result);
    }

    [TestMethod]
    public void Deserialize_DateTimeOffset_ParsesRoundTripFormat()
    {
        // Arrange
        var input = "2025-06-15T10:30:00.0000000-05:00";

        // Act
        var result = StorageSerializer.Deserialize<DateTimeOffset>(input, _jsonOptions);

        // Assert
        Assert.AreEqual(new DateTimeOffset(2025, 6, 15, 10, 30, 0, TimeSpan.FromHours(-5)), result);
    }

    // --- DateOnly ---

    [TestMethod]
    public void Serialize_DateOnly_ReturnsIsoFormat()
    {
        // Arrange
        var input = new DateOnly(2025, 6, 15);

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("2025-06-15", result);
    }

    [TestMethod]
    public void Deserialize_DateOnly_ParsesCorrectly()
    {
        // Arrange
        var input = "2025-06-15";

        // Act
        var result = StorageSerializer.Deserialize<DateOnly>(input, _jsonOptions);

        // Assert
        Assert.AreEqual(new DateOnly(2025, 6, 15), result);
    }

    // --- TimeOnly ---

    [TestMethod]
    public void Serialize_TimeOnly_ReturnsFormat()
    {
        // Arrange
        var input = new TimeOnly(14, 30, 45);

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("14:30:45.0000000", result);
    }

    [TestMethod]
    public void Deserialize_TimeOnly_ParsesCorrectly()
    {
        // Arrange
        var input = "14:30:45.0000000";

        // Act
        var result = StorageSerializer.Deserialize<TimeOnly>(input, _jsonOptions);

        // Assert
        Assert.AreEqual(new TimeOnly(14, 30, 45), result);
    }

    // --- TimeSpan ---

    [TestMethod]
    public void Serialize_TimeSpan_ReturnsString()
    {
        // Arrange
        var input = new TimeSpan(1, 2, 3, 4, 500);

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("1.02:03:04.5000000", result);
    }

    [TestMethod]
    public void Deserialize_TimeSpan_ParsesCorrectly()
    {
        // Arrange
        var input = "1.02:03:04.5000000";

        // Act
        var result = StorageSerializer.Deserialize<TimeSpan>(input, _jsonOptions);

        // Assert
        Assert.AreEqual(new TimeSpan(1, 2, 3, 4, 500), result);
    }
}
