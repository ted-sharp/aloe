// <copyright file="IExcelReader.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.ExcelReportLib.Models;

namespace Aloe.Apps.ExcelReportLib.Abstractions;

/// <summary>
/// Excelファイルを読み取り、ライブラリ非依存の中間モデルに変換するインターフェース。
/// NPOI, ClosedXML などの具象実装をDIで切り替えて使用する。
/// </summary>
public interface IExcelReader
{
    /// <summary>
    /// Excelファイルのストリームからシート情報を読み取り、<see cref="SheetModel"/> を返す。
    /// コメント・マクロ・数式は除外され、セル値は評価済みの文字列として格納される。
    /// </summary>
    /// <param name="stream">Excelファイル(.xlsx)のストリーム。</param>
    /// <param name="options">読み取りオプション。null の場合はデフォルト。</param>
    /// <returns>読み取ったシートの中間モデル。</returns>
    SheetModel Read(Stream stream, ExcelReadOptions? options = null);
}
