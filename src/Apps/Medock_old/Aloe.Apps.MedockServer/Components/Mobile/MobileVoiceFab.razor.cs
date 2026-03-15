using Aloe.Apps.MedockLib.Services.VoiceCommand;
using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Mobile;

public partial class MobileVoiceFab : ComponentBase
{
    [Parameter]
    public EventCallback<VoiceCommandResult> OnCommandReady { get; set; }

    private async Task HandleCommandReady(VoiceCommandResult result)
    {
        await this.OnCommandReady.InvokeAsync(result);
    }
}
