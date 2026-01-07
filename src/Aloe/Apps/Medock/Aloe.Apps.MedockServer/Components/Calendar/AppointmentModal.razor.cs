using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.Dtos;
using Aloe.Apps.MedockLib.Services.Dtos.Appointments;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
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
    /// ユーザーコンテキストサービス
    /// </summary>
    [Inject]
    private IUserContextService UserContextService { get; set; } = default!;

    /// <summary>
    /// DbContextファクトリ
    /// </summary>
    [Inject]
    private IDbContextFactory<MedockDbContext> ContextFactory { get; set; } = default!;

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
            this.FormModel = new AppointmentFormModel
            {
                Date = this.SelectedDate ?? DateOnly.FromDateTime(DateTime.Today),
                StartTimeString = this.SelectedStartMin.HasValue 
                    ? TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(this.SelectedStartMin.Value)).ToString("HH:mm") 
                    : BusinessHoursConstants.DefaultAppointmentStartTime,
                EndTimeString = this.SelectedStartMin.HasValue
                    ? TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(this.SelectedStartMin.Value + 60)).ToString("HH:mm")
                    : BusinessHoursConstants.DefaultAppointmentEndTime,
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
                StartTimeString = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(dto.StartMin)).ToString("HH:mm"),
                EndTimeString = BusinessHoursConstants.DefaultAppointmentEndTime, // EndTime removed from DTO
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
            this.Logger.LogError(ex, "Error loading appointment: {AppointmentId}", this.AppointmentId);
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

            await using var context = await this.ContextFactory.CreateDbContextAsync();

            this.AvailableEquipmentResources = await context.AppointmentResources
                .AsNoTracking()
                .Where(r => r.Floor.FacilityId == facilityId &&
                           !r.IsDeleted &&
                           r.ApptResTypeCode == (int)AppointmentResourceType.Equipment)
                .OrderBy(r => r.ApptResSeq)
                .ThenBy(r => r.ApptResName)
                .Select(r => new FilterItem
                {
                    Id = r.ApptResId,
                    Name = r.ApptResName
                })
                .ToListAsync();

            this.Logger.LogDebug("Loaded {Count} equipment resources", this.AvailableEquipmentResources.Count);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error loading available equipment resources");
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
            StartMin = this.FormModel.StartTime.HasValue
                ? this.FormModel.StartTime.Value.Hour * 60 + this.FormModel.StartTime.Value.Minute
                : null,
            // EndTime = this.FormModel.EndTime, // Removed from DTO
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

            // DbContextを使用して必要なデータを取得
            using var context = this.ContextFactory.CreateDbContext();

            // PatientIdがなければ患者を作成または取得（仮予約は null でも許可）
            Guid? patientId = null;
            if (this.FormModel.PatientId.HasValue)
            {
                patientId = this.FormModel.PatientId.Value;
            }
            else if (!String.IsNullOrWhiteSpace(this.FormModel.PatientName))
            {
                // 患者名が入力されている場合のみ患者を作成
                patientId = await this.GetOrCreatePatientAsync(context, facilityId, this.FormModel.PatientName);
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
                var defaultOrgId = await this.GetDefaultOrganizationAsync(context, facilityId);
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
                floorId = await this.GetDefaultFloorAsync(context, facilityId);
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
                StartMin = this.FormModel.StartTime.HasValue
                    ? this.FormModel.StartTime.Value.Hour * 60 + this.FormModel.StartTime.Value.Minute
                    : null,
                // EndTime = this.FormModel.EndTime, // Removed from DTO
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

    /// <summary>
    /// 患者を取得、または新規作成
    /// </summary>
    private async Task<Guid> GetOrCreatePatientAsync(MedockDbContext context, Guid facilityId, string patientName)
    {
        // 患者名が指定されている場合は検索
        if (!String.IsNullOrWhiteSpace(patientName))
        {
            // 同じ名前の患者を検索
            var existingPatient = await context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PtName == patientName && p.FacilityId == facilityId && !p.IsDeleted);

            if (existingPatient != null)
            {
                return existingPatient.PtId;
            }
        }

        // 患者が見つからない場合または名前が空の場合は新規作成
        var newPatient = new Patient
        {
            PtId = Guid.CreateVersion7(),
            FacilityId = facilityId,
            CanonicalPtId = Guid.CreateVersion7(),
            PtCode = $"PT{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            PtName = patientName ?? String.Empty,
            PtNameCompat = patientName ?? String.Empty,
            PrimaryOrgId = await this.GetDefaultOrganizationAsync(context, facilityId),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Patients.Add(newPatient);
        await context.SaveChangesAsync();

        this.Logger.LogInformation("Created new patient: {PatientName} ({PatientId})", String.IsNullOrEmpty(patientName) ? "(空欄)" : patientName, newPatient.PtId);

        return newPatient.PtId;
    }

    /// <summary>
    /// デフォルト組織を取得
    /// </summary>
    private async Task<Guid> GetDefaultOrganizationAsync(MedockDbContext context, Guid facilityId)
    {
        var defaultOrg = await context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.FacilityId == facilityId && !o.IsDeleted);

        return defaultOrg?.OrgId ?? Guid.Empty;
    }

    /// <summary>
    /// デフォルトフロアを取得
    /// </summary>
    private async Task<Guid> GetDefaultFloorAsync(MedockDbContext context, Guid facilityId)
    {
        var defaultFloor = await context.Floors
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FacilityId == facilityId && !f.IsDeleted);

        return defaultFloor?.FloorId ?? Guid.Empty;
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

        // 選択した機器リソースID
        public List<Guid> SelectedEquipmentResourceIds { get; set; } = new();

        public TimeOnly? StartTime => TimeOnly.TryParse(this.StartTimeString, out var t) ? t : null;
        public TimeOnly? EndTime => TimeOnly.TryParse(this.EndTimeString, out var t) ? t : null;
    }
}




