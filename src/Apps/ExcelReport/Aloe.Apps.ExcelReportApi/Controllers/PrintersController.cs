// <copyright file="PrintersController.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.ExcelReportLib.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Aloe.Apps.ExcelReportApi.Controllers;

/// <summary>
/// インストール済みプリンター一覧を返すコントローラー。
/// </summary>
[ApiController]
[Route("api/printers")]
public class PrintersController : ControllerBase
{
    private readonly ISheetPrinter _printer;

    /// <summary>
    /// <see cref="PrintersController"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public PrintersController(ISheetPrinter printer)
    {
        this._printer = printer;
    }

    /// <summary>
    /// インストール済みプリンター名の一覧を返す。
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        var printers = this._printer.GetInstalledPrinters();
        return this.Ok(new { printers });
    }
}
