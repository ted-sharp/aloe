using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Repositories;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockServer.Components.Calendar;
using Aloe.Apps.MedockServer.Components.FAB;
using Aloe.Apps.MedockServer.Components.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aloe.Apps.MedockServer.Components.Pages;

/// <summary>
/// カレンダーページのデータロード処理サービス
/// </summary>
public class CalendarDataService
{
    private readonly IAppointmentService _appointmentService;
    private readonly IFacilityService _facilityService;
    private readonly IAppointmentStatsRepository _appointmentStatsRepository;
    private readonly AuthenticationStateProvider _authStateProvider;

    public CalendarDataService(
        IAppointmentService appointmentService,
        IFacilityService facilityService,
        IAppointmentStatsRepository appointmentStatsRepository,
        AuthenticationStateProvider authStateProvider)
    {
        this._appointmentService = appointmentService;
        this._facilityService = facilityService;
        this._appointmentStatsRepository = appointmentStatsRepository;
        this._authStateProvider = authStateProvider;
    }

    /// <summary>
    /// グラフデータのJSONB構造（パース用）
    /// </summary>
    private class GraphDefinition
    {
        [JsonPropertyName("slots")]
        public List<GraphSlotItem> Slots { get; set; } = new();
    }

    /// <summary>
    /// グラフスロットアイテム（パース用）
    /// </summary>
    private class GraphSlotItem
    {
        [JsonPropertyName("time")]
        public string Time { get; set; } = String.Empty;

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("max")]
        public int Max { get; set; }
    }

    /// <summary>
    /// 営業時間をロードして状態に反映します。
    /// </summary>
    public async Task LoadBusinessHoursAsync(CalendarState state)
    {
        try
        {
            if (state.CurrentFacilityId.HasValue)
            {
                state.BusinessHours = await this._facilityService.GetBusinessHoursAsync(
                    state.CurrentFacilityId.Value,
                    state.CurrentDate);

                state.StartHour = state.BusinessHours.StartHour;
                state.EndHour = state.BusinessHours.EndHour;
            }
            else
            {
                state.BusinessHours = new BusinessHoursDto();
            }
        }
        catch (Exception)
        {
            state.BusinessHours = new BusinessHoursDto();
        }
    }

    /// <summary>
    /// 休日情報をロードして状態に反映します。
    /// </summary>
    public async Task LoadHolidaysAsync(CalendarState state)
    {
        try
        {
            var startDate = new DateOnly(state.CurrentDate.Year, 1, 1);
            var endDate = new DateOnly(state.CurrentDate.Year, 12, 31);
            var holidays = await this._appointmentService.GetHolidaysAsync(startDate, endDate);

            state.Holidays = holidays.ToDictionary(
                h => h.Date.ToString("yyyy-MM-dd"),
                h => h.Name
            );
        }
        catch (Exception)
        {
            state.Holidays = [];
        }
    }

    /// <summary>
    /// フィルター用の設備オプションを生成して状態に反映します。
    /// EquipmentはAppointmentResourceに統合されました。
    /// </summary>
    public async Task GenerateFilterOptionsAsync(CalendarState state)
    {
        // EquipmentはAppointmentResourceに統合されました
        // このメソッドは将来AppointmentResource用に実装されます
        await Task.CompletedTask;
    }

    /// <summary>
    /// 期間に応じた予約データを取得して状態に反映します。
    /// </summary>
    public async Task LoadAppointmentsAsync(
        CalendarState state,
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays = 7)
    {
        try
        {
            var (startDate, endDate) = GetDateRange(viewType, currentDate, weekDays);
            var appointments = await this._appointmentService.GetAppointmentsAsync(startDate, endDate);
            state.Appointments = appointments;
        }
        catch (Exception)
        {
            state.Appointments = [];
        }
    }

    /// <summary>
    /// MainリソースのStatsを日付ごとにグループ化して状態に反映します。
    /// </summary>
    public async Task LoadMainStatsAsync(
        CalendarState state,
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays = 7)
    {
        try
        {
            var (startDate, endDate) = GetDateRange(viewType, currentDate, weekDays);
            var mainStats = await this._appointmentStatsRepository.GetMainResourceStatsByDateRangeAsync(startDate, endDate);

            // 日付ごとにグループ化
            var statsByDate = mainStats.GroupBy(s => s.ApptDate).ToDictionary(g => g.Key, g => g.ToList());

            // 全日付を初期化
            state.MainStats.Clear();
            state.OriginalMainStats.Clear();
            state.MainStatsGrayedOut.Clear();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dateStr = date.ToString("yyyy-MM-dd");

                // その日のMainリソースStatsがあれば設定、なければ空リスト
                if (statsByDate.TryGetValue(date, out var mainStatsList))
                {
                    state.MainStats[dateStr] = mainStatsList;
                    state.OriginalMainStats[dateStr] = mainStatsList.ToList(); // コピーを作成
                }
                else
                {
                    state.MainStats[dateStr] = new List<AppointmentStats>();
                    state.OriginalMainStats[dateStr] = new List<AppointmentStats>();
                }

                state.MainStatsGrayedOut[dateStr] = false;
            }
        }
        catch (Exception)
        {
            // エラー時は空のMainStatsを設定
            state.MainStats.Clear();
            state.OriginalMainStats.Clear();
            state.MainStatsGrayedOut.Clear();
        }
    }

    /// <summary>
    /// ビューと日付に基づいて取得期間を計算します。
    /// </summary>
    private static (DateOnly StartDate, DateOnly EndDate) GetDateRange(
        CalendarViewType viewType,
        DateOnly currentDate,
        int weekDays)
    {
        return viewType switch
        {
            CalendarViewType.Year => (
                new DateOnly(currentDate.Year, 1, 1),
                new DateOnly(currentDate.Year, 12, 31)
            ),
            CalendarViewType.Month => (
                new DateOnly(currentDate.Year, currentDate.Month, 1),
                new DateOnly(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month))
            ),
            CalendarViewType.Week => (
                currentDate.AddDays(-((int)currentDate.DayOfWeek)),
                currentDate.AddDays(-((int)currentDate.DayOfWeek) + weekDays - 1)
            ),
            _ => (currentDate, currentDate)
        };
    }
}
