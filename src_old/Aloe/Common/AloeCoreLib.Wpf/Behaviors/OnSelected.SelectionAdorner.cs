using Aloe.Common.AloeCoreLib.Wpf.Adorners;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

// ReSharper disable ArrangeStaticMemberQualifier

namespace Aloe.Common.AloeCoreLib.Wpf.Behaviors;

public static partial class OnSelected
{
    private static WeakReference<SelectionAdorner>? s_lastAdornerRef;
    private static WeakReference<FrameworkElement>? s_lastElementRef;

    public static readonly DependencyProperty EnableSelectionAdornerProperty =
        DependencyProperty.RegisterAttached(
            "EnableSelectionAdorner",
            typeof(bool),
            typeof(OnSelected),
            new PropertyMetadata(false, OnEnableSelectionAdornerChanged));

    public static bool GetEnableSelectionAdorner(FrameworkElement obj)
    {
        return (bool)obj.GetValue(EnableSelectionAdornerProperty);
    }

    public static void SetEnableSelectionAdorner(FrameworkElement obj, bool value)
    {
        obj.SetValue(EnableSelectionAdornerProperty, value);
    }

    private static void OnEnableSelectionAdornerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement element)
        {
            if ((bool)e.NewValue)
            {
                element.MouseDown += Element_MouseDown;
                element.Unloaded += Element_Unloaded;
            }
            else
            {
                element.MouseDown -= Element_MouseDown;
                element.Unloaded -= Element_Unloaded;
            }
        }
    }

    private static void Element_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement element) return;

        // 前の選択を解除
        if (s_lastElementRef?.TryGetTarget(out var lastElement) == true &&
            s_lastAdornerRef?.TryGetTarget(out var lastAdorner) == true)
        {
            var oldLayer = AdornerLayer.GetAdornerLayer(lastElement);
            oldLayer?.Remove(lastAdorner);
        }

        // 新しい選択に Adorner を付加
        var layer = AdornerLayer.GetAdornerLayer(element);
        if (layer != null)
        {
            var adorner = new SelectionAdorner(element);
            layer.Add(adorner);

            s_lastElementRef = new WeakReference<FrameworkElement>(element);
            s_lastAdornerRef = new WeakReference<SelectionAdorner>(adorner);
        }
    }


    private static void Element_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is UIElement element &&
            s_lastElementRef?.TryGetTarget(out var lastElement) == true &&
            s_lastAdornerRef?.TryGetTarget(out var lastAdorner) == true &&
            element == lastElement)
        {
            var layer = AdornerLayer.GetAdornerLayer(lastElement);
            layer?.Remove(lastAdorner);
            s_lastElementRef = null;
            s_lastAdornerRef = null;
        }
    }

}
