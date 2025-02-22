using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Aloe.Common.AloeCoreLib.Wpf.Behaviors;

public static partial class OnEnter
{
    public static readonly DependencyProperty UpdateSourceProperty =
        DependencyProperty.RegisterAttached(
            "UpdateSource",
            typeof(bool),
            typeof(OnEnter),
            new PropertyMetadata(false, OnEnter.OnUpdateSourceChanged));

    public static bool GetUpdateSource(TextBox obj)
    {
        return (bool)obj.GetValue(OnEnter.UpdateSourceProperty);
    }

    public static void SetUpdateSource(TextBox obj, bool value)
    {
        obj.SetValue(OnEnter.UpdateSourceProperty, value);
    }

    private static void OnUpdateSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox element)
        {
            if ((bool)e.NewValue)
            {
                element.KeyDown += OnEnter.TextBox_KeyDown;
            }
            else
            {
                element.KeyDown -= OnEnter.TextBox_KeyDown;
            }
        }
    }

    private static void TextBox_KeyDown(object sender, KeyEventArgs e)
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
