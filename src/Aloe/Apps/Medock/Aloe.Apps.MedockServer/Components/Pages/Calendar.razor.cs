using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockServer.Components.Layout;
using Aloe.Apps.MedockServer.Components.FAB;
using Aloe.Apps.MedockServer.Components.Calendar;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Aloe.Apps.MedockServer.Components.Pages;

public partial class Calendar : ComponentBase
{
    [CascadingParameter]
    private CalendarLayout? Layout { get; set; }

    [Inject]
    private IAppointmentService AppointmentService { get; set; } = default!;

    [Inject]
    private CalendarFilterService FilterService { get; set; } = default!;

    [Inject]
    private CalendarUserService UserService { get; set; } = default!;

    [Inject]
    private CalendarDataService DataService { get; set; } = default!;

    // 状態管理
    private readonly CalendarState _state = new();

    // ドロワー状態（Layoutと連携）
    private bool _isDrawerOpen;
    private bool IsDrawerOpen
    {
        get => this.Layout?.IsDrawerOpen ?? this._isDrawerOpen;
        set
        {
            this._isDrawerOpen = value;
            if (this.Layout != null)
            {
                this.Layout.IsDrawerOpen = value;
            }
        }
    }

    // フィルターパネル参照
    private SearchFilterPanel? filterPanelRef;

    // プロパティ（razorファイルとの互換性維持）
    private DateOnly CurrentDate
    {
        get => this._state.CurrentDate;
        set => this._state.CurrentDate = value;
    }

    private CalendarViewType CurrentView
    {
        get => this._state.CurrentView;
        set => this._state.CurrentView = value;
    }

    private int WeekDays
    {
        get => this._state.WeekDays;
        set => this._state.WeekDays = value;
    }

    private bool ShowSlots
    {
        get => this._state.ShowSlots;
        set => this._state.ShowSlots = value;
    }

    private bool ShowFilterPanel
    {
        get => this._state.ShowFilterPanel;
        set => this._state.ShowFilterPanel = value;
    }

    private bool ShowSimpleView
    {
        get => this._state.ShowSimpleView;
        set => this._state.ShowSimpleView = value;
    }

    private bool ShowEquipmentGraph
    {
        get => this._state.ShowEquipmentGraph;
        set => this._state.ShowEquipmentGraph = value;
    }

    private string UserInitial => this._state.UserInitial;
    private string UserDisplayName => this._state.UserDisplayName;
    private string UserEmail => this._state.UserEmail;
    private string TenantName => this._state.TenantName;
    private string FacilityName => this._state.FacilityName;
    private string UserRole => this._state.UserRole;
    private Guid? CurrentFacilityId => this._state.CurrentFacilityId;
    private bool HasMultipleFacilities => this._state.HasMultipleFacilities;
    private List<FacilityInfo>? AvailableFacilities => this._state.AvailableFacilities;

    private bool IsModalOpen
    {
        get => this._state.IsModalOpen;
        set => this._state.IsModalOpen = value;
    }

    private DateOnly? ModalDate => this._state.ModalDate;
    private TimeOnly? ModalTime => this._state.ModalTime;
    private Guid? SelectedAppointmentId => this._state.SelectedAppointmentId;
    private DateOnly? SelectedDate => this._state.SelectedDate;
    private (DateOnly Start, DateOnly End)? SelectedDateRange => this._state.SelectedDateRange;

    private Dictionary<string, CalendarDayStats> SampleDayStats => this._state.DayStats;
    private Dictionary<string, CalendarDayStats> OriginalDayStats => this._state.OriginalDayStats;
    private List<CalendarAppointment> SampleAppointments => this._state.Appointments;
    private Dictionary<string, string> Holidays => this._state.Holidays;

    private BusinessHoursDto? BusinessHours => this._state.BusinessHours;
    private int StartHour => this._state.StartHour;
    private int EndHour => this._state.EndHour;

    private List<SearchFilterPanel.FilterItem> AvailableEquipments => this._state.AvailableEquipments;
    private SearchFilterPanel.SearchFilter? CurrentFilter => this._state.CurrentFilter;
    private int ActiveFilterCount => this._state.ActiveFilterCount;

    protected override async Task OnInitializedAsync()
    {
        await this.UserService.LoadUserInfoAsync(this._state);
        await this.DataService.LoadBusinessHoursAsync(this._state);
        this.GenerateSampleData();
        await this.DataService.GenerateFilterOptionsAsync(this._state);
        await this.DataService.LoadHolidaysAsync(this._state);
    }

    private async Task HandleLogout()
    {
        await this.UserService.HandleLogoutAsync();
    }

    private async Task HandleFacilitySwitch(Guid facilityId)
    {
        await this.UserService.HandleFacilitySwitchAsync(facilityId);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            this.RegisterLayoutActions();
        }
    }

    private void RegisterLayoutActions()
    {
        this.Layout?.RegisterCalendarActions(
            this.CurrentView.ToString().ToLower(),
            async () => { this.OpenNewAppointmentModal(); await Task.CompletedTask; },
            async () => { this.ToggleFilterPanel(); await Task.CompletedTask; },
            async () => { this.GoToToday(); await Task.CompletedTask; },
            async () => { this.PreviousPeriod(); await Task.CompletedTask; },
            async () => { this.NextPeriod(); await Task.CompletedTask; },
            async (view) => { this.SetViewFromString(view); await Task.CompletedTask; }
        );
    }

    private void ToggleFilterPanel()
    {
        this._state.ShowFilterPanel = !this._state.ShowFilterPanel;
        this.StateHasChanged();
    }

    private void CloseFilterPanel()
    {
        this._state.ShowFilterPanel = false;
        this.StateHasChanged();
    }

    private void SetViewFromString(string view)
    {
        this._state.SetViewFromString(view);
        this.Layout?.UpdateCurrentView(this.CurrentView.ToString().ToLower());
        this.RegisterLayoutActions();
    }

    private void GenerateSampleData()
    {
        SampleDataGenerator.GenerateDayStats(
            this._state.DayStats,
            this._state.OriginalDayStats,
            this._state.BusinessHours);

        this._state.Appointments = SampleDataGenerator.GenerateAppointments(this._state.BusinessHours);
    }

    private void SetView(CalendarViewType view)
    {
        this._state.SetView(view);
        this.Layout?.UpdateCurrentView(view.ToString().ToLower());
        this.RegisterLayoutActions();
    }

    private string GetCurrentPeriodTitle() => this._state.GetCurrentPeriodTitle();

    private void PreviousPeriod()
    {
        this._state.PreviousPeriod();
        this.StateHasChanged();
    }

    private void NextPeriod()
    {
        this._state.NextPeriod();
        this.StateHasChanged();
    }

    private void GoToToday()
    {
        this._state.GoToToday();
        this.StateHasChanged();
    }

    private void HandleDateClick(DateOnly date)
    {
        this._state.CurrentDate = date;
        if (this._state.CurrentView == CalendarViewType.Month)
        {
            this._state.CurrentView = CalendarViewType.Week;
            this._state.WeekDays = 7;
            this.Layout?.UpdateCurrentView("week");
            this.RegisterLayoutActions();
        }
    }

    private void HandleMonthClick((int Year, int Month) yearMonth)
    {
        this._state.CurrentDate = new DateOnly(yearMonth.Year, yearMonth.Month, 1);
        this._state.CurrentView = CalendarViewType.Month;
        this.Layout?.UpdateCurrentView("month");
        this.RegisterLayoutActions();
    }

    private void HandleMonthSelected((int Year, int Month) yearMonth)
    {
        this._state.CurrentDate = new DateOnly(yearMonth.Year, yearMonth.Month, 1);
        this.StateHasChanged();
    }

    private void HandleYearSelected(int year)
    {
        this._state.CurrentDate = new DateOnly(year, this._state.CurrentDate.Month, this._state.CurrentDate.Day);
        this.StateHasChanged();
    }

    private void HandleSimpleViewChanged(bool showSimpleView)
    {
        this._state.ShowSimpleView = showSimpleView;
        this.StateHasChanged();
    }

    private void HandleEquipmentGraphChanged(bool showEquipmentGraph)
    {
        this._state.ShowEquipmentGraph = showEquipmentGraph;
        this.StateHasChanged();
    }

    private void HandleDateSelect(DateOnly date)
    {
        this._state.SelectedDate = date;
        this._state.SelectedDateRange = null;
    }

    private void HandleDateDoubleClick(DateOnly date)
    {
        this._state.CurrentDate = date;
        this._state.CurrentView = CalendarViewType.Week;
        this._state.WeekDays = 1;
        this.Layout?.UpdateCurrentView("week");
        this.RegisterLayoutActions();
        this.StateHasChanged();
    }

    private void HandleDateRangeSelect((DateOnly Start, DateOnly End) range)
    {
        this._state.SelectedDate = null;
        this._state.SelectedDateRange = range;
    }

    private async Task HandleFilterApplied(SearchFilterPanel.SearchFilter filter)
    {
        this._state.CurrentFilter = filter;
        await this.FilterService.ApplyFilterAsync(
            filter,
            this._state.DayStats,
            this._state.OriginalDayStats,
            this._state.CurrentView,
            this._state.CurrentDate);
        this.StateHasChanged();
    }

    private async Task HandleFilterChangedRealtime(SearchFilterPanel.SearchFilter filter)
    {
        this._state.CurrentFilter = filter;

        if (filter.EquipIds.Any() && !this._state.ShowEquipmentGraph)
        {
            this._state.ShowEquipmentGraph = true;
        }

        await this.FilterService.ApplyFilterAsync(
            filter,
            this._state.DayStats,
            this._state.OriginalDayStats,
            this._state.CurrentView,
            this._state.CurrentDate);
        this.StateHasChanged();
    }

    private void HandleAppointmentClick(Guid apptId)
    {
        this._state.OpenModal(this._state.CurrentDate, new TimeOnly(9, 0), apptId);
    }

    private void HandleCreateRequest((DateOnly Date, TimeOnly Time) request)
    {
        this._state.OpenModal(request.Date, request.Time);
    }

    private void HandleWeekDaysChanged(int days)
    {
        this._state.WeekDays = days;
    }

    private void HandleShowSlotsChanged(bool showSlots)
    {
        this._state.ShowSlots = showSlots;
        this.StateHasChanged();
    }

    private void HandleGoToToday()
    {
        this.GoToToday();
    }

    private void OpenNewAppointmentModal()
    {
        this._state.OpenModal(this._state.CurrentDate, new TimeOnly(9, 0));
    }

    private void CloseModal()
    {
        this._state.CloseModal();
    }

    private void HandleSaveAppointment()
    {
        this.CloseModal();
    }

    private async Task HandleAppointmentMoved((Guid ApptId, DateOnly NewDate, TimeOnly NewTime) moveInfo)
    {
        try
        {
            var appt = this._state.Appointments.FirstOrDefault(a => a.Id == moveInfo.ApptId);
            if (appt != null)
            {
                var duration = appt.EndTime.HasValue && appt.StartTime.HasValue
                    ? appt.EndTime.Value - appt.StartTime.Value
                    : TimeSpan.FromHours(1);
                var newEndTime = moveInfo.NewTime.Add(duration);

                await this.AppointmentService.UpdateAppointmentAsync(
                    moveInfo.ApptId,
                    new UpdateAppointmentDto
                    {
                        Date = moveInfo.NewDate,
                        StartTime = moveInfo.NewTime,
                        EndTime = newEndTime
                    });

                appt.Date = moveInfo.NewDate;
                appt.StartTime = moveInfo.NewTime;
                appt.EndTime = newEndTime;
                this.StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"予約移動エラー: {ex.Message}");
        }
    }
}
