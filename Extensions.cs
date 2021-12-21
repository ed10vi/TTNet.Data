using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TTNet;

internal static class EnumAttributeGetter
{
    internal static T GetAttribute<T>(this Enum value)
        where T : notnull, Attribute
    {
        Type enumType = value.GetType();
        string name = Enum.GetName(enumType, value)!;
        return enumType.GetField(name)!.GetCustomAttribute<T>(false)!;
    }

    internal static TEnum GetByAttribute<TAttr, TEnum>(Func<TAttr, bool> filter)
        where TAttr : notnull, Attribute
    {
        var fields = typeof(TEnum).GetFields();
        var field = fields.SingleOrDefault(f =>
        {
            TAttr attr = f.GetCustomAttribute<TAttr>(false)!;
            return attr != null && filter(attr);
        });
        return (TEnum)field!.GetValue(null)!;
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

internal static class AsyncEventHandler
{
    internal static Task InvokeAsync<T>(this EventHandler<T> self, object sender, T e) =>
        Task.Run(() => self.Invoke(sender, e));
}
