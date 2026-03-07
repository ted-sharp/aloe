using System.ComponentModel;
using System.Text.Json;
using Aloe.Apps.MedockLib.Services;
using ModelContextProtocol.Server;

namespace Aloe.Apps.MedockMcp.Tools;

/// <summary>
/// 施設情報ツール
/// </summary>
[McpServerToolType]
internal class FacilityTools
{
    private readonly IFacilityService _facilityService;
    private readonly IAppointmentFormService _appointmentFormService;
    private readonly IUserContextService _userContextService;

    public FacilityTools(
        IFacilityService facilityService,
        IAppointmentFormService appointmentFormService,
        IUserContextService userContextService)
    {
        this._facilityService = facilityService;
        this._appointmentFormService = appointmentFormService;
        this._userContextService = userContextService;
    }

    [McpServerTool(Name = "GetBusinessHours")]
    [Description("施設の営業時間を取得します。対象日付を指定しない場合は今日の営業時間を返します。")]
    public async Task<string> GetBusinessHours(
        [Description("対象日付 (yyyy-MM-dd、省略可)")] string? targetDate = null)
    {
        var (_, facilityId, _) = this._userContextService.GetTenantContext();
        if (!facilityId.HasValue)
            return JsonSerializer.Serialize(new { error = "施設コンテキストが未設定です" });

        DateOnly? date = null;
        if (!String.IsNullOrEmpty(targetDate))
        {
            if (!DateOnly.TryParse(targetDate, out var parsed))
                return JsonSerializer.Serialize(new { error = $"日付の形式が不正です: {targetDate}" });
            date = parsed;
        }

        var result = await this._facilityService.GetBusinessHoursAsync(facilityId.Value, date);
        if (!result.IsSuccess)
            return JsonSerializer.Serialize(new { error = result.ErrorMessage });

        var bh = result.Value!;
        return JsonSerializer.Serialize(new
        {
            startTime = bh.StartTime,
            endTime = bh.EndTime,
            lunchStartTime = bh.LunchStartTime,
            lunchEndTime = bh.LunchEndTime
        });
    }

    [McpServerTool(Name = "GetEquipmentResources")]
    [Description("施設で利用可能な機器リソース一覧を取得します。")]
    public async Task<string> GetEquipmentResources()
    {
        var (_, facilityId, _) = this._userContextService.GetTenantContext();
        if (!facilityId.HasValue)
            return JsonSerializer.Serialize(new { error = "施設コンテキストが未設定です" });

        var resources = await this._appointmentFormService.GetAvailableEquipmentResourcesAsync(facilityId.Value);

        return JsonSerializer.Serialize(new
        {
            count = resources.Count,
            resources = resources.Select(r => new { id = r.Id, name = r.Name })
        });
    }
}
