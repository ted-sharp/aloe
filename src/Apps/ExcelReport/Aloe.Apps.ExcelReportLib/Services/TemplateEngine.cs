// <copyright file="TemplateEngine.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;
using Aloe.Apps.ExcelReportLib.Abstractions;
using Aloe.Apps.ExcelReportLib.Models;
using Microsoft.Extensions.Options;

namespace Aloe.Apps.ExcelReportLib.Services;

/// <summary>
/// <see cref="ITemplateEngine"/> のデフォルト実装。
/// セル値に含まれる <c>${keyword}</c> 形式のプレースホルダを辞書値で置換する。
/// プレフィックス/サフィックスは <see cref="TemplateOptions"/> で変更可能。
/// </summary>
public class TemplateEngine : ITemplateEngine
{
    private readonly TemplateOptions _options;

    /// <summary>
    /// <see cref="TemplateEngine"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    /// <param name="options">テンプレートオプション。</param>
    public TemplateEngine(IOptions<TemplateOptions> options)
    {
        _options = options?.Value ?? new TemplateOptions();
    }

    /// <summary>
    /// <see cref="TemplateEngine"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public TemplateEngine()
    {
        _options = new TemplateOptions();
    }

    /// <inheritdoc />
    public SheetModel Process(SheetModel model, IReadOnlyDictionary<string, string> variables)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(variables);

        if (variables.Count == 0)
        {
            return model;
        }

        foreach (var cell in model.Cells)
        {
            if (string.IsNullOrEmpty(cell.Value))
            {
                continue;
            }

            cell.Value = ReplaceVariables(cell.Value, variables);
        }

        foreach (var shape in model.Shapes)
        {
            if (string.IsNullOrEmpty(shape.Text))
            {
                continue;
            }

            shape.Text = ReplaceVariables(shape.Text, variables);
        }

        return model;
    }

    /// <inheritdoc />
    public SheetModel ClearKeywords(SheetModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        foreach (var cell in model.Cells)
        {
            if (!string.IsNullOrEmpty(cell.Value))
            {
                cell.Value = Regex.Replace(cell.Value, @"\$\{[^}]*\}", string.Empty);
            }
        }

        foreach (var shape in model.Shapes)
        {
            if (!string.IsNullOrEmpty(shape.Text))
            {
                shape.Text = Regex.Replace(shape.Text, @"\$\{[^}]*\}", string.Empty);
            }
        }

        return model;
    }

    private string ReplaceVariables(string text, IReadOnlyDictionary<string, string> variables)
    {
        string prefix = _options.Prefix;
        string suffix = _options.Suffix;

        foreach (var kvp in variables)
        {
            string placeholder = $"{prefix}{kvp.Key}{suffix}";
            if (text.Contains(placeholder, StringComparison.Ordinal))
            {
                text = text.Replace(placeholder, kvp.Value, StringComparison.Ordinal);
            }
        }

        return text;
    }
}
