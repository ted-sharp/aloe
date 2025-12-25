using Microsoft.AspNetCore.Components;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services.Dtos;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class YearView : ComponentBase
{
    /// <summary>
    /// 表示する年の基準日
    /// </summary>
    [Parameter]
    public DateOnly CurrentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// Mainリソース統計データ
    /// </summary>
    [Parameter]
    public Dictionary<string, List<AppointmentStats>>? MainStats { get; set; }

    /// <summary>
    /// グレーアウト状態（フィルター用）
    /// </summary>
    [Parameter]
    public Dictionary<string, bool>? MainStatsGrayedOut { get; set; }

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
    //[Parameter]
    // ShowEquipmentGraphプロパティは削除されました（EquipmentはAppointmentResourceに統合）
    // public bool ShowEquipmentGraph { get; set; } = false;

    /// <summary>
    /// 営業時間情報（昼休み時間帯の縦ライン描画用）
    /// </summary>
    [Parameter]
    public BusinessHoursDto? BusinessHours { get; set; }

    /// <summary>
    /// フィルター用の時間帯リスト（"09:00"形式）
    /// </summary>
    [Parameter]
    public List<string>? FilterTimeSlots { get; set; }

    /// <summary>
    /// カレンダーの高さ
    /// </summary>
    [Parameter]
    public string Height { get; set; } = "700px";

    /// <summary>
    /// 日付クリック時のコールバック（旧方式、互換性のため残す）
    /// </summary>
    [Parameter]
    public EventCallback<DateOnly> OnDateClick { get; set; }

    /// <summary>
    /// 月ヘッダークリック時のコールバック（月間表示に切り替え）
    /// </summary>
    [Parameter]
    public EventCallback<(int Year, int Month)> OnMonthClick { get; set; }

    /// <summary>
    /// 日付シングルクリック時のコールバック（選択）
    /// </summary>
    [Parameter]
    public EventCallback<DateOnly> OnDateSelect { get; set; }

    /// <summary>
    /// 日付ダブルクリック時のコールバック（その日のスケジュール表示）
    /// </summary>
    [Parameter]
    public EventCallback<DateOnly> OnDateDoubleClick { get; set; }

    /// <summary>
    /// 範囲選択時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<(DateOnly Start, DateOnly End)> OnDateRangeSelect { get; set; }

    /// <summary>
    /// 年選択時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<int> OnYearSelected { get; set; }

    /// <summary>
    /// 簡易表示切り替え時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<bool> OnSimpleViewChanged { get; set; }

    /// <summary>
    /// 設備表示スイッチ変更時のコールバック
    /// </summary>
    //[Parameter]
    // OnEquipmentGraphChangedイベントは削除されました（EquipmentはAppointmentResourceに統合）
    // public EventCallback<bool> OnEquipmentGraphChanged { get; set; }

    /// <summary>
    /// 今日ボタンクリック時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback OnGoToToday { get; set; }

    /// <summary>
    /// 日詳細ダイアログ表示時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<DateOnly> OnShowDayDetail { get; set; }

    private async Task HandleDateSelected(DateOnly date)
    {
        await this.OnDateClick.InvokeAsync(date);
    }

    private async Task HandleMonthClicked((int Year, int Month) yearMonth)
    {
        await this.OnMonthClick.InvokeAsync(yearMonth);
    }

    private async Task HandleDateSelectedSingle(DateOnly date)
    {
        await this.OnDateSelect.InvokeAsync(date);
    }

    private async Task HandleDateDoubleClicked(DateOnly date)
    {
        await this.OnDateDoubleClick.InvokeAsync(date);
    }

    private async Task HandleDateRangeSelected((DateOnly Start, DateOnly End) range)
    {
        await this.OnDateRangeSelect.InvokeAsync(range);
    }

    private async Task HandleYearSelected(int year)
    {
        await this.OnYearSelected.InvokeAsync(year);
    }

    private async Task HandleSimpleViewChanged(bool showSimpleView)
    {
        this.ShowSimpleView = showSimpleView;
        await this.OnSimpleViewChanged.InvokeAsync(this.ShowSimpleView);
    }

    // HandleEquipmentGraphChangedメソッドは削除されました（EquipmentはAppointmentResourceに統合）

    private async Task HandleGoToToday()
    {
        await this.OnGoToToday.InvokeAsync();
    }

    private async Task HandleShowDayDetail(DateOnly date)
    {
        await this.OnShowDayDetail.InvokeAsync(date);
    }
}


