
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
            return DateHelper.GetToday();
        }

        if (DateHelper.TryParseEx(dateString, out var date))
        {
            return date;
        }

        return DateHelper.GetToday();
    }
}
