using System.Globalization;

namespace D20Tek.Blazor.BrowserStorage.Internal;

internal static class StorageSerializer
{
    private static readonly Dictionary<Type, ITypeConverter> _converters = new()
    {
        [typeof(string)] = new TypeConverter<string>(v => v, s => s),
        [typeof(bool)] = new TypeConverter<bool>(v => v ? "true" : "false", s => bool.Parse(s)),
        [typeof(int)] = new TypeConverter<int>(v => v.ToString(CultureInfo.InvariantCulture), s => int.Parse(s, CultureInfo.InvariantCulture)),
        [typeof(uint)] = new TypeConverter<uint>(v => v.ToString(CultureInfo.InvariantCulture), s => uint.Parse(s, CultureInfo.InvariantCulture)),
        [typeof(long)] = new TypeConverter<long>(v => v.ToString(CultureInfo.InvariantCulture), s => long.Parse(s, CultureInfo.InvariantCulture)),
        [typeof(ulong)] = new TypeConverter<ulong>(v => v.ToString(CultureInfo.InvariantCulture), s => ulong.Parse(s, CultureInfo.InvariantCulture)),
        [typeof(short)] = new TypeConverter<short>(v => v.ToString(CultureInfo.InvariantCulture), s => short.Parse(s, CultureInfo.InvariantCulture)),
        [typeof(ushort)] = new TypeConverter<ushort>(v => v.ToString(CultureInfo.InvariantCulture), s => ushort.Parse(s, CultureInfo.InvariantCulture)),
        [typeof(byte)] = new TypeConverter<byte>(v => v.ToString(CultureInfo.InvariantCulture), s => byte.Parse(s, CultureInfo.InvariantCulture)),
        [typeof(sbyte)] = new TypeConverter<sbyte>(v => v.ToString(CultureInfo.InvariantCulture), s => sbyte.Parse(s, CultureInfo.InvariantCulture)),
        [typeof(char)] = new TypeConverter<char>(v => v.ToString(), s => s[0]),
        [typeof(float)] = new TypeConverter<float>(v => v.ToString(CultureInfo.InvariantCulture), s => float.Parse(s, CultureInfo.InvariantCulture)),
        [typeof(double)] = new TypeConverter<double>(v => v.ToString(CultureInfo.InvariantCulture), s => double.Parse(s, CultureInfo.InvariantCulture)),
        [typeof(decimal)] = new TypeConverter<decimal>(v => v.ToString(CultureInfo.InvariantCulture), s => decimal.Parse(s, CultureInfo.InvariantCulture)),
        [typeof(DateTime)] = new TypeConverter<DateTime>(
            v => v.ToString("O", CultureInfo.InvariantCulture), s => DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)),
        [typeof(DateTimeOffset)] = new TypeConverter<DateTimeOffset>(
            v => v.ToString("O", CultureInfo.InvariantCulture), s => DateTimeOffset.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)),
        [typeof(DateOnly)] = new TypeConverter<DateOnly>(v => v.ToString("O", CultureInfo.InvariantCulture), s => DateOnly.Parse(s, CultureInfo.InvariantCulture)),
        [typeof(TimeOnly)] = new TypeConverter<TimeOnly>(v => v.ToString("O", CultureInfo.InvariantCulture), s => TimeOnly.Parse(s, CultureInfo.InvariantCulture)),
        [typeof(TimeSpan)] = new TypeConverter<TimeSpan>(v => v.ToString(null, CultureInfo.InvariantCulture), s => TimeSpan.Parse(s, CultureInfo.InvariantCulture)),
        [typeof(Guid)] = new TypeConverter<Guid>(v => v.ToString(), s => Guid.Parse(s))
    };

    [RequiresUnreferencedCode(TrimmingMessages.RequiresUnreferencedCode)]
    [RequiresDynamicCode(TrimmingMessages.RequiresDynamicCode)]
    public static string Serialize<T>(T value, JsonSerializerOptions jsonOptions)
    {
        if (value is null) return "null";

        var underlyingType = GetUnderlyingType<T>();
        return _converters.TryGetValue(underlyingType, out var converter)
            ? converter.Serialize(value) 
            : JsonSerializer.Serialize(value, jsonOptions);
    }

    [RequiresUnreferencedCode(TrimmingMessages.RequiresUnreferencedCode)]
    [RequiresDynamicCode(TrimmingMessages.RequiresDynamicCode)]
    public static T? Deserialize<T>(string json, JsonSerializerOptions jsonOptions)
    {
        var underlyingType = GetUnderlyingType<T>();
        if (!_converters.TryGetValue(underlyingType, out var converter))
            return JsonSerializer.Deserialize<T>(json, jsonOptions);

        return (json == "null") ? default : (T)converter.Deserialize(json);
    }

    private static Type GetUnderlyingType<T>()
    {
        var type = typeof(T);
        return Nullable.GetUnderlyingType(type) ?? type;
    }
}
