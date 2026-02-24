using Aloe.Apps.MedockServer.Components.Calendar;
using Aloe.Apps.MedockServer.Components.Pages;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.Dtos.Appointments;
using Microsoft.AspNetCore.Components.Authorization;
using System.Diagnostics;
using System.Security.Claims;

namespace Aloe.Apps.MedockServer.ApplicationServices.Calendar;

/// <summary>
/// カレンダーページの全ビジネスロジックを統合するオーケストレーター実装
/// 既存の Coordinator を組み合わせて、Calendar.razor.cs に対して統一されたインターフェースを提供
/// </summary>
public class CalendarOrchestrator : ICalendarOrchestrator
{
    private readonly CalendarState _state;
    private readonly IUserContextService _userContext;
    private readonly IAuthService _authService;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly CalendarApplicationService _dataService;
    private readonly CalendarFilterService _filterService;
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<CalendarOrchestrator> _logger;

    public CalendarOrchestrator(
        CalendarState state,
        IUserContextService userContext,
        IAuthService authService,
        AuthenticationStateProvider authStateProvider,
        IDateTimeProvider dateTimeProvider,
        CalendarApplicationService dataService,
        CalendarFilterService filterService,
        IAppointmentService appointmentService,
        ILogger<CalendarOrchestrator> logger)
    {
        this._state = state;
        this._userContext = userContext;
        this._authService = authService;
        this._authStateProvider = authStateProvider;
        this._dateTimeProvider = dateTimeProvider;
        this._dataService = dataService;
        this._filterService = filterService;
        this._appointmentService = appointmentService;
        this._logger = logger;
    }

    // ===================================================================
    // Initialization
    // ===================================================================

    public async Task<CalendarInitializationResult> InitializeAsync(ClaimsPrincipal user)
    {
        var sw = Stopwatch.StartNew();
        this._logger.LogInformation("CalendarOrchestrator.InitializeAsync start");

        // CurrentDateが初期値の場合は今日の日付を設定
        if (this._state.CurrentDate == default)
        {
            this._state.CurrentDate = this._dateTimeProvider.TodayDateOnly;
        }

        this._state.IsLoading = true;

        try
        {
            // ユーザー情報をロード
            if (user.Identity?.IsAuthenticated == true)
            {
                var isValid = await this._userContext.InitializeFromClaimsAsync(user);
                if (!isValid)
                {
                    this._logger.LogWarning("CalendarOrchestrator: Facility validation failed");
                    return new CalendarInitializationResult(
                        Success: false,
                        RedirectUrl: "/api/auth/logout",
                        ErrorMessage: "Facility validation failed");
                }

                var currentUser = this._userContext.CurrentUser;
                if (currentUser != null)
                {
                    this._state.UserDisplayName = currentUser.UserDisplayName;
                    this._state.UserEmail = currentUser.Email;
                    this._state.TenantName = currentUser.TenantName;
                    this._state.FacilityName = currentUser.FacilityName;
                    this._state.CurrentFacilityId = currentUser.FacilityId;
                    this._state.UserRole = currentUser.Roles.FirstOrDefault() ?? "";
                    this._state.UserInitial = currentUser.Initial;
                    this._state.AvailableFacilities = await this._userContext.GetAccessibleFacilitiesAsync();
                    this._state.HasMultipleFacilities = this._state.AvailableFacilities.Count > 1;
                }
                else
                {
                    this._logger.LogWarning("CalendarOrchestrator: CurrentUser is null after InitializeFromClaimsAsync");
                }
            }
            else
            {
                this._logger.LogWarning("CalendarOrchestrator: User is not authenticated");
            }

            // データロード
            await this.LoadAllDataAsync();

            sw.Stop();
            this._logger.LogInformation("CalendarOrchestrator.InitializeAsync completed: {ElapsedMs}ms", sw.ElapsedMilliseconds);

            return new CalendarInitializationResult(Success: true, RedirectUrl: null);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "CalendarOrchestrator.InitializeAsync failed");
            return new CalendarInitializationResult(
                Success: false,
                RedirectUrl: null,
                ErrorMessage: ex.Message);
        }
        finally
        {
            this._state.IsLoading = false;
        }
    }

    // ===================================================================
    // Data Operations
    // ===================================================================

    public async Task RefreshCalendarDataAsync()
    {
        this._logger.LogDebug("CalendarOrchestrator.RefreshCalendarDataAsync: Starting");

        await this._dataService.LoadAllCalendarDataAsync(
            this._state,
            this._state.CurrentView,
            this._state.CurrentDate,
            this._state.WeekDays);

        await this.ReapplyCurrentFilterAsync();

        this._logger.LogDebug("CalendarOrchestrator.RefreshCalendarDataAsync: Completed");
    }

    // ===================================================================
    // Navigation
    // ===================================================================

    public async Task NavigatePreviousPeriodAsync()
    {
        this._state.PreviousPeriod();
        await this.RefreshCalendarDataAsync();
    }

    public async Task NavigateNextPeriodAsync()
    {
        this._state.NextPeriod();
        await this.RefreshCalendarDataAsync();
    }

    public async Task NavigatePreviousBigPeriodAsync()
    {
        this._state.PreviousBigPeriod();
        await this.RefreshCalendarDataAsync();
    }

    public async Task NavigateNextBigPeriodAsync()
    {
        this._state.NextBigPeriod();
        await this.RefreshCalendarDataAsync();
    }

    public async Task GoToTodayAsync()
    {
        this._state.GoToToday(this._dateTimeProvider.TodayDateOnly);
        await this.RefreshCalendarDataAsync();
    }

    public async Task SetViewAsync(CalendarViewType view)
    {
        var sw = Stopwatch.StartNew();
        this._logger.LogInformation("CalendarOrchestrator.SetView: {CurrentView} → {TargetView}",
            this._state.CurrentView, view);

        // ビュー切替前にデータをプリロード
        await this._dataService.LoadAllCalendarDataAsync(this._state, view, this._state.CurrentDate, this._state.WeekDays);

        // フィルター再適用
        await this.ReapplyCurrentFilterAsync();

        // ビュー状態更新
        this._state.SetView(view);

        sw.Stop();
        this._logger.LogInformation("CalendarOrchestrator.SetView completed: {ElapsedMs}ms",
            sw.ElapsedMilliseconds);
    }

    public async Task HandleDateClickAsync(DateOnly date)
    {
        this._state.CurrentDate = date;
        if (this._state.CurrentView == CalendarViewType.Month)
        {
            this._state.CurrentView = CalendarViewType.Week;
            this._state.WeekDays = 7;
        }
        await this.RefreshCalendarDataAsync();
    }

    public async Task HandleDateSelectedSingleAsync(DateOnly date)
    {
        this._state.CurrentDate = date;
        await this.RefreshCalendarDataAsync();
    }

    public async Task HandleMonthClickAsync(int year, int month)
    {
        this._state.CurrentDate = new DateOnly(year, month, 1);
        this._state.CurrentView = CalendarViewType.Month;
        await this.RefreshCalendarDataAsync();
    }

    public async Task HandleMonthSelectedAsync(int year, int month)
    {
        this._state.CurrentDate = new DateOnly(year, month, 1);
        await this.RefreshCalendarDataAsync();
    }

    public async Task HandleYearSelectedAsync(int year)
    {
        this._state.CurrentDate = new DateOnly(year, this._state.CurrentDate.Month, this._state.CurrentDate.Day);
        await this.RefreshCalendarDataAsync();
    }

    public async Task HandleDateDoubleClickAsync(DateOnly date)
    {
        this._logger.LogInformation("[TRACE] HandleDateDoubleClick: Date={Date}", date);
        this._state.CurrentDate = date;
        this._state.CurrentView = CalendarViewType.Week;
        this._state.WeekDays = 1;
        await this.RefreshCalendarDataAsync();
    }

    public async Task HandleWeekDaysChangedAsync(int days)
    {
        this._state.WeekDays = days;
        await this.RefreshCalendarDataAsync();
    }

    // ===================================================================
    // Filter Operations
    // ===================================================================

    public async Task ApplyFilterAsync(SearchFilterPanel.SearchFilter filter)
    {
        this._state.CurrentFilter = filter;
        await this.ReapplyCurrentFilterAsync();
    }

    public async Task ReapplyCurrentFilterAsync()
    {
        if (this._state.CurrentFilter is null)
        {
            return;
        }

        await this._filterService.ApplyFilterAsync(
            this._state.CurrentFilter,
            this._state.MainStats,
            this._state.MainStatsGrayedOut,
            this._state.CurrentView,
            this._state.CurrentDate,
            this._state.MainStatsSlots);
    }

    public async Task HandleFilterChangedRealtimeAsync(SearchFilterPanel.SearchFilter filter)
    {
        this._state.CurrentFilter = filter;

        // フロア、リソース、プランのフィルターが変更された場合はデータを再取得
        var needsReload = filter.SelectedFloorIds.Any() ||
                         filter.SelectedResourceIds.Any() ||
                         filter.SelectedPlanIds.Any();

        if (needsReload)
        {
            // データロード（この段階ではフィルター非適用）
            await this._dataService.LoadMainStatsAsync(
                this._state,
                this._state.CurrentView,
                this._state.CurrentDate,
                this._state.WeekDays);

            await this._dataService.LoadMainStatsSlotsAsync(
                this._state,
                this._state.CurrentView,
                this._state.CurrentDate,
                this._state.WeekDays);

            await this._dataService.LoadEquipmentStatsAsync(
                this._state,
                this._state.CurrentView,
                this._state.CurrentDate,
                this._state.WeekDays);

            await this.ReapplyCurrentFilterAsync();
        }
        else
        {
            await this.ReapplyCurrentFilterAsync();
        }
    }

    // ===================================================================
    // Modal Operations
    // ===================================================================

    public void OpenAppointmentModal(Guid apptId)
    {
        this._state.OpenModal(this._state.CurrentDate, 540, apptId);
    }

    public void OpenNewAppointmentModal(DateOnly date, int startMin)
    {
        this._state.OpenModal(date, startMin);
    }

    public void OpenCreateRequestModal(DateOnly date, int startMin)
    {
        this._state.OpenModal(date, startMin);
    }

    public void CloseModal()
    {
        this._state.CloseModal();
    }

    public async Task SaveAppointmentAsync()
    {
        this._state.CloseModal();
        // 予約保存後にデータを再取得（Statsも再計算されているので再取得）
        await this._dataService.LoadAppointmentsAsync(
            this._state,
            this._state.CurrentView,
            this._state.CurrentDate,
            this._state.WeekDays);

        // Statsを再取得
        await this._dataService.LoadMainStatsAsync(
            this._state,
            this._state.CurrentView,
            this._state.CurrentDate,
            this._state.WeekDays);

        await this._dataService.LoadMainStatsSlotsAsync(
            this._state,
            this._state.CurrentView,
            this._state.CurrentDate,
            this._state.WeekDays);

        await this._dataService.LoadEquipmentStatsAsync(
            this._state,
            this._state.CurrentView,
            this._state.CurrentDate,
            this._state.WeekDays);
    }

    public async Task DeleteAppointmentAsync()
    {
        this._state.CloseModal();
        // 予約削除後にデータを再取得（Statsも再計算されているので再取得）
        await this._dataService.LoadAppointmentsAsync(
            this._state,
            this._state.CurrentView,
            this._state.CurrentDate,
            this._state.WeekDays);

        // Statsを再取得
        await this._dataService.LoadMainStatsAsync(
            this._state,
            this._state.CurrentView,
            this._state.CurrentDate,
            this._state.WeekDays);

        await this._dataService.LoadMainStatsSlotsAsync(
            this._state,
            this._state.CurrentView,
            this._state.CurrentDate,
            this._state.WeekDays);

        await this._dataService.LoadEquipmentStatsAsync(
            this._state,
            this._state.CurrentView,
            this._state.CurrentDate,
            this._state.WeekDays);
    }

    public async Task<AppointmentMoveResult> MoveAppointmentAsync(Guid apptId, DateOnly newDate, int newStartMin)
    {
        try
        {
            var appt = this._state.Appointments.FirstOrDefault(a => a.Id == apptId);
            if (appt == null)
            {
                return new AppointmentMoveResult(false, "予約が見つかりません");
            }

            var result = await this._appointmentService.UpdateAppointmentAsync(
                apptId,
                new UpdateAppointmentDto
                {
                    Date = newDate,
                    StartMin = newStartMin,
                });

            if (!result.IsSuccess)
            {
                return new AppointmentMoveResult(false, $"予約の移動に失敗しました: {result.ErrorMessage}");
            }

            // 移動成功後にデータを再取得（Statsも再計算されているので再取得）
            await this._dataService.LoadAppointmentsAsync(
                this._state,
                this._state.CurrentView,
                this._state.CurrentDate,
                this._state.WeekDays);

            // Statsを再取得
            await this._dataService.LoadMainStatsAsync(
                this._state,
                this._state.CurrentView,
                this._state.CurrentDate,
                this._state.WeekDays);

            await this._dataService.LoadMainStatsSlotsAsync(
                this._state,
                this._state.CurrentView,
                this._state.CurrentDate,
                this._state.WeekDays);

            await this._dataService.LoadEquipmentStatsAsync(
                this._state,
                this._state.CurrentView,
                this._state.CurrentDate,
                this._state.WeekDays);

            return new AppointmentMoveResult(true, null);
        }
        catch (Exception ex)
        {
            return new AppointmentMoveResult(false, $"予約の移動処理中にエラーが発生しました: {ex.Message}");
        }
    }

    // ===================================================================
    // Auth Operations
    // ===================================================================

    public async Task<AuthOperationResult> LogoutAsync()
    {
        try
        {
            var authState = await this._authStateProvider.GetAuthenticationStateAsync();
            var sessionIdClaim = authState.User.FindFirst("session_id")?.Value;
            if (!String.IsNullOrEmpty(sessionIdClaim) && Guid.TryParse(sessionIdClaim, out var sessionId))
            {
                await this._authService.LogoutAsync(sessionId);
            }
        }
        catch
        {
            // ログアウトAPIの失敗は無視
        }

        return new AuthOperationResult(Success: true, RedirectUrl: "/api/auth/logout");
    }

    public async Task<AuthOperationResult> SwitchFacilityAsync(Guid facilityId)
    {
        var currentUser = this._userContext.CurrentUser;
        if (currentUser == null || currentUser.UserId == Guid.Empty)
        {
            return new AuthOperationResult(Success: false, RedirectUrl: null);
        }

        try
        {
            var result = await this._authService.SwitchFacilityAsync(currentUser.UserId, facilityId);
            if (result.IsSuccess)
            {
                return new AuthOperationResult(Success: true, RedirectUrl: "/calendar");
            }
            return new AuthOperationResult(Success: false, RedirectUrl: null);
        }
        catch
        {
            return new AuthOperationResult(Success: false, RedirectUrl: null);
        }
    }

    // ===================================================================
    // Private Helpers
    // ===================================================================

    private async Task LoadAllDataAsync()
    {
        var sw = Stopwatch.StartNew();
        this._logger.LogInformation("CalendarOrchestrator.LoadAllDataAsync: Starting initial data load");

        // 全データ一括読み込み（新メソッド使用）
        // ビジネスアワーとフィルターオプションは初期化時のみ読み込み
        await this._dataService.LoadAllCalendarDataAsync(
            this._state,
            this._state.CurrentView,
            this._state.CurrentDate,
            this._state.WeekDays,
            includeBusinessHours: true,
            includeFilterOptions: true);

        sw.Stop();
        this._logger.LogInformation("CalendarOrchestrator.LoadAllDataAsync: Completed in {ElapsedMs}ms", sw.ElapsedMilliseconds);
    }
}
