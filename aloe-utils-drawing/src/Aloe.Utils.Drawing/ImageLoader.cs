// <copyright file="ImageLoader.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Drawing;
using System.Runtime.Versioning;

namespace Aloe.Utils.Drawing;

/// <summary>
/// GDI+を利用して画像ファイルを読み込むためのクラス
/// </summary>
[SupportedOSPlatform("windows")]
public static class ImageLoader
{
    /// <summary>
    /// ファイルシステム上の画像ファイルからファイルロックしない形で Image を生成します。
    /// PNG, JPEG, BMP, GIF, TIFF等に対応しています。
    /// </summary>
    /// <param name="filePath">画像ファイルのパス</param>
    /// <returns>生成された Image</returns>
    /// <exception cref="ArgumentException">ファイルパスがnullまたは空文字列の場合。</exception>
    /// <exception cref="FileNotFoundException">指定されたファイルが存在しない場合。</exception>
    /// <exception cref="InvalidOperationException">画像の読み込みに失敗した場合。</exception>
    public static Image Load(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Image file not found: {filePath}");
        }
        
        try
        {
            var imageData = File.ReadAllBytes(filePath);
            using var ms = new MemoryStream(imageData);
            return Image.FromStream(ms);
        }
        catch (Exception ex) when (ex is not FileNotFoundException && ex is not ArgumentNullException)
        {
            var errorMessage = ex switch
            {
                OutOfMemoryException => $"Image file is corrupted or unsupported format: {filePath}",
                ArgumentException => $"Invalid image data or unsupported format: {filePath}",
                _ => $"Failed to load image from {filePath}"
            };
            throw new InvalidOperationException(errorMessage, ex);
        }
    }
}
