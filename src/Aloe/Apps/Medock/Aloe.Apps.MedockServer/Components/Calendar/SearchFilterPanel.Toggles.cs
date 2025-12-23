namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class SearchFilterPanel
{
    private void ToggleDay(int dayIndex)
    {
        if (this.SelectedDays.Contains(dayIndex))
        {
            this.SelectedDays.Remove(dayIndex);
        }
        else
        {
            this.SelectedDays.Add(dayIndex);
        }
        this.OnFilterChanged();
    }

    private void ToggleTimeSlot(string timeSlot)
    {
        if (this.SelectedTimeSlots.Contains(timeSlot))
        {
            this.SelectedTimeSlots.Remove(timeSlot);
        }
        else
        {
            this.SelectedTimeSlots.Add(timeSlot);
        }
        this.OnFilterChanged();
    }

    private void ToggleFloor(Guid floorId)
    {
        if (this.SelectedFloorIds.Contains(floorId))
        {
            this.SelectedFloorIds.Remove(floorId);
        }
        else
        {
            this.SelectedFloorIds.Add(floorId);
        }
        this.OnFilterChanged();
    }

    private void ToggleResourceGroup(Guid groupId)
    {
        if (this.SelectedResourceGroupIds.Contains(groupId))
        {
            this.SelectedResourceGroupIds.Remove(groupId);
        }
        else
        {
            this.SelectedResourceGroupIds.Add(groupId);
        }
        this.OnFilterChanged();
    }

    private void ToggleResource(Guid resourceId)
    {
        if (this.SelectedResourceIds.Contains(resourceId))
        {
            this.SelectedResourceIds.Remove(resourceId);
        }
        else
        {
            this.SelectedResourceIds.Add(resourceId);
        }
        this.OnFilterChanged();
    }

    private void TogglePlan(Guid planId)
    {
        if (this.SelectedPlanIds.Contains(planId))
        {
            this.SelectedPlanIds.Remove(planId);
        }
        else
        {
            this.SelectedPlanIds.Add(planId);
        }
        this.OnFilterChanged();
    }

    private void ToggleOption(Guid optionPlanId)
    {
        if (this.SelectedOptionPlanIds.Contains(optionPlanId))
        {
            this.SelectedOptionPlanIds.Remove(optionPlanId);
        }
        else
        {
            this.SelectedOptionPlanIds.Add(optionPlanId);
        }
        this.OnFilterChanged();
    }
}

