using Aloe.Apps.MedockServer.Components.Layout;
using Aloe.Apps.MedockServer.ApplicationServices.Calendar;
using Aloe.Apps.MedockServer.Components.Calendar;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.Dtos;
using Aloe.Apps.MedockLib.Services.Dtos.Appointments;
using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Aloe.Apps.MedockServer.Components.Pages;

public partial class Calendar : ComponentBase
{
    [CascadingParameter]
    private CalendarLayout? Layout { get; set; }

    [Inject]
    private CalendarState State { get; set; } = default!;

    [Inject]
    private ILogger<Calendar> Logger { get; set; } = default!;

    [Inject]
    private ICalendarOrchestrator Orchestrator { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    private IDateTimeProvider DateTimeProvider { get; set; } = default!;

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

    // プロパティ（razorファイルとの互換性維持 - .razorファイル変更不要にするため保持）
    private DateOnly CurrentDate
    {
        get => this.State.CurrentDate;
        set => this.State.CurrentDate = value;
    }

    private CalendarViewType CurrentView
    {
        get => this.State.CurrentView;
        set => this.State.CurrentView = value;
    }

    private int WeekDays
    {
        get => this.State.WeekDays;
        set => this.State.WeekDays = value;
    }

    private bool ShowSlots
    {
        get => this.State.ShowSlots;
        set => this.State.ShowSlots = value;
    }

    private bool ShowFilterPanel
    {
        get => this.State.ShowFilterPanel;
        set => this.State.ShowFilterPanel = value;
    }

    private bool ShowSimpleView
    {
        get => this.State.ShowSimpleView;
        set => this.State.ShowSimpleView = value;
    }

    private bool IsLoading => this.State.IsLoading;

    private string UserInitial => this.State.UserInitial;
    private string UserDisplayName => this.State.UserDisplayName;
    private string UserEmail => this.State.UserEmail;
    private string TenantName => this.State.TenantName;
    private string FacilityName => this.State.FacilityName;
    private string UserRole => this.State.UserRole;
    private Guid? CurrentFacilityId => this.State.CurrentFacilityId;
    private bool HasMultipleFacilities => this.State.HasMultipleFacilities;
    private List<FacilityInfo>? AvailableFacilities => this.State.AvailableFacilities;

    private bool IsModalOpen
    {
        get => this.State.IsModalOpen;
        set => this.State.IsModalOpen = value;
    }

    private DateOnly? ModalDate => this.State.ModalDate;
    private int? ModalStartMin => this.State.ModalStartMin;
    private Guid? SelectedAppointmentId => this.State.SelectedAppointmentId;
    private DateOnly? SelectedDate => this.State.SelectedDate;
    private (DateOnly Start, DateOnly End)? SelectedDateRange => this.State.SelectedDateRange;

    // 日詳細ダイアログの状態
    private bool IsDayDetailOpen { get; set; }
    private DateOnly? DayDetailDate { get; set; }

    private Dictionary<string, List<AppointmentStats>> MainStats => this.State.MainStats;
    private Dictionary<string, bool> MainStatsGrayedOut => this.State.MainStatsGrayedOut;
    private Dictionary<string, List<ResourceStatSlotsDto>>? EquipmentStatsOptimized => this.State.EquipmentStatsOptimized;
    private List<AppointmentDto> Appointments => this.State.Appointments;
    private Dictionary<string, string> Holidays => this.State.Holidays;

    private BusinessHoursDto? BusinessHours => this.State.BusinessHours;
    private int StartHour => this.State.StartHour;
    private int EndHour => this.State.EndHour;

    private SearchFilterPanel.SearchFilter? CurrentFilter => this.State.CurrentFilter;
    private int ActiveFilterCount => this.State.ActiveFilterCount;
    private List<string>? FilterTimeSlots => this.State.CurrentFilter?.TimeSlots;
    private List<SearchFilterPanel.FilterItem> AvailableFloors => this.State.AvailableFloors;
    private List<SearchFilterPanel.FilterItem> AvailableResources => this.State.AvailableResources;
    private List<SearchFilterPanel.FilterItem> AvailablePlans => this.State.AvailablePlans;

    protected override async Task OnInitializedAsync()
    {
        this.Logger.LogInformation("Calendar.OnInitializedAsync start");

        var authState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync();
        var result = await this.Orchestrator.InitializeAsync(authState.User);

        if (!result.Success && result.RedirectUrl != null)
        {
            this.NavigationManager.NavigateTo(result.RedirectUrl, forceLoad: true);
            return;
        }

        this.StateHasChanged();
    }

    private async Task HandleLogout()
    {
        var result = await this.Orchestrator.LogoutAsync();
        if (result.RedirectUrl != null)
        {
            this.NavigationManager.NavigateTo(result.RedirectUrl, forceLoad: true);
        }
    }

    private async Task HandleFacilitySwitch(Guid facilityId)
    {
        var result = await this.Orchestrator.SwitchFacilityAsync(facilityId);
        if (result.Success && result.RedirectUrl != null)
        {
            this.NavigationManager.NavigateTo(result.RedirectUrl, forceLoad: true);
        }
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
            async () => { await this.GoToToday(); },
            async () => { await this.PreviousPeriod(); },
            async () => { await this.NextPeriod(); },
            async (view) => { this.SetViewFromString(view); await Task.CompletedTask; }
        );
    }

    private void ToggleFilterPanel()
    {
        this.State.ShowFilterPanel = !this.State.ShowFilterPanel;
        this.StateHasChanged();
    }

    private void CloseFilterPanel()
    {
        this.State.ShowFilterPanel = false;
        this.StateHasChanged();
    }

    private void SetViewFromString(string view)
    {
        this.State.SetViewFromString(view);
        this.Layout?.UpdateCurrentView(this.CurrentView.ToString().ToLower());
        this.RegisterLayoutActions();
    }

    private async Task SetView(CalendarViewType view)
    {
        await this.Orchestrator.SetViewAsync(view);
        this.Layout?.UpdateCurrentView(view.ToString().ToLower());
        this.RegisterLayoutActions();
        this.StateHasChanged();
    }

    private string GetCurrentPeriodTitle() => this.State.GetCurrentPeriodTitle();

    // Navigation methods
    private async Task PreviousPeriod()
    {
        await this.Orchestrator.NavigatePreviousPeriodAsync();
        this.StateHasChanged();
    }

    private async Task NextPeriod()
    {
        await this.Orchestrator.NavigateNextPeriodAsync();
        this.StateHasChanged();
    }

    private async Task PreviousBigPeriod()
    {
        await this.Orchestrator.NavigatePreviousBigPeriodAsync();
        this.StateHasChanged();
    }

    private async Task NextBigPeriod()
    {
        await this.Orchestrator.NavigateNextBigPeriodAsync();
        this.StateHasChanged();
    }

    private async Task GoToToday()
    {
        await this.Orchestrator.GoToTodayAsync();
        this.StateHasChanged();
    }

    private async Task HandleDateClick(DateOnly date)
    {
        await this.Orchestrator.HandleDateClickAsync(date);
        this.Layout?.UpdateCurrentView("week");
        this.RegisterLayoutActions();
        this.StateHasChanged();
    }

    private async Task HandleDateSelectedSingle(DateOnly date)
    {
        await this.Orchestrator.HandleDateSelectedSingleAsync(date);
        this.StateHasChanged();
    }

    private async Task HandleMonthClick((int Year, int Month) yearMonth)
    {
        await this.Orchestrator.HandleMonthClickAsync(yearMonth.Year, yearMonth.Month);
        this.Layout?.UpdateCurrentView("month");
        this.RegisterLayoutActions();
        this.StateHasChanged();
    }

    private async Task HandleMonthSelected((int Year, int Month) yearMonth)
    {
        await this.Orchestrator.HandleMonthSelectedAsync(yearMonth.Year, yearMonth.Month);
        this.StateHasChanged();
    }

    private async Task HandleYearSelected(int year)
    {
        await this.Orchestrator.HandleYearSelectedAsync(year);
        this.StateHasChanged();
    }

    private void HandleSimpleViewChanged(bool showSimpleView)
    {
        this.State.ShowSimpleView = showSimpleView;
        this.StateHasChanged();
    }

    private void HandleDateSelect(DateOnly date)
    {
        this.State.SelectedDate = date;
        this.State.SelectedDateRange = null;
    }

    private async Task HandleDateDoubleClick(DateOnly date)
    {
        await this.Orchestrator.HandleDateDoubleClickAsync(date);
        this.Layout?.UpdateCurrentView("week");
        this.RegisterLayoutActions();
        this.StateHasChanged();
    }

    private void HandleDateRangeSelect((DateOnly Start, DateOnly End) range)
    {
        this.State.SelectedDate = null;
        this.State.SelectedDateRange = range;
    }

    // Filter handlers
    private async Task HandleFilterApplied(SearchFilterPanel.SearchFilter filter)
    {
        await this.Orchestrator.ApplyFilterAsync(filter);
        this.StateHasChanged();
    }

    private async Task ReapplyCurrentFilterAsync()
    {
        await this.Orchestrator.ReapplyCurrentFilterAsync();
    }

    private async Task HandleFilterChangedRealtime(SearchFilterPanel.SearchFilter filter)
    {
        await this.Orchestrator.HandleFilterChangedRealtimeAsync(filter);
        this.StateHasChanged();
    }

    // Modal handlers
    private void HandleAppointmentClick(Guid apptId)
    {
        this.Orchestrator.OpenAppointmentModal(apptId);
        this.StateHasChanged();
    }

    private void HandleCreateRequest((DateOnly Date, int StartMin) request)
    {
        this.Orchestrator.OpenCreateRequestModal(request.Date, request.StartMin);
        this.StateHasChanged();
    }

    private async Task HandleWeekDaysChanged(int days)
    {
        await this.Orchestrator.HandleWeekDaysChangedAsync(days);
        this.StateHasChanged();
    }

    private void HandleShowSlotsChanged(bool showSlots)
    {
        this.State.ShowSlots = showSlots;
        this.StateHasChanged();
    }

    private async Task HandleGoToToday()
    {
        await this.GoToToday();
    }

    private void OpenNewAppointmentModal()
    {
        this.Orchestrator.OpenNewAppointmentModal(this.State.CurrentDate, 540);
        this.StateHasChanged();
    }

    private void CloseModal()
    {
        this.Orchestrator.CloseModal();
        this.StateHasChanged();
    }

    private async Task HandleSaveAppointment()
    {
        await this.Orchestrator.SaveAppointmentAsync();
        this.CloseModal();
        this.StateHasChanged();
    }

    private async Task HandleDeleteAppointment()
    {
        await this.Orchestrator.DeleteAppointmentAsync();
        this.CloseModal();
        this.StateHasChanged();
    }

    private async Task HandleAppointmentMoved((Guid ApptId, DateOnly NewDate, int NewStartMin) moveInfo)
    {
        try
        {
            var result = await this.Orchestrator.MoveAppointmentAsync(
                moveInfo.ApptId,
                moveInfo.NewDate,
                moveInfo.NewStartMin);

            if (!result.Success && result.ErrorMessage != null)
            {
                this.Logger.LogWarning("Failed to move appointment: {ErrorMessage}", result.ErrorMessage);
                await this.JSRuntime.InvokeVoidAsync("alert", result.ErrorMessage);
            }
            else if (result.Success)
            {
                this.StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error moving appointment");
        }
    }

    // Day detail popup handlers
    private void HandleShowDayDetail(DateOnly date)
    {
        this.DayDetailDate = date;
        this.IsDayDetailOpen = true;
        this.StateHasChanged();
    }

    private void CloseDayDetail()
    {
        this.IsDayDetailOpen = false;
        this.DayDetailDate = null;
        this.StateHasChanged();
    }

    private List<AppointmentStats>? GetDayDetailStats()
    {
        if (!this.DayDetailDate.HasValue) return null;
        var dateStr = this.DayDetailDate.Value.ToString("yyyy-MM-dd");
        return this.MainStats.TryGetValue(dateStr, out var stats) ? stats : null;
    }

    private async Task HandleDayDetailGoToWeekView(DateOnly date)
    {
        this.CloseDayDetail();
        this.State.CurrentDate = date;
        this.State.CurrentView = CalendarViewType.Week;
        this.Layout?.UpdateCurrentView("week");
        this.RegisterLayoutActions();
        await this.Orchestrator.RefreshCalendarDataAsync();
        this.StateHasChanged();
    }

    private async Task HandleDayDetailNavigateToPreviousDay(DateOnly date)
    {
        this.DayDetailDate = date;
        this.State.CurrentDate = date;
        await this.Orchestrator.RefreshCalendarDataAsync();
        this.StateHasChanged();
    }

    private async Task HandleDayDetailNavigateToNextDay(DateOnly date)
    {
        this.DayDetailDate = date;
        this.State.CurrentDate = date;
        await this.Orchestrator.RefreshCalendarDataAsync();
        this.StateHasChanged();
    }

    // Keyboard handler (UI logic - kept in component)
    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        // 日詳細ダイアログが開いている場合の処理
        if (this.IsDayDetailOpen && this.DayDetailDate.HasValue)
        {
            switch (e.Key)
            {
                case "ArrowLeft":
                    await this.HandleDayDetailNavigateToPreviousDay(this.DayDetailDate.Value.AddDays(-1));
                    return;
                case "ArrowRight":
                    await this.HandleDayDetailNavigateToNextDay(this.DayDetailDate.Value.AddDays(1));
                    return;
                case "Escape":
                    this.CloseDayDetail();
                    return;
                default:
                    return;
            }
        }

        switch (e.Key)
        {
            case "Enter":
            case " ":
                this.HandleShowDayDetail(this.State.CurrentDate);
                break;
            case "ArrowLeft":
                await this.PreviousPeriod();
                break;
            case "ArrowRight":
                await this.NextPeriod();
                break;
            case "ArrowUp":
                await this.SetPreviousView();
                break;
            case "ArrowDown":
                await this.SetNextView();
                break;
            case "Home":
                await this.GoToToday();
                break;
            case "PageUp":
                await this.NextBigPeriod();
                break;
            case "PageDown":
                await this.PreviousBigPeriod();
                break;
            case "t":
            case "T":
                this.ToggleSimpleView();
                break;
            case "f":
            case "F":
                this.ToggleFilterPanel();
                break;
        }
    }

    private async Task SetPreviousView()
    {
        var nextView = this.CurrentView switch
        {
            CalendarViewType.Week => CalendarViewType.Month,
            CalendarViewType.Month => CalendarViewType.Year,
            CalendarViewType.Year => CalendarViewType.Week,
            _ => CalendarViewType.Month
        };
        await this.SetView(nextView);
    }

    private async Task SetNextView()
    {
        var nextView = this.CurrentView switch
        {
            CalendarViewType.Year => CalendarViewType.Month,
            CalendarViewType.Month => CalendarViewType.Week,
            CalendarViewType.Week => CalendarViewType.Year,
            _ => CalendarViewType.Month
        };
        await this.SetView(nextView);
    }

    private void ToggleSimpleView()
    {
        if (this.CurrentView == CalendarViewType.Week)
        {
            this.ShowSlots = !this.ShowSlots;
        }
        else
        {
            this.ShowSimpleView = !this.ShowSimpleView;
        }
        this.StateHasChanged();
    }

    private void HandleDayDetailSlotClicked(DayDetailPopup.SlotClickedEventArgs args)
    {
        this.Logger.LogInformation("日詳細スロット選択: Date={Date}, Time={Start}-{End}",
            args.Date, args.StartMin, args.EndMin);

        this.CloseDayDetail();
        this.State.OpenModal(args.Date, args.StartMin);
        this.StateHasChanged();
    }
}
