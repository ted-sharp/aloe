using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class WeekView : ComponentBase
{
    [Parameter]
    public DateOnly CurrentDate { get; set; }

    [Parameter]
    public EventCallback<DateTime> OnTimeSlotClick { get; set; }

    // サンプルデータ
    private IEnumerable<(string Name, string Org, int Status)> GetSampleHourAppointments(DateOnly date, int hour)
    {
        var hash = (date.GetHashCode() + hour) % 10;
        if (hash < 3) yield break;

        var names = new[] { "山田 太郎", "佐藤 花子", "鈴木 一郎" };
        var orgs = new[] { "ABC商事", "XYZ工業", "個人" };

        if (hash > 6)
        {
            yield return (names[hash % 3], orgs[hash % 3], hash % 4);
        }
        if (hash > 8)
        {
            yield return (names[(hash + 1) % 3], orgs[(hash + 1) % 3], (hash + 1) % 4);
        }
    }

    private string GetStatusClass(int status)
    {
        return status switch
        {
            0 => "bg-warning/20 border-l-4 border-warning",
            1 => "bg-info/20 border-l-4 border-info",
            2 => "bg-success/20 border-l-4 border-success",
            3 => "bg-error/20 border-l-4 border-error",
            _ => "bg-base-200"
        };
    }
}


