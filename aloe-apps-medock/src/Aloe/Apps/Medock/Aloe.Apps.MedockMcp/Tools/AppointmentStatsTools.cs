using System.ComponentModel;
using System.Text.Json;
using Aloe.Apps.MedockLib.Repositories;
using ModelContextProtocol.Server;

namespace Aloe.Apps.MedockMcp.Tools;

/// <summary>
/// 予約統計・スロット情報ツール
/// </summary>
[McpServerToolType]
internal class AppointmentStatsTools
{
    private readonly IAppointmentStatsRepository _statsRepository;

    public AppointmentStatsTools(IAppointmentStatsRepository statsRepository)
    {
        this._statsRepository = statsRepository;
    }

    [McpServerTool(Name = "GetSlotStats")]
    [Description("指定した日付範囲のメインリソースの予約統計・スロット空き状況を取得します。日別の予約数/容量/空き数とスロット詳細を返します。")]
    public async Task<string> GetSlotStats(
        [Description("開始日 (yyyy-MM-dd)")] string startDate,
        [Description("終了日 (yyyy-MM-dd)")] string endDate)
    {
        if (!DateOnly.TryParse(startDate, out var start))
            return JsonSerializer.Serialize(new { error = $"開始日の形式が不正です: {startDate}" });
        if (!DateOnly.TryParse(endDate, out var end))
            return JsonSerializer.Serialize(new { error = $"終了日の形式が不正です: {endDate}" });

        var stats = await this._statsRepository.GetMainResourceStatsByDateRangeAsync(start, end);
        var slots = await this._statsRepository.GetStatSlotsByDateRangeAsync(start, end);

        var dailyStats = stats
            .GroupBy(s => s.ApptDate)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var totalCap = g.Sum(s => s.ApptCap);
                var totalCount = g.Sum(s => s.ApptCount);
                var slotsForDay = g.SelectMany(s =>
                {
                    var key = (s.ApptDate, s.ApptResId);
                    return slots.TryGetValue(key, out var slotList) ? slotList : [];
                })
                .OrderBy(s => s.SlotStartMin)
                .Select(s => new
                {
                    start = $"{s.SlotStartMin / 60:D2}:{s.SlotStartMin % 60:D2}",
                    end = $"{s.SlotEndMin / 60:D2}:{s.SlotEndMin % 60:D2}",
                    cap = s.SlotCap,
                    count = s.SlotCount,
                    available = s.SlotCap - s.SlotCount
                });

                return new
                {
                    date = g.Key.ToString("yyyy-MM-dd"),
                    totalCapacity = totalCap,
                    totalCount,
                    totalAvailable = totalCap - totalCount,
                    slots = slotsForDay
                };
            });

        return JsonSerializer.Serialize(new
        {
            count = dailyStats.Count(),
            dailyStats
        });
    }
}
