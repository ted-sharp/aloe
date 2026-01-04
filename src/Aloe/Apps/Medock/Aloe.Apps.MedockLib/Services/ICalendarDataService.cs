using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services.Dtos;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// カレンダー表示用データ構築サービスインターフェース
/// </summary>
public interface ICalendarDataService
{
    /// <summary>
    /// カレンダー表示用データを構築します。
    /// SQL側で配列化されたResourceStatSlotsDtoを受け取ります。
    /// </summary>
    /// <param name="appointments">予約DTOのリスト</param>
    /// <param name="mainStats">Mainリソース統計データ（日付文字列 -> 統計リスト）</param>
    /// <param name="mainStatsGrayedOut">Mainリソース統計のグレーアウト状態（日付文字列 -> グレーアウトフラグ）</param>
    /// <param name="holidays">祝日データ（日付文字列 -> 祝日名）</param>
    /// <param name="filterTimeSlots">フィルター用の時間帯リスト（"09:00"形式、nullの場合はフィルターなし）</param>
    /// <param name="equipmentStats">Equipmentリソース統計データ（日付文字列 -> 統計リスト、nullの場合は表示しない）</param>
    /// <param name="businessHours">業務時間情報（時間外スロット判定に使用、nullの場合は判定しない）</param>
    /// <param name="mainStatsSlots">Mainリソース統計スロットデータ（(日付, リソースID) -> スロットリスト）</param>
    /// <returns>カレンダー表示用データDTO</returns>
    Task<CalendarDataDto> BuildCalendarDataAsync(
        IEnumerable<AppointmentDto> appointments,
        Dictionary<string, List<AppointmentStats>> mainStats,
        Dictionary<string, bool> mainStatsGrayedOut,
        Dictionary<string, string> holidays,
        List<string>? filterTimeSlots = null,
        Dictionary<string, List<ResourceStatSlotsDto>>? equipmentStats = null,
        BusinessHoursDto? businessHours = null,
        Dictionary<(DateOnly ApptDate, Guid ApptResId), List<AppointmentStatSlots>>? mainStatsSlots = null);
}

