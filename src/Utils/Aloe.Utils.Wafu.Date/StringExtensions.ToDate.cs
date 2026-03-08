// <copyright file="StringExtensions.ToDate.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

// ReSharper disable ArrangeStaticMemberQualifier
using System.ComponentModel;

namespace Aloe.Utils.Wafu.Date;

/// <summary>
/// String型の拡張メソッドを提供するクラス
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// 文字列をDateOnlyに変換します。変換できない場合は指定の日付を返します。
    /// </summary>
    /// <param name="dateString">変換する日付文字列</param>
    /// <param name="date">変換できない場合の日付</param>
    /// <returns>変換されたDateOnly値。変換できない場合は指定の日付</returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static DateOnly ToDateOr(this string dateString, DateOnly date)
    {
        if (String.IsNullOrWhiteSpace(dateString))
        {
            return date;
        }

        if (DateHelper.TryParseEx(dateString, out var date2))
        {
            return date2;
        }

        return date;
    }

    /// <summary>
    /// 文字列をDateOnlyに変換します。変換できない場合は今日の日付を返します。
    /// </summary>
    /// <param name="dateString">変換する日付文字列</param>
    /// <returns>変換されたDateOnly値。変換できない場合は今日の日付</returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static DateOnly ToDateOrToday(this string dateString)
    {
        return dateString.ToDateOr(DateHelper.GetToday());
    }
}
