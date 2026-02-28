using Aloe.Apps.MedockServer.Components.Calendar;
using Aloe.Apps.MedockServer.Components.Pages;
using System.Security.Claims;

namespace Aloe.Apps.MedockServer.ApplicationServices.Calendar;

/// <summary>
/// カレンダーページの全ビジネスロジックを統合するオーケストレーター
/// Calendar.razor.cs の依存関係を削減するための Facade パターン
/// </summary>
public interface ICalendarOrchestrator
{
    // ===================================================================
    // Initialization
    // ===================================================================

    /// <summary>
    /// カレンダー初期化処理（ユーザーコンテキスト、データロード）
    /// </summary>
    /// <param name="user">認証済みユーザー Claims</param>
    /// <returns>初期化結果（成功/失敗、リダイレクトURL）</returns>
    Task<CalendarInitializationResult> InitializeAsync(ClaimsPrincipal user);

    // ===================================================================
    // Data Operations
    // ===================================================================

    /// <summary>
    /// カレンダーデータを再ロード（統計、予約、休日など全データ）
    /// </summary>
    Task RefreshCalendarDataAsync();

    // ===================================================================
    // Navigation
    // ===================================================================

    /// <summary>前の期間へ移動</summary>
    Task NavigatePreviousPeriodAsync();

    /// <summary>次の期間へ移動</summary>
    Task NavigateNextPeriodAsync();

    /// <summary>前の大きな期間へ移動（年単位など）</summary>
    Task NavigatePreviousBigPeriodAsync();

    /// <summary>次の大きな期間へ移動（年単位など）</summary>
    Task NavigateNextBigPeriodAsync();

    /// <summary>今日の日付へ移動</summary>
    Task GoToTodayAsync();

    /// <summary>ビュー切り替え（年/月/週）</summary>
    Task SetViewAsync(CalendarViewType view);

    /// <summary>日付クリック時の処理（Weekビューへ切り替え）</summary>
    Task HandleDateClickAsync(DateOnly date);

    /// <summary>日付選択時の処理（ビュー切り替えなし）</summary>
    Task HandleDateSelectedSingleAsync(DateOnly date);

    /// <summary>月クリック時の処理（Monthビューへ切り替え）</summary>
    Task HandleMonthClickAsync(int year, int month);

    /// <summary>月選択時の処理（ビュー切り替えなし）</summary>
    Task HandleMonthSelectedAsync(int year, int month);

    /// <summary>年選択時の処理</summary>
    Task HandleYearSelectedAsync(int year);

    /// <summary>日付ダブルクリック時の処理（1日Weekビューへ切り替え）</summary>
    Task HandleDateDoubleClickAsync(DateOnly date);

    /// <summary>週表示日数変更時の処理</summary>
    Task HandleWeekDaysChangedAsync(int days);

    // ===================================================================
    // Filter Operations
    // ===================================================================

    /// <summary>フィルター適用</summary>
    Task ApplyFilterAsync(SearchFilterPanel.SearchFilter filter);

    /// <summary>現在のフィルターを再適用（月切り替え後など）</summary>
    Task ReapplyCurrentFilterAsync();

    /// <summary>フィルターがリアルタイムで変更された時の処理</summary>
    Task HandleFilterChangedRealtimeAsync(SearchFilterPanel.SearchFilter filter);

    // ===================================================================
    // Modal Operations
    // ===================================================================

    /// <summary>予約クリック時にモーダルを開く</summary>
    void OpenAppointmentModal(Guid apptId);

    /// <summary>新規予約モーダルを開く（日付・時刻指定）</summary>
    void OpenNewAppointmentModal(DateOnly date, int startMin);

    /// <summary>新規予約モーダルを開く（日付・時刻・患者名指定、音声コマンド用）</summary>
    void OpenNewAppointmentModal(DateOnly date, int startMin, string? patientName);

    /// <summary>予約作成リクエスト時にモーダルを開く</summary>
    void OpenCreateRequestModal(DateOnly date, int startMin);

    /// <summary>モーダルを閉じる</summary>
    void CloseModal();

    /// <summary>予約保存後の処理（データ再取得）</summary>
    Task SaveAppointmentAsync();

    /// <summary>予約削除後の処理（データ再取得）</summary>
    Task DeleteAppointmentAsync();

    /// <summary>予約をドラッグ&ドロップで移動した時の処理</summary>
    Task<AppointmentMoveResult> MoveAppointmentAsync(Guid apptId, DateOnly newDate, int newStartMin);

    // ===================================================================
    // Auth Operations
    // ===================================================================

    /// <summary>ログアウト処理</summary>
    /// <returns>操作結果（リダイレクトURL）</returns>
    Task<AuthOperationResult> LogoutAsync();

    /// <summary>施設切り替え処理</summary>
    /// <returns>操作結果（リダイレクトURL）</returns>
    Task<AuthOperationResult> SwitchFacilityAsync(Guid facilityId);
}

/// <summary>カレンダー初期化結果</summary>
public record CalendarInitializationResult(
    bool Success,
    string? RedirectUrl,
    string? ErrorMessage = null);

/// <summary>認証操作結果</summary>
public record AuthOperationResult(
    bool Success,
    string? RedirectUrl);

/// <summary>予約移動結果</summary>
public record AppointmentMoveResult(
    bool Success,
    string? ErrorMessage = null);
