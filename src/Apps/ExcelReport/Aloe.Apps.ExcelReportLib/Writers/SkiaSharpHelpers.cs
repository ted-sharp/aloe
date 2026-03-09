// <copyright file="SkiaSharpHelpers.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace Aloe.Apps.ExcelReportLib.Writers;

/// <summary>
/// QuestPDFとSkiaSharpの統合用ヘルパー。
/// QuestPDF公式ドキュメントのSkiaSharp Integrationパターンに基づく。
/// </summary>
internal static class SkiaSharpHelpers
{
    /// <summary>
    /// SkiaSharpのSVGキャンバスを使用してベクター描画を行う。
    /// </summary>
    public static void SkiaSharpSvgCanvas(this IContainer container, Action<SKCanvas, Size> drawOnCanvas)
    {
        container.Svg(size =>
        {
            using var stream = new MemoryStream();

            using (var canvas = SKSvgCanvas.Create(new SKRect(0, 0, size.Width, size.Height), stream))
            {
                drawOnCanvas(canvas, size);
            }

            var svgData = stream.ToArray();
            return Encoding.UTF8.GetString(svgData);
        });
    }
}
