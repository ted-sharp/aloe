using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class DayView : ComponentBase
{
    [Parameter]
    public DateOnly CurrentDate { get; set; }

    [Parameter]
    public EventCallback<DateTime> OnTimeSlotClick { get; set; }

    // サンプルデータ
    private IEnumerable<(string Name, string Org, int Status)> GetSampleHourAppointments(int hour)
    {
        var hash = (this.CurrentDate.GetHashCode() + hour) % 10;
        if (hash < 4) yield break;

        var names = new[] { "山田 太郎", "佐藤 花子", "鈴木 一郎", "田中 美咲" };
        var orgs = new[] { "株式会社ABC", "XYZ商事", "個人", "医療法人DEF" };

        yield return (names[hash % 4], orgs[hash % 4], hash % 4);

        if (hash > 7)
        {
            yield return (names[(hash + 1) % 4], orgs[(hash + 1) % 4], (hash + 1) % 4);
        }
    }

    private string GetStatusBorderClass(int status)
    {
        return status switch
        {
            0 => "border-l-4 border-warning",
            1 => "border-l-4 border-info",
            2 => "border-l-4 border-success",
            3 => "border-l-4 border-error",
            _ => ""
        };
    }

    private string GetStatusBadgeClass(int status)
    {
        return status switch
        {
            0 => "badge-warning",
            1 => "badge-info",
            2 => "badge-success",
            3 => "badge-error",
            _ => ""
        };
    }

    private string GetStatusText(int status)
    {
        return status switch
        {
            0 => "予約",
            1 => "待機中",
            2 => "来院済み",
            3 => "キャンセル",
            _ => "不明"
        };
    }
}


