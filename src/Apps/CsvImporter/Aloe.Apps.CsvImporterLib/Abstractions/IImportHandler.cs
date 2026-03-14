// <copyright file="IImportHandler.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.CsvImporterLib.Models;

namespace Aloe.Apps.CsvImporterLib.Abstractions;

/// <summary>
/// CSV インポートハンドラーの契約。HandlerKey は CLI サブコマンド名と一致する。
/// </summary>
public interface IImportHandler
{
    /// <summary>
    /// CLI サブコマンド名と一致するハンドラーキー（例: "postal-code"）。
    /// </summary>
    string HandlerKey { get; }

    /// <summary>
    /// インポート処理を実行する。
    /// </summary>
    /// <param name="options">インポートオプション。</param>
    /// <param name="progress">進捗レポーター。null の場合は進捗を報告しない。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    Task<ImportResult> RunAsync(
        ImportOptions options,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
