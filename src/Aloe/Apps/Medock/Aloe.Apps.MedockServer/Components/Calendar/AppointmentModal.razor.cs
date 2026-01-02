using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class AppointmentModal : ComponentBase
{
    /// <summary>
    /// 予約サービス
    /// </summary>
    [Inject]
    private IAppointmentService AppointmentService { get; set; } = default!;

    /// <summary>
    /// ロガー
    /// </summary>
    [Inject]
    private ILogger<AppointmentModal> Logger { get; set; } = default!;

    /// <summary>
    /// モーダルの開閉状態
    /// </summary>
    [Parameter]
    public bool IsOpen { get; set; }

    /// <summary>
    /// 編集対象の予約ID（新規作成時はnull）
    /// </summary>
    [Parameter]
    public Guid? AppointmentId { get; set; }

    /// <summary>
    /// 選択された日付（新規作成時）
    /// </summary>
    [Parameter]
    public DateOnly? SelectedDate { get; set; }

    /// <summary>
    /// 選択された時間（新規作成時）
    /// </summary>
    [Parameter]
    public TimeOnly? SelectedTime { get; set; }

    /// <summary>
    /// 閉じる時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    /// <summary>
    /// 保存成功時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback OnSave { get; set; }

    /// <summary>
    /// 削除成功時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback OnDelete { get; set; }

    private AppointmentFormModel FormModel { get; set; } = new();
    private bool IsEditMode => this.AppointmentId.HasValue;
    private bool IsSaving { get; set; }
    private bool IsDeleting { get; set; }
    private bool IsProcessing => this.IsSaving || this.IsDeleting;
    private string? ErrorMessage { get; set; }

    // 時間スロット（30分刻み）
    private static readonly string[] TimeSlots = Enumerable.Range(8, 11)
        .SelectMany(h => new[] { $"{h:D2}:00", $"{h:D2}:30" })
        .ToArray();

    protected override async Task OnParametersSetAsync()
    {
        // パラメータが変わったら常に初期化
        this.Logger.LogInformation("OnParametersSetAsync called: IsOpen={IsOpen}, AppointmentId={AppointmentId}", this.IsOpen, this.AppointmentId);
        await this.InitializeFormAsync();
    }

    private async Task InitializeFormAsync()
    {
        this.ErrorMessage = null;

        if (this.IsEditMode)
        {
            // 編集モード：AppointmentService から最新データを取得
            await this.LoadAppointmentDataAsync();
        }
        else
        {
            // 新規作成モード
            this.FormModel = new AppointmentFormModel
            {
                Date = this.SelectedDate ?? DateOnly.FromDateTime(DateTime.Today),
                StartTimeString = this.SelectedTime?.ToString("HH:mm") ?? BusinessHoursConstants.DefaultAppointmentStartTime,
                EndTimeString = this.SelectedTime?.AddHours(1).ToString("HH:mm") ?? BusinessHoursConstants.DefaultAppointmentEndTime,
                Status = 0,
                PatientName = String.Empty,
                OrganizationName = String.Empty
            };
        }
    }

    /// <summary>
    /// 既存の予約データを読み込む
    /// </summary>
    private async Task LoadAppointmentDataAsync()
    {
        if (!this.AppointmentId.HasValue)
        {
            this.ErrorMessage = "予約IDが指定されていません。";
            return;
        }

        try
        {
            this.Logger.LogInformation("Loading appointment: {AppointmentId}", this.AppointmentId);

            var result = await this.AppointmentService.GetAppointmentAsync(this.AppointmentId.Value);

            if (!result.IsSuccess || result.Value == null)
            {
                this.Logger.LogWarning("Appointment not found: {AppointmentId}", this.AppointmentId);
                this.ErrorMessage = "予約データが見つかりませんでした。";
                return;
            }

            var dto = result.Value;

            // DTO → フォームモデルへマッピング
            this.FormModel = new AppointmentFormModel
            {
                Date = dto.Date,
                StartTimeString = dto.StartTime?.ToString("HH:mm")
                    ?? BusinessHoursConstants.DefaultAppointmentStartTime,
                EndTimeString = dto.EndTime?.ToString("HH:mm")
                    ?? BusinessHoursConstants.DefaultAppointmentEndTime,
                Status = dto.Status,
                PatientName = dto.PatientName ?? String.Empty,
                OrganizationName = dto.OrganizationName ?? String.Empty,
                PatientId = dto.PatientId,
                OrganizationId = dto.OrganizationId,
                FloorId = dto.FloorId,
                Memo = dto.Memo,
                UpdatedAt = dto.UpdatedAt  // 楽観的ロック用に保存
            };

            this.Logger.LogInformation("FormModel loaded: UpdatedAt={UpdatedAt}", this.FormModel.UpdatedAt);

            this.Logger.LogDebug("Appointment loaded successfully");
            this.StateHasChanged();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error loading appointment: {AppointmentId}", this.AppointmentId);
            this.ErrorMessage = "予約データの読み込み中にエラーが発生しました。";
        }
    }

    private async Task HandleSubmit()
    {
        if (String.IsNullOrWhiteSpace(this.FormModel.PatientName))
        {
            this.ErrorMessage = "患者名を入力してください。";
            return;
        }

        this.IsSaving = true;
        this.ErrorMessage = null;
        this.StateHasChanged();

        try
        {
            if (this.IsEditMode)
            {
                // 編集モード：UpdateAppointmentAsync を呼び出す
                await this.UpdateAppointmentAsync();
            }
            else
            {
                // 新規作成モード：CreateAppointmentAsync を呼び出す
                await this.CreateAppointmentAsync();
            }

            await this.OnSave.InvokeAsync();
            await this.HandleClose();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error saving appointment");
            this.ErrorMessage = $"保存に失敗しました: {ex.Message}";
        }
        finally
        {
            this.IsSaving = false;
            this.StateHasChanged();
        }
    }

    /// <summary>
    /// 既存の予約を更新
    /// </summary>
    private async Task UpdateAppointmentAsync()
    {
        if (!this.AppointmentId.HasValue)
        {
            this.ErrorMessage = "予約IDが指定されていません。";
            return;
        }

        this.Logger.LogInformation("Updating appointment: {AppointmentId}", this.AppointmentId);

        var dto = new UpdateAppointmentDto
        {
            Date = this.FormModel.Date,
            StartTime = this.FormModel.StartTime,
            EndTime = this.FormModel.EndTime,
            Status = this.FormModel.Status,
            PatientId = this.FormModel.PatientId,
            OrganizationId = this.FormModel.OrganizationId,
            FloorId = this.FormModel.FloorId,
            Memo = this.FormModel.Memo,
            ExpectedUpdatedAt = this.FormModel.UpdatedAt  // 楽観的ロック用
        };

        this.Logger.LogInformation("Sending update: ExpectedUpdatedAt={ExpectedUpdatedAt}", dto.ExpectedUpdatedAt);

        var result = await this.AppointmentService.UpdateAppointmentAsync(this.AppointmentId.Value, dto);

        if (!result.IsSuccess)
        {
            this.Logger.LogError("Failed to update appointment: {Error}", result.ErrorMessage);
            throw new Exception(result.ErrorMessage ?? "予約の更新に失敗しました。");
        }

        this.Logger.LogInformation("Appointment updated successfully: {AppointmentId}", this.AppointmentId);
    }

    /// <summary>
    /// 新しい予約を作成
    /// </summary>
    private async Task CreateAppointmentAsync()
    {
        // 新規作成時は必須フィールドの確認
        if (!this.FormModel.PatientId.HasValue)
        {
            this.ErrorMessage = "患者IDが指定されていません。";
            return;
        }

        if (!this.FormModel.OrganizationId.HasValue)
        {
            this.ErrorMessage = "組織IDが指定されていません。";
            return;
        }

        if (!this.FormModel.FloorId.HasValue)
        {
            this.ErrorMessage = "フロアIDが指定されていません。";
            return;
        }

        this.Logger.LogInformation("Creating new appointment");

        var dto = new CreateAppointmentDto
        {
            Date = this.FormModel.Date,
            StartTime = this.FormModel.StartTime,
            EndTime = this.FormModel.EndTime,
            Status = this.FormModel.Status,
            PatientId = this.FormModel.PatientId.Value,
            OrganizationId = this.FormModel.OrganizationId.Value,
            FloorId = this.FormModel.FloorId.Value
        };

        var result = await this.AppointmentService.CreateAppointmentAsync(dto);

        if (!result.IsSuccess)
        {
            this.Logger.LogError("Failed to create appointment: {Error}", result.ErrorMessage);
            throw new Exception(result.ErrorMessage ?? "予約の作成に失敗しました。");
        }

        this.Logger.LogInformation("Appointment created successfully");
    }

    private async Task HandleDelete()
    {
        if (!this.IsEditMode) return;

        this.IsDeleting = true;
        this.ErrorMessage = null;
        this.StateHasChanged();

        try
        {
            await this.DeleteAppointmentAsync();
            await this.OnDelete.InvokeAsync();
            await this.OnClose.InvokeAsync();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error deleting appointment: {AppointmentId}", this.AppointmentId);
            this.ErrorMessage = $"削除に失敗しました: {ex.Message}";
        }
        finally
        {
            this.IsDeleting = false;
            this.StateHasChanged();
        }
    }

    /// <summary>
    /// 予約を削除
    /// </summary>
    private async Task DeleteAppointmentAsync()
    {
        if (!this.AppointmentId.HasValue)
        {
            this.ErrorMessage = "予約IDが指定されていません。";
            return;
        }

        this.Logger.LogInformation("Deleting appointment: {AppointmentId}", this.AppointmentId);

        var result = await this.AppointmentService.DeleteAppointmentAsync(this.AppointmentId.Value);

        if (!result.IsSuccess)
        {
            this.Logger.LogError("Failed to delete appointment: {Error}", result.ErrorMessage);
            throw new Exception(result.ErrorMessage ?? "予約の削除に失敗しました。");
        }

        this.Logger.LogInformation("Appointment deleted successfully: {AppointmentId}", this.AppointmentId);
    }

    private async Task HandleClose()
    {
        if (!this.IsProcessing)
        {
            await this.OnClose.InvokeAsync();
        }
    }

    /// <summary>
    /// フォームモデル
    /// </summary>
    public class AppointmentFormModel
    {
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        public string StartTimeString { get; set; } = BusinessHoursConstants.DefaultAppointmentStartTime;
        public string EndTimeString { get; set; } = BusinessHoursConstants.DefaultAppointmentEndTime;
        public int Status { get; set; } = 0;
        public string PatientName { get; set; } = String.Empty;
        public string? OrganizationName { get; set; }
        public string? Memo { get; set; }

        // 予約に関連するID
        public Guid? PatientId { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? FloorId { get; set; }

        // 楽観的ロック用：最終更新日時
        public DateTime? UpdatedAt { get; set; }

        public TimeOnly? StartTime => TimeOnly.TryParse(this.StartTimeString, out var t) ? t : null;
        public TimeOnly? EndTime => TimeOnly.TryParse(this.EndTimeString, out var t) ? t : null;
    }
}




