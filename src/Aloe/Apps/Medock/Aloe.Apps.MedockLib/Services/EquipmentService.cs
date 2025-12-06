using System.Text.Json;
using Aloe.Apps.MedockLib.Data;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 設備データと統計情報を取得するサービスの実装
/// </summary>
public class EquipmentService : IEquipmentService
{
    private readonly IDbContextFactory<MedockDbContext> _contextFactory;

    public EquipmentService(IDbContextFactory<MedockDbContext> contextFactory)
    {
        this._contextFactory = contextFactory;
    }

    /// <summary>
    /// 指定テナントの設備一覧を取得
    /// </summary>
    public async Task<List<EquipmentDto>> GetEquipmentsByTenantAsync(Guid tenantId)
    {
        using var context = this._contextFactory.CreateDbContext();

        // Tenant → Facility → Floor → Equipment の関連を辿る
        return await context.Equipments
            .Include(e => e.Floor)
                .ThenInclude(f => f!.Facility)
            .Where(e => !e.IsDeleted
                && e.Floor != null
                && !e.Floor.IsDeleted
                && e.Floor.Facility != null
                && !e.Floor.Facility.IsDeleted
                && e.Floor.Facility.TenantId == tenantId)
            .OrderBy(e => e.EquipSeq)
            .Select(e => new EquipmentDto
            {
                EquipId = e.EquipId,
                EquipName = e.EquipName,
                EquipDesc = e.EquipDesc,
                EquipSeq = e.EquipSeq
            })
            .ToListAsync();
    }

    /// <summary>
    /// 設備別予約統計を取得
    /// </summary>
    public async Task<List<EquipmentAppointmentStatsDto>> GetEquipmentStatsAsync(
        List<Guid> equipIds, DateOnly date)
    {
        using var context = this._contextFactory.CreateDbContext();

        var stats = await context.EquipmentAppointmentStats
            .Where(s => equipIds.Contains(s.EquipId)
                && s.ApptDate == date
                && !s.IsDeleted)
            .ToListAsync();

        return stats.Select(s => new EquipmentAppointmentStatsDto
        {
            EquipId = s.EquipId,
            ApptDate = s.ApptDate,
            ApptCount = s.ApptCount,
            ApptMax = s.ApptMax,
            ApptGraph = this.DeserializeApptGraph(s.ApptGraph)
        }).ToList();
    }

    /// <summary>
    /// appt_graph JSONB フィールドをデシリアライズ
    /// </summary>
    private EquipmentApptGraphData DeserializeApptGraph(string apptGraphJson)
    {
        try
        {
            if (String.IsNullOrWhiteSpace(apptGraphJson))
            {
                return new EquipmentApptGraphData();
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<EquipmentApptGraphData>(apptGraphJson, options)
                ?? new EquipmentApptGraphData();
        }
        catch (JsonException)
        {
            // デシリアライズに失敗した場合は空のデータを返す
            return new EquipmentApptGraphData();
        }
    }
}
