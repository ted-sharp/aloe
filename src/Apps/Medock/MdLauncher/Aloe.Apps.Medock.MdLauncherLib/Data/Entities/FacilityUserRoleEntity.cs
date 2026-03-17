// <copyright file="FacilityUserRoleEntity.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloe.Apps.Medock.MdLauncherLib.Data.Entities;

/// <summary>
/// 施設ユーザーロールエンティティ（読取専用・最低限のカラム）。
/// </summary>
[Table("facility_user_roles")]
public class FacilityUserRoleEntity
{
    /// <summary>施設ユーザーロール ID (PK)。</summary>
    [Key]
    [Column("facility_user_role_id")]
    public Guid FacilityUserRoleId { get; set; }

    /// <summary>施設ユーザー ID。</summary>
    [Column("facility_user_id")]
    public Guid FacilityUserId { get; set; }

    /// <summary>ロールコード。</summary>
    [Column("role_code")]
    [MaxLength(10)]
    public string RoleCode { get; set; } = string.Empty;

    /// <summary>削除フラグ。</summary>
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }
}
