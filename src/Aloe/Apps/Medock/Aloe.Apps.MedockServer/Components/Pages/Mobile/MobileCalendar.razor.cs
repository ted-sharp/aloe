using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockServer.ApplicationServices.Calendar;
using Aloe.Apps.MedockServer.Components.Calendar;
using Aloe.Apps.MedockServer.Components.Layout;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.VoiceCommand;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Aloe.Apps.MedockServer.Components.Pages.Mobile;

public partial class MobileCalendar : ComponentBase
{
    private enum MobileViewType { Month, Day }

    [CascadingParameter]
    private MobileCalendarLayout? Layout { get; set; }

    [Inject]
    private CalendarState State { get; set; } = default!;

    [Inject]
    private ICalendarOrchestrator Orchestrator { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    private IDateTimeProvider DateTimeProvider { get; set; } = default!;

    private MobileViewType _viewType = MobileViewType.Month;
    private DateOnly? _selectedDate;
    private readonly int? _defaultStartMin = 540;
    private bool _filterOpen;
    private bool _longPressMenuOpen;
    private DateOnly? _longPressDate;
    private bool _appointmentModalOpen;
    private DateOnly? _appointmentModalDate;
    private int _appointmentInitialStatus;

    protected override async Task OnInitializedAsync()
    {
        var authState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync();
        var result = await this.Orchestrator.InitializeAsync(authState.User);

        if (!result.Success && result.RedirectUrl != null)
        {
            this.NavigationManager.NavigateTo(result.RedirectUrl, forceLoad: true);
            return;
        }

        this.Layout?.RegisterVoiceCommandHandler(this.HandleVoiceCommand);
        this.StateHasChanged();
    }

    private void SetView(MobileViewType viewType)
    {
        this._viewType = viewType;
        this.StateHasChanged();
    }

    private string GetPeriodTitle()
    {
        return this._viewType == MobileViewType.Month
            ? $"{this.State.CurrentDate.Year}年{this.State.CurrentDate.Month}月"
            : this.State.CurrentDate.ToString("M月d日（ddd）", new System.Globalization.CultureInfo("ja-JP"));
    }

    private async Task PreviousPeriod()
    {
        if (this._viewType == MobileViewType.Month)
            await this.Orchestrator.NavigatePreviousPeriodAsync();
        else
            this.State.CurrentDate = this.State.CurrentDate.AddDays(-1);

        this._selectedDate = null;
        this.StateHasChanged();
    }

    private async Task NextPeriod()
    {
        if (this._viewType == MobileViewType.Month)
            await this.Orchestrator.NavigateNextPeriodAsync();
        else
            this.State.CurrentDate = this.State.CurrentDate.AddDays(1);

        this._selectedDate = null;
        this.StateHasChanged();
    }

    private async Task GoToToday()
    {
        await this.Orchestrator.GoToTodayAsync();
        this._selectedDate = null;
        this.StateHasChanged();
    }

    private void ToggleFilter()
    {
        this._filterOpen = !this._filterOpen;
        this.StateHasChanged();
    }

    private void HandleOpenDrawer()
    {
        this.Layout?.OpenDrawer();
    }

    private async Task HandleDateSelected(DateOnly date)
    {
        this._selectedDate = date;
        await this.Orchestrator.HandleDateSelectedSingleAsync(date);
        this.StateHasChanged();
    }

    private void HandleLongPress(DateOnly date)
    {
        this._longPressDate = date;
        this._longPressMenuOpen = true;
        this.StateHasChanged();
    }

    private async Task HandleFilterApply(SearchFilterPanel.SearchFilter filter)
    {
        await this.Orchestrator.ApplyFilterAsync(filter);
        this.StateHasChanged();
    }

    private void HandleLongPressCreate(int status)
    {
        this._appointmentModalDate = this._longPressDate;
        this._appointmentInitialStatus = status;
        this._appointmentModalOpen = true;
        this.StateHasChanged();
    }

    private void HandleCreateFromDayDetail(DateOnly date)
    {
        this._appointmentModalDate = date;
        this._appointmentInitialStatus = 0;
        this._appointmentModalOpen = true;
        this.StateHasChanged();
    }

    private void HandleCreateFromDay(DateOnly date)
    {
        this._appointmentModalDate = date;
        this._appointmentInitialStatus = 0;
        this._appointmentModalOpen = true;
        this.StateHasChanged();
    }

    private async Task HandleAppointmentSaved()
    {
        await this.Orchestrator.RefreshCalendarDataAsync();
        this.StateHasChanged();
    }

    private List<AppointmentStats>? GetSelectedDayStats()
    {
        if (!this._selectedDate.HasValue) return null;
        var key = this._selectedDate.Value.ToString("yyyy-MM-dd");
        return this.State.MainStats.TryGetValue(key, out var stats) ? stats : null;
    }

    private async Task HandleVoiceCommand(VoiceCommandResult command)
    {
        switch (command.Intent)
        {
            case VoiceCommandIntent.CreateAppointment:
            case VoiceCommandIntent.TentativeAppointment:
                this._appointmentModalDate = command.Date ?? this.State.CurrentDate;
                this._appointmentInitialStatus = command.Intent == VoiceCommandIntent.TentativeAppointment ? 1 : 0;
                this._appointmentModalOpen = true;
                this.StateHasChanged();
                break;

            case VoiceCommandIntent.SearchAppointment:
                if (command.Date.HasValue)
                    this.State.CurrentDate = command.Date.Value;
                await this.Orchestrator.RefreshCalendarDataAsync();
                this.StateHasChanged();
                break;
        }
    }
}
