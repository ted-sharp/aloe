// <copyright file="SystemFontResolver.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Globalization;
using PdfSharp.Fonts;

namespace Aloe.Apps.ExcelReportLib.Writers;

/// <summary>
/// Windows のシステムフォントフォルダからフォントを解決する <see cref="IFontResolver"/> 実装。
/// PDFsharp 6.x Core ビルドではデフォルトのフォントリゾルバが日本語フォントに対応しないため、
/// このクラスでフォントファミリー名からフォントファイルへのマッピングを行う。
/// </summary>
public class SystemFontResolver : IFontResolver
{
    private static readonly string FontDir =
        Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

    /// <summary>
    /// フォント名とスタイルから対応するフォントファイルの情報を返す。
    /// </summary>
    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        string key = BuildFaceKey(familyName, isBold, isItalic);
        return new FontResolverInfo(key, mustSimulateBold: false, mustSimulateItalic: isItalic);
    }

    /// <summary>
    /// フォントキーに対応するフォントファイルのバイト列を返す。
    /// </summary>
    public byte[]? GetFont(string faceName)
    {
        string? path = ResolveFontPath(faceName);
        if (path != null && File.Exists(path))
        {
            return File.ReadAllBytes(path);
        }

        // 最終フォールバック: Arial
        string arialPath = Path.Combine(FontDir, "arial.ttf");
        if (File.Exists(arialPath))
        {
            return File.ReadAllBytes(arialPath);
        }

        return null;
    }

    private static string BuildFaceKey(string familyName, bool isBold, bool isItalic)
    {
        string suffix = (isBold, isItalic) switch
        {
            (true, true) => "|bi",
            (true, false) => "|b",
            (false, true) => "|i",
            _ => string.Empty,
        };
        return familyName + suffix;
    }

    private static string? ResolveFontPath(string faceName)
    {
        int sep = faceName.IndexOf('|', StringComparison.Ordinal);
        string family = sep >= 0 ? faceName[..sep] : faceName;
        string style = sep >= 0 ? faceName[(sep + 1)..] : string.Empty;
        bool bold = style.Contains('b', StringComparison.Ordinal);

        string normalized = family.ToLower(CultureInfo.InvariantCulture)
            .Replace(" ", string.Empty, StringComparison.Ordinal);

        // 游ゴシック / Yu Gothic
        if (normalized is "yugothic" or "游ゴシック" or "游ゴシック体")
        {
            return TryFont("NotoSansJP-VF.ttf")
                ?? TryFont(bold ? "arialbd.ttf" : "arial.ttf");
        }

        // 游明朝 / Yu Mincho
        if (normalized is "yumincho" or "游明朝" or "游明朝体")
        {
            return TryFont("NotoSerifJP-VF.ttf")
                ?? TryFont(bold ? "arialbd.ttf" : "arial.ttf");
        }

        // メイリオ / Meiryo
        if (normalized is "meiryo" or "メイリオ")
        {
            return TryFont("NotoSansJP-VF.ttf")
                ?? TryFont(bold ? "arialbd.ttf" : "arial.ttf");
        }

        // MS ゴシック / MS Gothic
        if (normalized is "msgothic" or "msゴシック" or "mspゴシック" or "mspgothic")
        {
            return TryFont("NotoSansJP-VF.ttf")
                ?? TryFont(bold ? "arialbd.ttf" : "arial.ttf");
        }

        // MS 明朝 / MS Mincho
        if (normalized is "msmincho" or "ms明朝" or "msp明朝" or "mspmincho")
        {
            return TryFont("NotoSerifJP-VF.ttf")
                ?? TryFont(bold ? "arialbd.ttf" : "arial.ttf");
        }

        // 欧文フォント
        if (normalized is "arial")
        {
            return TryFont(bold ? "arialbd.ttf" : "arial.ttf");
        }

        if (normalized is "calibri")
        {
            return TryFont(bold ? "calibrib.ttf" : "calibri.ttf");
        }

        if (normalized is "timesnewroman")
        {
            return TryFont(bold ? "timesbd.ttf" : "times.ttf");
        }

        // 汎用フォールバック: ファイル名パターンで検索(TTF のみ)
        return TryFontByPattern(family);
    }

    private static string? TryFont(string fileName)
    {
        string path = Path.Combine(FontDir, fileName);
        return File.Exists(path) ? path : null;
    }

    private static string? TryFontByPattern(string familyName)
    {
        if (!Directory.Exists(FontDir))
        {
            return null;
        }

        string pattern = familyName.Replace(" ", string.Empty, StringComparison.Ordinal);
        foreach (string file in Directory.EnumerateFiles(FontDir, "*.ttf"))
        {
            string name = Path.GetFileNameWithoutExtension(file);
            if (name.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return file;
            }
        }

        return null;
    }
}
