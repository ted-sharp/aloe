// <copyright file="ImageData.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.ExcelReportLib.Models;

/// <summary>
/// Excelに埋め込まれた画像の情報を保持するモデル。
/// </summary>
public class ImageData
{
    /// <summary>画像の左上アンカー。</summary>
    public CellAnchor From { get; set; } = new();

    /// <summary>画像の右下アンカー。</summary>
    public CellAnchor To { get; set; } = new();

    /// <summary>画像のバイナリデータ。</summary>
    public byte[] ImageBytes { get; set; } = [];

    /// <summary>画像のMIMEタイプ(例: "image/png")。</summary>
    public string MimeType { get; set; } = "image/png";
}
