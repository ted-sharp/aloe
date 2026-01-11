using Aloe.Apps.MedockServer.Components.Pages;
using Aloe.Apps.MedockLib.Services.Dtos.Appointments;
using Aloe.Apps.MedockLib.Services;

namespace Aloe.Apps.MedockServer.ApplicationServices.Calendar;

/// <summary>
/// カレンダーのモーダルダイアログ処理を調整するクラス
/// </summary>
public class CalendarModalCoordinator
{
    private readonly CalendarApplicationService _dataService;
    private readonly IAppointmentService _appointmentService;

    public CalendarModalCoordinator(
        CalendarApplicationService dataService,
        IAppointmentService appointmentService)
    {
        this._dataService = dataService;
        this._appointmentService = appointmentService;
    }

    /// <summary>
    /// 予約をクリックしてモーダルを開く
    /// </summary>
    public void HandleAppointmentClick(
        Guid apptId,
        DateOnly currentDate,
        CalendarState state,
        Action<DateOnly, int, Guid> openModal)
    {
        openModal(currentDate, 540, apptId);
    }

    /// <summary>
    /// 予約作成リクエストを処理してモーダルを開く
    /// </summary>
    public void HandleCreateRequest(
        (DateOnly Date, int StartMin) request,
        CalendarState state,
        Action<DateOnly, int> openModalForCreate)
    {
        openModalForCreate(request.Date, request.StartMin);
    }

    /// <summary>
    /// 新規予約モーダルを開く
    /// </summary>
    public void OpenNewAppointmentModal(
        DateOnly currentDate,
        CalendarState state,
        Action<DateOnly, int> openModalForCreate)
    {
        openModalForCreate(currentDate, 540);
    }

    /// <summary>
    /// モーダルを閉じる
    /// </summary>
    public void CloseModal(
        CalendarState state,
        Action closeModalAction)
    {
        closeModalAction();
    }

    /// <summary>
    /// 予約保存後の処理
    /// </summary>
    public async Task HandleSaveAppointmentAsync(
        CalendarState state,
        Action closeModalAction,
        Func<Task> onRefreshData)
    {
        closeModalAction();
        // 予約保存後にデータを再取得
        await this._dataService.LoadAppointmentsAsync(
            state,
            state.CurrentView,
            state.CurrentDate,
            state.WeekDays);
        await onRefreshData();
    }

    /// <summary>
    /// 予約削除後の処理
    /// </summary>
    public async Task HandleDeleteAppointmentAsync(
        CalendarState state,
        Action closeModalAction,
        Func<Task> onRefreshData)
    {
        closeModalAction();
        // 予約削除後にデータを再取得
        await this._dataService.LoadAppointmentsAsync(
            state,
            state.CurrentView,
            state.CurrentDate,
            state.WeekDays);
        await onRefreshData();
    }

    /// <summary>
    /// 予約をドラッグ&ドロップで移動した場合の処理
    /// </summary>
    public async Task<bool> HandleAppointmentMovedAsync(
        (Guid ApptId, DateOnly NewDate, int NewStartMin) moveInfo,
        CalendarState state,
        Func<Task> onRefreshData,
        Func<string, Task> onError)
    {
        try
        {
            var appt = state.Appointments.FirstOrDefault(a => a.Id == moveInfo.ApptId);
            if (appt == null)
            {
                await onError("予約が見つかりません");
                return false;
            }

            var result = await this._appointmentService.UpdateAppointmentAsync(
                moveInfo.ApptId,
                new UpdateAppointmentDto
                {
                    Date = moveInfo.NewDate,
                    StartMin = moveInfo.NewStartMin,
                });

            if (!result.IsSuccess)
            {
                await onError($"予約の移動に失敗しました: {result.ErrorMessage}");
                return false;
            }

            // 移動成功後にデータを再取得
            await this._dataService.LoadAppointmentsAsync(
                state,
                state.CurrentView,
                state.CurrentDate,
                state.WeekDays);
            await onRefreshData();

            return true;
        }
        catch (Exception ex)
        {
            await onError($"予約の移動処理中にエラーが発生しました: {ex.Message}");
            return false;
        }
    }
}
