using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

// ReSharper disable ArrangeStaticMemberQualifier

namespace Aloe.Common.AloeCoreLib.Wpf.Behaviors;

public static partial class OnEsc
{
    public static readonly DependencyProperty EnableCloseProperty =
        DependencyProperty.RegisterAttached(
            "EnableClose",
            typeof(bool),
            typeof(OnEsc),
            new PropertyMetadata(false, OnEnableCloseChanged));

    public static bool GetEnableClose(Window obj)
    {
        return (bool)obj.GetValue(EnableCloseProperty);
    }

    public static void SetEnableClose(Window obj, bool value)
    {
        obj.SetValue(EnableCloseProperty, value);
    }

    private static void OnEnableCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // PreviewKeyDownはバブルアップイベント(要素開始)の反対でトンネリングイベント(Window開始)となる。
        if (d is Window element)
        {
            if ((bool)e.NewValue)
            {
                element.PreviewKeyDown += CloseWindow_PreviewKeyDown;
            }
            else
            {
                element.PreviewKeyDown -= CloseWindow_PreviewKeyDown;
            }
        }
    }

    private static void CloseWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Escape)
        {
            if (Keyboard.FocusedElement is DependencyObject focused)
            {
                // VisualTree をたどって Window を探す
                var window = Window.GetWindow(focused);
                if (window != null)
                {
                    e.Handled = true;
                    window.Close();
                }
            }
        }
    }
}
