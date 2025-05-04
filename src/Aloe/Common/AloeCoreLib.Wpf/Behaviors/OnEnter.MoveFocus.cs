using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

// ReSharper disable ArrangeStaticMemberQualifier

namespace Aloe.Common.AloeCoreLib.Wpf.Behaviors;

public static partial class OnEnter
{
    public static readonly DependencyProperty EnableMoveFocusProperty =
        DependencyProperty.RegisterAttached(
            "EnableMoveFocus",
            typeof(bool),
            typeof(OnEnter),
            new PropertyMetadata(false, OnEnableMoveFocusChanged));

    public static bool GetEnableMoveFocus(Window obj)
    {
        return (bool)obj.GetValue(EnableMoveFocusProperty);
    }

    public static void SetEnableMoveFocus(Window obj, bool value)
    {
        obj.SetValue(EnableMoveFocusProperty, value);
    }

    private static void OnEnableMoveFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // PreviewKeyDownはバブルアップイベント(要素開始)の反対でトンネリングイベント(Window開始)となる。
        if (d is Window element)
        {
            if ((bool)e.NewValue)
            {
                element.PreviewKeyDown += MoveFocusWindow_PreviewKeyDown;
            }
            else
            {
                element.PreviewKeyDown -= MoveFocusWindow_PreviewKeyDown;
            }
        }
    }

    private static void MoveFocusWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            if (Keyboard.FocusedElement is UIElement focusedElement)
            {
                e.Handled = true;

                if (focusedElement is Button button)
                {
                    // Button なら Click を発火
                    button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    return;
                }

                var direction = FocusNavigationDirection.Next;
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    direction = FocusNavigationDirection.Previous;
                }

                var request = new TraversalRequest(direction);
                focusedElement.MoveFocus(request);
            }

        }
    }
}
