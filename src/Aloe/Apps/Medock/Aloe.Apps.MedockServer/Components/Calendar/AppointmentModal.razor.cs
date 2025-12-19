using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class AppointmentModal : ComponentBase
{
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

    protected override void OnParametersSet()
    {
        if (this.IsOpen)
        {
            this.InitializeForm();
        }
    }

    private void InitializeForm()
    {
        this.ErrorMessage = null;

        if (this.IsEditMode)
        {
            // TODO: AppointmentServiceから予約データを取得
            // 今はダミーデータ
            this.FormModel = new AppointmentFormModel
            {
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTimeString = "09:00",
                EndTimeString = "10:00",
                Status = 0,
                PatientName = "編集中の患者",
                OrganizationName = "団体名"
            };
        }
        else
        {
            // 新規作成
            this.FormModel = new AppointmentFormModel
            {
                Date = this.SelectedDate ?? DateOnly.FromDateTime(DateTime.Today),
                StartTimeString = this.SelectedTime?.ToString("HH:mm") ?? "09:00",
                EndTimeString = this.SelectedTime?.AddHours(1).ToString("HH:mm") ?? "10:00",
                Status = 0,
                PatientName = String.Empty,
                OrganizationName = String.Empty
            };
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
            // TODO: AppointmentServiceで保存
            await Task.Delay(500); // 仮の処理時間

            await this.OnSave.InvokeAsync();
        }
        catch (Exception ex)
        {
            this.ErrorMessage = $"保存に失敗しました: {ex.Message}";
        }
        finally
        {
            this.IsSaving = false;
            this.StateHasChanged();
        }
    }

    private async Task HandleDelete()
    {
        if (!this.IsEditMode) return;

        this.IsDeleting = true;
        this.ErrorMessage = null;
        this.StateHasChanged();

        try
        {
            // TODO: AppointmentServiceで削除
            await Task.Delay(500); // 仮の処理時間

            await this.OnDelete.InvokeAsync();
            await this.OnClose.InvokeAsync();
        }
        catch (Exception ex)
        {
            this.ErrorMessage = $"削除に失敗しました: {ex.Message}";
        }
        finally
        {
            this.IsDeleting = false;
            this.StateHasChanged();
        }
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
        public string StartTimeString { get; set; } = "09:00";
        public string EndTimeString { get; set; } = "10:00";
        public int Status { get; set; } = 0;
        public string PatientName { get; set; } = String.Empty;
        public string? OrganizationName { get; set; }
        public string? Memo { get; set; }

        public TimeOnly? StartTime => TimeOnly.TryParse(this.StartTimeString, out var t) ? t : null;
        public TimeOnly? EndTime => TimeOnly.TryParse(this.EndTimeString, out var t) ? t : null;
    }
}




