using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class MonthView : ComponentBase
{
    /// <summary>
    /// 表示する月の基準日
    /// </summary>
    [Parameter]
    public DateOnly CurrentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// 日別統計データ
    /// </summary>
    [Parameter]
    public Dictionary<string, CalendarCanvas.CalendarDayStats>? DayStats { get; set; }

    /// <summary>
    /// 祝日データ（日付文字列 -> 祝日名）
    /// </summary>
    [Parameter]
    public Dictionary<string, string>? Holidays { get; set; }

    /// <summary>
    /// 簡易表示モード（記号表示）を表示するかどうか
    /// </summary>
    [Parameter]
    public bool ShowSimpleView { get; set; } = false;

    /// <summary>
    /// 設備折れ線グラフを表示するかどうか
    /// </summary>
    [Parameter]
    public bool ShowEquipmentGraph { get; set; } = false;

    /// <summary>
    /// カレンダーの高さ
    /// </summary>
    [Parameter]
    public string Height { get; set; } = "600px";

    /// <summary>
    /// 日付クリック時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<DateOnly> OnDateClick { get; set; }

    /// <summary>
    /// 日付ダブルクリック時のコールバック（その日のスケジュール表示）
    /// </summary>
    [Parameter]
    public EventCallback<DateOnly> OnDateDoubleClick { get; set; }

    /// <summary>
    /// 月選択時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<(int Year, int Month)> OnMonthSelected { get; set; }

    /// <summary>
    /// 簡易表示切り替え時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<bool> OnSimpleViewChanged { get; set; }

    /// <summary>
    /// 設備表示スイッチ変更時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<bool> OnEquipmentGraphChanged { get; set; }

    /// <summary>
    /// 今日ボタンクリック時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback OnGoToToday { get; set; }

    private async Task HandleDateSelected(DateOnly date)
    {
        await this.OnDateClick.InvokeAsync(date);
    }

    private async Task HandleDateDoubleClicked(DateOnly date)
    {
        await this.OnDateDoubleClick.InvokeAsync(date);
    }

    private async Task HandleMonthSelected(int year, int month)
    {
        await this.OnMonthSelected.InvokeAsync((year, month));
    }

    private async Task HandleSimpleViewChanged(bool showSimpleView)
    {
        this.ShowSimpleView = showSimpleView;
        await this.OnSimpleViewChanged.InvokeAsync(this.ShowSimpleView);
    }

    private async Task HandleEquipmentGraphChanged()
    {
        await this.OnEquipmentGraphChanged.InvokeAsync(this.ShowEquipmentGraph);
    }

    private async Task HandleGoToToday()
    {
        await this.OnGoToToday.InvokeAsync();
    }
}


