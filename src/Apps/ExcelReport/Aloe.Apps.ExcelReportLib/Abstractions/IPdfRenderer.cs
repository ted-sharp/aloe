// <copyright file="IPdfRenderer.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.ExcelReportLib.Models;

namespace Aloe.Apps.ExcelReportLib.Abstractions;

/// <summary>
/// <see cref="SheetModel"/> からPDFを生成するインターフェース。
/// PDFsharp, QuestPDF などの具象実装をDIで切り替えて使用する。
/// </summary>
public interface IPdfRenderer
{
    /// <summary>
    /// シートモデルをPDFとして出力ストリームに描画する。
    /// セル位置・罫線・図形・画像を絶対座標で配置する。
    /// </summary>
    /// <param name="model">描画対象のシートモデル。</param>
    /// <param name="output">PDF出力先のストリーム。</param>
    /// <param name="options">PDF描画オプション。null の場合はデフォルト。</param>
    void Render(SheetModel model, Stream output, PdfRenderOptions? options = null);
}
