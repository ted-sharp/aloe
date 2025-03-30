
using System.Globalization;

namespace Aloe.Common.AloeCoreLib.Util;

public static class DateOnlyExtensions
{

    public static DateTime ToDateTime(this DateOnly date)
    {
        return date.ToDateTime(TimeOnly.MinValue);
    }

    public static DateOnly ToDateOrToday(this string dateString)
    {
        if (String.IsNullOrWhiteSpace(dateString))
        {
            return DateOnlyHelper.GetToday();
        }

        if (DateOnlyHelper.TryParseEx(dateString, out var date))
        {
            return date;
        }

        return DateOnlyHelper.GetToday();
    }
}
