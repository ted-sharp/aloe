using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;

namespace Aloe.Common.AloeCoreLib.Wpf.Adorners;

public class ToolTipAdorner : Adorner
{
    public ToolTipAdorner(UIElement adornedElement)
        : base(adornedElement)
    {
    }

    //protected override void OnRender(DrawingContext drawingContext)
    //{
    //    base.OnRender(drawingContext);

    //    // 例として右上に小さな赤い円を描画する
    //    var radius = 5;
    //    var brush = Brushes.Tomato;
    //    var pen = new Pen(brush, 1);
    //    if (this.AdornedElement is FrameworkElement adornedElement)
    //    {
    //        var offset = new Point(adornedElement.ActualWidth - radius * 2, 0);
    //        drawingContext.DrawEllipse(brush, pen, new Point(offset.X + radius, offset.Y + radius), radius, radius);
    //    }
    //}

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        // 三角形のサイズと描画色、線の設定
        double triangleSize = 5.0;
        var fillBrush = Brushes.Tomato;
        var borderPen = new Pen(fillBrush, 1);

        if (this.AdornedElement is FrameworkElement adornedElement)
        {
            // テキストボックスなどの右上の角に合わせた三角形の頂点座標を定義
            // 右上の角の座標は (ActualWidth, 0)
            var point1 = new Point(adornedElement.ActualWidth, 0);
            // 右上から左に triangleSize 分ずらした点
            var point2 = new Point(adornedElement.ActualWidth - triangleSize, 0);
            // 右上から下に triangleSize 分ずらした点
            var point3 = new Point(adornedElement.ActualWidth, triangleSize);

            // StreamGeometry を用いて三角形を描画
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(point1, true, true);
                ctx.LineTo(point2, true, false);
                ctx.LineTo(point3, true, false);
            }
            geometry.Freeze();

            drawingContext.DrawGeometry(fillBrush, borderPen, geometry);
        }
    }
}
