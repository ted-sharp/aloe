using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;

namespace Aloe.Common.AloeCoreLib.Wpf.Adorners;

public class WatermarkAdorner : Adorner
{
    private readonly VisualCollection _visuals;
    private readonly TextBlock _textBlock;

    public WatermarkAdorner(UIElement adornedElement, string watermarkText)
        : base(adornedElement)
    {
        this._visuals = new VisualCollection(this);
        this._textBlock = new TextBlock
        {
            Text = watermarkText,
            Foreground = Brushes.LightGray,
            Margin = new Thickness(5, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = false
        };
        this._visuals.Add(this._textBlock);
        this.IsHitTestVisible = false;
    }

    /// <summary>
    /// ウォーターマークテキストの更新
    /// </summary>
    /// <param name="watermarkText">新しいウォーターマークテキスト</param>
    public void UpdateText(string watermarkText)
    {
        this._textBlock.Text = watermarkText;
    }

    #region Adorner

    protected override Size ArrangeOverride(Size finalSize)
    {
        this._textBlock.Arrange(new Rect(finalSize));
        return finalSize;
    }

    protected override int VisualChildrenCount => this._visuals.Count;

    protected override Visual GetVisualChild(int index) => this._visuals[index];

    #endregion Adorner
}
