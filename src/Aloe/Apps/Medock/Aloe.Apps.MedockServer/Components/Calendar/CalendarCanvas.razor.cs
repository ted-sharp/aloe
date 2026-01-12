using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services.Dtos;
using Aloe.Apps.MedockLib.Services.Dtos.Appointments;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class CalendarCanvas : ComponentBase, IAsyncDisposable
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private ICalendarDataService CalendarDataService { get; set; } = default!;

    [Inject]
    private IDateTimeProvider DateTimeProvider { get; set; } = default!;

    /// <summary>
    /// Current view type: "year", "month", "week"
    /// </summary>
    [Parameter]
    public string ViewType { get; set; } = "month";

    /// <summary>
    /// Current date (center of the view)
    /// </summary>
    [Parameter]
    public DateOnly CurrentDate { get; set; }

    /// <summary>
    /// Appointment data for the calendar
    /// </summary>
    [Parameter]
    public IEnumerable<AppointmentDto>? Appointments { get; set; }

    /// <summary>
    /// Mainリソース統計データ（日付ごと）
    /// </summary>
    [Parameter]
    public Dictionary<string, List<AppointmentStats>>? MainStats { get; set; }

    /// <summary>
    /// Mainリソーススロット統計データ（日付とリソースIDでグループ化）
    /// </summary>
    [Parameter]
    public Dictionary<(DateOnly ApptDate, Guid ApptResId), List<AppointmentStatSlots>>? MainStatsSlots { get; set; }

    /// <summary>
    /// グレーアウト状態（フィルター用）
    /// </summary>
    [Parameter]
    public Dictionary<string, bool>? MainStatsGrayedOut { get; set; }

    /// <summary>
    /// Equipmentリソース統計データ（FromSql + array_agg 最適化版、日付ごと）
    /// </summary>
    [Parameter]
    public Dictionary<string, List<ResourceStatSlotsDto>>? EquipmentStatsOptimized { get; set; }

    /// <summary>
    /// Holidays (date string -> holiday name)
    /// </summary>
    [Parameter]
    public Dictionary<string, string>? Holidays { get; set; }

    /// <summary>
    /// Number of days to show in week view (1, 3, 7, 14, 31)
    /// </summary>
    [Parameter]
    public int WeekDays { get; set; } = 7;

    /// <summary>
    /// Show slots mode (true) or detail mode with avatars (false)
    /// </summary>
    [Parameter]
    public bool ShowSlots { get; set; } = true;

    /// <summary>
    /// Show simple view mode (symbol display instead of bar chart)
    /// </summary>
    [Parameter]
    public bool ShowSimpleView { get; set; } = false;

    // ShowEquipmentGraphパラメータは削除されました（EquipmentはAppointmentResourceに統合）
    // public bool ShowEquipmentGraph { get; set; } = false;

    /// <summary>
    /// Start hour for week/day view
    /// </summary>
    [Parameter]
    public int StartHour { get; set; } = 8;

    /// <summary>
    /// End hour for week/day view
    /// </summary>
    [Parameter]
    public int EndHour { get; set; } = 18;

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
    /// Height of the canvas container
    /// </summary>
    [Parameter]
    public string Height { get; set; } = "600px";

    /// <summary>
    /// Additional CSS classes
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Callback when a date is clicked
    /// </summary>
    [Parameter]
    public EventCallback<DateOnly> OnDateSelected { get; set; }

    /// <summary>
    /// Callback when an appointment is clicked
    /// </summary>
    [Parameter]
    public EventCallback<Guid> OnAppointmentClicked { get; set; }

    /// <summary>
    /// Callback when create is requested (date + time)
    /// </summary>
    [Parameter]
    public EventCallback<(DateOnly Date, int StartMin)> OnCreateRequested { get; set; }

    /// <summary>
    /// Callback when an appointment is moved via drag and drop (apptId, newDate, newTime)
    /// </summary>
    [Parameter]
    public EventCallback<(Guid ApptId, DateOnly NewDate, int NewStartMin)> OnAppointmentMoved { get; set; }

    /// <summary>
    /// Callback when a month header is clicked (for switching to month view)
    /// </summary>
    [Parameter]
    public EventCallback<(int Year, int Month)> OnMonthClicked { get; set; }

    /// <summary>
    /// Callback when a date is single-clicked (for selection)
    /// </summary>
    [Parameter]
    public EventCallback<DateOnly> OnDateSelectedSingle { get; set; }

    /// <summary>
    /// Callback when a date is double-clicked (for day schedule view)
    /// </summary>
    [Parameter]
    public EventCallback<DateOnly> OnDateDoubleClicked { get; set; }

    /// <summary>
    /// Callback when a date range is selected via drag
    /// </summary>
    [Parameter]
    public EventCallback<(DateOnly Start, DateOnly End)> OnDateRangeSelected { get; set; }

    /// <summary>
    /// Callback when day detail popup should be shown (double-click on date)
    /// </summary>
    [Parameter]
    public EventCallback<DateOnly> OnShowDayDetail { get; set; }

    private string ContainerId { get; } = $"calendar-canvas-{Guid.CreateVersion7():N}";
    private DotNetObjectReference<CalendarCanvas>? _dotNetRef;
    private bool _isInitialized = false;
    private string? _lastViewType;
    private DateOnly _lastDate;
    private int _lastWeekDays;
    private bool _lastShowSlots;
    private bool _lastShowSimpleView;
    // _lastShowEquipmentGraphは削除されました（EquipmentはAppointmentResourceに統合）
    private int _lastMainStatsCount = 0;  // MainStats の変更検出用
    private int _lastMainStatsSlotsCount = 0;  // MainStatsSlots の変更検出用

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            this._dotNetRef = DotNetObjectReference.Create(this);
            await this.InitializeCalendarAsync();
            // 初期化フラグは InitializeCalendarAsync の中で設定される
        }
        else if (this._isInitialized)
        {
            // Check if we need to update the view
            if (this._lastViewType != this.ViewType || this._lastDate != this.CurrentDate || this._lastWeekDays != this.WeekDays || this._lastShowSlots != this.ShowSlots || this._lastShowSimpleView != this.ShowSimpleView)
            {
                await this.ChangeViewAsync();
                this._lastViewType = this.ViewType;
                this._lastDate = this.CurrentDate;
                this._lastWeekDays = this.WeekDays;
                this._lastShowSlots = this.ShowSlots;
                this._lastShowSimpleView = this.ShowSimpleView;
            }
        }
    }

    protected override void OnInitialized()
    {
        if (this.CurrentDate == default)
        {
            this.CurrentDate = this.DateTimeProvider.TodayDateOnly;
        }
        base.OnInitialized();
    }

    protected override async Task OnParametersSetAsync()
    {
        var currentMainStatsCount = this.MainStats?.Count ?? 0;
        var currentMainStatsSlotsCount = this.MainStatsSlots?.Count ?? 0;
        var hasDataChanged = this.HasDataChanged(currentMainStatsCount, currentMainStatsSlotsCount);
        var hasData = this.Appointments != null || this.MainStats != null || this.MainStatsGrayedOut != null;

        // 初期化済みかつ（データ変更あり、またはデータが設定されている）場合に更新
        // UpdateDataAsync内でエラーハンドリングされているため、ここでは例外をキャッチしない
        if (this._isInitialized && (hasDataChanged || hasData))
        {
            await this.UpdateDataAsync();
        }

        // カウントを常に更新
        this._lastMainStatsCount = currentMainStatsCount;
        this._lastMainStatsSlotsCount = currentMainStatsSlotsCount;
    }

    private bool HasDataChanged(int currentMainStatsCount, int currentMainStatsSlotsCount)
    {
        return currentMainStatsCount != this._lastMainStatsCount || 
               currentMainStatsSlotsCount != this._lastMainStatsSlotsCount;
    }


    public async ValueTask DisposeAsync()
    {
        if (this._isInitialized)
        {
            try
            {
                await this.JSRuntime.InvokeVoidAsync("MedockCalendar.destroy");
            }
            catch
            {
                // Ignore errors during disposal
            }
        }

        this._dotNetRef?.Dispose();
    }
}


