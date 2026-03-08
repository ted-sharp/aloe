using System.ComponentModel;
using System.Text.Json;
using Aloe.Apps.MedockLib.Services;
using ModelContextProtocol.Server;

namespace Aloe.Apps.MedockMcp.Tools;

/// <summary>
/// 予約検索・祝日取得ツール
/// </summary>
[McpServerToolType]
internal class AppointmentQueryTools
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentQueryTools(IAppointmentService appointmentService)
    {
        this._appointmentService = appointmentService;
    }

    [McpServerTool(Name = "GetAppointments")]
    [Description("指定した日付範囲の予約一覧を取得します。日付はyyyy-MM-dd形式で指定してください。")]
    public async Task<string> GetAppointments(
        [Description("開始日 (yyyy-MM-dd)")] string startDate,
        [Description("終了日 (yyyy-MM-dd)")] string endDate)
    {
        if (!DateOnly.TryParse(startDate, out var start))
            return JsonSerializer.Serialize(new { error = $"開始日の形式が不正です: {startDate}" });
        if (!DateOnly.TryParse(endDate, out var end))
            return JsonSerializer.Serialize(new { error = $"終了日の形式が不正です: {endDate}" });

        var result = await this._appointmentService.GetAppointmentsAsync(start, end);
        if (!result.IsSuccess)
            return JsonSerializer.Serialize(new { error = result.ErrorMessage });

        return JsonSerializer.Serialize(new
        {
            count = result.Value!.Count,
            appointments = result.Value.Select(a => new
            {
                id = a.Id,
                date = a.Date.ToString("yyyy-MM-dd"),
                startMin = a.StartMin,
                startTime = $"{a.StartMin / 60:D2}:{a.StartMin % 60:D2}",
                patientName = a.PatientName,
                organizationName = a.OrganizationName,
                floorName = a.FloorName,
                status = a.Status,
                memo = a.Memo,
                equipmentResources = a.EquipmentResources.Select(e => new { e.Id, e.Name })
            })
        });
    }

    [McpServerTool(Name = "GetAppointment")]
    [Description("予約IDで1件の予約を取得します。")]
    public async Task<string> GetAppointment(
        [Description("予約ID (GUID)")] string appointmentId)
    {
        if (!Guid.TryParse(appointmentId, out var id))
            return JsonSerializer.Serialize(new { error = $"予約IDの形式が不正です: {appointmentId}" });

        var result = await this._appointmentService.GetAppointmentAsync(id);
        if (!result.IsSuccess)
            return JsonSerializer.Serialize(new { error = result.ErrorMessage });

        var a = result.Value!;
        return JsonSerializer.Serialize(new
        {
            id = a.Id,
            date = a.Date.ToString("yyyy-MM-dd"),
            startMin = a.StartMin,
            startTime = $"{a.StartMin / 60:D2}:{a.StartMin % 60:D2}",
            patientName = a.PatientName,
            organizationName = a.OrganizationName,
            floorName = a.FloorName,
            status = a.Status,
            memo = a.Memo,
            equipmentResources = a.EquipmentResources.Select(e => new { e.Id, e.Name })
        });
    }

    [McpServerTool(Name = "GetHolidays")]
    [Description("指定した日付範囲の祝日一覧を取得します。")]
    public async Task<string> GetHolidays(
        [Description("開始日 (yyyy-MM-dd)")] string startDate,
        [Description("終了日 (yyyy-MM-dd)")] string endDate)
    {
        if (!DateOnly.TryParse(startDate, out var start))
            return JsonSerializer.Serialize(new { error = $"開始日の形式が不正です: {startDate}" });
        if (!DateOnly.TryParse(endDate, out var end))
            return JsonSerializer.Serialize(new { error = $"終了日の形式が不正です: {endDate}" });

        var result = await this._appointmentService.GetHolidaysAsync(start, end);
        if (!result.IsSuccess)
            return JsonSerializer.Serialize(new { error = result.ErrorMessage });

        return JsonSerializer.Serialize(new
        {
            count = result.Value!.Count,
            holidays = result.Value.Select(h => new
            {
                date = h.Date.ToString("yyyy-MM-dd"),
                name = h.Name
            })
        });
    }
}
