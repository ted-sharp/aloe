using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Aloe.Common.AloeCoreLib.Util;

namespace Aloe.Common.AloeCoreLib.Wpf.Converters;

/// <summary>
/// VM側がDateOnly、V側がTextの場合に使用します。
/// </summary>
public class DateOnlyToDateTimeConverter : IValueConverter
{
    /// <summary>
    /// 表示時に使います。
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateOnly dateOnly)
        {
            return dateOnly.ToDateTime(TimeOnly.MinValue);
        }

        return null;
    }

    /// <summary>
    /// プロパティに格納時に使います。
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            return DateOnly.FromDateTime(dateTime);
        }

        // パースできなければ適当に null か既定値を返す
        return Binding.DoNothing;
    }
}
