using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.Dtos;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockServer.Components.Layout;
using Aloe.Apps.MedockServer.Components.FAB;
using Aloe.Apps.MedockServer.Components.Calendar;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Diagnostics;

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
    private IUserContextService UserContextService { get; set; } = default!;

    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private CalendarDataService DataService { get; set; } = default!;

    // 状態管理（Scoped Service として DI 注入）
    [Inject]
    private CalendarState State { get; set; } = default!;

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

    // ShowEquipmentGraphプロパティは削除されました（EquipmentはAppointmentResourceに統合）

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
    private TimeOnly? ModalTime => this.State.ModalTime;
    private Guid? SelectedAppointmentId => this.State.SelectedAppointmentId;
    private DateOnly? SelectedDate => this.State.SelectedDate;
    private (DateOnly Start, DateOnly End)? SelectedDateRange => this.State.SelectedDateRange;

    // 日詳細ダイアログの状態
    private bool IsDayDetailOpen { get; set; }
    private DateOnly? DayDetailDate { get; set; }

    private Dictionary<string, List<AppointmentStats>> MainStats => this.State.MainStats;
    private Dictionary<string, List<AppointmentStats>> OriginalMainStats => this.State.OriginalMainStats;
    private Dictionary<string, bool> MainStatsGrayedOut => this.State.MainStatsGrayedOut;
    private List<AppointmentDto> Appointments => this.State.Appointments;
    private Dictionary<string, string> Holidays => this.State.Holidays;

    private BusinessHoursDto? BusinessHours => this.State.BusinessHours;
    private int StartHour => this.State.StartHour;
    private int EndHour => this.State.EndHour;

    // AvailableEquipmentsプロパティは削除されました（EquipmentはAppointmentResourceに統合）
    private SearchFilterPanel.SearchFilter? CurrentFilter => this.State.CurrentFilter;
    private int ActiveFilterCount => this.State.ActiveFilterCount;
    private List<SearchFilterPanel.FilterItem> AvailableFloors => this.State.AvailableFloors;
    private List<SearchFilterPanel.FilterItem> AvailableResourceGroups => this.State.AvailableResourceGroups;
    private List<SearchFilterPanel.FilterItem> AvailableResources => this.State.AvailableResources;
    private List<SearchFilterPanel.FilterItem> AvailablePlans => this.State.AvailablePlans;
    private List<SearchFilterPanel.FilterItem> AvailableOptions => this.State.AvailableOptions;

    protected override async Task OnInitializedAsync()
    {
        var sw = Stopwatch.StartNew();
        Console.WriteLine($"[Performance] Calendar.OnInitializedAsync start: ViewType={this.State.CurrentView}");
        this.State.IsLoading = true;
        this.StateHasChanged();
        try
        {
            // ユーザー情報をロード（ログインスキップ時：前回のセッションが維持されている場合でも再初期化する）
            var authState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                // IUserContextServiceを初期化（フォールバック処理込み：Cookieにfacility_idがない場合はDBから取得）
                await this.UserContextService.InitializeFromClaimsAsync(user);
                var currentUser = this.UserContextService.CurrentUser;
                if (currentUser != null)
                {
                    this.State.UserDisplayName = currentUser.UserDisplayName;
                    this.State.UserEmail = currentUser.Email;
                    this.State.TenantName = currentUser.TenantName;
                    this.State.FacilityName = currentUser.FacilityName;
                    this.State.CurrentFacilityId = currentUser.FacilityId;
                    this.State.UserRole = currentUser.Roles.FirstOrDefault() ?? "";
                    this.State.UserInitial = currentUser.Initial;
                    this.State.AvailableFacilities = await this.UserContextService.GetAccessibleFacilitiesAsync();
                    this.State.HasMultipleFacilities = this.State.AvailableFacilities.Count > 1;
                }
                else
                {
                    Console.WriteLine("[WARNING] Calendar.OnInitializedAsync: CurrentUser is null after InitializeFromClaimsAsync");
                }
            }
            else
            {
                Console.WriteLine("[WARNING] Calendar.OnInitializedAsync: User is not authenticated");
            }

            await this.DataService.LoadBusinessHoursAsync(this.State);
            await this.DataService.LoadMainStatsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            await this.DataService.LoadAppointmentsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            await this.DataService.LoadFilterOptionsAsync(this.State);
            await this.DataService.LoadHolidaysAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
        }
        finally
        {
            this.State.IsLoading = false;
            sw.Stop();
            Console.WriteLine($"[Performance] Calendar.OnInitializedAsync total: {sw.ElapsedMilliseconds}ms");
            this.StateHasChanged();
        }
    }

    private async Task HandleLogout()
    {
        try
        {
            var authState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync();
            var sessionIdClaim = authState.User.FindFirst("session_id")?.Value;
            if (!String.IsNullOrEmpty(sessionIdClaim) && Guid.TryParse(sessionIdClaim, out var sessionId))
            {
                await this.AuthService.LogoutAsync(sessionId);
            }
        }
        catch
        {
            // ログアウトAPIの失敗は無視
        }
        finally
        {
            this.NavigationManager.NavigateTo("/api/auth/logout", forceLoad: true);
        }
    }

    private async Task HandleFacilitySwitch(Guid facilityId)
    {
        var currentUser = this.UserContextService.CurrentUser;
        if (currentUser == null || currentUser.UserId == Guid.Empty)
        {
            return;
        }

        try
        {
            var result = await this.AuthService.SwitchFacilityAsync(currentUser.UserId, facilityId);
            if (result.IsSuccess)
            {
                this.NavigationManager.NavigateTo("/calendar", forceLoad: true);
            }
        }
        catch
        {
            // 施設切替失敗時は何もしない
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
        var sw = Stopwatch.StartNew();
        Console.WriteLine($"[Performance] Calendar.SetView start: ViewType={view}");
        this.State.SetView(view);
        this.State.IsLoading = true;
        this.StateHasChanged();
        try
        {
            await this.DataService.LoadMainStatsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            await this.DataService.LoadAppointmentsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            await this.DataService.LoadHolidaysAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            this.Layout?.UpdateCurrentView(view.ToString().ToLower());
            this.RegisterLayoutActions();
        }
        finally
        {
            this.State.IsLoading = false;
            sw.Stop();
            Console.WriteLine($"[Performance] Calendar.SetView total: {sw.ElapsedMilliseconds}ms");
            this.StateHasChanged();
        }
    }

    private string GetCurrentPeriodTitle() => this.State.GetCurrentPeriodTitle();

    private async Task PreviousPeriod()
    {
        this.State.PreviousPeriod();
        this.State.IsLoading = true;
        this.StateHasChanged();
        try
        {
            await this.DataService.LoadMainStatsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            await this.DataService.LoadAppointmentsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            await this.DataService.LoadHolidaysAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
        }
        finally
        {
            this.State.IsLoading = false;
            this.StateHasChanged();
        }
    }

    private async Task NextPeriod()
    {
        this.State.NextPeriod();
        this.State.IsLoading = true;
        this.StateHasChanged();
        try
        {
            await this.DataService.LoadMainStatsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            await this.DataService.LoadAppointmentsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            await this.DataService.LoadHolidaysAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
        }
        finally
        {
            this.State.IsLoading = false;
            this.StateHasChanged();
        }
    }

    private async Task PreviousBigPeriod()
    {
        this.State.PreviousBigPeriod();
        this.State.IsLoading = true;
        this.StateHasChanged();
        try
        {
            await this.DataService.LoadMainStatsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            await this.DataService.LoadAppointmentsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            await this.DataService.LoadHolidaysAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
        }
        finally
        {
            this.State.IsLoading = false;
            this.StateHasChanged();
        }
    }

    private async Task NextBigPeriod()
    {
        this.State.NextBigPeriod();
        this.State.IsLoading = true;
        this.StateHasChanged();
        try
        {
            await this.DataService.LoadMainStatsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            await this.DataService.LoadAppointmentsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            await this.DataService.LoadHolidaysAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
        }
        finally
        {
            this.State.IsLoading = false;
            this.StateHasChanged();
        }
    }

    private async Task GoToToday()
    {
        this.State.GoToToday();
        this.State.IsLoading = true;
        this.StateHasChanged();
        try
        {
            await this.DataService.LoadMainStatsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            await this.DataService.LoadAppointmentsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
        }
        finally
        {
            this.State.IsLoading = false;
            this.StateHasChanged();
        }
    }

    private async Task HandleDateClick(DateOnly date)
    {
        this.State.CurrentDate = date;
        if (this.State.CurrentView == CalendarViewType.Month)
        {
            this.State.CurrentView = CalendarViewType.Week;
            this.State.WeekDays = 7;
            await this.DataService.LoadMainStatsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            await this.DataService.LoadAppointmentsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            this.Layout?.UpdateCurrentView("week");
            this.RegisterLayoutActions();
        }
        else
        {
            await this.DataService.LoadMainStatsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            await this.DataService.LoadAppointmentsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
        }
        this.StateHasChanged();
    }

    private async Task HandleMonthClick((int Year, int Month) yearMonth)
    {
        this.State.CurrentDate = new DateOnly(yearMonth.Year, yearMonth.Month, 1);
        this.State.CurrentView = CalendarViewType.Month;
        await this.DataService.LoadMainStatsAsync(
            this.State,
            this.State.CurrentView,
            this.State.CurrentDate,
            this.State.WeekDays);
        await this.DataService.LoadAppointmentsAsync(
            this.State,
            this.State.CurrentView,
            this.State.CurrentDate,
            this.State.WeekDays);
        this.Layout?.UpdateCurrentView("month");
        this.RegisterLayoutActions();
        this.StateHasChanged();
    }

    private async Task HandleMonthSelected((int Year, int Month) yearMonth)
    {
        this.State.CurrentDate = new DateOnly(yearMonth.Year, yearMonth.Month, 1);
        await this.DataService.LoadMainStatsAsync(
            this.State,
            this.State.CurrentView,
            this.State.CurrentDate,
            this.State.WeekDays);
        await this.DataService.LoadAppointmentsAsync(
            this.State,
            this.State.CurrentView,
            this.State.CurrentDate,
            this.State.WeekDays);
        this.StateHasChanged();
    }

    private async Task HandleYearSelected(int year)
    {
        this.State.CurrentDate = new DateOnly(year, this.State.CurrentDate.Month, this.State.CurrentDate.Day);
        this.State.IsLoading = true;
        this.StateHasChanged();
        try
        {
            await this.DataService.LoadMainStatsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            await this.DataService.LoadAppointmentsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            await this.DataService.LoadHolidaysAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
        }
        finally
        {
            this.State.IsLoading = false;
            this.StateHasChanged();
        }
    }

    private void HandleSimpleViewChanged(bool showSimpleView)
    {
        this.State.ShowSimpleView = showSimpleView;
        this.StateHasChanged();
    }

    // HandleEquipmentGraphChangedメソッドは削除されました（EquipmentはAppointmentResourceに統合）

    private void HandleDateSelect(DateOnly date)
    {
        this.State.SelectedDate = date;
        this.State.SelectedDateRange = null;
    }

    private async Task HandleDateDoubleClick(DateOnly date)
    {
        this.State.CurrentDate = date;
        this.State.CurrentView = CalendarViewType.Week;
        this.State.WeekDays = 1;
        await this.DataService.LoadMainStatsAsync(
            this.State,
            this.State.CurrentView,
            this.State.CurrentDate,
            this.State.WeekDays);
        await this.DataService.LoadAppointmentsAsync(
            this.State,
            this.State.CurrentView,
            this.State.CurrentDate,
            this.State.WeekDays);
        this.Layout?.UpdateCurrentView("week");
        this.RegisterLayoutActions();
        this.StateHasChanged();
    }

    private void HandleDateRangeSelect((DateOnly Start, DateOnly End) range)
    {
        this.State.SelectedDate = null;
        this.State.SelectedDateRange = range;
    }

    private async Task HandleFilterApplied(SearchFilterPanel.SearchFilter filter)
    {
        this.State.CurrentFilter = filter;
        await this.FilterService.ApplyFilterAsync(
            filter,
            this.State.MainStats,
            this.State.OriginalMainStats,
            this.State.MainStatsGrayedOut,
            this.State.CurrentView,
            this.State.CurrentDate);
        this.StateHasChanged();
    }

    private async Task HandleFilterChangedRealtime(SearchFilterPanel.SearchFilter filter)
    {
        this.State.CurrentFilter = filter;

        // フロア、リソースグループ、リソース、プラン・オプションのフィルターが変更された場合はデータを再取得
        var needsReload = filter.SelectedFloorIds.Any() ||
                         filter.SelectedResourceGroupIds.Any() ||
                         filter.SelectedResourceIds.Any() ||
                         filter.SelectedPlanIds.Any() ||
                         filter.SelectedOptionPlanIds.Any();

        if (needsReload)
        {
            this.State.IsLoading = true;
            this.StateHasChanged();
            try
            {
                await this.DataService.LoadMainStatsAsync(
                    this.State,
                    this.State.CurrentView,
                    this.State.CurrentDate,
                    this.State.WeekDays);
                await this.DataService.LoadAppointmentsAsync(
                    this.State,
                    this.State.CurrentView,
                    this.State.CurrentDate,
                    this.State.WeekDays);
            }
            finally
            {
                this.State.IsLoading = false;
            }
        }

        await this.FilterService.ApplyFilterAsync(
            filter,
            this.State.MainStats,
            this.State.OriginalMainStats,
            this.State.MainStatsGrayedOut,
            this.State.CurrentView,
            this.State.CurrentDate);
        this.StateHasChanged();
    }

    private void HandleAppointmentClick(Guid apptId)
    {
        this.State.OpenModal(this.State.CurrentDate, new TimeOnly(9, 0), apptId);
    }

    private void HandleCreateRequest((DateOnly Date, TimeOnly Time) request)
    {
        this.State.OpenModal(request.Date, request.Time);
    }

    private async Task HandleWeekDaysChanged(int days)
    {
        this.State.WeekDays = days;
        await this.DataService.LoadMainStatsAsync(
            this.State,
            this.State.CurrentView,
            this.State.CurrentDate,
            this.State.WeekDays);
        await this.DataService.LoadAppointmentsAsync(
            this.State,
            this.State.CurrentView,
            this.State.CurrentDate,
            this.State.WeekDays);
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
        this.State.OpenModal(this.State.CurrentDate, new TimeOnly(9, 0));
    }

    private void CloseModal()
    {
        this.State.CloseModal();
    }

    private async Task HandleSaveAppointment()
    {
        this.CloseModal();
        // 予約保存後にデータを再取得
        await this.DataService.LoadMainStatsAsync(
            this.State,
            this.State.CurrentView,
            this.State.CurrentDate,
            this.State.WeekDays);
        await this.DataService.LoadAppointmentsAsync(
            this.State,
            this.State.CurrentView,
            this.State.CurrentDate,
            this.State.WeekDays);
        this.StateHasChanged();
    }

    private async Task HandleAppointmentMoved((Guid ApptId, DateOnly NewDate, TimeOnly NewTime) moveInfo)
    {
        try
        {
            var appt = this.State.Appointments.FirstOrDefault(a => a.Id == moveInfo.ApptId);
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

                // 予約更新後にデータを再取得
                await this.DataService.LoadMainStatsAsync(
                    this.State,
                    this.State.CurrentView,
                    this.State.CurrentDate,
                    this.State.WeekDays);
                await this.DataService.LoadAppointmentsAsync(
                    this.State,
                    this.State.CurrentView,
                    this.State.CurrentDate,
                    this.State.WeekDays);
                this.StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"予約移動エラー: {ex.Message}");
        }
    }

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
        this.State.WeekDays = 1;
        await this.DataService.LoadMainStatsAsync(
            this.State,
            this.State.CurrentView,
            this.State.CurrentDate,
            this.State.WeekDays);
        await this.DataService.LoadAppointmentsAsync(
            this.State,
            this.State.CurrentView,
            this.State.CurrentDate,
            this.State.WeekDays);
        this.Layout?.UpdateCurrentView("week");
        this.RegisterLayoutActions();
        this.StateHasChanged();
    }
}
