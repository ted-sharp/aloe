namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class SearchFilterPanel
{
    private void SelectAllDays()
    {
        this.SelectedDays = Enumerable.Range(0, 7).ToHashSet();
        this.OnFilterChanged();
    }

    private void ClearAllDays()
    {
        this.SelectedDays.Clear();
        this.OnFilterChanged();
    }

    private void SelectAllTimeSlots()
    {
        this.SelectedTimeSlots = this.TimeSlots.ToHashSet();
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
            this.SelectedFloorIds = this.AvailableFloors.Select(f => f.Id).ToHashSet();
        }
        this.OnFilterChanged();
    }

    private void ClearAllFloors()
    {
        this.SelectedFloorIds.Clear();
        this.OnFilterChanged();
    }

    private void SelectAllResourceGroups()
    {
        if (this.AvailableResourceGroups != null)
        {
            this.SelectedResourceGroupIds = this.AvailableResourceGroups.Select(g => g.Id).ToHashSet();
        }
        this.OnFilterChanged();
    }

    private void ClearAllResourceGroups()
    {
        this.SelectedResourceGroupIds.Clear();
        this.OnFilterChanged();
    }

    private void SelectAllResources()
    {
        if (this.AvailableResources != null)
        {
            this.SelectedResourceIds = this.AvailableResources.Select(r => r.Id).ToHashSet();
        }
        this.OnFilterChanged();
    }

    private void ClearAllResources()
    {
        this.SelectedResourceIds.Clear();
        this.OnFilterChanged();
    }

    private void SelectAllPlans()
    {
        if (this.AvailablePlans != null)
        {
            this.SelectedPlanIds = this.AvailablePlans.Select(p => p.Id).ToHashSet();
        }
        this.OnFilterChanged();
    }

    private void ClearAllPlans()
    {
        this.SelectedPlanIds.Clear();
        this.OnFilterChanged();
    }

    private void SelectAllOptions()
    {
        if (this.AvailableOptions != null)
        {
            this.SelectedOptionPlanIds = this.AvailableOptions.Select(o => o.Id).ToHashSet();
        }
        this.OnFilterChanged();
    }

    private void ClearAllOptions()
    {
        this.SelectedOptionPlanIds.Clear();
        this.OnFilterChanged();
    }
}

