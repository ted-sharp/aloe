using Aloe.Apps.MedockLib.Repositories;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 予約統計サービス実装
/// </summary>
public class AppointmentStatisticsService : IAppointmentStatisticsService
{
    private readonly IAppointmentStatsRepository _appointmentStatsRepository;

    // AM/PM の時間境界
    private const int AmStartHour = 8;
    private const int AmEndHour = 12;
    private const int PmStartHour = 13;
    private const int PmEndHour = 18;

    public AppointmentStatisticsService(IAppointmentStatsRepository appointmentStatsRepository)
    {
        this._appointmentStatsRepository = appointmentStatsRepository;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, MainStatsDto>> GetMainStatsAsync(DateOnly startDate, DateOnly endDate)
    {
        var appointments = await this._appointmentStatsRepository.GetForMainStatsAsync(startDate, endDate);

        var result = new Dictionary<string, MainStatsDto>([]);

        // 指定期間の全日付を初期化
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dateStr = date.ToString("yyyy-MM-dd");
            result[dateStr] = new MainStatsDto
            {
                AmCount = 0,
                PmCount = 0,
                AmMax = 10, // TODO: フロア/施設の設定から取得
                PmMax = 10
            };
        }

        // 予約を集計
        foreach (var (apptDate, apptStartAt) in appointments)
        {
            if (!apptDate.HasValue) continue;

            var dateStr = apptDate.Value.ToString("yyyy-MM-dd");
            if (!result.TryGetValue(dateStr, out var stats)) continue;

            // 時間から AM/PM を判定
            var hour = apptStartAt?.Hour ?? AmStartHour;
            if (hour >= AmStartHour && hour < AmEndHour)
            {
                stats.AmCount++;
            }
            else if (hour >= PmStartHour && hour < PmEndHour)
            {
                stats.PmCount++;
            }
            else if (hour < PmStartHour)
            {
                // 12-13時は昼休み、AMにカウント
                stats.AmCount++;
            }
            else
            {
                // 18時以降はPMにカウント
                stats.PmCount++;
            }
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Dictionary<int, int>> GetStatusCountByFloorAndDateAsync(Guid floorId, DateOnly date)
    {
        return await this._appointmentStatsRepository.GetStatusCountByFloorAndDateAsync(floorId, date);
    }
}
