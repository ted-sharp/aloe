using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Aloe.Apps.MedockServer.Components.Mobile;

public partial class MobileMonthCalendar : ComponentBase, IDisposable
{
    [Parameter]
    public DateOnly CurrentDate { get; set; }

    [Parameter]
    public DateOnly? SelectedDate { get; set; }

    [Parameter]
    public Dictionary<string, List<AppointmentStats>> MainStats { get; set; } = new();

    [Parameter]
    public EventCallback<DateOnly> OnDateSelected { get; set; }

    [Parameter]
    public EventCallback<DateOnly> OnLongPress { get; set; }

    private List<CalendarCell> _cells = new();
    private DateOnly _today;
    private System.Threading.Timer? _longPressTimer;
    private DateOnly? _longPressDate;

    protected override void OnInitialized()
    {
        this._today = DateOnly.FromDateTime(DateTime.Today);
    }

    protected override void OnParametersSet()
    {
        this.BuildCells();
    }

    private void BuildCells()
    {
        this._cells.Clear();

        var firstDay = new DateOnly(this.CurrentDate.Year, this.CurrentDate.Month, 1);
        // 月の最初の曜日（日曜=0）
        var startDayOfWeek = (int)firstDay.DayOfWeek;
        var gridStart = firstDay.AddDays(-startDayOfWeek);

        // 6週分 = 42マス
        for (int i = 0; i < 42; i++)
        {
            var date = gridStart.AddDays(i);
            var key = date.ToString("yyyy-MM-dd");
            var stats = this.MainStats.TryGetValue(key, out var s) ? s : null;
            var totalCap = stats?.Sum(x => x.ApptCap) ?? 0;
            var totalCount = stats?.Sum(x => x.ApptCount) ?? 0;

            this._cells.Add(new CalendarCell
            {
                Date = date,
                ApptCap = totalCap,
                ApptCount = totalCount
            });
        }

        // 最後の週が不要（月末が42マス未満の場合は35マスに縮小）
        if (this._cells.Count == 42 && this._cells[35].Date.Month != this.CurrentDate.Month &&
            this._cells[34].Date.Month != this.CurrentDate.Month)
        {
            this._cells = this._cells.Take(35).ToList();
        }
    }

    private async Task HandleCellClick(DateOnly date)
    {
        await this.OnDateSelected.InvokeAsync(date);
    }

    private void HandlePointerDown(DateOnly date, PointerEventArgs e)
    {
        this._longPressDate = date;
        this._longPressTimer?.Dispose();
        this._longPressTimer = new System.Threading.Timer(
            async _ =>
            {
                await this.InvokeAsync(async () =>
                {
                    if (this._longPressDate.HasValue)
                    {
                        await this.OnLongPress.InvokeAsync(this._longPressDate.Value);
                    }
                });
            },
            null,
            500,
            System.Threading.Timeout.Infinite);
    }

    private void HandlePointerUp()
    {
        this._longPressTimer?.Dispose();
        this._longPressTimer = null;
        this._longPressDate = null;
    }

    public void Dispose()
    {
        this._longPressTimer?.Dispose();
    }

    private class CalendarCell
    {
        public DateOnly Date { get; set; }
        public int ApptCap { get; set; }
        public int ApptCount { get; set; }
    }
}
