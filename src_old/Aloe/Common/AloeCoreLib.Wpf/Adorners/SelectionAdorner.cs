using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using System.Globalization;

namespace Aloe.Common.AloeCoreLib.Wpf.Adorners;

public class SelectionAdorner : Adorner
{
    private static readonly Brush s_overlayBrush = new SolidColorBrush(Color.FromArgb(64, 0, 122, 204));

    public SelectionAdorner(UIElement adornedElement)
        : base(adornedElement)
    {
        this.IsHitTestVisible = false;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        drawingContext.DrawRectangle(SelectionAdorner.s_overlayBrush, null, new Rect(this.AdornedElement.RenderSize));
    }
}
