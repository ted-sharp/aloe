using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Views.Behaviors;


// Behavior<UIElement> 使わなくてよいの？
public static class KeyInputBehavior
{
    #region KeyDown

    public static readonly DependencyProperty KeyDownCommandProperty =
        DependencyProperty.RegisterAttached(
            "KeyDownCommand",
            typeof(ICommand),
            typeof(KeyInputBehavior),
            new PropertyMetadata(null, OnKeyDownCommandChanged));

    public static ICommand GetKeyDownCommand(DependencyObject obj)
    {
        return (ICommand)obj.GetValue(KeyDownCommandProperty);
    }

    public static void SetKeyDownCommand(DependencyObject obj, ICommand value)
    {
        obj.SetValue(KeyDownCommandProperty, value);
    }

    private static void OnKeyDownCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            element.PreviewKeyDown -= KeyInputBehavior.Element_OnPreviewKeyDown;
            if (e.NewValue is ICommand)
            {
                element.PreviewKeyDown += KeyInputBehavior.Element_OnPreviewKeyDown;
            }
        }
    }

    private static void Element_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is UIElement element)
        {
            var command = GetKeyDownCommand(element);
            if (command?.CanExecute(e) ?? false)
            {
                command.Execute(e);
            }
        }
    }

    #endregion KeyDown

    #region KeyUp

    public static readonly DependencyProperty KeyUpCommandProperty =
        DependencyProperty.RegisterAttached(
            "KeyUpCommand",
            typeof(ICommand),
            typeof(KeyInputBehavior),
            new PropertyMetadata(null, OnKeyUpCommandChanged));

    public static ICommand GetKeyUpCommand(DependencyObject obj)
    {
        return (ICommand)obj.GetValue(KeyUpCommandProperty);
    }

    public static void SetKeyUpCommand(DependencyObject obj, ICommand value)
    {
        obj.SetValue(KeyUpCommandProperty, value);
    }

    private static void OnKeyUpCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            element.PreviewKeyUp -= KeyInputBehavior.Element_OnPreviewKeyUp;
            if (e.NewValue is ICommand)
            {
                element.PreviewKeyUp += KeyInputBehavior.Element_OnPreviewKeyUp;
            }
        }
    }

    private static void Element_OnPreviewKeyUp(object sender, KeyEventArgs e)
    {
        if (sender is UIElement element)
        {
            var command = GetKeyUpCommand(element);
            if (command?.CanExecute(e) ?? false)
            {
                command.Execute(e);
            }
        }
    }

    #endregion KeyUp
}
