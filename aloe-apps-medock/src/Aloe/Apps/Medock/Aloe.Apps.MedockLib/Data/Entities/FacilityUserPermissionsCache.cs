namespace Aloe.Apps.MedockLib.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 施設ユーザー権限キャッシュエンティティ
/// 存在すれば使用、なければ作成
/// </summary>
/// <remarks>
/// FUTURE FEATURE: Role-Based Access Control (RBAC) 実装用に予約されています。
/// 現在はアクティブに使用されていません。実装計画については CLAUDE.md を参照してください。
/// </remarks>
[Table("facility_user_permissions_cache")]
public class FacilityUserPermissionsCache
{
    /// <summary>施設ユーザーID (PK)</summary>
    [Key]
    [Column("facility_user_id")]
    public Guid FacilityUserId { get; set; }

    /// <summary>権限コード配列</summary>
    [Column("permission_codes")]
    public string[] PermissionCodes { get; set; } = Array.Empty<string>();

    /// <summary>有効期限</summary>
    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    // Navigation Properties
    public virtual FacilityUser FacilityUser { get; set; } = null!;
}

