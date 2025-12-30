using Microsoft.AspNetCore.Components;
using Aloe.Apps.MedockLib.Services.Dtos;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class WeekScheduler : ComponentBase
{
    /// <summary>
    /// 日数オプション
    /// </summary>
    private static readonly int[] DayOptions = { 1, 3, 7, 14, 31 };

    /// <summary>
    /// 表示する週の基準日
    /// </summary>
    [Parameter]
    public DateOnly CurrentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// 表示日数（デフォルト7日）
    /// </summary>
    [Parameter]
    public int WeekDays { get; set; } = 7;

    /// <summary>
    /// スロット表示かどうか
    /// </summary>
    [Parameter]
    public bool ShowSlots { get; set; } = true;

    /// <summary>
    /// 予約データ
    /// </summary>
    [Parameter]
    public IEnumerable<AppointmentDto>? Appointments { get; set; }

    /// <summary>
    /// Mainリソース統計データ
    /// </summary>
    [Parameter]
    public Dictionary<string, List<Aloe.Apps.MedockLib.Data.Entities.AppointmentStats>>? MainStats { get; set; }

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
    /// 開始時間
    /// </summary>
    [Parameter]
    public int StartHour { get; set; } = 8;

    /// <summary>
    /// 終了時間
    /// </summary>
    [Parameter]
    public int EndHour { get; set; } = 18;

    /// <summary>
    /// カレンダーの高さ
    /// </summary>
    [Parameter]
    public string Height { get; set; } = "600px";

    /// <summary>
    /// ローディング状態
    /// </summary>
    [Parameter]
    public bool IsLoading { get; set; }

    /// <summary>
    /// 日付選択時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<DateOnly> OnDateSelected { get; set; }

    /// <summary>
    /// 予約クリック時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<Guid> OnAppointmentClicked { get; set; }

    /// <summary>
    /// 予約作成リクエスト時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<(DateOnly Date, TimeOnly Time)> OnCreateRequested { get; set; }

    /// <summary>
    /// 表示日数変更時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<int> OnWeekDaysChanged { get; set; }

    /// <summary>
    /// 表示モード変更時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<bool> OnShowSlotsChanged { get; set; }

    /// <summary>
    /// 予約移動時のコールバック（D&D）
    /// </summary>
    [Parameter]
    public EventCallback<(Guid ApptId, DateOnly NewDate, TimeOnly NewTime)> OnAppointmentMoved { get; set; }

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

    /// <summary>
    /// 日付が単一選択された時のコールバック（JavaScript から呼ばれる）
    /// </summary>
    [Parameter]
    public EventCallback<DateOnly> OnDateSelectedSingle { get; set; }

    private async Task SetWeekDays(int days)
    {
        this.WeekDays = days;
        await this.OnWeekDaysChanged.InvokeAsync(days);
    }

    private async Task SetShowSlots(bool showSlots)
    {
        this.ShowSlots = showSlots;
        await this.OnShowSlotsChanged.InvokeAsync(showSlots);
    }

    private async Task HandleDateSelected(DateOnly date)
    {
        await this.OnDateSelected.InvokeAsync(date);
    }

    private async Task HandleAppointmentClicked(Guid apptId)
    {
        await this.OnAppointmentClicked.InvokeAsync(apptId);
    }

    private async Task HandleCreateRequested((DateOnly Date, TimeOnly Time) request)
    {
        await this.OnCreateRequested.InvokeAsync(request);
    }

    private async Task HandleAppointmentMoved((Guid ApptId, DateOnly NewDate, TimeOnly NewTime) moveInfo)
    {
        await this.OnAppointmentMoved.InvokeAsync(moveInfo);
    }

    private async Task HandleGoToToday()
    {
        await this.OnGoToToday.InvokeAsync();
    }

    private async Task HandleShowDayDetail(DateOnly date)
    {
        await this.OnShowDayDetail.InvokeAsync(date);
    }

    private async Task HandleDateSelectedSingle(DateOnly date)
    {
        await this.OnDateSelectedSingle.InvokeAsync(date);
    }
}
