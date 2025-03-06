using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using Aloe.Common.AloeCoreLib.Wpf.Adorners;

namespace Aloe.Common.AloeCoreLib.Wpf.Behaviors;

public static partial class Hint
{
    // 添付プロパティ「Watermark」を定義
    public static readonly DependencyProperty WatermarkProperty = DependencyProperty.RegisterAttached(
        "Watermark",
        typeof(string),
        typeof(Hint),
        new FrameworkPropertyMetadata(String.Empty, FrameworkPropertyMetadataOptions.AffectsRender, OnWatermarkChanged));

    public static void SetWatermark(DependencyObject element, string value)
    {
        element.SetValue(WatermarkProperty, value);
    }

    public static string GetWatermark(DependencyObject element)
    {
        return (string)element.GetValue(WatermarkProperty);
    }

    private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            // Loaded イベントと TextChanged イベントでウォーターマークの表示を更新する
            textBox.Loaded += Hint.WatermarkTextBox_Loaded;
            textBox.TextChanged += Hint.WatermarkTextBox_TextChanged;
        }
    }

    private static void WatermarkTextBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            ShowOrHideWatermark(textBox);
        }
    }

    private static void WatermarkTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            ShowOrHideWatermark(textBox);
        }
    }

    private static void ShowOrHideWatermark(TextBox textBox)
    {
        var adornerLayer = AdornerLayer.GetAdornerLayer(textBox);
        if (adornerLayer == null)
        {
            return;
        }

        var watermarkAdorner = adornerLayer.GetAdorners(textBox)
            ?.OfType<WatermarkAdorner>()
            .FirstOrDefault();

        // TextBox が空の場合はウォーターマークを表示
        if (String.IsNullOrEmpty(textBox.Text))
        {
            if (watermarkAdorner == null)
            {
                watermarkAdorner = new WatermarkAdorner(textBox, GetWatermark(textBox));
                adornerLayer.Add(watermarkAdorner);
            }
            else
            {
                watermarkAdorner.UpdateText(GetWatermark(textBox));
                watermarkAdorner.Visibility = Visibility.Visible;
            }
        }
        else
        {
            if (watermarkAdorner != null)
            {
                watermarkAdorner.Visibility = Visibility.Collapsed;
            }
        }
    }
}
