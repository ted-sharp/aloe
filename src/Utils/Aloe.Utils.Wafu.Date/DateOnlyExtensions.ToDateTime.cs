// <copyright file="DateOnlyExtensions.ToDateTime.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

// ReSharper disable ArrangeStaticMemberQualifier
namespace Aloe.Utils.Wafu.Date;

/// <summary>
/// DateOnly型の拡張メソッドを提供するクラス
/// </summary>
/// <remarks>
/// このクラスは、DateOnly型の操作を簡素化するための拡張メソッドを提供します。
/// </remarks>
public static class DateOnlyExtensions
{
    /// <summary>
    /// DateOnlyをDateTimeに変換します。時刻は最小値（00:00:00）が設定されます。
    /// </summary>
    /// <param name="date">変換するDateOnly値</param>
    /// <returns>変換されたDateTime値</returns>
    public static DateTime ToDateTime(this DateOnly date) => date.ToDateTime(TimeOnly.MinValue);

    /// <summary>
    /// NullableなDateOnly型をNullableなDateTime型に変換します
    /// </summary>
    /// <param name="date">変換するNullableなDateOnly値</param>
    /// <returns>変換されたNullableなDateTime値。入力がnullの場合はnullを返します</returns>
    public static DateTime? ToDateTime(this DateOnly? date) => date?.ToDateTime(TimeOnly.MinValue);
}
