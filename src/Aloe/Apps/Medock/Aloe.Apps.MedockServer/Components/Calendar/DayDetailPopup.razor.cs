using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class DayDetailPopup : ComponentBase
{
    /// <summary>
    /// スロット選択イベントの引数
    /// </summary>
    public class SlotClickedEventArgs
    {
        public DateOnly Date { get; set; }
        public int StartMin { get; set; }
        public int EndMin { get; set; }
        public int Capacity { get; set; }
        public int Count { get; set; }
    }

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter]
    public bool IsOpen { get; set; }

    [Parameter]
    public DateOnly? SelectedDate { get; set; }

    [Parameter]
    public List<Aloe.Apps.MedockLib.Data.Entities.AppointmentStats>? MainStats { get; set; }

    [Parameter]
    public Dictionary<(DateOnly ApptDate, Guid ApptResId), List<Aloe.Apps.MedockLib.Data.Entities.AppointmentStatSlots>>? MainStatsSlots { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    [Parameter]
    public EventCallback<DateOnly> OnGoToWeekView { get; set; }

    [Parameter]
    public EventCallback<DateOnly> OnNavigateToPreviousDay { get; set; }

    [Parameter]
    public EventCallback<DateOnly> OnNavigateToNextDay { get; set; }

    [Parameter]
    public EventCallback<SlotClickedEventArgs> OnSlotClicked { get; set; }

    [Parameter]
    public Aloe.Apps.MedockLib.Services.Dtos.BusinessHoursDto? BusinessHours { get; set; }

    private string ChartContainerId { get; } = $"day-detail-chart-{Guid.CreateVersion7():N}";
    private bool _isChartRendered;
    private bool _wasOpen;
    private DateOnly? _previousSelectedDate;
    private string? HolidayName { get; set; }
    private SlotClickedEventArgs? _selectedSlot { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // モーダルが開いた時にグラフを描画
        if (this.IsOpen && !this._wasOpen && this.SelectedDate.HasValue)
        {
            this._wasOpen = true;
            this._previousSelectedDate = this.SelectedDate;
            await this.RenderChartAsync();
            await this.LoadHolidayNameAsync();
        }
        else if (!this.IsOpen && this._wasOpen)
        {
            this._wasOpen = false;
            this._isChartRendered = false;
            this._previousSelectedDate = null;
            this.HolidayName = null;
            this._selectedSlot = null;
            await this.DestroyChartAsync();
        }
        // SelectedDateが変更された場合、グラフを再描画
        else if (this.IsOpen && this._wasOpen && this.SelectedDate != this._previousSelectedDate)
        {
            this._previousSelectedDate = this.SelectedDate;
            this._isChartRendered = false;
            this._selectedSlot = null;
            await this.DestroyChartAsync();
            await this.RenderChartAsync();
            await this.LoadHolidayNameAsync();
        }
    }

    private async Task LoadHolidayNameAsync()
    {
        if (!this.SelectedDate.HasValue)
        {
            this.HolidayName = null;
            return;
        }

        try
        {
            var dateStr = this.SelectedDate.Value.ToString("yyyy-MM-dd");
            this.HolidayName = await this.JSRuntime.InvokeAsync<string?>(
                "MedockCalendar.getHolidayName", dateStr);
            this.StateHasChanged();
        }
        catch
        {
            this.HolidayName = null;
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

                // MainStatSlotsデータをJavaScript形式に変換
                var dayStatsData = await this.ConvertMainStatsSlotsToJSFormatAsync(dateStr);

                // DotNetObjectReferenceを作成してJavaScript側に渡す
                var dotNetRef = DotNetObjectReference.Create(this);
                await this.JSRuntime.InvokeVoidAsync("MedockCalendar.renderDayDetailPopup",
                    this.ChartContainerId, dateStr, dotNetRef, (object)dayStatsData);
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
            // Get slots for this stat from MainStatsSlots if available
            var slots = new List<Aloe.Apps.MedockLib.Data.Entities.AppointmentStatSlots>();
            if (this.MainStatsSlots != null)
            {
                var key = (stat.ApptDate, stat.ApptResId);
                if (this.MainStatsSlots.TryGetValue(key, out var foundSlots))
                {
                    slots = foundSlots;
                }
            }

            foreach (var statSlot in slots.Where(s => !s.IsDeleted))
            {
                var slotStartTime = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(statSlot.SlotStartMin));
                var slotEndTime = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(statSlot.SlotEndMin));
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

    private string GetDateColorClass()
    {
        if (!this.SelectedDate.HasValue)
            return "";

        // 祝日は赤
        if (!String.IsNullOrEmpty(this.HolidayName))
            return "text-red-600";

        var date = this.SelectedDate.Value;
        var dayOfWeek = (int)date.DayOfWeek;

        // 日曜日は赤、土曜日は青
        if (dayOfWeek == 0)
            return "text-red-600";
        if (dayOfWeek == 6)
            return "text-blue-600";

        return "";
    }

    private int GetTotalCount()
    {
        var slots = this.GetSlotsFromStats();
        return slots.Sum(s => s.Count);
    }

    private int GetTotalCapacity()
    {
        var slots = this.GetSlotsFromStats();
        return slots.Sum(s => s.Cap);
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
        var slots = this.GetSlotsFromStats();
        var totalCount = slots.Sum(s => s.Count);
        var totalCap = slots.Sum(s => s.Cap);
        var totalAvailable = totalCap - totalCount;

        // データなし（available と capacity が両方 0）
        if (totalAvailable == 0 && totalCap == 0)
            return "–";

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
        var slots = this.GetSlotsFromStats();
        var totalCount = slots.Sum(s => s.Count);
        var totalCap = slots.Sum(s => s.Cap);
        var totalAvailable = totalCap - totalCount;

        // データなし（available と capacity が両方 0）
        if (totalAvailable == 0 && totalCap == 0)
            return "text-gray-400";

        var vacancyRatio = this.GetOverallVacancyRatio();
        return vacancyRatio switch
        {
            <= 0 => "text-red-500",      // × 満杯
            < 0.3 => "text-yellow-500",  // △ やや空き
            < 0.6 => "text-emerald-500", // ○ 空きあり
            _ => "text-emerald-500"      // ◎ 十分空き
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

    /// <summary>
    /// JavaScriptからスロット選択を受け取る（シングルクリック用）
    /// </summary>
    [JSInvokable("OnSlotSelected")]
    public Task OnSlotSelectedFromJs(string dateStr, int startMinutes, int endMinutes, int capacity, int count)
    {
        try
        {
            if (!DateOnly.TryParse(dateStr, out var date))
            {
                return Task.CompletedTask;
            }

            this._selectedSlot = new SlotClickedEventArgs
            {
                Date = date,
                StartMin = startMinutes,
                EndMin = endMinutes,
                Capacity = capacity,
                Count = count
            };

            this.StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DayDetailPopup.OnSlotSelected error: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// JavaScriptからスロット選択解除を受け取る
    /// </summary>
    [JSInvokable("OnSlotDeselected")]
    public Task OnSlotDeselectedFromJs()
    {
        try
        {
            this._selectedSlot = null;
            this.StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DayDetailPopup.OnSlotDeselected error: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// JavaScriptからスロット選択を受け取る（ダブルクリック用）
    /// </summary>
    [JSInvokable("OnSlotClicked")]
    public async Task OnSlotClickedFromJs(string dateStr, int startMinutes, int endMinutes, int capacity, int count)
    {
        try
        {
            if (!DateOnly.TryParse(dateStr, out var date))
            {
                return;
            }

            var args = new SlotClickedEventArgs
            {
                Date = date,
                StartMin = startMinutes,
                EndMin = endMinutes,
                Capacity = capacity,
                Count = count
            };

            // イベント発火（容量チェックなし）
            await this.OnSlotClicked.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DayDetailPopup.OnSlotClicked error: {ex.Message}");
        }
    }

    /// <summary>
    /// 仮予約ボタンのクリックハンドラ
    /// </summary>
    private async Task HandleTentativeReservationClick()
    {
        if (this._selectedSlot != null)
        {
            await this.OnSlotClicked.InvokeAsync(this._selectedSlot);
        }
    }

    /// <summary>
    /// 予約ボタンのクリックハンドラ
    /// </summary>
    private async Task HandleReservationClick()
    {
        if (this._selectedSlot != null)
        {
            await this.OnSlotClicked.InvokeAsync(this._selectedSlot);
        }
    }

    /// <summary>
    /// MainStatsSlotsデータをJavaScript形式に変換
    /// JavaScriptは並列配列形式を期待しています
    /// </summary>
    private async Task<object> ConvertMainStatsSlotsToJSFormatAsync(string dateStr)
    {
        if (this.MainStatsSlots == null || !this.MainStatsSlots.Any())
        {
            return new
            {
                slotStartMins = new int[] { },
                slotEndMins = new int[] { },
                slotCounts = new int[] { },
                slotCaps = new int[] { },
                slotAvailables = new int[] { },
                slotFlags = (byte[]?)null,
                isDayGrayedOut = false
            };
        }

        // 選択日付のスロットを取得
        if (!this.SelectedDate.HasValue)
        {
            return new
            {
                slotStartMins = new int[] { },
                slotEndMins = new int[] { },
                slotCounts = new int[] { },
                slotCaps = new int[] { },
                slotAvailables = new int[] { },
                slotFlags = (byte[]?)null,
                isDayGrayedOut = false
            };
        }

        var dateSlotsData = this.MainStatsSlots
            .Where(kvp => kvp.Key.ApptDate == this.SelectedDate.Value)
            .SelectMany(kvp => kvp.Value)
            .OrderBy(s => s.SlotStartMin)
            .ToList();

        if (!dateSlotsData.Any())
        {
            return new
            {
                slotStartMins = new int[] { },
                slotEndMins = new int[] { },
                slotCounts = new int[] { },
                slotCaps = new int[] { },
                slotAvailables = new int[] { },
                slotFlags = (byte[]?)null,
                isDayGrayedOut = false
            };
        }

        // 並列配列形式に変換
        var slotStarts = dateSlotsData.Select(s => s.SlotStartMin).ToArray();
        var slotEnds = dateSlotsData.Select(s => s.SlotEndMin).ToArray();
        var slotCounts = dateSlotsData.Select(s => s.SlotCount).ToArray();
        var slotCaps = dateSlotsData.Select(s => s.SlotCap).ToArray();
        var slotAvailables = dateSlotsData.Select(s => Math.Max(0, s.SlotCap - s.SlotCount)).ToArray();

        // slotFlags を生成（時間外スロット判定用）
        var slotFlags = new byte[dateSlotsData.Count];
        if (this.BusinessHours != null)
        {
            for (int i = 0; i < dateSlotsData.Count; i++)
            {
                var slot = dateSlotsData[i];
                var (isOutsideHoursBefore, isOutsideHoursAfter, isOutsideHoursLunch) =
                    this.BusinessHours.CheckSlotOutsideHours(slot.SlotStartMin, slot.SlotEndMin);

                byte flags = 0;
                if (isOutsideHoursBefore) flags |= 0b0010;  // ビット1: IsOutsideHoursBefore（朝）
                if (isOutsideHoursAfter) flags |= 0b0100;   // ビット2: IsOutsideHoursAfter（夕方）
                if (isOutsideHoursLunch) flags |= 0b1000;   // ビット3: IsOutsideHoursLunch（昼休み）
                slotFlags[i] = flags;
            }
        }

        return new
        {
            slotStartMins = slotStarts,
            slotEndMins = slotEnds,
            slotCounts = slotCounts,
            slotCaps = slotCaps,
            slotAvailables = slotAvailables,
            slotFlags = slotFlags,
            isDayGrayedOut = false
        };
    }
}
