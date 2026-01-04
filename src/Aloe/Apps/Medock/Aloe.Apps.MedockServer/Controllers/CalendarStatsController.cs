using Aloe.Apps.MedockLib.Repositories;
using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Aloe.Apps.MedockServer.Controllers;

/// <summary>
/// カレンダー統計データ用APIコントローラー
/// </summary>
[ApiController]
[Route("api/calendar/stats")]
public class CalendarStatsController : ControllerBase
{
    private readonly IAppointmentStatsRepository _appointmentStatsRepository;
    private readonly ILogger<CalendarStatsController> _logger;

    public CalendarStatsController(
        IAppointmentStatsRepository appointmentStatsRepository,
        ILogger<CalendarStatsController> logger)
    {
        this._appointmentStatsRepository = appointmentStatsRepository;
        this._logger = logger;
    }

    /// <summary>
    /// 差分データを取得
    /// </summary>
    [HttpPost("diff")]
    public async Task<IActionResult> GetDiffData([FromBody] DiffDataRequest request)
    {
        try
        {
            if (request == null || String.IsNullOrEmpty(request.Date))
            {
                return this.BadRequest("Date is required");
            }

            if (request.ResourceIds == null || !request.ResourceIds.Any())
            {
                return this.BadRequest("ResourceIds are required");
            }

            var date = DateOnly.Parse(request.Date);
            var resourceIds = request.ResourceIds.Select(Guid.Parse).ToList();

            var stats = await this._appointmentStatsRepository.GetMainResourceStatsByDateAndResourcesAsync(
                date,
                resourceIds);

            // Load slots separately
            var slots = await this._appointmentStatsRepository.GetStatSlotsByDateRangeAsync(
                date,
                date,
                resourceIds);

            // DTOに変換（必要に応じて）
            var result = stats.Select(s => new
            {
                apptStatId = s.ApptStatId.ToString(),
                apptDate = s.ApptDate.ToString("yyyy-MM-dd"),
                apptResId = s.ApptResId.ToString(),
                apptCap = s.ApptCap,
                apptCount = s.ApptCount,
                apptAvailable = s.ApptAvailable,
                appointmentResource = new
                {
                    apptResId = s.AppointmentResource.ApptResId.ToString(),
                    apptResName = s.AppointmentResource.ApptResName,
                    apptResTypeCode = s.AppointmentResource.ApptResTypeCode
                },
                appointmentStatSlots = (slots.TryGetValue((s.ApptDate, s.ApptResId), out var statSlots) ? statSlots : new List<AppointmentStatSlots>())
                    .Where(slot => !slot.IsDeleted)
                    .Select(slot => new
                    {
                        apptStatSlotId = slot.ApptStatSlotId.ToString(),
                        slotStart = slot.SlotStart,
                        slotEnd = slot.SlotEnd,
                        slotCap = slot.SlotCap,
                        slotCount = slot.SlotCount,
                        slotAvailable = slot.SlotAvailable
                    })
                    .ToList()
            }).ToList();

            // ETagとCache-Controlヘッダーを設定（10秒キャッシュ）
            var dataHash = ComputeHash(result);
            this.Response.Headers.ETag = $"\"{dataHash}\"";
            this.Response.Headers["Cache-Control"] = "private, max-age=10";

            return this.Ok(result);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to get diff data");
            return this.StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// データのハッシュを計算
    /// </summary>
    private static string ComputeHash(object data)
    {
        var json = JsonSerializer.Serialize(data);
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(json));
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// 差分データリクエスト
    /// </summary>
    public class DiffDataRequest
    {
        public string Date { get; set; } = String.Empty;
        public List<string> ResourceIds { get; set; } = new();
        public object? Filter { get; set; }
    }
}

