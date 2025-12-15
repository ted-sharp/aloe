using Aloe.Apps.MedockLib.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class CalendarCanvas : ComponentBase, IAsyncDisposable
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Current view type: "year", "month", "week"
    /// </summary>
    [Parameter]
    public string ViewType { get; set; } = "month";

    /// <summary>
    /// Current date (center of the view)
    /// </summary>
    [Parameter]
    public DateOnly CurrentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// Appointment data for the calendar
    /// </summary>
    [Parameter]
    public IEnumerable<CalendarAppointment>? Appointments { get; set; }

    /// <summary>
    /// Day statistics (AM/PM counts per day)
    /// </summary>
    [Parameter]
    public Dictionary<string, CalendarDayStats>? DayStats { get; set; }

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
    public EventCallback<(DateOnly Date, TimeOnly Time)> OnCreateRequested { get; set; }

    /// <summary>
    /// Callback when an appointment is moved via drag and drop (apptId, newDate, newTime)
    /// </summary>
    [Parameter]
    public EventCallback<(Guid ApptId, DateOnly NewDate, TimeOnly NewTime)> OnAppointmentMoved { get; set; }

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

    private string ContainerId { get; } = $"calendar-canvas-{Guid.CreateVersion7():N}";
    private DotNetObjectReference<CalendarCanvas>? _dotNetRef;
    private bool _isInitialized = false;
    private string? _lastViewType;
    private DateOnly _lastDate;
    private int _lastWeekDays;
    private bool _lastShowSlots;
    private bool _lastShowSimpleView;
    // _lastShowEquipmentGraphは削除されました（EquipmentはAppointmentResourceに統合）

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            this._dotNetRef = DotNetObjectReference.Create(this);
            await this.InitializeCalendarAsync();
            this._isInitialized = true;
            this._lastViewType = this.ViewType;
            this._lastDate = this.CurrentDate;
            this._lastWeekDays = this.WeekDays;
            this._lastShowSlots = this.ShowSlots;
            this._lastShowSimpleView = this.ShowSimpleView;
            // _lastShowEquipmentGraphの設定は削除されました
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

    protected override async Task OnParametersSetAsync()
    {
        if (this._isInitialized && (this.Appointments != null || this.DayStats != null))
        {
            await this.UpdateDataAsync();
        }
    }

    private async Task InitializeCalendarAsync()
    {
        // ES Moduleの読み込み完了を待つ
        var maxRetries = 50;
        var retryDelay = 100; // 100ms
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var isReady = await this.JSRuntime.InvokeAsync<bool>("eval", "typeof window.MedockCalendar !== 'undefined'");
                if (isReady)
                {
                    break;
                }
            }
            catch
            {
                // エラーは無視してリトライ
            }

            if (i < maxRetries - 1)
            {
                await Task.Delay(retryDelay);
            }
            else
            {
                // 最終的に読み込まれなかった場合はエラーをログに出力
                Console.WriteLine("Warning: MedockCalendar module not loaded after retries");
                return;
            }
        }

        var data = CalendarCanvasInterop.BuildDataObject(
            this.Appointments,
            this.DayStats,
            this.Holidays);
        var options = CalendarCanvasInterop.BuildOptions(
            this.WeekDays,
            this.ShowSlots,
            this.ShowSimpleView,
            this.StartHour,
            this.EndHour,
            this.BusinessHours);

        await this.JSRuntime.InvokeVoidAsync(
            "MedockCalendar.init",
            this.ContainerId,
            data,
            options,
            this._dotNetRef);

        // Set initial view
        await this.JSRuntime.InvokeVoidAsync(
            "MedockCalendar.changeView",
            this.ViewType,
            this.CurrentDate.ToString("yyyy-MM-dd"));
    }

    private async Task UpdateDataAsync()
    {
        if (!this._isInitialized) return;

        var data = CalendarCanvasInterop.BuildDataObject(
            this.Appointments,
            this.DayStats,
            this.Holidays);
        await this.JSRuntime.InvokeVoidAsync("MedockCalendar.updateData", data);
    }

    private async Task ChangeViewAsync()
    {
        if (!this._isInitialized) return;

        // Update options if weekDays, showSlots, or showSimpleView changed
        if (this._lastWeekDays != this.WeekDays || this._lastShowSlots != this.ShowSlots || this._lastShowSimpleView != this.ShowSimpleView)
        {
            var options = CalendarCanvasInterop.BuildOptions(
                this.WeekDays,
                this.ShowSlots,
                this.ShowSimpleView,
                this.StartHour,
                this.EndHour,
                this.BusinessHours);
            await this.JSRuntime.InvokeVoidAsync("MedockCalendar.setOptions", options);
        }

        await this.JSRuntime.InvokeVoidAsync(
            "MedockCalendar.changeView",
            this.ViewType,
            this.CurrentDate.ToString("yyyy-MM-dd"));
    }

    // ============================================================
    // JSInvokable callbacks (called from JavaScript)
    // ============================================================

    [JSInvokable]
    public async Task OnDateSelectedCallback(string dateStr)
    {
        if (DateOnly.TryParse(dateStr, out var date))
        {
            await this.OnDateSelected.InvokeAsync(date);
        }
    }

    [JSInvokable("OnDateSelected")]
    public async Task OnDateSelectedFromJs(string dateStr)
    {
        if (DateOnly.TryParse(dateStr, out var date))
        {
            await this.OnDateSelected.InvokeAsync(date);
        }
    }

    [JSInvokable("OnAppointmentClicked")]
    public async Task OnAppointmentClickedFromJs(string apptId)
    {
        if (Guid.TryParse(apptId, out var id))
        {
            await this.OnAppointmentClicked.InvokeAsync(id);
        }
    }

    [JSInvokable("OnCreateRequested")]
    public async Task OnCreateRequestedFromJs(string dateStr, string timeStr)
    {
        if (DateOnly.TryParse(dateStr, out var date) &&
            TimeOnly.TryParse(timeStr, out var time))
        {
            await this.OnCreateRequested.InvokeAsync((date, time));
        }
    }

    [JSInvokable("OnAppointmentMoved")]
    public async Task OnAppointmentMovedFromJs(string apptId, string dateStr, string timeStr)
    {
        if (Guid.TryParse(apptId, out var id) &&
            DateOnly.TryParse(dateStr, out var date) &&
            TimeOnly.TryParse(timeStr, out var time))
        {
            await this.OnAppointmentMoved.InvokeAsync((id, date, time));
        }
    }

    [JSInvokable("OnMonthClicked")]
    public async Task OnMonthClickedFromJs(int year, int month)
    {
        await this.OnMonthClicked.InvokeAsync((year, month));
    }

    [JSInvokable("OnDateSelectedSingle")]
    public async Task OnDateSelectedSingleFromJs(string dateStr)
    {
        if (DateOnly.TryParse(dateStr, out var date))
        {
            await this.OnDateSelectedSingle.InvokeAsync(date);
        }
    }

    [JSInvokable("OnDateDoubleClicked")]
    public async Task OnDateDoubleClickedFromJs(string dateStr)
    {
        if (DateOnly.TryParse(dateStr, out var date))
        {
            await this.OnDateDoubleClicked.InvokeAsync(date);
        }
    }

    [JSInvokable("OnDateRangeSelected")]
    public async Task OnDateRangeSelectedFromJs(string startDateStr, string endDateStr)
    {
        if (DateOnly.TryParse(startDateStr, out var startDate) &&
            DateOnly.TryParse(endDateStr, out var endDate))
        {
            await this.OnDateRangeSelected.InvokeAsync((startDate, endDate));
        }
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
