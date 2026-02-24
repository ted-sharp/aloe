namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class SearchFilterPanel
{
    private void SelectAllDays()
    {
        this.CalendarState.FilterSelectedDays.Clear();
        foreach (var i in Enumerable.Range(0, 7))
        {
            this.CalendarState.FilterSelectedDays.Add(i);
        }
        this.OnFilterChanged();
    }

    private void ClearAllDays()
    {
        this.SelectedDays.Clear();
        this.OnFilterChanged();
    }

    private void SelectAllTimeSlots()
    {
        this.CalendarState.FilterSelectedTimeSlots.Clear();
        foreach (var slot in this.TimeSlots)
        {
            this.CalendarState.FilterSelectedTimeSlots.Add(slot);
        }
        this.OnFilterChanged();
    }

    private void ClearAllTimeSlots()
    {
        this.SelectedTimeSlots.Clear();
        this.OnFilterChanged();
    }

    private void ClearAllResources()
    {
        this.SelectedResourceIds.Clear();
        this.OnFilterChanged();
    }

    private void ClearAllPlans()
    {
        this.SelectedPlanIds.Clear();
        this._relatedOptionIds.Clear(); // 関連オプションIDのキャッシュをクリア
        this.OnFilterChanged();
        this.StateHasChanged(); // UIを更新してオプションの選択可否状態を反映
    }

}
