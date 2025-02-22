using System.Globalization;
using System.Windows.Data;

namespace Aloe.Common.AloeCoreLib.Wpf.Converters;

public class StringToDateConverter : IValueConverter
{
    /// <summary>
    /// プロパティ→画面の変換を行います。
    /// </summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string dateString && DateTime.TryParse(dateString, out DateTime date))
        {
            return date;
        }
        return String.Empty;
    }

    /// <summary>
    /// プロパティ→画面の変換を行います。
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToString("yyyy/MM/dd");
        }
        return null;
    }
}
