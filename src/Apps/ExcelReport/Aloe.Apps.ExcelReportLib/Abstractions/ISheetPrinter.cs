// <copyright file="ISheetPrinter.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.ExcelReportLib.Models;

namespace Aloe.Apps.ExcelReportLib.Abstractions;

/// <summary>
/// シートモデルをプリンターに送信する抽象インターフェース。
/// </summary>
public interface ISheetPrinter
{
    /// <summary>
    /// インストール済みプリンター名の一覧を返す。
    /// </summary>
    IReadOnlyList<string> GetInstalledPrinters();

    /// <summary>
    /// シートモデルを指定プリンターに印刷する。
    /// </summary>
    /// <param name="model">印刷するシートモデル。</param>
    /// <param name="options">印刷オプション。</param>
    void Print(SheetModel model, PrintOptions options);
}
