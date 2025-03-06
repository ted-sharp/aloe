using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Aloe.Common.AloeCoreLib.Wpf.Behaviors;

public static partial class OnEnter
{
    public static readonly DependencyProperty EnableUpdateSourceProperty =
        DependencyProperty.RegisterAttached(
            "EnableUpdateSource",
            typeof(bool),
            typeof(OnEnter),
            new PropertyMetadata(false, OnEnter.OnEnableUpdateSourceChanged));

    public static bool GetEnableUpdateSource(TextBox obj)
    {
        return (bool)obj.GetValue(OnEnter.EnableUpdateSourceProperty);
    }

    public static void SetEnableUpdateSource(TextBox obj, bool value)
    {
        obj.SetValue(OnEnter.EnableUpdateSourceProperty, value);
    }

    private static void OnEnableUpdateSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox element)
        {
            if ((bool)e.NewValue)
            {
                element.KeyDown += OnEnter.UpdateSourceTextBox_KeyDown;
            }
            else
            {
                element.KeyDown -= OnEnter.UpdateSourceTextBox_KeyDown;
            }
        }
    }

    private static void UpdateSourceTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            if (sender is TextBox textBox)
            {
                var bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
                if (bindingExpression != null)
                {
                    bindingExpression.UpdateSource();
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }
        }
    }
}
