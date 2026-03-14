// <copyright file="ImportMode.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.CsvImporterLib.Models;

/// <summary>
/// インポートモード。
/// </summary>
public enum ImportMode
{
    /// <summary>前回モードを自動判定（実装依存）。</summary>
    Auto,

    /// <summary>差分取り込み（追加・削除差分ファイルを使用）。</summary>
    Delta,

    /// <summary>全件洗い替え（全量ファイルを使用）。</summary>
    Full,
}
