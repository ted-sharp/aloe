// <copyright file="JisLevel2Converter.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System;
using System.Text;

namespace Aloe.Utils.Wafu.JisCompat;

/// <summary>
/// JIS Level 2 に縮退します。
/// </summary>
public static class JisLevel2Converter
{
    /// <summary>
    /// 第3水準、第4水準漢字を第2水準漢字に縮退します。
    /// null を入力した場合は null を返します。
    /// </summary>
    /// <param name="input">変換対象の文字列</param>
    /// <returns>変換後の文字列</returns>
    public static string? ConvertToLevel2(string? input)
    {
        if (input is null)
        {
            return null;
        }

        var length = input.Length;
        if (length == 0)
        {
            return String.Empty;
        }

        // Dictionary をローカル変数にキャッシュ
        var mapping = JisLevel2Mapping.Map;

        // string.Create を使って一度だけ文字列を確保し、高速に変換
        return String.Create(length, input, (span, src) =>
        {
            for (var i = 0; i < src.Length; i++)
            {
                var c = src[i];
                span[i] = mapping.TryGetValue(c, out char converted) ? converted : c;
            }
        });
    }
}
