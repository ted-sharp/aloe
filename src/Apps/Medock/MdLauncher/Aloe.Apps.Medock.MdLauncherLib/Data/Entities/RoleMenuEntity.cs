// <copyright file="RoleMenuEntity.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloe.Apps.Medock.MdLauncherLib.Data.Entities;

/// <summary>
/// ロール・メニュー紐付けエンティティ。
/// </summary>
[Table("role_menus")]
public class RoleMenuEntity
{
    /// <summary>ロールメニュー ID (PK)。</summary>
    [Key]
    [Column("role_menu_id")]
    public Guid RoleMenuId { get; set; }

    /// <summary>ロールコード。</summary>
    [Column("role_code")]
    [MaxLength(10)]
    public string RoleCode { get; set; } = string.Empty;

    /// <summary>メニューコード。</summary>
    [Column("menu_code")]
    [MaxLength(10)]
    public string MenuCode { get; set; } = string.Empty;

    /// <summary>削除フラグ。</summary>
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    /// <summary>作成日時。</summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>更新日時。</summary>
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
