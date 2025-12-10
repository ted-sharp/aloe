using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class SearchFilterPanel : ComponentBase
{
    /// <summary>
    /// 利用可能な設備のリスト
    /// </summary>
    [Parameter]
    public List<FilterItem> AvailableEquipments { get; set; } = new();

    /// <summary>
    /// フィルター変更時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<SearchFilter> OnFilterApplied { get; set; }

    /// <summary>
    /// リアルタイム検索用のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<SearchFilter> OnFilterChangedRealtime { get; set; }

    /// <summary>
    /// パネルを閉じる時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    // フィルター値
    private HashSet<int> SelectedDays { get; set; } = new(); // デフォルト全オフ
    private HashSet<string> SelectedTimeSlots { get; set; } = new(); // 時間帯複数選択
    private int RequiredCapacity { get; set; } = 1;
    private HashSet<Guid> SelectedEquipments { get; set; } = new();

    // 定数
    private readonly string[] DayNames = { "日", "月", "火", "水", "木", "金", "土" };
    private readonly string[] TimeSlots = { "08:00", "09:00", "10:00", "11:00", "13:00", "14:00", "15:00", "16:00" };

    private int ActiveFilterCount =>
        (this.SelectedDays.Any() ? 1 : 0) +
        (this.SelectedTimeSlots.Any() ? 1 : 0) +
        (this.RequiredCapacity > 1 ? 1 : 0) +
        (this.SelectedEquipments.Any() ? 1 : 0);

    private async Task HandleClose()
    {
        await this.OnClose.InvokeAsync();
    }

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

    private void ToggleEquipment(Guid equipId)
    {
        if (this.SelectedEquipments.Contains(equipId))
        {
            this.SelectedEquipments.Remove(equipId);
        }
        else
        {
            this.SelectedEquipments.Add(equipId);
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

    private void ClearFilters()
    {
        this.SelectedDays = new();
        this.SelectedTimeSlots.Clear();
        this.RequiredCapacity = 1;
        this.SelectedEquipments.Clear();
        this.OnFilterChanged();
    }

    private async void OnFilterChanged()
    {
        // リアルタイム検索
        var filter = this.BuildFilter();
        await this.OnFilterChangedRealtime.InvokeAsync(filter);
    }

    private SearchFilter BuildFilter()
    {
        return new SearchFilter
        {
            SelectedDays = this.SelectedDays.ToList(),
            TimeSlots = this.SelectedTimeSlots.ToList(),
            RequiredCapacity = this.RequiredCapacity,
            EquipIds = this.SelectedEquipments.ToList()
        };
    }

    /// <summary>
    /// フィルター選択肢の項目
    /// </summary>
    public class FilterItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = String.Empty;
    }

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

    private void SelectAllEquipments()
    {
        this.SelectedEquipments = this.AvailableEquipments.Select(e => e.Id).ToHashSet();
        this.OnFilterChanged();
    }

    private void ClearAllEquipments()
    {
        this.SelectedEquipments.Clear();
        this.OnFilterChanged();
    }

    /// <summary>
    /// 検索フィルターのデータ
    /// </summary>
    public class SearchFilter
    {
        public List<int> SelectedDays { get; set; } = new();
        public List<string> TimeSlots { get; set; } = new();
        public int RequiredCapacity { get; set; } = 1;
        public List<Guid> EquipIds { get; set; } = new();

        /// <summary>
        /// フィルターが有効かどうか
        /// </summary>
        public bool IsActive =>
            this.SelectedDays.Any() ||
            this.TimeSlots.Any() ||
            this.RequiredCapacity > 1 ||
            this.EquipIds.Any();
    }
}

