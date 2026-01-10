using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// アプリ標準タイムゾーン（デフォルト JST）を用いる DateTime プロバイダ。
/// </summary>
public sealed class JstDateTimeProvider : IDateTimeProvider
{
    public TimeZoneInfo TimeZone { get; }

    /// <summary>
    /// デフォルトは JST（Windows: Tokyo Standard Time / Linux: Asia/Tokyo）。
    /// </summary>
    public JstDateTimeProvider()
        : this(GetDefaultTimeZone())
    {
    }

    /// <summary>
    /// 任意の TimeZoneInfo を指定するコンストラクタ。
    /// （テストや別 TZ 用）
    /// </summary>
    public JstDateTimeProvider(TimeZoneInfo timeZone)
    {
        this.TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
    }

    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime Now
    {
        get
        {
            // UTC → アプリ標準 TZ に変換
            var local = TimeZoneInfo.ConvertTimeFromUtc(this.UtcNow, this.TimeZone);

            // OS ローカルとは切り離したいので Kind=Unspecified に統一して返す
            return DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        }
    }

    public DateTime Today => this.Now.Date;

    public DateOnly TodayDateOnly => DateOnly.FromDateTime(this.Now);

    public DateTime NowRoundedToSeconds
    {
        get
        {
            var now = this.Now;
            return new DateTime(now.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond);
        }
    }

    public DateTime RoundToSeconds(DateTime dateTime)
    {
        return new DateTime(dateTime.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond);
    }

    private static TimeZoneInfo GetDefaultTimeZone()
    {
        // 環境に応じて JST を取得
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows: "Tokyo Standard Time"
            return TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
        }

        // Linux / macOS: "Asia/Tokyo"
        return TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo");
    }
}
