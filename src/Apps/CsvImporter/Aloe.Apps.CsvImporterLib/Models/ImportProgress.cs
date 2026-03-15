// <copyright file="ImportProgress.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.CsvImporterLib.Models;

/// <summary>
/// インポート処理の進捗ステージ。
/// </summary>
public enum ImportProgressStage
{
    /// <summary>ダウンロード中。</summary>
    Downloading,

    /// <summary>インポート中。</summary>
    Importing,
}

/// <summary>
/// インポート処理の進捗情報。
/// </summary>
/// <param name="Stage">現在のステージ。</param>
/// <param name="Percent">進捗パーセント（0-100）。null の場合は不確定。</param>
public sealed record ImportProgress(
    ImportProgressStage Stage,
    int? Percent = null);
