using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Views.Converters;

public class Int32ValueToScaleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && Double.TryParse(s, out var d))
        {
            return d / 100.0;
        }

        var d2 = System.Convert.ToDouble(value);
        if (d2 != 0.0)
        {
            return d2 / 100.0;
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            return (int)(d * 100.0);
        }
        return value;
    }
}
