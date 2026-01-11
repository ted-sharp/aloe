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
    private readonly CalendarFilterCoordinator _filterCoordinator;
    private readonly CalendarModalCoordinator _modalCoordinator;
    private readonly ILogger<CalendarOrchestrator> _logger;

    public CalendarOrchestrator(
        CalendarState state,
        IUserContextService userContext,
        IAuthService authService,
        AuthenticationStateProvider authStateProvider,
        IDateTimeProvider dateTimeProvider,
        CalendarApplicationService dataService,
        CalendarFilterCoordinator filterCoordinator,
        CalendarModalCoordinator modalCoordinator,
        ILogger<CalendarOrchestrator> logger)
    {
        _state = state;
        _userContext = userContext;
        _authService = authService;
        _authStateProvider = authStateProvider;
        _dateTimeProvider = dateTimeProvider;
        _dataService = dataService;
        _filterCoordinator = filterCoordinator;
        _modalCoordinator = modalCoordinator;
        _logger = logger;
    }

    // ===================================================================
    // Initialization
    // ===================================================================

    public async Task<CalendarInitializationResult> InitializeAsync(ClaimsPrincipal user)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("CalendarOrchestrator.InitializeAsync start");

        // CurrentDateが初期値の場合は今日の日付を設定
        if (_state.CurrentDate == default)
        {
            _state.CurrentDate = _dateTimeProvider.TodayDateOnly;
        }

        _state.IsLoading = true;

        try
        {
            // ユーザー情報をロード
            if (user.Identity?.IsAuthenticated == true)
            {
                var isValid = await _userContext.InitializeFromClaimsAsync(user);
                if (!isValid)
                {
                    _logger.LogWarning("CalendarOrchestrator: Facility validation failed");
                    return new CalendarInitializationResult(
                        Success: false,
                        RedirectUrl: "/api/auth/logout",
                        ErrorMessage: "Facility validation failed");
                }

                var currentUser = _userContext.CurrentUser;
                if (currentUser != null)
                {
                    _state.UserDisplayName = currentUser.UserDisplayName;
                    _state.UserEmail = currentUser.Email;
                    _state.TenantName = currentUser.TenantName;
                    _state.FacilityName = currentUser.FacilityName;
                    _state.CurrentFacilityId = currentUser.FacilityId;
                    _state.UserRole = currentUser.Roles.FirstOrDefault() ?? "";
                    _state.UserInitial = currentUser.Initial;
                    _state.AvailableFacilities = await _userContext.GetAccessibleFacilitiesAsync();
                    _state.HasMultipleFacilities = _state.AvailableFacilities.Count > 1;
                }
                else
                {
                    _logger.LogWarning("CalendarOrchestrator: CurrentUser is null after InitializeFromClaimsAsync");
                }
            }
            else
            {
                _logger.LogWarning("CalendarOrchestrator: User is not authenticated");
            }

            // データロード
            await LoadAllDataAsync();

            sw.Stop();
            _logger.LogInformation("CalendarOrchestrator.InitializeAsync completed: {ElapsedMs}ms", sw.ElapsedMilliseconds);

            return new CalendarInitializationResult(Success: true, RedirectUrl: null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CalendarOrchestrator.InitializeAsync failed");
            return new CalendarInitializationResult(
                Success: false,
                RedirectUrl: null,
                ErrorMessage: ex.Message);
        }
        finally
        {
            _state.IsLoading = false;
        }
    }

    // ===================================================================
    // Data Operations
    // ===================================================================

    public async Task RefreshCalendarDataAsync()
    {
        _logger.LogDebug("CalendarOrchestrator.RefreshCalendarDataAsync: Starting");

        await _dataService.LoadAllCalendarDataAsync(
            _state,
            _state.CurrentView,
            _state.CurrentDate,
            _state.WeekDays);

        await _filterCoordinator.ReapplyCurrentFilterAsync(_state);

        _logger.LogDebug("CalendarOrchestrator.RefreshCalendarDataAsync: Completed");
    }

    // ===================================================================
    // Navigation
    // ===================================================================

    public async Task NavigatePreviousPeriodAsync()
    {
        _state.PreviousPeriod();
        await RefreshCalendarDataAsync();
    }

    public async Task NavigateNextPeriodAsync()
    {
        _state.NextPeriod();
        await RefreshCalendarDataAsync();
    }

    public async Task NavigatePreviousBigPeriodAsync()
    {
        _state.PreviousBigPeriod();
        await RefreshCalendarDataAsync();
    }

    public async Task NavigateNextBigPeriodAsync()
    {
        _state.NextBigPeriod();
        await RefreshCalendarDataAsync();
    }

    public async Task GoToTodayAsync()
    {
        _state.GoToToday(_dateTimeProvider.TodayDateOnly);
        await RefreshCalendarDataAsync();
    }

    public async Task SetViewAsync(CalendarViewType view)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("CalendarOrchestrator.SetView: {CurrentView} → {TargetView}",
            _state.CurrentView, view);

        // ビュー切替前にデータをプリロード
        await _dataService.LoadAllCalendarDataAsync(_state, view, _state.CurrentDate, _state.WeekDays);

        // フィルター再適用
        await _filterCoordinator.ReapplyCurrentFilterAsync(_state);

        // ビュー状態更新
        _state.SetView(view);

        sw.Stop();
        _logger.LogInformation("CalendarOrchestrator.SetView completed: {ElapsedMs}ms",
            sw.ElapsedMilliseconds);
    }

    public async Task HandleDateClickAsync(DateOnly date)
    {
        _state.CurrentDate = date;
        if (_state.CurrentView == CalendarViewType.Month)
        {
            _state.CurrentView = CalendarViewType.Week;
            _state.WeekDays = 7;
        }
        await RefreshCalendarDataAsync();
    }

    public async Task HandleDateSelectedSingleAsync(DateOnly date)
    {
        _state.CurrentDate = date;
        await RefreshCalendarDataAsync();
    }

    public async Task HandleMonthClickAsync(int year, int month)
    {
        _state.CurrentDate = new DateOnly(year, month, 1);
        _state.CurrentView = CalendarViewType.Month;
        await RefreshCalendarDataAsync();
    }

    public async Task HandleMonthSelectedAsync(int year, int month)
    {
        _state.CurrentDate = new DateOnly(year, month, 1);
        await RefreshCalendarDataAsync();
    }

    public async Task HandleYearSelectedAsync(int year)
    {
        _state.CurrentDate = new DateOnly(year, _state.CurrentDate.Month, _state.CurrentDate.Day);
        await RefreshCalendarDataAsync();
    }

    public async Task HandleDateDoubleClickAsync(DateOnly date)
    {
        _logger.LogInformation("[TRACE] HandleDateDoubleClick: Date={Date}", date);
        _state.CurrentDate = date;
        _state.CurrentView = CalendarViewType.Week;
        _state.WeekDays = 1;
        await RefreshCalendarDataAsync();
    }

    public async Task HandleWeekDaysChangedAsync(int days)
    {
        _state.WeekDays = days;
        await RefreshCalendarDataAsync();
    }

    // ===================================================================
    // Filter Operations
    // ===================================================================

    public async Task ApplyFilterAsync(SearchFilterPanel.SearchFilter filter)
    {
        await _filterCoordinator.HandleFilterAppliedAsync(filter, _state, () => Task.CompletedTask);
    }

    public async Task ReapplyCurrentFilterAsync()
    {
        await _filterCoordinator.ReapplyCurrentFilterAsync(_state);
    }

    public async Task HandleFilterChangedRealtimeAsync(SearchFilterPanel.SearchFilter filter)
    {
        await _filterCoordinator.HandleFilterChangedRealtimeAsync(
            filter,
            _state,
            () => Task.CompletedTask,
            () => Task.CompletedTask);
    }

    // ===================================================================
    // Modal Operations
    // ===================================================================

    public void OpenAppointmentModal(Guid apptId)
    {
        _modalCoordinator.HandleAppointmentClick(
            apptId,
            _state.CurrentDate,
            _state,
            (date, startMin, id) => _state.OpenModal(date, startMin, id));
    }

    public void OpenNewAppointmentModal(DateOnly date, int startMin)
    {
        _modalCoordinator.OpenNewAppointmentModal(
            date,
            _state,
            (d, min) => _state.OpenModal(d, min));
    }

    public void OpenCreateRequestModal(DateOnly date, int startMin)
    {
        _modalCoordinator.HandleCreateRequest(
            (date, startMin),
            _state,
            (d, min) => _state.OpenModal(d, min));
    }

    public void CloseModal()
    {
        _modalCoordinator.CloseModal(_state, () => _state.CloseModal());
    }

    public async Task SaveAppointmentAsync()
    {
        await _modalCoordinator.HandleSaveAppointmentAsync(
            _state,
            () => _state.CloseModal(),
            () => Task.CompletedTask);
    }

    public async Task DeleteAppointmentAsync()
    {
        await _modalCoordinator.HandleDeleteAppointmentAsync(
            _state,
            () => _state.CloseModal(),
            () => Task.CompletedTask);
    }

    public async Task<AppointmentMoveResult> MoveAppointmentAsync(Guid apptId, DateOnly newDate, int newStartMin)
    {
        string? errorMessage = null;
        var success = await _modalCoordinator.HandleAppointmentMovedAsync(
            (apptId, newDate, newStartMin),
            _state,
            () => Task.CompletedTask,
            async (error) => { errorMessage = error; await Task.CompletedTask; });

        return new AppointmentMoveResult(success, errorMessage);
    }

    // ===================================================================
    // Auth Operations
    // ===================================================================

    public async Task<AuthOperationResult> LogoutAsync()
    {
        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var sessionIdClaim = authState.User.FindFirst("session_id")?.Value;
            if (!string.IsNullOrEmpty(sessionIdClaim) && Guid.TryParse(sessionIdClaim, out var sessionId))
            {
                await _authService.LogoutAsync(sessionId);
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
        var currentUser = _userContext.CurrentUser;
        if (currentUser == null || currentUser.UserId == Guid.Empty)
        {
            return new AuthOperationResult(Success: false, RedirectUrl: null);
        }

        try
        {
            var result = await _authService.SwitchFacilityAsync(currentUser.UserId, facilityId);
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
        _logger.LogInformation("CalendarOrchestrator.LoadAllDataAsync: Starting initial data load");

        // 全データ一括読み込み（新メソッド使用）
        // ビジネスアワーとフィルターオプションは初期化時のみ読み込み
        await _dataService.LoadAllCalendarDataAsync(
            _state,
            _state.CurrentView,
            _state.CurrentDate,
            _state.WeekDays,
            includeBusinessHours: true,
            includeFilterOptions: true);

        sw.Stop();
        _logger.LogInformation("CalendarOrchestrator.LoadAllDataAsync: Completed in {ElapsedMs}ms", sw.ElapsedMilliseconds);
    }
}
