// <copyright file="ConfigurationExtensions.AddJsonFileIf.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

// ReSharper disable ArrangeStaticMemberQualifier
namespace Aloe.Utils.Configuration.Json;

/// <summary>
/// ConfigurationBuilderの拡張メソッドを提供します。
/// </summary>
public static partial class ConfigurationExtensions
{
    /// <summary>
    /// 条件がtrueの場合にJSON設定ファイルを追加します。
    /// </summary>
    /// <param name="builder">設定ビルダー</param>
    /// <param name="condition">追加条件</param>
    /// <param name="path">JSONファイルのパス</param>
    /// <param name="optional">ファイルが存在しない場合に例外をスローしない場合はtrue</param>
    /// <param name="reloadOnChange">ファイルの変更を監視する場合はtrue</param>
    /// <returns>メソッドチェーンのための設定ビルダー</returns>
    public static IConfigurationBuilder AddJsonFileIf(
        this IConfigurationBuilder builder,
        bool condition,
        string path,
        bool optional = false,
        bool reloadOnChange = false)
    {
        if (condition)
        {
            builder.AddJsonFile(path, optional, reloadOnChange);
        }

        return builder;
    }

    /// <summary>
    /// 条件がtrueの場合にJSON設定ファイルを追加します。
    /// </summary>
    /// <param name="builder">設定ビルダー</param>
    /// <param name="condition">追加条件</param>
    /// <param name="provider">ファイルプロバイダー</param>
    /// <param name="path">JSONファイルのパス</param>
    /// <param name="optional">ファイルが存在しない場合に例外をスローしない場合はtrue</param>
    /// <param name="reloadOnChange">ファイルの変更を監視する場合はtrue</param>
    /// <returns>メソッドチェーンのための設定ビルダー</returns>
    public static IConfigurationBuilder AddJsonFileIf(
        this IConfigurationBuilder builder,
        bool condition,
        IFileProvider provider,
        string path,
        bool optional = false,
        bool reloadOnChange = false)
    {
        if (condition)
        {
            builder.AddJsonFile(provider, path, optional, reloadOnChange);
        }

        return builder;
    }
}
