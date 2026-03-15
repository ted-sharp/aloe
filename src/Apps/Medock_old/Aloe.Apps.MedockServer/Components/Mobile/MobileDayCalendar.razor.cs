using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Mobile;

public partial class MobileDayCalendar : ComponentBase
{
    [Parameter]
    public DateOnly CurrentDate { get; set; }

    [Parameter]
    public Dictionary<string, List<AppointmentStats>> MainStats { get; set; } = new();

    [Parameter]
    public EventCallback<DateOnly> OnCreateRequested { get; set; }

    private List<DayStatItem> _dayStats = new();

    protected override void OnParametersSet()
    {
        this.BuildDayStats();
    }

    private void BuildDayStats()
    {
        this._dayStats.Clear();
        var key = this.CurrentDate.ToString("yyyy-MM-dd");
        if (!this.MainStats.TryGetValue(key, out var stats)) return;

        foreach (var stat in stats)
        {
            this._dayStats.Add(new DayStatItem
            {
                ResourceName = stat.AppointmentResource?.ApptResName ?? "リソース",
                ApptCap = stat.ApptCap,
                ApptCount = stat.ApptCount
            });
        }
    }

    private async Task HandleSlotClick(DayStatItem item)
    {
        await this.OnCreateRequested.InvokeAsync(this.CurrentDate);
    }

    private class DayStatItem
    {
        public string ResourceName { get; set; } = "";
        public int ApptCap { get; set; }
        public int ApptCount { get; set; }
    }
}
