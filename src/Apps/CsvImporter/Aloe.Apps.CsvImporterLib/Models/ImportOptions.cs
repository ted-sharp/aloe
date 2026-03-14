// <copyright file="ImportOptions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.CsvImporterLib.Models;

/// <summary>
/// インポート実行オプション。
/// </summary>
/// <param name="Mode">インポートモード。</param>
/// <param name="Yymm">差分モード時の対象年月（例: "2501"）。Full モードでは無視。</param>
/// <param name="ConnectionString">PostgreSQL 接続文字列。</param>
public sealed record ImportOptions(
    ImportMode Mode,
    string? Yymm,
    string ConnectionString);
