using System.Text;

namespace NovaStaff.DataLayers.Helpers;

public static class EnumConstraintHelper
{
    public static string BuildInConstraint<TEnum>(string columnName)
    where TEnum : struct, Enum
    {
        var underlyingType = Enum.GetUnderlyingType(typeof(TEnum));

        if (underlyingType != typeof(byte))
            throw new InvalidOperationException(
                $"Enum {typeof(TEnum).Name} must be byte to match tinyint column.");

        var values = Enum.GetValues<TEnum>()
                         .Select(v => Convert.ToByte(v));

        return $"\"{columnName}\" IN ({string.Join(", ", values)})";
    }
}



