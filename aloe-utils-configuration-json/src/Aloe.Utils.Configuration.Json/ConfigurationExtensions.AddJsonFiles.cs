// <copyright file="ConfigurationExtensions.AddJsonFiles.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Microsoft.Extensions.Configuration;

// ReSharper disable ArrangeStaticMemberQualifier
namespace Aloe.Utils.Configuration.Json;

/// <summary>
/// 複数のJSON設定ファイルを一括で追加するための拡張メソッドを提供します。
/// </summary>
public static partial class ConfigurationExtensions
{
    /// <summary>
    /// 複数のJSON設定ファイルを設定ビルダーに一括で追加します。
    /// </summary>
    /// <param name="builder">設定ビルダー</param>
    /// <param name="files">追加するJSONファイルのパス一覧</param>
    /// <param name="optional">ファイルが存在しない場合でもエラーを発生させないかどうか</param>
    /// <param name="reloadOnChange">ファイルの変更を監視し、変更があった場合に自動的に再読み込みするかどうか</param>
    /// <returns>メソッドチェーンのための設定ビルダー</returns>
    /// <remarks>
    /// 各ファイルは指定された設定で追加されます。
    /// </remarks>
    /// <exception cref="ArgumentNullException">builder または files が null の場合にスローされます。</exception>
    /// <exception cref="ArgumentException">files に空のパスが含まれる場合にスローされます。</exception>
    public static IConfigurationBuilder AddJsonFiles(
        this IConfigurationBuilder builder,
        IEnumerable<string> files,
        bool optional = false,
        bool reloadOnChange = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(files);

        foreach (var file in files)
        {
            if (String.IsNullOrWhiteSpace(file))
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(file);
            }

            builder.AddJsonFile(file, optional, reloadOnChange);
        }

        return builder;
    }
}
