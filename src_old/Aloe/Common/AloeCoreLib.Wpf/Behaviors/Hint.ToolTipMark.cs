using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows;
using Aloe.Common.AloeCoreLib.Wpf.Adorners;
using System.Windows.Controls;

namespace Aloe.Common.AloeCoreLib.Wpf.Behaviors;

public static partial class Hint
{
    public static readonly DependencyProperty EnableToolTipMarkProperty =
        DependencyProperty.RegisterAttached(
            "EnableToolTipMark",
            typeof(bool),
            typeof(Hint),
            new PropertyMetadata(false, Hint.OnEnableToolTipMarkChanged));

    public static bool GetEnableToolTipMark(DependencyObject obj)
    {
        return (bool)obj.GetValue(Hint.EnableToolTipMarkProperty);
    }

    public static void SetEnableToolTipMark(DependencyObject obj, bool value)
    {
        obj.SetValue(Hint.EnableToolTipMarkProperty, value);
    }

    private static void OnEnableToolTipMarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement element)
        {
            // Loaded時に処理する（VisualTreeが構築されるのを待つ）
            element.Loaded -= Hint.ToolTipMarkElement_Loaded;
            element.Loaded += Hint.ToolTipMarkElement_Loaded;
        }
    }

    private static void ToolTipMarkElement_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            ShowToolTipMark(element);
        }
    }

    private static void ShowToolTipMark(FrameworkElement element)
    {
        var adornerLayer = AdornerLayer.GetAdornerLayer(element);
        if (adornerLayer == null)
        {
            return;
        }

        var toolTipAdorner = adornerLayer.GetAdorners(element)
            ?.OfType<ToolTipMarkAdorner>()
            .FirstOrDefault();
        if (toolTipAdorner is null)
        {
            adornerLayer.Add(new ToolTipMarkAdorner(element));
        }
    }
}
