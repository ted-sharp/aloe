using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.Dtos;
using Aloe.Apps.MedockLib.Services.Dtos.Appointments;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class AppointmentModal : ComponentBase
{
    /// <summary>
    /// 予約サービス
    /// </summary>
    [Inject]
    private IAppointmentService AppointmentService { get; set; } = default!;

    /// <summary>
    /// 予約フォームサービス
    /// </summary>
    [Inject]
    private IAppointmentFormService FormService { get; set; } = default!;

    /// <summary>
    /// ユーザーコンテキストサービス
    /// </summary>
    [Inject]
    private IUserContextService UserContextService { get; set; } = default!;

    /// <summary>
    /// ロガー
    /// </summary>
    [Inject]
    private ILogger<AppointmentModal> Logger { get; set; } = default!;

    /// <summary>
    /// 日時プロバイダー
    /// </summary>
    [Inject]
    private IDateTimeProvider DateTimeProvider { get; set; } = default!;

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
    public int? SelectedStartMin { get; set; }

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

    // 利用可能な機器リソース
    private List<FilterItem> AvailableEquipmentResources { get; set; } = new();

    protected override async Task OnParametersSetAsync()
    {
        // パラメータが変わったら常に初期化
        this.Logger.LogInformation("OnParametersSetAsync called: IsOpen={IsOpen}, AppointmentId={AppointmentId}", this.IsOpen, this.AppointmentId);
        await this.InitializeFormAsync();
        await this.LoadAvailableEquipmentResourcesAsync();
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
            var startMin = this.SelectedStartMin ?? BusinessHoursConstants.DefaultAppointmentStartMin;
            this.FormModel = new AppointmentFormModel
            {
                Date = this.SelectedDate ?? this.DateTimeProvider.TodayDateOnly,
                StartTimeString = TimeConstants.MinutesToTimeString(startMin),
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
                StartTimeString = TimeConstants.MinutesToTimeString(dto.StartMin),
                Status = dto.Status,
                PatientName = dto.PatientName ?? String.Empty,
                OrganizationName = dto.OrganizationName ?? String.Empty,
                PatientId = dto.PatientId,
                OrganizationId = dto.OrganizationId,
                FloorId = dto.FloorId,
                Memo = dto.Memo,
                UpdatedAt = dto.UpdatedAt,  // 楽観的ロック用に保存
                SelectedEquipmentResourceIds = dto.EquipmentResources
                    .Select(r => r.Id)
                    .ToList()
            };

            this.Logger.LogInformation("FormModel loaded: UpdatedAt={UpdatedAt}", this.FormModel.UpdatedAt);

            this.Logger.LogDebug("Appointment loaded successfully");
            this.StateHasChanged();
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this.UserContextService.GetTenantContext();
            this.Logger.LogError(ex,
                "Error loading appointment: {AppointmentId} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}",
                this.AppointmentId, tenantId, facilityId, userId);
            this.ErrorMessage = "予約データの読み込み中にエラーが発生しました。";
        }
    }

    /// <summary>
    /// 利用可能な機器リソースを読み込む
    /// </summary>
    private async Task LoadAvailableEquipmentResourcesAsync()
    {
        try
        {
            var facilityId = this.UserContextService.CurrentUser?.FacilityId ?? Guid.Empty;
            if (facilityId == Guid.Empty)
            {
                this.Logger.LogWarning("Facility ID not found in user context");
                return;
            }

            var resources = await this.FormService.GetAvailableEquipmentResourcesAsync(facilityId);
            this.AvailableEquipmentResources = resources
                .Select(r => new FilterItem { Id = r.Id, Name = r.Name })
                .ToList();

            this.Logger.LogDebug("Loaded {Count} equipment resources", this.AvailableEquipmentResources.Count);
        }
        catch (Exception ex)
        {
            var (tenantId, facilityId, userId) = this.UserContextService.GetTenantContext();
            this.Logger.LogError(ex,
                "Error loading available equipment resources | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}",
                tenantId, facilityId, userId);
        }
    }

    /// <summary>
    /// 機器リソースの選択状態をトグルする
    /// </summary>
    private void ToggleEquipmentResource(Guid resourceId)
    {
        if (this.FormModel.SelectedEquipmentResourceIds.Contains(resourceId))
        {
            this.FormModel.SelectedEquipmentResourceIds.Remove(resourceId);
        }
        else
        {
            this.FormModel.SelectedEquipmentResourceIds.Add(resourceId);
        }
    }

    /// <summary>
    /// すべての機器リソースを選択する
    /// </summary>
    private void SelectAllEquipment()
    {
        this.FormModel.SelectedEquipmentResourceIds = this.AvailableEquipmentResources
            .Select(r => r.Id)
            .ToList();
    }

    /// <summary>
    /// すべての機器リソースの選択を解除する
    /// </summary>
    private void ClearAllEquipment()
    {
        this.FormModel.SelectedEquipmentResourceIds.Clear();
    }

    private async Task HandleSubmit()
    {
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
            var (tenantId, facilityId, userId) = this.UserContextService.GetTenantContext();
            this.Logger.LogError(ex,
                "Error saving appointment | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}",
                tenantId, facilityId, userId);
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
            StartMin = GetStartMinFromTimeString(this.FormModel.StartTimeString),
            Status = this.FormModel.Status,
            PatientId = this.FormModel.PatientId,
            OrganizationId = this.FormModel.OrganizationId,
            FloorId = this.FormModel.FloorId,
            Memo = this.FormModel.Memo,
            ExpectedUpdatedAt = this.FormModel.UpdatedAt,  // 楽観的ロック用
            EquipmentResourceIds = this.FormModel.SelectedEquipmentResourceIds
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
        try
        {
            this.Logger.LogInformation("Creating new appointment");

            // ユーザーコンテキストから施設IDを取得
            var facilityId = this.UserContextService.CurrentUser?.FacilityId ?? Guid.Empty;

            if (facilityId == Guid.Empty)
            {
                this.ErrorMessage = "施設が選択されていません。";
                return;
            }

            // PatientIdがなければ患者を作成または取得（仮予約は null でも許可）
            Guid? patientId = null;
            if (this.FormModel.PatientId.HasValue)
            {
                patientId = this.FormModel.PatientId.Value;
            }
            else if (!String.IsNullOrWhiteSpace(this.FormModel.PatientName))
            {
                // 患者名が入力されている場合のみ患者を作成
                patientId = await this.FormService.GetOrCreatePatientAsync(facilityId, this.FormModel.PatientName);
            }
            // else: patientId は null のまま（仮予約）

            // OrganizationIdがなければデフォルト組織を取得（仮予約は null でも許可）
            Guid? organizationId = null;
            if (this.FormModel.OrganizationId.HasValue)
            {
                organizationId = this.FormModel.OrganizationId.Value;
            }
            else
            {
                var defaultOrgId = await this.FormService.GetDefaultOrganizationAsync(facilityId);
                if (defaultOrgId != Guid.Empty)
                {
                    organizationId = defaultOrgId;
                }
                // else: organizationId は null のまま（デフォルト組織がない場合でも仮予約は許可）
            }

            // FloorIdがなければデフォルトフロアを取得
            Guid floorId;
            if (this.FormModel.FloorId.HasValue)
            {
                floorId = this.FormModel.FloorId.Value;
            }
            else
            {
                floorId = await this.FormService.GetDefaultFloorAsync(facilityId);
                if (floorId == Guid.Empty)
                {
                    this.ErrorMessage = "デフォルトフロアが見つかりません。";
                    return;
                }
            }

            this.Logger.LogInformation("Using IDs - PatientId: {PatientId}, OrgId: {OrgId}, FloorId: {FloorId}", patientId?.ToString() ?? "(null)", organizationId?.ToString() ?? "(null)", floorId);

            var dto = new CreateAppointmentDto
            {
                Date = this.FormModel.Date,
                StartMin = GetStartMinFromTimeString(this.FormModel.StartTimeString),
                Status = this.FormModel.Status,
                PatientId = patientId,
                OrganizationId = organizationId,
                FloorId = floorId,
                Memo = this.FormModel.Memo,
                EquipmentResourceIds = this.FormModel.SelectedEquipmentResourceIds
            };

            var result = await this.AppointmentService.CreateAppointmentAsync(dto);

            if (!result.IsSuccess)
            {
                this.Logger.LogError("Failed to create appointment: {Error}", result.ErrorMessage);
                throw new Exception(result.ErrorMessage ?? "予約の作成に失敗しました。");
            }

            this.Logger.LogInformation("Appointment created successfully");
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error in CreateAppointmentAsync");
            throw;
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
    /// フィルター項目（リソース表示用）
    /// </summary>
    public class FilterItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = String.Empty;
    }
    /// <summary>
    /// 時刻文字列（"HH:mm"）から分単位の int に変換
    /// </summary>
    private static int? GetStartMinFromTimeString(string timeStr)
    {
        return TimeConstants.TryTimeStringToMinutes(timeStr);
    }

    public class AppointmentFormModel
    {
        public DateOnly Date { get; set; }
        public string StartTimeString { get; set; } = TimeConstants.MinutesToTimeString(BusinessHoursConstants.DefaultAppointmentStartMin);
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

        // 選択した機器リソースID
        public List<Guid> SelectedEquipmentResourceIds { get; set; } = new();
    }
}




