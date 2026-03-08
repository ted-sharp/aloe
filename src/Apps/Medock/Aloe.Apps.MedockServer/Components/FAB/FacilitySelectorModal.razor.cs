using Aloe.Apps.MedockLib.Services;
using Microsoft.AspNetCore.Components;
using System.Linq;

namespace Aloe.Apps.MedockServer.Components.FAB;

public partial class FacilitySelectorModal : ComponentBase
{
    [Parameter]
    public List<FacilityInfo>? Facilities { get; set; }

    [Parameter]
    public Guid? CurrentFacilityId { get; set; }

    [Parameter]
    public EventCallback<Guid> OnSelect { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    private Dictionary<string, List<FacilityInfo>>? GroupedFacilities =>
        this.Facilities?.GroupBy(f => f.TenantName)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.OrderBy(f => f.FacilityName).ToList());

    private async Task HandleBackdropClick()
    {
        await this.Close();
    }

    private async Task Close()
    {
        await this.OnClose.InvokeAsync();
    }

    private async Task SelectFacility(Guid facilityId)
    {
        await this.OnSelect.InvokeAsync(facilityId);
        await this.Close();
    }
}



