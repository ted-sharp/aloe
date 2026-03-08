using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Services;
using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Mobile;

public partial class MobileAppointmentModal : ComponentBase
{
    [Inject]
    private IAppointmentService AppointmentService { get; set; } = default!;

    [Inject]
    private IAppointmentFormService FormService { get; set; } = default!;

    [Inject]
    private IUserContextService UserContextService { get; set; } = default!;

    [Inject]
    private IDateTimeProvider DateTimeProvider { get; set; } = default!;

    [Parameter]
    public bool IsOpen { get; set; }

    [Parameter]
    public Guid? AppointmentId { get; set; }

    [Parameter]
    public DateOnly? SelectedDate { get; set; }

    [Parameter]
    public int? SelectedStartMin { get; set; }

    [Parameter]
    public int InitialStatus { get; set; } = 0;

    [Parameter]
    public EventCallback OnClose { get; set; }

    [Parameter]
    public EventCallback OnSave { get; set; }

    private bool IsEditMode => this.AppointmentId.HasValue;
    private bool _isProcessing;
    private string? _errorMessage;

    private DateOnly _formDate;
    private string _startTimeString = "09:00";
    private int _status;
    private string _patientName = "";
    private string? _memo;

    private static readonly string[] _timeSlots = Enumerable.Range(8, 11)
        .SelectMany(h => new[] { $"{h:D2}:00", $"{h:D2}:30" })
        .ToArray();

    private void HandleDateChange(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {
        if (DateOnly.TryParse(e.Value?.ToString(), out var d))
            this._formDate = d;
    }

    private void HandleTimeChange(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {
        this._startTimeString = e.Value?.ToString() ?? this._startTimeString;
    }

    protected override void OnParametersSet()
    {
        this._formDate = this.SelectedDate ?? this.DateTimeProvider.TodayDateOnly;
        this._startTimeString = TimeConstants.MinutesToTimeString(this.SelectedStartMin ?? BusinessHoursConstants.DefaultAppointmentStartMin);
        this._status = this.InitialStatus;
        this._patientName = "";
        this._memo = null;
        this._errorMessage = null;
    }

    private async Task HandleSubmit()
    {
        this._isProcessing = true;
        this._errorMessage = null;
        this.StateHasChanged();

        try
        {
            var facilityId = this.UserContextService.CurrentUser?.FacilityId ?? Guid.Empty;
            if (facilityId == Guid.Empty)
            {
                this._errorMessage = "施設が選択されていません。";
                return;
            }

            Guid? patientId = null;
            if (!String.IsNullOrWhiteSpace(this._patientName))
            {
                patientId = await this.FormService.GetOrCreatePatientAsync(facilityId, this._patientName);
            }

            var defaultOrgId = await this.FormService.GetDefaultOrganizationAsync(facilityId);
            Guid? organizationId = defaultOrgId != Guid.Empty ? defaultOrgId : null;

            var floorId = await this.FormService.GetDefaultFloorAsync(facilityId);
            if (floorId == Guid.Empty)
            {
                this._errorMessage = "デフォルトフロアが見つかりません。";
                return;
            }

            var startMin = TimeConstants.TryTimeStringToMinutes(this._startTimeString);
            var dto = new Aloe.Apps.MedockLib.Services.Dtos.Appointments.CreateAppointmentDto
            {
                Date = this._formDate,
                StartMin = startMin,
                Status = this._status,
                PatientId = patientId,
                OrganizationId = organizationId,
                FloorId = floorId,
                Memo = this._memo,
                EquipmentResourceIds = new List<Guid>()
            };

            var result = await this.AppointmentService.CreateAppointmentAsync(dto);
            if (!result.IsSuccess)
            {
                this._errorMessage = result.ErrorMessage ?? "予約の作成に失敗しました。";
                return;
            }

            await this.OnSave.InvokeAsync();
            await this.OnClose.InvokeAsync();
        }
        catch (Exception ex)
        {
            this._errorMessage = $"エラーが発生しました: {ex.Message}";
        }
        finally
        {
            this._isProcessing = false;
            this.StateHasChanged();
        }
    }

    private async Task HandleClose()
    {
        if (!this._isProcessing)
        {
            await this.OnClose.InvokeAsync();
        }
    }
}
