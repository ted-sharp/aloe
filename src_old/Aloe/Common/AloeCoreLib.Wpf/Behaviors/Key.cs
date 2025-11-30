using System.Windows;
using System.Windows.Input;

namespace Aloe.Common.AloeCoreLib.Wpf.Behaviors;

public static partial class Key
{
    public static readonly DependencyProperty KeyDownCommandProperty =
        DependencyProperty.RegisterAttached(
            "KeyDownCommand",
            typeof(ICommand),
            typeof(Key),
            new PropertyMetadata(null, Key.OnKeyDownCommandChanged));

    public static ICommand GetKeyDownCommand(DependencyObject obj)
    {
        return (ICommand)obj.GetValue(Key.KeyDownCommandProperty);
    }

    public static void SetKeyDownCommand(DependencyObject obj, ICommand value)
    {
        obj.SetValue(Key.KeyDownCommandProperty, value);
    }

    private static void OnKeyDownCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            element.PreviewKeyDown -= Key.Element_OnPreviewKeyDown;
            if (e.NewValue is ICommand)
            {
                element.PreviewKeyDown += Key.Element_OnPreviewKeyDown;
            }
        }
    }

    private static void Element_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is UIElement element)
        {
            var command = Key.GetKeyDownCommand(element);
            if (command?.CanExecute(e) ?? false)
            {
                command.Execute(e);
                //e.Handled = true;
            }
        }
    }
}
