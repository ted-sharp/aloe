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

    private void SelectAllFloors()
    {
        if (this.AvailableFloors != null)
        {
            this.CalendarState.FilterSelectedFloorIds.Clear();
            foreach (var floor in this.AvailableFloors)
            {
                this.CalendarState.FilterSelectedFloorIds.Add(floor.Id);
            }
        }
        this.OnFilterChanged();
    }

    private void ClearAllFloors()
    {
        this.SelectedFloorIds.Clear();
        this.OnFilterChanged();
    }

    private void SelectAllResources()
    {
        if (this.AvailableResources != null)
        {
            this.CalendarState.FilterSelectedResourceIds.Clear();
            foreach (var resource in this.AvailableResources)
            {
                this.CalendarState.FilterSelectedResourceIds.Add(resource.Id);
            }
        }
        this.OnFilterChanged();
    }

    private void ClearAllResources()
    {
        this.SelectedResourceIds.Clear();
        this.OnFilterChanged();
    }

    private async void SelectAllPlans()
    {
        if (this.AvailablePlans == null)
        {
            return;
        }

        this.CalendarState.FilterSelectedPlanIds.Clear();
        this._relatedOptionIds.Clear();

        // PlanTypeCode=1（Plan）の場合は最初のPlanのみ選択
        var firstPlan = this.AvailablePlans.FirstOrDefault(p => p.PlanTypeCode == 1);
        if (firstPlan != null)
        {
            this.CalendarState.FilterSelectedPlanIds.Add(firstPlan.Id);
            // このPlanに関連するオプションIDを取得してキャッシュ
            var relatedOptionIds = await this.GetRelatedOptionIdsAsync(firstPlan.Id);
            this._relatedOptionIds = new HashSet<Guid>(relatedOptionIds);
        }

        this.OnFilterChanged();
        this.StateHasChanged(); // UIを更新してオプションの選択可否状態を反映
    }

    private void ClearAllPlans()
    {
        this.SelectedPlanIds.Clear();
        this._relatedOptionIds.Clear(); // 関連オプションIDのキャッシュをクリア
        this.OnFilterChanged();
        this.StateHasChanged(); // UIを更新してオプションの選択可否状態を反映
    }
}
