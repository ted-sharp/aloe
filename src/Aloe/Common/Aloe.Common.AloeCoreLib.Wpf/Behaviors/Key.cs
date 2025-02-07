using System.Windows;
using System.Windows.Input;

namespace Aloe.Common.AloeCoreLib.Wpf.Behaviors;

public static class Key
{
    #region KeyDown

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
                e.Handled = true;
            }
        }
    }

    #endregion KeyDown

    #region KeyUp

    public static readonly DependencyProperty KeyUpCommandProperty =
        DependencyProperty.RegisterAttached(
            "KeyUpCommand",
            typeof(ICommand),
            typeof(Key),
            new PropertyMetadata(null, Key.OnKeyUpCommandChanged));

    public static ICommand GetKeyUpCommand(DependencyObject obj)
    {
        return (ICommand)obj.GetValue(Key.KeyUpCommandProperty);
    }

    public static void SetKeyUpCommand(DependencyObject obj, ICommand value)
    {
        obj.SetValue(Key.KeyUpCommandProperty, value);
    }

    private static void OnKeyUpCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            element.PreviewKeyUp -= Key.Element_OnPreviewKeyUp;
            if (e.NewValue is ICommand)
            {
                element.PreviewKeyUp += Key.Element_OnPreviewKeyUp;
            }
        }
    }

    private static void Element_OnPreviewKeyUp(object sender, KeyEventArgs e)
    {
        if (sender is UIElement element)
        {
            var command = Key.GetKeyUpCommand(element);
            if (command?.CanExecute(e) ?? false)
            {
                command.Execute(e);
                e.Handled = true;
            }
        }
    }

    #endregion KeyUp
}
