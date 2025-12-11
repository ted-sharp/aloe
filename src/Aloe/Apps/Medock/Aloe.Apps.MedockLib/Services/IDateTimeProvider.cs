using System;
using System.Collections.Generic;
using System.Text;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// アプリケーションで統一して使用する日時プロバイダ。
/// Now は「アプリ標準ローカル時刻」（JST 想定）を返す。
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// アプリ標準のローカル現在時刻（JST 前提）。
    /// DateTimeKind は Unspecified として扱う想定。
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// UTC 現在時刻。
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// アプリ標準ローカル日付（JST 今日 0:00）。
    /// </summary>
    DateTime Today { get; }

    /// <summary>
    /// アプリ標準のタイムゾーン（JST）。
    /// </summary>
    TimeZoneInfo TimeZone { get; }

    /// <summary>
    /// アプリ標準ローカル日付（DateOnly型）。
    /// EF Core + PostgreSQL の date 型マッピング用。
    /// </summary>
    DateOnly TodayDateOnly { get; }
}
