using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace Aloe.Apps.MedockServer.Components.Pages;

public partial class Error : ComponentBase
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    private string? RequestId { get; set; }
    private bool ShowRequestId => !String.IsNullOrEmpty(this.RequestId);

    protected override void OnInitialized() =>
        this.RequestId = Activity.Current?.Id ?? this.HttpContext?.TraceIdentifier;
}




