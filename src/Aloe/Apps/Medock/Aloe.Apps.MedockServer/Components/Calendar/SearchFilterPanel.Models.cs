namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class SearchFilterPanel
{
    /// <summary>
    /// フィルター選択肢の項目
    /// </summary>
    public class FilterItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = String.Empty;
    }

    /// <summary>
    /// 検索フィルターのデータ
    /// </summary>
    public class SearchFilter
    {
        public List<int> SelectedDays { get; set; } = new();
        public List<string> TimeSlots { get; set; } = new();
        public int RequiredCapacity { get; set; } = 1;
        public List<Guid> SelectedFloorIds { get; set; } = new();
        public List<Guid> SelectedResourceGroupIds { get; set; } = new();
        public List<Guid> SelectedResourceIds { get; set; } = new();
        public List<Guid> SelectedPlanIds { get; set; } = new();
        public List<Guid> SelectedOptionPlanIds { get; set; } = new();

        /// <summary>
        /// フィルターが有効かどうか
        /// </summary>
        public bool IsActive =>
            this.SelectedDays.Any() ||
            this.TimeSlots.Any() ||
            this.RequiredCapacity > 1 ||
            this.SelectedFloorIds.Any() ||
            this.SelectedResourceGroupIds.Any() ||
            this.SelectedResourceIds.Any() ||
            this.SelectedPlanIds.Any() ||
            this.SelectedOptionPlanIds.Any();
    }
}

