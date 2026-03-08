using Microsoft.AspNetCore.Components;

namespace Aloe.Apps.MedockServer.Components.Mobile;

public partial class MobileAvatarGroup : ComponentBase
{
    [Parameter]
    public List<string> OnlineUsers { get; set; } = new();
}
