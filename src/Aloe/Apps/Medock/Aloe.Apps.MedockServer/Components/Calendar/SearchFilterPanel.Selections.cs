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

    private void SelectAllResourceGroups()
    {
        if (this.AvailableResourceGroups != null)
        {
            this.CalendarState.FilterSelectedResourceGroupIds.Clear();
            foreach (var group in this.AvailableResourceGroups)
            {
                this.CalendarState.FilterSelectedResourceGroupIds.Add(group.Id);
            }
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

    private void SelectAllPlans()
    {
        if (this.AvailablePlans != null)
        {
            this.CalendarState.FilterSelectedPlanIds.Clear();
            foreach (var plan in this.AvailablePlans)
            {
                this.CalendarState.FilterSelectedPlanIds.Add(plan.Id);
            }
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
            this.CalendarState.FilterSelectedOptionPlanIds.Clear();
            foreach (var option in this.AvailableOptions)
            {
                this.CalendarState.FilterSelectedOptionPlanIds.Add(option.Id);
            }
        }
        this.OnFilterChanged();
    }

    private void ClearAllOptions()
    {
        this.SelectedOptionPlanIds.Clear();
        this.OnFilterChanged();
    }
}
