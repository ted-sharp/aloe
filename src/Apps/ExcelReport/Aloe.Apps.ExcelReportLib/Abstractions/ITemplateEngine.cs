// <copyright file="ITemplateEngine.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.ExcelReportLib.Models;

namespace Aloe.Apps.ExcelReportLib.Abstractions;

/// <summary>
/// シートモデル内のセル値に含まれる置換キーワードを処理するインターフェース。
/// デフォルトでは <c>${keyword}</c> 形式のプレースホルダを辞書の値で置換する。
/// </summary>
public interface ITemplateEngine
{
    /// <summary>
    /// シートモデル内の全セル値に対して置換処理を実行し、新しいモデルを返す。
    /// </summary>
    /// <param name="model">元のシートモデル。</param>
    /// <param name="variables">キーワードと置換値の辞書。</param>
    /// <returns>置換処理済みのシートモデル。</returns>
    SheetModel Process(SheetModel model, IReadOnlyDictionary<string, string> variables);

    /// <summary>
    /// シートモデル内の全セル値から <c>${...}</c> 形式のプレースホルダを空文字列に消去する。
    /// </summary>
    /// <param name="model">元のシートモデル。</param>
    /// <returns>プレースホルダ消去済みのシートモデル。</returns>
    SheetModel ClearKeywords(SheetModel model);
}
