using Aloe.Apps.MedockLib.Services.VoiceCommand;
using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Layout;

public partial class MobileCalendarLayout : LayoutComponentBase
{
    private bool _drawerOpen;

    private Func<VoiceCommandResult, Task>? _onVoiceCommand;

    public void OpenDrawer()
    {
        this._drawerOpen = true;
        this.StateHasChanged();
    }

    public void CloseDrawer()
    {
        this._drawerOpen = false;
        this.StateHasChanged();
    }

    public void RegisterVoiceCommandHandler(Func<VoiceCommandResult, Task> handler)
    {
        this._onVoiceCommand = handler;
    }

    private async Task HandleVoiceCommand(VoiceCommandResult command)
    {
        if (this._onVoiceCommand != null)
        {
            await this._onVoiceCommand(command);
        }
    }
}
