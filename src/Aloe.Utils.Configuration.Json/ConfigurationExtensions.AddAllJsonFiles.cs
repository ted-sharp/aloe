// <copyright file="ConfigurationExtensions.AddAllJsonFiles.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

// ReSharper disable ArrangeStaticMemberQualifier
namespace Aloe.Utils.Configuration.Json;

/// <summary>
/// 複数のJSON設定ファイルを一括で追加するための拡張メソッドを提供します。
/// </summary>
public static partial class ConfigurationExtensions
{
    /// <summary>
    /// 指定されたパターンのJSON設定ファイルを設定ビルダーに一括で追加します。
    /// </summary>
    /// <param name="builder">設定ビルダー</param>
    /// <param name="pattern">追加するJSONファイルのパターン</param>
    /// <param name="recursive">再帰検索するかどうか</param>
    /// <param name="optional">ファイルが存在しない場合でもエラーを発生させないかどうか</param>
    /// <param name="reloadOnChange">ファイルの変更を監視し、変更があった場合に自動的に再読み込みするかどうか</param>
    /// <returns>メソッドチェーンのための設定ビルダー</returns>
    /// <remarks>
    /// 各ファイルは指定された設定で追加されます。
    /// </remarks>
    /// <exception cref="ArgumentNullException">builder または pattern が null の場合にスローされます。</exception>
    /// <exception cref="ArgumentException">pattern が空文字または空白のみの場合にスローされます。</exception>
    /// <exception cref="DirectoryNotFoundException">検索対象ディレクトリが存在しない場合にスローされます。</exception>
    /// <exception cref="FileNotFoundException">optional=false かつパターンにマッチするファイルが存在しない場合にスローされます。</exception>
    public static IConfigurationBuilder AddAllJsonFiles(
        this IConfigurationBuilder builder,
        string pattern,
        bool recursive = true,
        bool optional = false,
        bool reloadOnChange = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

        // IFileProvider からベースパスを取得（PhysicalFileProvider に限定）
        string basePath;
        var fileProvider = builder.GetFileProvider();
        if (fileProvider is PhysicalFileProvider physical)
        {
            basePath = physical.Root;
        }
        else
        {
            basePath = Directory.GetCurrentDirectory();
        }

        // パターンからディレクトリ部とファイルパターン部を分割
        var directoryPart = Path.GetDirectoryName(pattern);
        var filePattern = Path.GetFileName(pattern);

        var searchDir = String.IsNullOrWhiteSpace(directoryPart)
            ? basePath
            : Path.Combine(basePath, directoryPart);

        if (!Directory.Exists(searchDir))
        {
            throw new DirectoryNotFoundException($"検索対象ディレクトリが見つかりません: {searchDir}");
        }

        // ファイル列挙
        var files = Directory.EnumerateFiles(
            searchDir,
            filePattern,
            recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .ToList();

        if (!files.Any() && !optional)
        {
            throw new FileNotFoundException($"パターン '{pattern}' にマッチするファイルが見つかりませんでした。");
        }

        foreach (var fullPath in files)
        {
            if (String.IsNullOrWhiteSpace(fullPath))
            {
                throw new ArgumentException("ファイルパスが空です。", nameof(pattern));
            }

            var relativePath = Path.GetRelativePath(basePath, fullPath);
            builder.AddJsonFile(relativePath, optional, reloadOnChange);
        }

        return builder;
    }
}
