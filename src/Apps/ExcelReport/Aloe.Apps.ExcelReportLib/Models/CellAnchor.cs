// <copyright file="CellAnchor.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.ExcelReportLib.Models;

/// <summary>
/// 図形や画像のアンカー位置を表すモデル。
/// セルインデックスとEMU(English Metric Units)オフセットで位置を特定する。
/// 1 inch = 914,400 EMU, 1 point = 12,700 EMU。
/// </summary>
public class CellAnchor
{
    /// <summary>行インデックス(0-based)。</summary>
    public int Row { get; set; }

    /// <summary>列インデックス(0-based)。</summary>
    public int Column { get; set; }

    /// <summary>セル内の行方向オフセット(EMU)。</summary>
    public long RowOffsetEmu { get; set; }

    /// <summary>セル内の列方向オフセット(EMU)。</summary>
    public long ColumnOffsetEmu { get; set; }
}
