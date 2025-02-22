using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Aloe.Common.AloeCoreLib.Wpf.Converters;

public sealed class StringContainsConverter : IValueConverter
{
    public required string Substring { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var headerText = this.ExtractHeaderText(value);
        if (String.IsNullOrEmpty(headerText))
        {
            return false;
        }

        return headerText.Contains(this.Substring, StringComparison.CurrentCulture);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }

    /// <summary>
    /// Header が TextBlock ならその Text を、文字列ならそのまま返す。
    /// それ以外の場合は VisualTree をたどって最初に見つかった TextBlock の Text を返す。
    /// </summary>
    private string ExtractHeaderText(object header)
    {
        switch (header)
        {
            case string s:
                return s;

            case TextBlock tb:
                return tb.Text;

            case FrameworkElement fe:
                // もしヘッダにテンプレートなどが入り、TextBlock がさらにネストされている場合は
                // VisualTree を検索して最初に見つかった TextBlock の Text を返す、など。
                // （ここでは簡易的に実装）
                var foundTextBlock = this.FindChildTextBlock(fe);
                if (foundTextBlock != null)
                {
                    return foundTextBlock.Text;
                }
                break;
        }
        return header.ToString();
    }

    private TextBlock FindChildTextBlock(DependencyObject parent)
    {
        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < count; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);
            if (child is TextBlock tb)
            {
                return tb;
            }
            var desc = this.FindChildTextBlock(child);
            if (desc != null)
            {
                return desc;
            }
        }
        return null;
    }
}
