using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services.Dtos;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// カレンダー表示用データ構築サービスインターフェース
/// </summary>
public interface ICalendarDataService
{
    /// <summary>
    /// カレンダー表示用データを構築します（従来版：AppointmentStatsを受け取る）。
    /// </summary>
    /// <param name="appointments">予約DTOのリスト</param>
    /// <param name="mainStats">Mainリソース統計データ（日付文字列 -> 統計リスト）</param>
    /// <param name="mainStatsGrayedOut">Mainリソース統計のグレーアウト状態（日付文字列 -> グレーアウトフラグ）</param>
    /// <param name="holidays">祝日データ（日付文字列 -> 祝日名）</param>
    /// <param name="filterTimeSlots">フィルター用の時間帯リスト（"09:00"形式、nullの場合はフィルターなし）</param>
    /// <param name="equipmentStats">Equipmentリソース統計データ（日付文字列 -> 統計リスト、nullの場合は表示しない）</param>
    /// <returns>カレンダー表示用データDTO</returns>
    Task<CalendarDataDto> BuildCalendarDataAsync(
        IEnumerable<AppointmentDto> appointments,
        Dictionary<string, List<AppointmentStats>> mainStats,
        Dictionary<string, bool> mainStatsGrayedOut,
        Dictionary<string, string> holidays,
        List<string>? filterTimeSlots = null,
        Dictionary<string, List<AppointmentStats>>? equipmentStats = null);

    /// <summary>
    /// カレンダー表示用データを構築します（最適化版：FromSqlで取得した配列化済みデータを受け取る）。
    /// パフォーマンス最適化用：SQL側で配列化されたEquipmentResourceStatsDtoを受け取る。
    /// </summary>
    Task<CalendarDataDto> BuildCalendarDataFromOptimizedAsync(
        IEnumerable<AppointmentDto> appointments,
        Dictionary<string, List<AppointmentStats>> mainStats,
        Dictionary<string, bool> mainStatsGrayedOut,
        Dictionary<string, string> holidays,
        List<string>? filterTimeSlots = null,
        Dictionary<string, List<EquipmentResourceStatsDto>>? equipmentStats = null);
}

