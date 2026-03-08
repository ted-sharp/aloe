// <copyright file="DateTimeExtensions.ToDateOnly.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

// ReSharper disable ArrangeStaticMemberQualifier
namespace Aloe.Utils.Wafu.Date;

/// <summary>
/// DateTime型の拡張メソッドを提供するクラス
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// DateTime型をDateOnly型に変換します
    /// </summary>
    /// <param name="dateTime">変換するDateTime値</param>
    /// <returns>変換されたDateOnly値</returns>
    public static DateOnly ToDateOnly(this DateTime dateTime) => DateOnly.FromDateTime(dateTime);

    /// <summary>
    /// NullableなDateTime型をNullableなDateOnly型に変換します
    /// </summary>
    /// <param name="dateTime">変換するNullableなDateTime値</param>
    /// <returns>変換されたNullableなDateOnly値。入力がnullの場合はnullを返します</returns>
    public static DateOnly? ToDateOnly(this DateTime? dateTime) => dateTime?.ToDateOnly();
}
