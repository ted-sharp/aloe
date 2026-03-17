// <copyright file="FacilityUserEntity.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloe.Apps.Medock.MdLauncherLib.Data.Entities;

/// <summary>
/// 施設ユーザーエンティティ（読取専用・最低限のカラム）。
/// </summary>
[Table("facility_users")]
public class FacilityUserEntity
{
    /// <summary>施設ユーザー ID (PK)。</summary>
    [Key]
    [Column("facility_user_id")]
    public Guid FacilityUserId { get; set; }

    /// <summary>施設 ID。</summary>
    [Column("facility_id")]
    public Guid FacilityId { get; set; }

    /// <summary>ユーザー ID。</summary>
    [Column("user_id")]
    public Guid UserId { get; set; }

    /// <summary>削除フラグ。</summary>
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }
}
