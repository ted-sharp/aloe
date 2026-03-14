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
    Task<ImportResult> RunAsync(ImportOptions options, CancellationToken cancellationToken = default);
}
