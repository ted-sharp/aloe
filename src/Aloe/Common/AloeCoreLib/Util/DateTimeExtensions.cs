
using System.Globalization;

namespace Aloe.Common.AloeCoreLib.Util;

public static class DateTimeExtensions
{

    public static DateOnly ToDateOnly(this DateTime dateTime)
    {
        return DateOnly.FromDateTime(dateTime);
    }

    public static DateOnly? ToDateOnly(this DateTime? dateTime)
    {
        return dateTime.HasValue ? DateOnly.FromDateTime(dateTime.Value) : null;
    }

}
