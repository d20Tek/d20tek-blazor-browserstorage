using System.Text.Json;
using D20Tek.Blazor.BrowserStorage.Internal;

namespace D20Tek.Blazor.BrowserStorage.Tests;

[TestClass]
public class StorageSerializerTests
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    // --- String ---

    [TestMethod]
    public void Serialize_String_ReturnsRawString()
    {
        // Arrange
        var input = "hello";

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("hello", result);
    }

    [TestMethod]
    public void Deserialize_String_ReturnsRawString()
    {
        // Arrange
        var input = "hello";

        // Act
        var result = StorageSerializer.Deserialize<string>(input, _jsonOptions);

        // Assert
        Assert.AreEqual("hello", result);
    }

    // --- Bool ---

    [TestMethod]
    [DataRow(true, "true")]
    [DataRow(false, "false")]
    public void Serialize_Bool_ReturnsLowerCase(bool input, string expected)
    {
        // Arrange & Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    [DataRow("true", true)]
    [DataRow("false", false)]
    public void Deserialize_Bool_ParsesCorrectly(string input, bool expected)
    {
        // Arrange & Act
        var result = StorageSerializer.Deserialize<bool>(input, _jsonOptions);

        // Assert
        Assert.AreEqual(expected, result);
    }

    // --- Int ---

    [TestMethod]
    public void Serialize_Int_ReturnsString()
    {
        // Arrange
        var input = 42;

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void Deserialize_Int_ParsesCorrectly()
    {
        // Arrange
        var input = "42";

        // Act
        var result = StorageSerializer.Deserialize<int>(input, _jsonOptions);

        // Assert
        Assert.AreEqual(42, result);
    }

    // --- Uint ---

    [TestMethod]
    public void Serialize_Uint_ReturnsString()
    {
        // Arrange
        var input = 100u;

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("100", result);
    }

    [TestMethod]
    public void Deserialize_Uint_ParsesCorrectly()
    {
        // Arrange
        var input = "100";

        // Act
        var result = StorageSerializer.Deserialize<uint>(input, _jsonOptions);

        // Assert
        Assert.AreEqual(100u, result);
    }

    // --- Long ---

    [TestMethod]
    public void Serialize_Long_ReturnsString()
    {
        // Arrange
        var input = 9876543210L;

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("9876543210", result);
    }

    [TestMethod]
    public void Deserialize_Long_ParsesCorrectly()
    {
        // Arrange
        var input = "9876543210";

        // Act
        var result = StorageSerializer.Deserialize<long>(input, _jsonOptions);

        // Assert
        Assert.AreEqual(9876543210L, result);
    }

    // --- Ulong ---

    [TestMethod]
    public void Serialize_Ulong_ReturnsString()
    {
        // Arrange
        var input = ulong.MaxValue;

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("18446744073709551615", result);
    }

    [TestMethod]
    public void Deserialize_Ulong_ParsesCorrectly()
    {
        // Arrange
        var input = "18446744073709551615";

        // Act
        var result = StorageSerializer.Deserialize<ulong>(input, _jsonOptions);

        // Assert
        Assert.AreEqual(ulong.MaxValue, result);
    }

    // --- Short ---

    [TestMethod]
    public void Serialize_Short_ReturnsString()
    {
        // Arrange
        var input = (short)123;

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("123", result);
    }

    [TestMethod]
    public void Deserialize_Short_ParsesCorrectly()
    {
        // Arrange
        var input = "123";

        // Act
        var result = StorageSerializer.Deserialize<short>(input, _jsonOptions);

        // Assert
        Assert.AreEqual((short)123, result);
    }

    // --- Ushort ---

    [TestMethod]
    public void Serialize_Ushort_ReturnsString()
    {
        // Arrange
        var input = (ushort)456;

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("456", result);
    }

    [TestMethod]
    public void Deserialize_Ushort_ParsesCorrectly()
    {
        // Arrange
        var input = "456";

        // Act
        var result = StorageSerializer.Deserialize<ushort>(input, _jsonOptions);

        // Assert
        Assert.AreEqual((ushort)456, result);
    }

    // --- Byte ---

    [TestMethod]
    public void Serialize_Byte_ReturnsString()
    {
        // Arrange
        var input = (byte)255;

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("255", result);
    }

    [TestMethod]
    public void Deserialize_Byte_ParsesCorrectly()
    {
        // Arrange
        var input = "255";

        // Act
        var result = StorageSerializer.Deserialize<byte>(input, _jsonOptions);

        // Assert
        Assert.AreEqual((byte)255, result);
    }

    // --- Sbyte ---

    [TestMethod]
    public void Serialize_Sbyte_ReturnsString()
    {
        // Arrange
        var input = (sbyte)-100;

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("-100", result);
    }

    [TestMethod]
    public void Deserialize_Sbyte_ParsesCorrectly()
    {
        // Arrange
        var input = "-100";

        // Act
        var result = StorageSerializer.Deserialize<sbyte>(input, _jsonOptions);

        // Assert
        Assert.AreEqual((sbyte)-100, result);
    }

    // --- Char ---

    [TestMethod]
    public void Serialize_Char_ReturnsString()
    {
        // Arrange
        var input = 'A';

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("A", result);
    }

    [TestMethod]
    public void Deserialize_Char_ParsesCorrectly()
    {
        // Arrange
        var input = "A";

        // Act
        var result = StorageSerializer.Deserialize<char>(input, _jsonOptions);

        // Assert
        Assert.AreEqual('A', result);
    }

    // --- Float ---

    [TestMethod]
    public void Serialize_Float_ReturnsString()
    {
        // Arrange
        var input = 3.14f;

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("3.14", result);
    }

    [TestMethod]
    public void Deserialize_Float_ParsesCorrectly()
    {
        // Arrange
        var input = "3.14";

        // Act
        var result = StorageSerializer.Deserialize<float>(input, _jsonOptions);

        // Assert
        Assert.AreEqual(3.14f, result);
    }

    // --- Double ---

    [TestMethod]
    public void Serialize_Double_ReturnsString()
    {
        // Arrange
        var input = 3.14159265358979;

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("3.14159265358979", result);
    }

    [TestMethod]
    public void Deserialize_Double_ParsesCorrectly()
    {
        // Arrange
        var input = "3.14159265358979";

        // Act
        var result = StorageSerializer.Deserialize<double>(input, _jsonOptions);

        // Assert
        Assert.AreEqual(3.14159265358979, result);
    }

    // --- Decimal ---

    [TestMethod]
    public void Serialize_Decimal_ReturnsString()
    {
        // Arrange
        var input = 99.99m;

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("99.99", result);
    }

    [TestMethod]
    public void Deserialize_Decimal_ParsesCorrectly()
    {
        // Arrange
        var input = "99.99";

        // Act
        var result = StorageSerializer.Deserialize<decimal>(input, _jsonOptions);

        // Assert
        Assert.AreEqual(99.99m, result);
    }

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

    // --- Guid ---

    [TestMethod]
    public void Serialize_Guid_ReturnsString()
    {
        // Arrange
        var input = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("a1b2c3d4-e5f6-7890-abcd-ef1234567890", result);
    }

    [TestMethod]
    public void Deserialize_Guid_ParsesCorrectly()
    {
        // Arrange
        var input = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

        // Act
        var result = StorageSerializer.Deserialize<Guid>(input, _jsonOptions);

        // Assert
        Assert.AreEqual(Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), result);
    }

    // --- Nullable primitives ---

    [TestMethod]
    public void Serialize_NullableInt_WithValue_ReturnsString()
    {
        // Arrange
        int? input = 42;

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void Serialize_NullableInt_Null_ReturnsNullString()
    {
        // Arrange
        int? input = null;

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void Deserialize_NullableInt_WithValue_ParsesCorrectly()
    {
        // Arrange
        var input = "42";

        // Act
        var result = StorageSerializer.Deserialize<int?>(input, _jsonOptions);

        // Assert
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void Deserialize_NullableInt_NullString_ReturnsNull()
    {
        // Arrange
        var input = "null";

        // Act
        var result = StorageSerializer.Deserialize<int?>(input, _jsonOptions);

        // Assert
        Assert.IsNull(result);
    }

    // --- Complex types (JSON) ---

    [TestMethod]
    public void Serialize_ComplexType_ReturnsJson()
    {
        // Arrange
        var input = new TestObject { Name = "test", Count = 5 };

        // Act
        var result = StorageSerializer.Serialize(input, _jsonOptions);

        // Assert
        Assert.AreEqual("{\"name\":\"test\",\"count\":5}", result);
    }

    [TestMethod]
    public void Deserialize_ComplexType_ParsesJson()
    {
        // Arrange
        var input = "{\"name\":\"test\",\"count\":5}";

        // Act
        var result = StorageSerializer.Deserialize<TestObject>(input, _jsonOptions);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("test", result.Name);
        Assert.AreEqual(5, result.Count);
    }

    private sealed class TestObject
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
