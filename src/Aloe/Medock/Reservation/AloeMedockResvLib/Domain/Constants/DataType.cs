using System.Globalization;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;

public static class DataType
{
    public static readonly string Boolean = "bool";
    public static readonly string Int32 = "int";
    public static readonly string String = "string";

    public static T? ConvertTo<T>(string dataType, string input)
        where T: struct
    {
        try
        {
            object? result = dataType.ToLower() switch
            {
                "string" => input,
                "bool" => System.Boolean.TryParse(input, out var boolVal) ? boolVal : null,
                "int" => System.Int32.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intVal) ? intVal : null,
                "double" => System.Double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleVal) ? doubleVal : null,
                "datetime" => System.DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeVal) ? dateTimeVal : null,
                _ => System.Convert.ChangeType(input, typeof(T))
            };

            return (result is not null) ? (T)result : null;
        }
        catch
        {
            return null;
        }
    }
}
