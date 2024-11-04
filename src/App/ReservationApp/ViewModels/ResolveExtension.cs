using System.Windows.Markup;

namespace AloeReservationGrid.App.ReservationApp.ViewModels;

/// <summary>
/// 依存性注入（DI）コンテナから指定された型を解決するカスタム MarkupExtension です。
/// このクラスを使用することで、XAML 内で DI コンテナに登録されたサービスや ViewModel を動的に解決し、
/// XAML バインディング内で DI を使用することができます。
///
/// <example>
/// XAMLでの使用例:
/// <code>
/// <Window x:Class="MyApp.MainWindow"
///         xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
///         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
///         xmlns:local="clr-namespace:MyApp"
///         Title="MainWindow"
///         DataContext="{local:Resolve {x:Type local:MainViewModel}}"
/// >
/// </Window>
/// </code>
/// </example>
/// </summary>
/// <remarks>
/// MarkupExtension の規則として、クラス名から "Extension" を省略して名前解決が行われます。
/// そのため、XAML内で <c>{local:Resolve}</c> と記述することで <c>{local:ResolveExtension}</c> が呼び出されます。
/// 省略せずに使用する場合は、次のように記述します:
/// <c>DataContext="{local:ResolveExtension {x:Type local:MainViewModel}}"</c>
/// </remarks>
public class ResolveExtension : MarkupExtension
{
    private readonly Type _type;

    public ResolveExtension(Type type)
    {
        this._type = type;
    }

    /// <summary>
    /// MarkupExtension で評価されるメソッドです。
    /// </summary>
    /// <remarks>
    /// <c>IHost</c> の DI コンテナで解決したいため、<c>App.Resolve()</c> を呼んでいます。
    /// </remarks>
    public override object ProvideValue(IServiceProvider wpfServiceProvider)
    {
        return App.Resolve(this._type);
    }
}
