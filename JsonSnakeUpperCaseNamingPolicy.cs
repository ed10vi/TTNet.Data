using System.Text;
using System.Text.Json;

namespace TTNet.Data;

/// <summary>
/// Naming policy to convert from CamelCase to SNAKE_UPPER_CASE.
/// </summary>
public class JsonSnakeUpperCaseNamingPolicy : JsonNamingPolicy
{
    private const string _separator = "_";

    /// <summary>
    /// Converts the specified name to snake uppercase
    /// </summary>
    public override string ConvertName(string name)
    {
        var result = new StringBuilder();

        for (int i = 0; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]))
            {
                if (i > 0 && !char.IsUpper(name[i - 1]))
                    result.Append(_separator);
                result.Append(name[i]);
            }
            else
            {
                result.Append(char.ToUpper(name[i]));
            }
        }

        return result.ToString();
    }
}
