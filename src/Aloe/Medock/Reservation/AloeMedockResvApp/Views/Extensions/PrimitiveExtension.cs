using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Views.Extensions;

// TODO: こちらの Extension は拡張メソッド(Extensions)ではなく、MarkupExtension の方

public class PrimitiveExtension<T>(T value) : MarkupExtension
    where T : notnull
{
    public T Value { get; set; } = value;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this.Value;
    }
}

[MarkupExtensionReturnType(typeof(int))]
public class Int32Extension(int value) : PrimitiveExtension<int>(value);

