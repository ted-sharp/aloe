using System.Windows.Markup;

// 拡張メソッド(Extensions)ではなく、MarkupExtension の名前空間です。
namespace Aloe.Common.AloeCoreLib.Wpf.Extension;

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

