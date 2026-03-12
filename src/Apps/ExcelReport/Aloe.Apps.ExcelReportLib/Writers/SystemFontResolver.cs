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
/// カスタムフォントディレクトリ（デフォルト: 実行ファイル横の fonts/）を優先検索する。
/// </summary>
public class SystemFontResolver : IFontResolver
{
    private static readonly string FontDir =
        Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

    private readonly string _customFontDir;

    /// <summary>
    /// デフォルトコンストラクタ。カスタムフォントディレクトリは AppContext.BaseDirectory/fonts/ を使用する。
    /// </summary>
    public SystemFontResolver()
        : this(Path.Combine(AppContext.BaseDirectory, "fonts"))
    {
    }

    /// <summary>
    /// カスタムフォントディレクトリを指定してインスタンスを作成する。
    /// </summary>
    /// <param name="customFontDir">TTF ファイルを配置するカスタムフォントディレクトリのパス。</param>
    public SystemFontResolver(string customFontDir)
    {
        _customFontDir = customFontDir;
    }

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

    /// <summary>
    /// フォント名とスタイルに対応するフォントファイルのパスを返す。見つからない場合は null。
    /// </summary>
    /// <param name="fontName">フォントファミリー名。</param>
    /// <param name="bold">太字かどうか。</param>
    /// <returns>フォントファイルのフルパス。見つからない場合は null。</returns>
    public string? GetFontFilePath(string fontName, bool bold)
    {
        string faceName = bold ? fontName + "|b" : fontName;
        return ResolveFontPath(faceName);
    }

    private string? ResolveFontPath(string faceName)
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

    private string? TryFont(string fileName)
    {
        string customPath = Path.Combine(_customFontDir, fileName);
        if (File.Exists(customPath))
        {
            return customPath;
        }

        string systemPath = Path.Combine(FontDir, fileName);
        return File.Exists(systemPath) ? systemPath : null;
    }

    private string? TryFontByPattern(string familyName)
    {
        string pattern = familyName.Replace(" ", string.Empty, StringComparison.Ordinal);

        if (Directory.Exists(_customFontDir))
        {
            foreach (string file in Directory.EnumerateFiles(_customFontDir, "*.ttf"))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                if (name.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return file;
                }
            }
        }

        if (!Directory.Exists(FontDir))
        {
            return null;
        }

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
