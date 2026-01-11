using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.Dtos;
using Aloe.Apps.MedockLib.Services.Dtos.Appointments;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockServer.Components.Layout;
using Aloe.Apps.MedockServer.Components.FAB;
using Aloe.Apps.MedockServer.Components.Calendar;
using Aloe.Apps.MedockServer.ApplicationServices.Calendar;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.JSInterop;

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
    private CalendarApplicationService DataService { get; set; } = default!;

    // 状態管理（Scoped Service として DI 注入）
    [Inject]
    private CalendarState State { get; set; } = default!;

    [Inject]
    private ILogger<Calendar> Logger { get; set; } = default!;

    [Inject]
    private IDateTimeProvider DateTimeProvider { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private CalendarNavigationService NavigationService { get; set; } = default!;

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

    // AvailableEquipmentsプロパティは削除されました（EquipmentはAppointmentResourceに統合）
    private SearchFilterPanel.SearchFilter? CurrentFilter => this.State.CurrentFilter;
    private int ActiveFilterCount => this.State.ActiveFilterCount;
    private List<string>? FilterTimeSlots => this.State.CurrentFilter?.TimeSlots;
    private List<SearchFilterPanel.FilterItem> AvailableFloors => this.State.AvailableFloors;
    private List<SearchFilterPanel.FilterItem> AvailableResources => this.State.AvailableResources;
    private List<SearchFilterPanel.FilterItem> AvailablePlans => this.State.AvailablePlans;

    protected override async Task OnInitializedAsync()
    {
        var sw = Stopwatch.StartNew();
        this.Logger.LogInformation("Calendar.OnInitializedAsync start: ViewType={ViewType}", this.State.CurrentView);

        // CurrentDateが初期値の場合は今日の日付を設定
        if (this.State.CurrentDate == default)
        {
            this.State.CurrentDate = this.DateTimeProvider.TodayDateOnly;
        }

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
                var isValid = await this.UserContextService.InitializeFromClaimsAsync(user);
                if (!isValid)
                {
                    // 施設が存在しない、またはアクセス権がない場合はログインページにリダイレクト
                    this.Logger.LogWarning("Calendar.OnInitializedAsync: Facility validation failed, redirecting to login");
                    this.NavigationManager.NavigateTo("/api/auth/logout", forceLoad: true);
                    return;
                }

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
                    this.Logger.LogWarning("Calendar.OnInitializedAsync: CurrentUser is null after InitializeFromClaimsAsync");
                }
            }
            else
            {
                this.Logger.LogWarning("Calendar.OnInitializedAsync: User is not authenticated");
            }

            var businessHoursSw = Stopwatch.StartNew();
            await this.DataService.LoadBusinessHoursAsync(this.State);
            businessHoursSw.Stop();
            this.Logger.LogInformation("[TRACE] OnInitializedAsync - LoadBusinessHours: {ElapsedMs}ms", businessHoursSw.ElapsedMilliseconds);

            var mainStatsSw = Stopwatch.StartNew();
            await this.DataService.LoadMainStatsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            mainStatsSw.Stop();
            this.Logger.LogInformation("[TRACE] OnInitializedAsync - LoadMainStats: {ElapsedMs}ms", mainStatsSw.ElapsedMilliseconds);

            // Load slot-level statistics for calendar display
            var mainStatSlotsSw = Stopwatch.StartNew();
            await this.DataService.LoadMainStatsSlotsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            mainStatSlotsSw.Stop();
            this.Logger.LogInformation("[TRACE] OnInitializedAsync - LoadMainStatsSlots: {ElapsedMs}ms", mainStatSlotsSw.ElapsedMilliseconds);

            var equipmentStatsSw = Stopwatch.StartNew();
            await this.DataService.LoadEquipmentStatsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            equipmentStatsSw.Stop();
            this.Logger.LogInformation("[TRACE] OnInitializedAsync - LoadEquipmentStats: {ElapsedMs}ms", equipmentStatsSw.ElapsedMilliseconds);

            var appointmentsSw = Stopwatch.StartNew();
            await this.DataService.LoadAppointmentsAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            appointmentsSw.Stop();
            this.Logger.LogInformation("[TRACE] OnInitializedAsync - LoadAppointments: {ElapsedMs}ms", appointmentsSw.ElapsedMilliseconds);

            var filterOptionsSw = Stopwatch.StartNew();
            await this.DataService.LoadFilterOptionsAsync(this.State);
            filterOptionsSw.Stop();
            this.Logger.LogInformation("[TRACE] OnInitializedAsync - LoadFilterOptions: {ElapsedMs}ms", filterOptionsSw.ElapsedMilliseconds);

            var holidaysSw = Stopwatch.StartNew();
            await this.DataService.LoadHolidaysAsync(
                this.State,
                this.State.CurrentView,
                this.State.CurrentDate,
                this.State.WeekDays);
            holidaysSw.Stop();
            this.Logger.LogInformation("[TRACE] OnInitializedAsync - LoadHolidays: {ElapsedMs}ms", holidaysSw.ElapsedMilliseconds);
        }
        finally
        {
            this.State.IsLoading = false;
            sw.Stop();
            this.Logger.LogInformation("Calendar.OnInitializedAsync total: {ElapsedMs}ms", sw.ElapsedMilliseconds);
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
        this.Logger.LogInformation("Calendar.SetView start: CurrentView={CurrentView}, TargetView={TargetView}", this.State.CurrentView, view);

        // 先にターゲットビュー用のデータをロード（CurrentView はまだ変更しない）
        await this.DataService.LoadMainStatsAsync(this.State, view, this.State.CurrentDate, this.State.WeekDays);
        await this.DataService.LoadMainStatsSlotsAsync(this.State, view, this.State.CurrentDate, this.State.WeekDays);
        await this.DataService.LoadEquipmentStatsAsync(this.State, view, this.State.CurrentDate, this.State.WeekDays);
        await this.DataService.LoadAppointmentsAsync(this.State, view, this.State.CurrentDate, this.State.WeekDays);
        await this.DataService.LoadHolidaysAsync(this.State, view, this.State.CurrentDate, this.State.WeekDays);
        await this.ReapplyCurrentFilterAsync();

        // データロード完了後にビューを切り替えて描画
        this.State.SetView(view);
        this.Layout?.UpdateCurrentView(view.ToString().ToLower());
        this.RegisterLayoutActions();
        this.StateHasChanged();

        sw.Stop();
        this.Logger.LogInformation("Calendar.SetView total: {ElapsedMs}ms", sw.ElapsedMilliseconds);
    }

    private string GetCurrentPeriodTitle() => this.State.GetCurrentPeriodTitle();

    /// <summary>
    /// カレンダーデータを再ロードする共通メソッド
    /// </summary>
    private async Task RefreshCalendarDataAsync()
    {
        await this.DataService.LoadMainStatsAsync(
            this.State,
            this.State.CurrentView,
            this.State.CurrentDate,
            this.State.WeekDays);
        await this.DataService.LoadMainStatsSlotsAsync(
            this.State,
            this.State.CurrentView,
            this.State.CurrentDate,
            this.State.WeekDays);
        await this.DataService.LoadEquipmentStatsAsync(
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
        await this.ReapplyCurrentFilterAsync();
        // StateHasChanged() は呼び出し元で実行される
    }

    private async Task PreviousPeriod() =>
        await this.NavigationService.NavigateAsync(
            CalendarNavigationService.NavigationDirection.PreviousPeriod,
            this.State,
            this.RefreshCalendarDataAsync,
            this.StateHasChanged);

    private async Task NextPeriod() =>
        await this.NavigationService.NavigateAsync(
            CalendarNavigationService.NavigationDirection.NextPeriod,
            this.State,
            this.RefreshCalendarDataAsync,
            this.StateHasChanged);

    private async Task PreviousBigPeriod() =>
        await this.NavigationService.NavigateAsync(
            CalendarNavigationService.NavigationDirection.PreviousBigPeriod,
            this.State,
            this.RefreshCalendarDataAsync,
            this.StateHasChanged);

    private async Task NextBigPeriod() =>
        await this.NavigationService.NavigateAsync(
            CalendarNavigationService.NavigationDirection.NextBigPeriod,
            this.State,
            this.RefreshCalendarDataAsync,
            this.StateHasChanged);

    private async Task GoToToday()
    {
        this.State.GoToToday(this.DateTimeProvider.TodayDateOnly);
        await this.RefreshCalendarDataAsync();
        this.StateHasChanged();
    }

    private async Task HandleDateClick(DateOnly date)
    {
        this.State.CurrentDate = date;
        if (this.State.CurrentView == CalendarViewType.Month)
        {
            this.State.CurrentView = CalendarViewType.Week;
            this.State.WeekDays = 7;
            this.Layout?.UpdateCurrentView("week");
            this.RegisterLayoutActions();
        }
        await this.RefreshCalendarDataAsync();
        this.StateHasChanged();
    }

    private async Task HandleDateSelectedSingle(DateOnly date)
    {
        // JavaScript側で日付がクリックされた時にCurrentDateを更新
        // ビュー切り替えはせず、選択状態のみ更新
        this.State.CurrentDate = date;
        await this.RefreshCalendarDataAsync();
        this.StateHasChanged();
    }

    private async Task HandleMonthClick((int Year, int Month) yearMonth)
    {
        this.State.CurrentDate = new DateOnly(yearMonth.Year, yearMonth.Month, 1);
        this.State.CurrentView = CalendarViewType.Month;
        this.Layout?.UpdateCurrentView("month");
        this.RegisterLayoutActions();
        await this.RefreshCalendarDataAsync();
        this.StateHasChanged();
    }

    private async Task HandleMonthSelected((int Year, int Month) yearMonth)
    {
        this.State.CurrentDate = new DateOnly(yearMonth.Year, yearMonth.Month, 1);
        await this.RefreshCalendarDataAsync();
        this.StateHasChanged();
    }

    private async Task HandleYearSelected(int year)
    {
        this.State.CurrentDate = new DateOnly(year, this.State.CurrentDate.Month, this.State.CurrentDate.Day);
        await this.RefreshCalendarDataAsync();
        this.StateHasChanged();
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
        this.Logger.LogInformation("[TRACE] HandleDateDoubleClick: 日付={Date} へ Week 切り替え", date);
        this.State.CurrentDate = date;
        this.State.CurrentView = CalendarViewType.Week;
        this.State.WeekDays = 1;
        this.Layout?.UpdateCurrentView("week");
        this.RegisterLayoutActions();
        await this.RefreshCalendarDataAsync();
        this.StateHasChanged();
        this.Logger.LogInformation("[TRACE] HandleDateDoubleClick: Appointments 件数={Count}", this.State.Appointments?.Count ?? 0);
    }

    private void HandleDateRangeSelect((DateOnly Start, DateOnly End) range)
    {
        this.State.SelectedDate = null;
        this.State.SelectedDateRange = range;
    }

    private async Task HandleFilterApplied(SearchFilterPanel.SearchFilter filter)
    {
        this.State.CurrentFilter = filter;
        await this.ReapplyCurrentFilterAsync();
        this.StateHasChanged();
    }

    /// <summary>
    /// 現在のフィルターを再適用する（月切り替えやビュー変更後に呼び出す）
    /// </summary>
    private async Task ReapplyCurrentFilterAsync()
    {
        if (this.State.CurrentFilter is null)
        {
            return;
        }

        await this.FilterService.ApplyFilterAsync(
            this.State.CurrentFilter,
            this.State.MainStats,
            this.State.MainStatsGrayedOut,
            this.State.CurrentView,
            this.State.CurrentDate);
    }

    private async Task HandleFilterChangedRealtime(SearchFilterPanel.SearchFilter filter)
    {
        this.State.CurrentFilter = filter;

        // フロア、リソース、プランのフィルターが変更された場合はデータを再取得
        var needsReload = filter.SelectedFloorIds.Any() ||
                         filter.SelectedResourceIds.Any() ||
                         filter.SelectedPlanIds.Any();

        if (needsReload)
        {
            await this.RefreshCalendarDataAsync();
            this.StateHasChanged();
        }
        else
        {
            await this.ReapplyCurrentFilterAsync();
            this.StateHasChanged();
        }
    }

    private void HandleAppointmentClick(Guid apptId)
    {
        this.State.OpenModal(this.State.CurrentDate, 540, apptId);
        this.StateHasChanged();
    }

    private void HandleCreateRequest((DateOnly Date, int StartMin) request)
    {
        this.State.OpenModal(request.Date, request.StartMin);
        this.StateHasChanged();
    }

    private async Task HandleWeekDaysChanged(int days)
    {
        this.State.WeekDays = days;
        await this.RefreshCalendarDataAsync();
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
        this.State.OpenModal(this.State.CurrentDate, 540);
        this.StateHasChanged();
    }

    private void CloseModal()
    {
        this.State.CloseModal();
        this.StateHasChanged();
    }

    private async Task HandleSaveAppointment()
    {
        this.CloseModal();
        // 予約保存後にデータを再取得
        await this.RefreshCalendarDataAsync();
        this.StateHasChanged();
    }

    private async Task HandleDeleteAppointment()
    {
        this.CloseModal();
        // 予約削除後にデータを再取得
        await this.RefreshCalendarDataAsync();
        this.StateHasChanged();
    }

    private async Task HandleAppointmentMoved((Guid ApptId, DateOnly NewDate, int NewStartMin) moveInfo)
    {
        try
        {
            var appt = this.State.Appointments.FirstOrDefault(a => a.Id == moveInfo.ApptId);
            if (appt != null)
            {
                var result = await this.AppointmentService.UpdateAppointmentAsync(
                    moveInfo.ApptId,
                    new UpdateAppointmentDto
                    {
                        Date = moveInfo.NewDate,
                        StartMin = moveInfo.NewStartMin,
                        // EndTime = newEndTime
                    });

                if (!result.IsSuccess)
                {
                    this.Logger.LogWarning("Failed to move appointment {ApptId}: {ErrorMessage}", moveInfo.ApptId, result.ErrorMessage);
                    await this.JSRuntime.InvokeVoidAsync("alert", $"予約の移動に失敗しました: {result.ErrorMessage}");
                    return;
                }

                // 予約更新後にデータを再取得
                await this.RefreshCalendarDataAsync();
                this.StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error moving appointment");
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
        this.Logger.LogInformation("[TRACE] HandleDayDetailGoToWeekView: 日詳細ポップアップから Week へ切り替え、日付={Date}", date);
        this.CloseDayDetail();
        this.State.CurrentDate = date;
        this.State.CurrentView = CalendarViewType.Week;
        this.Layout?.UpdateCurrentView("week");
        this.RegisterLayoutActions();
        await this.RefreshCalendarDataAsync();
        this.StateHasChanged();
        this.Logger.LogInformation("[TRACE] HandleDayDetailGoToWeekView: LoadMainStatsAsync 完了, Appointments 件数={Count}", this.State.Appointments?.Count ?? 0);
    }

    private async Task HandleDayDetailNavigateToPreviousDay(DateOnly date)
    {
        this.DayDetailDate = date;
        this.State.CurrentDate = date;
        await this.RefreshCalendarDataAsync();
        this.StateHasChanged();
    }

    private async Task HandleDayDetailNavigateToNextDay(DateOnly date)
    {
        this.DayDetailDate = date;
        this.State.CurrentDate = date;
        await this.RefreshCalendarDataAsync();
        this.StateHasChanged();
    }

    /// <summary>
    /// キーボード入力ハンドラー
    /// </summary>
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
                    // その他のすべてのキーを吸収（応答しない）
                    return;
            }
        }

        switch (e.Key)
        {
            case "Enter":
            case " ":
                // CurrentDateの日詳細を開く
                this.HandleShowDayDetail(this.State.CurrentDate);
                break;
            case "ArrowLeft":
                await this.PreviousPeriod();
                break;
            case "ArrowRight":
                await this.NextPeriod();
                break;
            case "ArrowUp":
                // ビュー切り替え: 週 → 月 → 年
                await this.SetPreviousView();
                break;
            case "ArrowDown":
                // ビュー切り替え: 年 → 月 → 週
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

    /// <summary>
    /// ビュー切り替え（逆順: 週 → 月 → 年）
    /// </summary>
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

    /// <summary>
    /// ビュー切り替え（順序: 年 → 月 → 週）
    /// </summary>
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

    /// <summary>
    /// 簡易/詳細表示の切り替え
    /// </summary>
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

    /// <summary>
    /// 日詳細ダイアログでスロットが選択された時の処理
    /// </summary>
    private void HandleDayDetailSlotClicked(DayDetailPopup.SlotClickedEventArgs args)
    {
        this.Logger.LogInformation("日詳細スロット選択: Date={Date}, Time={Start}-{End}",
            args.Date, args.StartMin, args.EndMin);

        // ポップアップを閉じる
        this.CloseDayDetail();

        // 予約モーダルを開く
        this.State.OpenModal(args.Date, args.StartMin);
        this.StateHasChanged();
    }
}
