using Aloe.Apps.MedockLib.Data;
using Microsoft.EntityFrameworkCore;

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

    private void ToggleResource(Guid resourceId)
    {
        var or1Contains = this.CalendarState.FilterSelectedResourceIdsOr1.Contains(resourceId);
        var or2Contains = this.CalendarState.FilterSelectedResourceIdsOr2.Contains(resourceId);
        var isInAnyOrGroup = or1Contains || or2Contains;

        // アクティブなORグループが設定されている場合
        if (this.CalendarState.ActiveOrGroup.HasValue)
        {
            var activeGroup = this.CalendarState.ActiveOrGroup.Value;

            if (activeGroup == 1)
            {
                // OR1グループの処理
                if (or1Contains)
                {
                    // OR1から削除
                    this.CalendarState.FilterSelectedResourceIdsOr1.Remove(resourceId);
                    this.SelectedResourceIds.Remove(resourceId);
                }
                else
                {
                    // OR2から削除（既に含まれている場合）
                    if (or2Contains)
                    {
                        this.CalendarState.FilterSelectedResourceIdsOr2.Remove(resourceId);
                    }
                    // OR1に追加
                    this.CalendarState.FilterSelectedResourceIdsOr1.Add(resourceId);
                    this.SelectedResourceIds.Add(resourceId);
                }
            }
            else if (activeGroup == 2)
            {
                // OR2グループの処理
                if (or2Contains)
                {
                    // OR2から削除
                    this.CalendarState.FilterSelectedResourceIdsOr2.Remove(resourceId);
                    this.SelectedResourceIds.Remove(resourceId);
                }
                else
                {
                    // OR1から削除（既に含まれている場合）
                    if (or1Contains)
                    {
                        this.CalendarState.FilterSelectedResourceIdsOr1.Remove(resourceId);
                    }
                    // OR2に追加
                    this.CalendarState.FilterSelectedResourceIdsOr2.Add(resourceId);
                    this.SelectedResourceIds.Add(resourceId);
                }
            }
        }
        else
        {
            // アクティブなORグループが設定されていない場合は従来の動作
            if (this.SelectedResourceIds.Contains(resourceId))
            {
                // 選択解除
                this.CalendarState.FilterSelectedResourceIdsOr1.Remove(resourceId);
                this.CalendarState.FilterSelectedResourceIdsOr2.Remove(resourceId);
                this.SelectedResourceIds.Remove(resourceId);
            }
            else
            {
                // 通常の選択として追加（ORグループには含めない）
                this.SelectedResourceIds.Add(resourceId);
            }
        }

        this.OnFilterChanged();
    }

    private async void TogglePlan(Guid planId)
    {
        if (this.AvailablePlans == null)
        {
            return;
        }

        var plan = this.AvailablePlans.FirstOrDefault(p => p.Id == planId);
        if (plan == null)
        {
            return;
        }

        // PlanTypeCode=1（Plan）の場合
        if (plan.PlanTypeCode == 1)
        {
            if (this.SelectedPlanIds.Contains(planId))
            {
                // 選択解除
                this.SelectedPlanIds.Remove(planId);
                // このPlanに関連していたオプションもすべて解除
                var relatedOptionIds = await this.GetRelatedOptionIdsAsync(planId);
                foreach (var optionId in relatedOptionIds)
                {
                    this.SelectedPlanIds.Remove(optionId);
                }
                // 関連オプションIDのキャッシュをクリア
                this._relatedOptionIds.Clear();
            }
            else
            {
                // 他のPlanをすべて解除
                var otherPlans = this.AvailablePlans
                    .Where(p => p.PlanTypeCode == 1 && p.Id != planId)
                    .Select(p => p.Id)
                    .ToList();
                foreach (var otherPlanId in otherPlans)
                {
                    this.SelectedPlanIds.Remove(otherPlanId);
                    // 他のPlanに関連していたオプションもすべて解除
                    var relatedOptionIds = await this.GetRelatedOptionIdsAsync(otherPlanId);
                    foreach (var optionId in relatedOptionIds)
                    {
                        this.SelectedPlanIds.Remove(optionId);
                    }
                }

                // このPlanを選択
                this.SelectedPlanIds.Add(planId);

                // このPlanに関連するオプションIDを取得してキャッシュ
                var newRelatedOptionIds = await this.GetRelatedOptionIdsAsync(planId);
                this._relatedOptionIds = new HashSet<Guid>(newRelatedOptionIds);
            }
        }
        // PlanTypeCode=2（Option）の場合
        else if (plan.PlanTypeCode == 2)
        {
            // 親Planが選択されているかチェック
            var parentPlanId = await this.GetParentPlanIdAsync(planId);
            if (parentPlanId.HasValue && this.SelectedPlanIds.Contains(parentPlanId.Value))
            {
                // 親Planが選択されている場合のみトグル可能
                if (this.SelectedPlanIds.Contains(planId))
                {
                    this.SelectedPlanIds.Remove(planId);
                }
                else
                {
                    this.SelectedPlanIds.Add(planId);
                }
            }
            // 親Planが選択されていない場合は何もしない（選択不可）
        }

        this.OnFilterChanged();
        this.StateHasChanged(); // UIを更新してオプションの選択可否状態を反映
    }

    /// <summary>
    /// オプションの親PlanIDを取得
    /// </summary>
    private async Task<Guid?> GetParentPlanIdAsync(Guid optionPlanId)
    {
        try
        {
            using var context = this.DbContextFactory.CreateDbContext();
            var parentPlanId = await context.PlanOptions
                .AsNoTracking()
                .Where(po => po.OptionPlanId == optionPlanId && !po.IsDeleted)
                .Select(po => po.PlanId)
                .FirstOrDefaultAsync();
            return parentPlanId == Guid.Empty ? null : parentPlanId;
        }
        catch
        {
            return null;
        }
    }
}

