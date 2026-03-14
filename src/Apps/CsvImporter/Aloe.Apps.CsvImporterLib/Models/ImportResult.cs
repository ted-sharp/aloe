// <copyright file="ImportResult.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.CsvImporterLib.Models;

/// <summary>
/// インポート実行結果。
/// </summary>
/// <param name="Success">成功したかどうか。</param>
/// <param name="RowsLoaded">ロードした行数。</param>
/// <param name="StartedAt">開始時刻。</param>
/// <param name="FinishedAt">終了時刻。</param>
/// <param name="ErrorMessage">エラーメッセージ（失敗時のみ）。</param>
public sealed record ImportResult(
    bool Success,
    long RowsLoaded,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt,
    string? ErrorMessage = null);
