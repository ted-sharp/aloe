using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class DayDetailPopup : ComponentBase
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter]
    public bool IsOpen { get; set; }

    [Parameter]
    public DateOnly? SelectedDate { get; set; }

    [Parameter]
    public List<Aloe.Apps.MedockLib.Data.Entities.AppointmentStats>? MainStats { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    [Parameter]
    public EventCallback<DateOnly> OnGoToWeekView { get; set; }

    [Parameter]
    public EventCallback<DateOnly> OnNavigateToPreviousDay { get; set; }

    [Parameter]
    public EventCallback<DateOnly> OnNavigateToNextDay { get; set; }

    private string ChartContainerId { get; } = $"day-detail-chart-{Guid.CreateVersion7():N}";
    private bool _isChartRendered;
    private bool _wasOpen;
    private DateOnly? _previousSelectedDate;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // モーダルが開いた時にグラフを描画
        if (this.IsOpen && !this._wasOpen && this.SelectedDate.HasValue)
        {
            this._wasOpen = true;
            this._previousSelectedDate = this.SelectedDate;
            await this.RenderChartAsync();
        }
        else if (!this.IsOpen && this._wasOpen)
        {
            this._wasOpen = false;
            this._isChartRendered = false;
            this._previousSelectedDate = null;
            await this.DestroyChartAsync();
        }
        // SelectedDateが変更された場合、グラフを再描画
        else if (this.IsOpen && this._wasOpen && this.SelectedDate != this._previousSelectedDate)
        {
            this._previousSelectedDate = this.SelectedDate;
            this._isChartRendered = false;
            await this.DestroyChartAsync();
            await this.RenderChartAsync();
        }
    }

    private async Task RenderChartAsync()
    {
        if (!this.SelectedDate.HasValue || this._isChartRendered)
            return;

        try
        {
            // MedockCalendarが読み込まれるまで待機
            var isReady = await this.JSRuntime.InvokeAsync<bool>("eval", "typeof window.MedockCalendar !== 'undefined' && typeof window.MedockCalendar.renderDayDetailPopup !== 'undefined'");
            if (!isReady)
            {
                // フォールバック: 少し待機して再試行
                await Task.Delay(100);
                isReady = await this.JSRuntime.InvokeAsync<bool>("eval", "typeof window.MedockCalendar !== 'undefined'");
            }

            if (isReady)
            {
                var dateStr = this.SelectedDate.Value.ToString("yyyy-MM-dd");
                await this.JSRuntime.InvokeVoidAsync("MedockCalendar.renderDayDetailPopup", this.ChartContainerId, dateStr);
                this._isChartRendered = true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DayDetailPopup: Failed to render chart: {ex.Message}");
        }
    }

    private async Task DestroyChartAsync()
    {
        try
        {
            var isReady = await this.JSRuntime.InvokeAsync<bool>("eval", "typeof window.MedockCalendar !== 'undefined' && typeof window.MedockCalendar.destroyDayDetailPopup !== 'undefined'");
            if (isReady)
            {
                await this.JSRuntime.InvokeVoidAsync("MedockCalendar.destroyDayDetailPopup", this.ChartContainerId);
            }
        }
        catch
        {
            // 破棄時のエラーは無視
        }
    }

    private List<TimeSlotStats> GetSlotsFromStats()
    {
        if (this.MainStats == null || !this.MainStats.Any())
            return new List<TimeSlotStats>();

        var slotMap = new Dictionary<string, (TimeOnly Start, TimeOnly End, int Count, int Cap)>();

        foreach (var stat in this.MainStats)
        {
            if (stat.AppointmentStatSlots != null)
            {
                foreach (var statSlot in stat.AppointmentStatSlots.Where(s => !s.IsDeleted))
                {
                    var slotStartTime = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(statSlot.SlotStart));
                    var slotEndTime = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(statSlot.SlotEnd));
                    var timeRangeKey = $"{slotStartTime:HH:mm}-{slotEndTime:HH:mm}";

                    if (slotMap.ContainsKey(timeRangeKey))
                    {
                        var existing = slotMap[timeRangeKey];
                        slotMap[timeRangeKey] = (existing.Start, existing.End, existing.Count + statSlot.SlotCount, existing.Cap + statSlot.SlotCap);
                    }
                    else
                    {
                        slotMap[timeRangeKey] = (slotStartTime, slotEndTime, statSlot.SlotCount, statSlot.SlotCap);
                    }
                }
            }
        }

        return slotMap.Select(kvp => new TimeSlotStats
        {
            Time = kvp.Key,
            Count = kvp.Value.Count,
            Cap = kvp.Value.Cap,
            IsGrayedOut = false,
            FilteredCount = 0
        }).OrderBy(s => s.Time).ToList();
    }

    private static readonly string[] DayOfWeekNames = ["日曜日", "月曜日", "火曜日", "水曜日", "木曜日", "金曜日", "土曜日"];

    private string GetDayOfWeekJapanese(DateOnly date)
    {
        return DayOfWeekNames[(int)date.DayOfWeek];
    }

    private int GetTotalCount()
    {
        var slots = this.GetSlotsFromStats();
        return slots.Sum(s => s.Count);
    }

    private double GetOverallVacancyRatio()
    {
        var slots = this.GetSlotsFromStats();
        var totalCount = slots.Sum(s => s.Count);
        var totalCap = slots.Sum(s => s.Cap);
        if (totalCap == 0) return 0;
        return (double)(totalCap - totalCount) / totalCap;
    }

    private string GetSymbol()
    {
        var vacancyRatio = this.GetOverallVacancyRatio();
        return vacancyRatio switch
        {
            <= 0 => "×",
            < 0.3 => "△",
            < 0.6 => "○",
            _ => "◎"
        };
    }

    private string GetSymbolColorClass()
    {
        var vacancyRatio = this.GetOverallVacancyRatio();
        return vacancyRatio switch
        {
            <= 0 => "text-red-500",
            < 0.3 => "text-yellow-500",
            _ => "text-emerald-500"
        };
    }

    private async Task HandleClose()
    {
        await this.OnClose.InvokeAsync();
    }

    private async Task HandleGoToWeekView()
    {
        if (this.SelectedDate.HasValue)
        {
            await this.OnGoToWeekView.InvokeAsync(this.SelectedDate.Value);
        }
        await this.HandleClose();
    }

    private async Task HandleNavigateToPreviousDay()
    {
        if (this.SelectedDate.HasValue)
        {
            await this.OnNavigateToPreviousDay.InvokeAsync(this.SelectedDate.Value.AddDays(-1));
        }
    }

    private async Task HandleNavigateToNextDay()
    {
        if (this.SelectedDate.HasValue)
        {
            await this.OnNavigateToNextDay.InvokeAsync(this.SelectedDate.Value.AddDays(1));
        }
    }
}
