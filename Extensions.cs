using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TTNet
{
    internal static class EnumAttributeGetter
    {
        internal static T GetAttribute<T>(this Enum value)
            where T : Attribute
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            return enumType.GetField(name).GetCustomAttribute<T>(false);
        }

        internal static TEnum GetByAttribute<TAttr, TEnum>(Func<TAttr, bool> filter)
            where TAttr : Attribute
        {
            var fields = typeof(TEnum).GetFields();
            var field = fields.SingleOrDefault(f =>
            {
                TAttr attr = f.GetCustomAttribute<TAttr>(false);
                return attr != null && filter.Invoke(attr);
            });
            return (TEnum)field.GetValue(null);
        }
    }

    internal static class HexConvert
    {
        internal static string ToHexString(this byte[] bytes)
        {
            byte[] result = new byte[bytes.Length * 2];
            for (byte i = 0, j = 0; i < bytes.Length; i++, j += 2)
            {
                result[j] = ToASCII((byte)((bytes[i] >> 4) & 0x0F));
                result[j + 1] = ToASCII((byte)(bytes[i] & 0x0F));
            }
            return Encoding.ASCII.GetString(result);
        }

        private static byte ToASCII(byte b) => (byte)(b + (b < 10 ? 48 : 55));

        internal static byte[] HexToByteArray(this string str)
        {
            byte[] bytes = new byte[str.Length / 2];
            for (byte i = 0, j = 0; i < bytes.Length; i++, j += 2)
                bytes[i] = (byte)((((FromASCII(str[j])) << 4) & 0xF0) | ((FromASCII(str[j + 1])) & 0x0F));
            return bytes;
        }

        private static byte FromASCII(char c)
        {
            byte b = Convert.ToByte(c);
            return (byte)(b - (b < 65 ? 48 : 55));
        }
    }

    /// <summary>
    /// Extension methods for JSON Element.
    /// </summary>
    public static class JsonConverter
    {
        /// <summary>
        /// Converts a JSON Element into a given type.
        /// </summary>
        /// <returns>The converted element.</returns>
        /// <param name="je">JSON Element to convert.</param>
        /// <typeparam name="T">The destination type</typeparam>
        public static T ConvertTo<T>(this JsonElement je) where T : new()
        {
            T obj = new T();
            JsonElement element;

            foreach (var p in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                if (p.GetCustomAttribute<JsonIgnoreAttribute>() == null && je.TryGetProperty(p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name, out element))
                    p.SetValue(obj, element.GetFromType(p.PropertyType));

            return obj;
        }

        private static object GetFromType(this JsonElement element, Type type)
        {
            object result = null;
            switch (element.ValueKind)
            {
                case JsonValueKind.False:
                case JsonValueKind.True:
                    result = element.GetBoolean();
                    break;
                case JsonValueKind.String:
                    if (type == typeof(byte[]))
                        result = element.GetBytesFromBase64();
                    else if (type == typeof(DateTime) || type == typeof(DateTime?))
                        result = element.GetDateTime();
                    else if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?))
                        result = element.GetDateTimeOffset();
                    else if (type == typeof(Guid) || type == typeof(Guid?))
                        result = element.GetGuid();
                    else
                        result = element.GetString();
                    break;
                case JsonValueKind.Number:
                    if (type == typeof(byte) || type == typeof(byte?))
                        result = element.GetByte();
                    else if (type == typeof(sbyte) || type == typeof(sbyte?))
                        result = element.GetSByte();
                    else if (type == typeof(short) || type == typeof(short?))
                        result = element.GetInt16();
                    else if (type == typeof(ushort) || type == typeof(ushort?))
                        result = element.GetUInt16();
                    else if (type == typeof(int) || type == typeof(int?))
                        result = element.GetInt32();
                    else if (type == typeof(uint) || type == typeof(uint?))
                        result = element.GetUInt32();
                    else if (type == typeof(long) || type == typeof(long?))
                        result = element.GetInt64();
                    else if (type == typeof(ulong) || type == typeof(ulong?))
                        result = element.GetUInt64();
                    else if (type == typeof(float) || type == typeof(float?))
                        result = element.GetSingle();
                    else if (type == typeof(double) || type == typeof(double?))
                        result = element.GetDouble();
                    else if (type == typeof(decimal) || type == typeof(decimal?))
                        result = element.GetDecimal();
                    break;
                case JsonValueKind.Array:
                    var et = type.GetElementType();
                    var arr = Array.CreateInstance(et, element.GetArrayLength());
                    var en = element.EnumerateArray();
                    for (int i = 0; en.MoveNext(); i++)
                        arr.SetValue(en.Current.GetFromType(et), i);
                    result = arr;
                    break;
                case JsonValueKind.Object:
                    if (type == typeof(JsonElement) || type == typeof(JsonElement?))
                        result = element;
                    else if (type == typeof(string))
                        result = element.ToString();
                    else
                        result = typeof(JsonConverter).GetMethod("ConvertTo").MakeGenericMethod(type).Invoke(null, new object[] { element });
                    break;
            }
            return result;
        }

        /// <summary>
        /// Converts an object into a JSON string.
        /// </summary>
        /// <returns>The JSON string.</returns>
        /// <param name="obj">The element to convert.</param>
        /// <typeparam name="T">The object type</typeparam>
        public static string From<T>(T obj)
        {
            var type = typeof(T);
            using (var stream = new MemoryStream())
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions()))
            {
                if (type.GetInterface("IEnumerable") != null)
                    writer.WriteArray((IEnumerable) obj, type);
                else
                    writer.WriteObject(obj, type);
                writer.Flush();
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private static void WriteObject(this Utf8JsonWriter writer, object obj, Type objType, string propertyName = null)
        {
            string name;
            Type type;
            object value;
            if (propertyName == null)
                writer.WriteStartObject();
            else
                writer.WriteStartObject(propertyName);
            foreach (var p in objType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                if (p.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                {
                    type = p.PropertyType;
                    name = p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name;
                    value = p.GetValue(obj);

                    if (value != null)
                    {
                        if (type == typeof(bool) || type == typeof(bool?))
                            writer.WriteBoolean(name, (bool) value);
                        else if (type == typeof(byte) || type == typeof(byte?))
                            writer.WriteNumber(name, (byte) value);
                        else if (type == typeof(sbyte) || type == typeof(sbyte?))
                            writer.WriteNumber(name, (sbyte) value);
                        else if (type == typeof(short) || type == typeof(short?))
                            writer.WriteNumber(name, (short) value);
                        else if (type == typeof(ushort) || type == typeof(ushort?))
                            writer.WriteNumber(name, (ushort) value);
                        else if (type == typeof(int) || type == typeof(int?))
                            writer.WriteNumber(name, (int) value);
                        else if (type == typeof(uint) || type == typeof(uint?))
                            writer.WriteNumber(name, (uint) value);
                        else if (type == typeof(long) || type == typeof(long?))
                            writer.WriteNumber(name, (long) value);
                        else if (type == typeof(ulong) || type == typeof(ulong?))
                            writer.WriteNumber(name, (ulong) value);
                        else if (type == typeof(float) || type == typeof(float?))
                            writer.WriteNumber(name, (float) value);
                        else if (type == typeof(double) || type == typeof(double?))
                            writer.WriteNumber(name, (double) value);
                        else if (type == typeof(decimal) || type == typeof(decimal?))
                            writer.WriteNumber(name, (decimal) value);
                        else if (type == typeof(string))
                            writer.WriteString(name, (string) value);
                        else if (type == typeof(DateTime) || type == typeof(DateTime?))
                            writer.WriteString(name, (DateTime) value);
                        else if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?))
                            writer.WriteString(name, (DateTimeOffset) value);
                        else if (type == typeof(Guid) || type == typeof(Guid?))
                            writer.WriteString(name, (Guid) value);
                        else if (type == typeof(byte[]))
                            writer.WriteBase64String(name, (byte[]) value);
                        else if (type.GetInterface("IEnumerable") != null)
                            writer.WriteArray((IEnumerable) value, type, name);
                        else
                            writer.WriteObject(value, type, name);
                    }
                }
            writer.WriteEndObject();
        }

        private static void WriteArray(this Utf8JsonWriter writer, IEnumerable obj, Type objType, string propertyName = null)
        {
            Type type;
            if (propertyName == null)
                writer.WriteStartArray();
            else
                writer.WriteStartArray(propertyName);

            foreach (var value in obj)
            {
                if (value != null)
                {
                    type = value.GetType();
                    if (type == typeof(bool) || type == typeof(bool?))
                        writer.WriteBooleanValue((bool) value);
                    else if (type == typeof(byte) || type == typeof(byte?))
                        writer.WriteNumberValue((byte) value);
                    else if (type == typeof(sbyte) || type == typeof(sbyte?))
                        writer.WriteNumberValue((sbyte) value);
                    else if (type == typeof(short) || type == typeof(short?))
                        writer.WriteNumberValue((short) value);
                    else if (type == typeof(ushort) || type == typeof(ushort?))
                        writer.WriteNumberValue((ushort) value);
                    else if (type == typeof(int) || type == typeof(int?))
                        writer.WriteNumberValue((int) value);
                    else if (type == typeof(uint) || type == typeof(uint?))
                        writer.WriteNumberValue((uint) value);
                    else if (type == typeof(long) || type == typeof(long?))
                        writer.WriteNumberValue((long) value);
                    else if (type == typeof(ulong) || type == typeof(ulong?))
                        writer.WriteNumberValue((ulong) value);
                    else if (type == typeof(float) || type == typeof(float?))
                        writer.WriteNumberValue((float) value);
                    else if (type == typeof(double) || type == typeof(double?))
                        writer.WriteNumberValue((double) value);
                    else if (type == typeof(decimal) || type == typeof(decimal?))
                        writer.WriteNumberValue((decimal) value);
                    else if (type == typeof(string))
                        writer.WriteStringValue((string) value);
                    else if (type == typeof(DateTime) || type == typeof(DateTime?))
                        writer.WriteStringValue((DateTime) value);
                    else if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?))
                        writer.WriteStringValue((DateTimeOffset) value);
                    else if (type == typeof(Guid) || type == typeof(Guid?))
                        writer.WriteStringValue((Guid) value);
                    else if (type == typeof(byte[]))
                        writer.WriteBase64StringValue((byte[]) value);
                    else if (type.GetInterface("IEnumerable") != null)
                        writer.WriteArray((IEnumerable) value, type);
                    else
                        writer.WriteObject(value, type);
                }
            }

            writer.WriteEndArray();
        }
    }
}