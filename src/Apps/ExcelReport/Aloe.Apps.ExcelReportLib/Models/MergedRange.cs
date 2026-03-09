// <copyright file="MergedRange.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.ExcelReportLib.Models;

/// <summary>
/// 結合セルの範囲を表すモデル(0-based)。
/// </summary>
public class MergedRange
{
    /// <summary>結合範囲の開始行。</summary>
    public int FirstRow { get; set; }

    /// <summary>結合範囲の開始列。</summary>
    public int FirstColumn { get; set; }

    /// <summary>結合範囲の終了行。</summary>
    public int LastRow { get; set; }

    /// <summary>結合範囲の終了列。</summary>
    public int LastColumn { get; set; }

    /// <summary>
    /// 指定されたセルがこの結合範囲内にあるかどうかを判定する。
    /// </summary>
    public bool Contains(int row, int column)
    {
        return row >= FirstRow && row <= LastRow
            && column >= FirstColumn && column <= LastColumn;
    }
}
