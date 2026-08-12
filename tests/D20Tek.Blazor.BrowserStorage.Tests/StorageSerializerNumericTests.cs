namespace D20Tek.Blazor.BrowserStorage.Tests;

[TestClass]
public class StorageSerializerNumericTests
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

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
}
