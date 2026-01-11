namespace Aloe.Apps.MedockLib.Services.Dtos;

/// <summary>
/// 祝日DTO
/// </summary>
/// <remarks>
/// FUTURE FEATURE: Role-Based Access Control (RBAC) 実装用に予約されています。
/// 現在はアクティブに使用されていません。実装計画については CLAUDE.md を参照してください。
/// </remarks>
public class HolidayDto
{
    public DateOnly Date { get; set; }
    public string Name { get; set; } = String.Empty;
}

