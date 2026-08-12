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
