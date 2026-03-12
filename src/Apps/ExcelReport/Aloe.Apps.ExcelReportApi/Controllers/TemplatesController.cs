// <copyright file="TemplatesController.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.ExcelReportApi.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Aloe.Apps.ExcelReportApi.Controllers;

/// <summary>
/// サーバー側テンプレートを管理するコントローラー。
/// </summary>
[ApiController]
[Route("api/templates")]
public class TemplatesController : ControllerBase
{
    private readonly ExcelReportApiOptions _options;

    /// <summary>
    /// <see cref="TemplatesController"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public TemplatesController(IOptions<ExcelReportApiOptions> options)
    {
        this._options = options.Value;
    }

    private string GetBasePath()
    {
        return Path.IsPathRooted(this._options.TemplatePath)
            ? this._options.TemplatePath
            : Path.Combine(AppContext.BaseDirectory, this._options.TemplatePath);
    }

    /// <summary>
    /// テンプレートファイルをアップロードして保存する。
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAsync(IFormFile file)
    {
        var fileName = Path.GetFileName(file.FileName);
        if (fileName.Contains('/') || fileName.Contains('\\'))
        {
            return this.BadRequest("ファイル名にパス区切り文字を含めることはできません。");
        }

        var basePath = this.GetBasePath();
        Directory.CreateDirectory(basePath);
        var destPath = Path.Combine(basePath, fileName);

        using var stream = System.IO.File.Create(destPath);
        await file.CopyToAsync(stream);

        return this.Ok(new { fileName });
    }

    /// <summary>
    /// 保存済みテンプレートの一覧を返す。
    /// </summary>
    [HttpGet]
    public IActionResult List()
    {
        var basePath = this.GetBasePath();
        if (!Directory.Exists(basePath))
        {
            return this.Ok(Array.Empty<string>());
        }

        var fileNames = Directory.GetFiles(basePath)
            .Select(Path.GetFileName)
            .ToArray();

        return this.Ok(fileNames);
    }

    /// <summary>
    /// 指定テンプレートを削除する。
    /// </summary>
    [HttpDelete("{templateName}")]
    public IActionResult Delete(string templateName)
    {
        if (templateName.Contains('/') || templateName.Contains('\\'))
        {
            return this.BadRequest("ファイル名にパス区切り文字を含めることはできません。");
        }

        var basePath = this.GetBasePath();
        var filePath = Path.Combine(basePath, templateName);
        if (!System.IO.File.Exists(filePath))
        {
            return this.NotFound($"テンプレート '{templateName}' が見つかりません。");
        }

        System.IO.File.Delete(filePath);
        return this.NoContent();
    }
}
