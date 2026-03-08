using Aloe.Apps.MedockLib.Services.VoiceCommand;
using Aloe.Apps.MedockServer.Components.FAB;
using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Layout;

public partial class CalendarLayout : LayoutComponentBase
{
    // ドロワー状態（Calendar.razorから参照される）
    private bool _isDrawerOpen;
    public bool IsDrawerOpen
    {
        get => this._isDrawerOpen;
        set
        {
            if (this._isDrawerOpen != value)
            {
                this._isDrawerOpen = value;
                this.StateHasChanged();
            }
        }
    }

    // カレンダーからの状態
    public string CurrentView { get; private set; } = "month";

    // カレンダーからのコールバック
    private Func<Task>? _onNewAppointment;
    private Func<Task>? _onToggleFilter;
    private Func<Task>? _onGoToToday;
    private Func<Task>? _onPreviousPeriod;
    private Func<Task>? _onNextPeriod;
    private Func<string, Task>? _onViewChange;
    private Func<VoiceCommandResult, Task>? _onVoiceCommand;

    /// <summary>
    /// カレンダーページからアクションを登録する
    /// </summary>
    public void RegisterCalendarActions(
        string currentView,
        Func<Task>? onNewAppointment = null,
        Func<Task>? onToggleFilter = null,
        Func<Task>? onGoToToday = null,
        Func<Task>? onPreviousPeriod = null,
        Func<Task>? onNextPeriod = null,
        Func<string, Task>? onViewChange = null,
        Func<VoiceCommandResult, Task>? onVoiceCommand = null)
    {
        this.CurrentView = currentView;
        this._onNewAppointment = onNewAppointment;
        this._onToggleFilter = onToggleFilter;
        this._onGoToToday = onGoToToday;
        this._onPreviousPeriod = onPreviousPeriod;
        this._onNextPeriod = onNextPeriod;
        this._onViewChange = onViewChange;
        this._onVoiceCommand = onVoiceCommand;
        this.StateHasChanged();
    }

    /// <summary>
    /// 現在のビューを更新する
    /// </summary>
    public void UpdateCurrentView(string view)
    {
        this.CurrentView = view;
        this.StateHasChanged();
    }

    private void CloseDrawer()
    {
        this.IsDrawerOpen = false;
    }

    private async Task HandleNewAppointment()
    {
        if (this._onNewAppointment != null)
        {
            await this._onNewAppointment();
        }
    }

    private async Task HandleToggleFilter()
    {
        if (this._onToggleFilter != null)
        {
            await this._onToggleFilter();
        }
    }

    private async Task HandleGoToToday()
    {
        if (this._onGoToToday != null)
        {
            await this._onGoToToday();
        }
    }

    private async Task HandlePreviousPeriod()
    {
        if (this._onPreviousPeriod != null)
        {
            await this._onPreviousPeriod();
        }
    }

    private async Task HandleNextPeriod()
    {
        if (this._onNextPeriod != null)
        {
            await this._onNextPeriod();
        }
    }

    private async Task HandleViewChange(string view)
    {
        if (this._onViewChange != null)
        {
            await this._onViewChange(view);
        }
    }

    private async Task HandleVoiceCommand(VoiceCommandResult command)
    {
        if (this._onVoiceCommand != null)
        {
            await this._onVoiceCommand(command);
        }
    }
}




