namespace D20Tek.Blazor.BrowserStorage.Internal;

internal interface ITypeConverter
{
    string Serialize(object value);

    object Deserialize(string raw);
}

internal sealed class TypeConverter<T>(Func<T, string> serialize, Func<string, T> deserialize) : ITypeConverter
{
    public string Serialize(object value) => serialize((T)value);

    public object Deserialize(string raw) => deserialize(raw)!;
}
